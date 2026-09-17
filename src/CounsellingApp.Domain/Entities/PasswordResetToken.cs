namespace CounsellingApp.Domain.Entities;

public class PasswordResetToken
{
    /// <summary>Internal auto-increment id - fast primary key for lookups/updates.</summary>
    public int Id { get; set; }

    /// <summary>Public-facing GUID identifier.</summary>
    public Guid Guid { get; set; }

    /// <summary>GUID because it relates to Users, which is GUID-only.</summary>
    public Guid UserId { get; set; }

    /// <summary>SHA-256 hash of the raw token that was emailed to the user.</summary>
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiryDate { get; set; }
    public bool IsUsed { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime? UpdatedOn { get; set; }
}
