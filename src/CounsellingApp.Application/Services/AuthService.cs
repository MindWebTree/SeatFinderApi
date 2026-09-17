using System.Security.Cryptography;
using System.Text;
using CounsellingApp.Application.DTOs;
using CounsellingApp.Application.Exceptions;
using CounsellingApp.Application.Interfaces;
using CounsellingApp.Application.Settings;
using CounsellingApp.Domain.Constants;
using CounsellingApp.Domain.Entities;
using Microsoft.Extensions.Options;

namespace CounsellingApp.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly IEmailService _emailService;
    private readonly ISmsService _smsService;
    private readonly JwtSettings _jwtSettings;

    public AuthService(
        IUserRepository userRepository,
        ITokenService tokenService,
        IEmailService emailService,
        ISmsService smsService,
        IOptions<JwtSettings> jwtSettings)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
        _emailService = emailService;
        _smsService = smsService;
        _jwtSettings = jwtSettings.Value;
    }

    /// <summary>Creates the account directly - no OTP needed here. The first time this
    /// person actually logs in (via /login + /login/verify-otp), that's when their
    /// email/phone gets exercised for real.</summary>
    public async Task<RegisterResponseDto> RegisterAsync(RegisterRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) && string.IsNullOrWhiteSpace(request.PhoneNumber))
            throw new AppException("Provide an email or phone number.", 400);

        var normalizedEmail = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim().ToLowerInvariant();
        var normalizedPhone = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim();

        var existing = await _userRepository.GetByEmailOrPhoneAsync(normalizedEmail, normalizedPhone);
        if (existing is not null)
            throw new AppException("An account with this email or phone number already exists.", 409);

        var userId = await _userRepository.CreateUserViaOtpAsync(
            request.FullName.Trim(), normalizedEmail, normalizedPhone, request.StateId, RoleIds.Student);

        return new RegisterResponseDto
        {
            UserId = userId,
            FullName = request.FullName.Trim(),
            Email = normalizedEmail ?? string.Empty
        };
    }

    /// <summary>Step 1 of login: send an OTP to an EXISTING account's email or phone.
    /// Errors if no account exists - this endpoint never creates one (Register does that).</summary>
    public async Task LoginAsync(LoginRequestDto request)
    {
        var identifier = request.EmailOrPhoneNumber.Trim();
        var isEmail = LooksLikeEmail(identifier);
        var normalizedIdentifier = isEmail ? identifier.ToLowerInvariant() : identifier;

        var user = isEmail
            ? await _userRepository.GetByEmailOrPhoneAsync(normalizedIdentifier, null)
            : await _userRepository.GetByEmailOrPhoneAsync(null, normalizedIdentifier);

        if (user is null)
            throw new AppException("No account found with this email or phone number. Please sign up first.", 404);
        if (!user.IsActive)
            throw new AppException("This account is inactive.", 401);

        var otpCode = GenerateOtpCode();
        var otpHash = HashToken(otpCode);
        var expiry = DateTime.UtcNow.AddMinutes(10);
        var identifierType = isEmail ? "Email" : "Phone";
        await _userRepository.CreateOtpAsync(normalizedIdentifier, identifierType, otpHash, expiry);

        if (isEmail)
            await _emailService.SendOtpEmailAsync(normalizedIdentifier, otpCode);
        else
            await _smsService.SendOtpSmsAsync(normalizedIdentifier, otpCode);
    }

    /// <summary>Step 2 of login: verify the OTP and issue tokens.</summary>
    public async Task<LoginResponseDto> VerifyLoginOtpAsync(VerifyLoginOtpRequestDto request)
    {
        var identifier = request.EmailOrPhoneNumber.Trim();
        var isEmail = LooksLikeEmail(identifier);
        var normalizedIdentifier = isEmail ? identifier.ToLowerInvariant() : identifier;

        var otp = await _userRepository.GetLatestOtpAsync(normalizedIdentifier);
        if (otp is null || otp.ExpiryDate < DateTime.UtcNow)
            throw new AppException("OTP has expired. Request a new one.", 400);
        if (otp.Attempts >= 5)
            throw new AppException("Too many attempts. Request a new OTP.", 429);

        if (otp.OtpHash != HashToken(request.OtpCode.Trim()))
        {
            await _userRepository.IncrementOtpAttemptsAsync(otp.Id);
            throw new AppException("Incorrect code.", 400);
        }
        await _userRepository.MarkOtpUsedAsync(otp.Id);

        var user = isEmail
            ? await _userRepository.GetByEmailOrPhoneAsync(normalizedIdentifier, null)
            : await _userRepository.GetByEmailOrPhoneAsync(null, normalizedIdentifier);

        if (user is null || !user.IsActive)
            throw new AppException("Account not found or inactive.", 401);

        return await IssueTokensAsync(user);
    }

    public async Task<LoginResponseDto> RefreshTokenAsync(string refreshToken)
    {
        var storedToken = await _userRepository.GetRefreshTokenAsync(refreshToken);
        if (storedToken is null || storedToken.IsRevoked || storedToken.ExpiryDate < DateTime.UtcNow)
            throw new AppException("Invalid or expired refresh token.", 401);

        var user = await _userRepository.GetByIdAsync(storedToken.UserId);
        if (user is null || !user.IsActive)
            throw new AppException("Account not found or inactive.", 401);

        // Rotate the refresh token so a leaked/used token can't be replayed.
        await _userRepository.RevokeRefreshTokenAsync(refreshToken);

        return await IssueTokensAsync(user);
    }

    /// <summary>
    /// Simple heuristic: an '@' means treat the identifier as an email, otherwise
    /// treat it as a phone number. Good enough since the two formats never overlap.
    /// </summary>
    private static bool LooksLikeEmail(string identifier) => identifier.Contains('@');

    private async Task<LoginResponseDto> IssueTokensAsync(User user)
    {
        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();
        var refreshExpiry = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays);

        await _userRepository.CreateRefreshTokenAsync(user.Id, refreshToken, refreshExpiry);

        var roleNames = (user.Roles ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();

        return new LoginResponseDto
        {
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Roles = roleNames.Count > 0 ? roleNames : new List<string> { "Student" },
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            AccessTokenExpiry = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpiryMinutes)
        };
    }

    /// <summary>6-digit numeric OTP, e.g. "042913". Cryptographically random, not Random/next().</summary>
    private static string GenerateOtpCode()
    {
        var value = RandomNumberGenerator.GetInt32(0, 1_000_000);
        return value.ToString("D6");
    }

    private static string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes); // what actually gets stored in the database
    }
}