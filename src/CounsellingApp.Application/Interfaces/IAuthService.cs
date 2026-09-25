using CounsellingApp.Application.DTOs;

namespace CounsellingApp.Application.Interfaces;

public interface IAuthService
{
    //Task SendOtpAsync(SendOtpRequestDto request);
    Task<RegisterResponseDto> RegisterAsync(RegisterRequestDto request);
    Task LoginAsync(LoginRequestDto request);
    Task<LoginResponseDto> VerifyLoginOtpAsync(VerifyLoginOtpRequestDto request);
    Task<LoginResponseDto> RefreshTokenAsync(string refreshToken);
    Task<bool> DeleteAccountAsync(Guid userId);
}
