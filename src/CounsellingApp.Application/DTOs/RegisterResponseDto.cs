namespace CounsellingApp.Application.DTOs;

public class RegisterResponseDto
{
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Message { get; set; } = "Registration successful. Please login to continue.";
}
