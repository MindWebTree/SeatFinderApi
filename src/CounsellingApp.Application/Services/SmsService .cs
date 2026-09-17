using System.Net.Http;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CounsellingApp.Application.Interfaces;
using CounsellingApp.Application.Settings;

namespace CounsellingApp.Application.Services;

public class SmsService : ISmsService
{
    private readonly HttpClient _httpClient;
    private readonly OtpSettings _settings;
    private readonly ILogger<SmsService> _logger;

    public SmsService(IHttpClientFactory httpClientFactory, IOptions<OtpSettings> settings, ILogger<SmsService> logger)
    {
        _httpClient = httpClientFactory.CreateClient();
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SendOtpSmsAsync(string phoneNumber, string otpCode, string? recipientName = null)
    {
        if (string.IsNullOrWhiteSpace(_settings.Url))
        {
            _logger.LogInformation("SMS gateway not configured. OTP for {Phone}: {Otp}", phoneNumber, otpCode);
            return;
        }

        var message = string.Format(_settings.MessageTemplate, recipientName ?? "Student", otpCode);

        // Template has two number placeholders ({0}{1}) - {0} is the country code,
        // {1} is the local number. Strip any leading "+"/"91" the user might have
        // typed so we don't accidentally double it up.
        var localNumber = phoneNumber.TrimStart('+');
        if (localNumber.StartsWith("91") && localNumber.Length > 10)
            localNumber = localNumber[2..];

        var url = string.Format(_settings.Url, "91", localNumber, Uri.EscapeDataString(message));

        try
        {
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("SMS gateway returned {Status} for {Phone}: {Body}", response.StatusCode, phoneNumber, body);
            }
        }
        catch (Exception ex)
        {
            // Never let an SMS delivery failure break the OTP flow - the user can
            // still get the code by email, and the OTP is already stored server-side.
            _logger.LogError(ex, "Failed to send OTP SMS to {Phone}", phoneNumber);
        }
    }
}