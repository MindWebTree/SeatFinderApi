namespace CounsellingApp.Application.Interfaces;

public interface IEmailService
{
    Task SendPasswordResetEmailAsync(string toEmail, string fullName, string resetLink);
    Task SendOtpEmailAsync(string toEmail, string otpCode);
}
