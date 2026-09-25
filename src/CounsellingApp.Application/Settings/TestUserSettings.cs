using System;
using System.Collections.Generic;
using System.Text;

namespace CounsellingApp.Application.Settings
{
    public class TestUserSettings
    {
        public bool Enabled { get; set; }
        public Guid UserId { get; set; }
        public string StaticOtp { get; set; } = "123456";
    }
}
