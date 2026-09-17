namespace CounsellingApp.Domain.Entities;

public class RefreshToken
{
    public int Id { get; set; }
    public Guid Guid { get; set; }

    /// <summary>GUID because it relates to Users, which is GUID-only.</summary>
    public Guid UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiryDate { get; set; }
    public bool IsRevoked { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime? UpdatedOn { get; set; }
}
