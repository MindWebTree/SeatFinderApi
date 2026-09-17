namespace CounsellingApp.Application.DTOs;

public class SeatFinderResponseDto
{
    public int TotalMatches { get; set; }
    public List<SeatDto> ClosedJustBeforeYou { get; set; } = new();
    public List<SeatDto> WithinReach { get; set; } = new();
}
