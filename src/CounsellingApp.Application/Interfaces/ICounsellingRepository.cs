using CounsellingApp.Application.DTOs;

namespace CounsellingApp.Application.Interfaces;

public interface ICounsellingRepository
{
    Task<IEnumerable<SeatDto>> SearchSeatsAsync(int rank, string category, string? instituteTypeCodes, string? courseIds, string? stateIds, string? quota);
    Task<IEnumerable<StateDto>> GetStatesAsync();
    Task<IEnumerable<CourseDto>> GetCoursesAsync(string? type,string? clinicaltype);
    Task LogSearchAsync(Guid userId, int rank, string category, string? instituteTypeCodes,
    string? courseIds, string? stateIds, string? quota, int totalMatches);
}
