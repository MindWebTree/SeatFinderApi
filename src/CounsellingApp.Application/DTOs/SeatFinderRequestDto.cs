using System.ComponentModel.DataAnnotations;

namespace CounsellingApp.Application.DTOs;

public class SeatFinderRequestDto
{
    [Required] public int Rank { get; set; }
    [Required] public string Category { get; set; } = string.Empty;

    /// <summary>Comma-separated InstituteType names: "Govt,Private". Empty = all.</summary>
    public string? InstituteTypes { get; set; }

    /// <summary>Comma-separated course ids: "1,2,3". Empty = all.</summary>
    public string? CourseIds { get; set; }

    /// <summary>Comma-separated state ids: "5,12". Empty = all.</summary>
    public string? StateIds { get; set; }

    public string? Quota { get; set; }
    public string? Rounds { get; set; }

}
