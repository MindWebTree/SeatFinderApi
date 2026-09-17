using System;
using System.Collections.Generic;
using System.Text;

namespace CounsellingApp.Application.Settings
{
    public class OtpSettings
    {
        public string MessageTemplate { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string MailSubject { get; set; } = string.Empty;
        public string MailTemplateName { get; set; } = string.Empty;
    }
}
