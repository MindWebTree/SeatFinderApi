using CounsellingApp.Domain.Enum;

namespace CounsellingApp.Application.DTOs;

public class SeatDto
{
    public int SeatId { get; set; }
    public Guid SeatGuid { get; set; }
    public string CollegeName { get; set; } = string.Empty;
    public InstituteType InstituteType { get; set; }
    public string StateName { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public string SeatCategory { get; set; } = string.Empty;
    public int TotalSeats { get; set; }
    public int? Round1ClosingRank { get; set; }
    public int? Round2ClosingRank { get; set; }
    public int? Round3ClosingRank { get; set; }

    /// <summary>Round1ClosingRank minus the user's rank. Negative = closed before you.</summary>
    public int RankDifference { get; set; }
}
