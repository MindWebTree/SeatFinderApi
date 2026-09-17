using CounsellingApp.Application.DTOs;
using CounsellingApp.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

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
}