namespace CounsellingApp.Domain.Entities;

public class SeatSearchLog
{
    public int Id { get; set; }
    public Guid Guid { get; set; }
    public Guid UserId { get; set; }
    public int CandidateRank { get; set; }
    public string Category { get; set; } = string.Empty;
    public string? InstituteTypes { get; set; }
    public string? CourseIds { get; set; }
    public string? StateIds { get; set; }
    public string? Quota { get; set; }
    public int? TotalMatches { get; set; }
    public DateTime CreatedOn { get; set; }
}