namespace CounsellingApp.Domain.Entities;

public class SeatAllotment
{
    public int Id { get; set; }
    public Guid Guid { get; set; }

    /// <summary>INT - relates to Colleges / Courses, regular dual-key tables.</summary>
    public int CollegeId { get; set; }
    public int CourseId { get; set; }
    public string Quota { get; set; } = "AIQ";

    /// <summary>Open, OBC, SC, ST, EWS, PwD.</summary>
    public string SeatCategory { get; set; } = string.Empty;
    public int TotalSeats { get; set; }
    public int? Round1ClosingRank { get; set; }
    public int? Round2ClosingRank { get; set; }
    public int? Round3ClosingRank { get; set; }
    public string AcademicYear { get; set; } = string.Empty;
}
