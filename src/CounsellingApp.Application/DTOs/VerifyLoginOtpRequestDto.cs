using System.ComponentModel.DataAnnotations;

namespace CounsellingApp.Application.DTOs;

public class VerifyLoginOtpRequestDto
{
    /// <summary>Same value that was passed to /login - either email or phone.</summary>
    [Required]
    public string EmailOrPhoneNumber { get; set; } = string.Empty;

    [Required, StringLength(6, MinimumLength = 6)]
    public string OtpCode { get; set; } = string.Empty;
}