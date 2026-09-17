using System;
using System.Collections.Generic;
using System.Text;

namespace CounsellingApp.Application.Interfaces
{
    public interface ISmsService
    {
        
            Task SendOtpSmsAsync(string phoneNumber, string otpCode, string? recipientName = null);
        
    }
}
