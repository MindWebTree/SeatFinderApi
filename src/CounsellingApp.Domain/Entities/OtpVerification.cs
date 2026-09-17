using System;
using System.Collections.Generic;
using System.Text;

namespace CounsellingApp.Domain.Entities
{
    public class OtpVerification
    {
        public int Id { get; set; }
        public Guid Guid { get; set; }
        public string Identifier { get; set; } = string.Empty;
        public string IdentifierType { get; set; } = string.Empty; // "Email" or "Phone"
        public string OtpHash { get; set; } = string.Empty;
        public DateTime ExpiryDate { get; set; }
        public bool IsUsed { get; set; }
        public int Attempts { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
