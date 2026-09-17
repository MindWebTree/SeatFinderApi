using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CounsellingApp.Application.DTOs
{
    public class SendOtpRequestDto
    {
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
    }

    public class VerifyOtpRequestDto
    {
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        [Required] public string OtpCode { get; set; } = string.Empty;

        // Only needed the first time (signup) - ignored if the user already exists.
        public string? FullName { get; set; }
        public int? StateId { get; set; }
    }
}
