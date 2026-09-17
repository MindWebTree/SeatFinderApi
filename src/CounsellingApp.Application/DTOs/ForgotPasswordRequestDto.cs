using System.ComponentModel.DataAnnotations;

namespace CounsellingApp.Application.DTOs;

public class ForgotPasswordRequestDto
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;
}
