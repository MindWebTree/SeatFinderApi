using System.ComponentModel.DataAnnotations;

namespace CounsellingApp.Application.DTOs;

public class LoginRequestDto
{
    /// <summary>Either the user's email address or their phone number.</summary>
    [Required]
    public string EmailOrPhoneNumber { get; set; } = string.Empty;
}