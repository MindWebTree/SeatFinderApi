using CounsellingApp.Application.DTOs;

namespace CounsellingApp.Application.Interfaces;

public interface ICounsellingService
{
    //Task<SeatFinderResponseDto> SearchSeatsAsync(SeatFinderRequestDto request);
    Task<IEnumerable<StateDto>> GetStatesAsync();
    Task<IEnumerable<CourseDto>> GetCoursesAsync(string? type, string? clinicaltype);
    Task<SeatFinderResponseDto> SearchSeatsAsync(SeatFinderRequestDto request, Guid userId);

}
