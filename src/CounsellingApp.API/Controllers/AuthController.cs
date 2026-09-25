using CounsellingApp.Application.DTOs;
using CounsellingApp.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace CounsellingApp.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>Create a new account directly - no OTP required here.</summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        var result = await _authService.RegisterAsync(request);
        return CreatedAtAction(nameof(Register), new { result.UserId }, new { success = true, data = result });
    }

    /// <summary>Step 1 of login: sends a 6-digit OTP to an existing account's email or phone.</summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        await _authService.LoginAsync(request);
        return Ok(new { success = true, message = "OTP sent." });
    }

    /// <summary>Step 2 of login: verify the OTP, receive a JWT access token + refresh token.</summary>
    [HttpPost("login/verify-otp")]
    public async Task<IActionResult> VerifyLoginOtp([FromBody] VerifyLoginOtpRequestDto request)
    {
        var result = await _authService.VerifyLoginOtpAsync(request);
        return Ok(new { success = true, data = result });
    }

    /// <summary>Exchange a valid refresh token for a new access token + refresh token.</summary>
    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto request)
    {
        var result = await _authService.RefreshTokenAsync(request.RefreshToken);
        return Ok(new { success = true, data = result });
    }

    
    [Authorize]
    [HttpDelete("delete-account")]
    public async Task<IActionResult> DeleteAccount()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue("sub");

        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized(new { success = false, message = "Invalid or missing user identity." });

        var deleted = await _authService.DeleteAccountAsync(userId);

        return deleted
            ? Ok(new { success = true, message = "Your account has been deleted." })
            : NotFound(new { success = false, message = "Account not found or already deleted." });
    }
}