using System.Net;
using System.Net.Mail;
using CounsellingApp.Application.Interfaces;
using CounsellingApp.Application.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CounsellingApp.Application.Services;

public class EmailService : IEmailService
{
    private readonly SmtpSettings _smtpSettings;
    private readonly OtpSettings _otpSettings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(
        IOptions<SmtpSettings> smtpSettings,
        IOptions<OtpSettings> otpSettings,
        ILogger<EmailService> logger)
    {
        _smtpSettings = smtpSettings.Value;
        _otpSettings = otpSettings.Value;
        _logger = logger;
    }

    public async Task SendPasswordResetEmailAsync(string toEmail, string fullName, string resetLink)
    {
        const string subject = "Reset your Counselling App password";
        var body =
            $"Hi {fullName},\n\n" +
            "We received a request to reset your password. Use the link below to set a new one.\n" +
            "This link expires in 30 minutes and can be used only once.\n\n" +
            $"{resetLink}\n\n" +
            "If you did not request this, you can safely ignore this email.";

        await SendAsync(toEmail, subject, body, logContext: $"Password reset link for {{Email}}: {{Content}}");
    }

    public async Task SendOtpEmailAsync(string toEmail, string otpCode)
    {
        
        var subject = string.IsNullOrWhiteSpace(_otpSettings.MailSubject)
            ? "Your verification code"
            : string.Format(_otpSettings.MailSubject, "Counselling App");

        var body =
            $"Your verification code is: {otpCode}\n\n" +
            "This code expires in 10 minutes and can be used only once.\n\n" +
            "If you did not request this, you can safely ignore this email.";

        await SendAsync(toEmail, subject, body, logContext: $"OTP for {{Email}}: {{Content}}");
    }

    private async Task SendAsync(string toEmail, string subject, string body, string logContext)
    {
        if (string.IsNullOrWhiteSpace(_smtpSettings.Host))
        {
            // No SMTP server configured (e.g. local development) - log instead of failing the request.
            _logger.LogInformation(logContext, toEmail, body);
            return;
        }

        using var client = new SmtpClient(_smtpSettings.Host, _smtpSettings.Port)
        {
            Credentials = new NetworkCredential(_smtpSettings.Username, _smtpSettings.Password),
            EnableSsl = _smtpSettings.EnableSsl
        };

        using var message = new MailMessage(_smtpSettings.FromEmail, toEmail, subject, body);
        await client.SendMailAsync(message);
    }
}