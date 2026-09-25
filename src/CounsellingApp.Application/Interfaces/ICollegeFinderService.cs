using CounsellingApp.Application.DTOs.CollegeFinder;
using CounsellingApp.Domain.Enum;


namespace CounsellingApp.Application.Interfaces;

public interface ICollegeFinderService
{
    Task<IEnumerable<CfCourseDto>> GetCoursesAsync();
    Task<IEnumerable<CfStateDto>> GetStatesAsync(IEnumerable<Guid>? courseGuids);
    Task<IEnumerable<CfCollegeOptionDto>> GetCollegesAsync(IEnumerable<Guid>? courseGuids, Guid? stateGuid,
        CollegeCategory? category, string? search);
    Task<CfSearchResponseDto> SearchAsync(CfSearchRequestDto request);
    Task<CfCollegeDetailDto?> GetCollegeDetailAsync(Guid collegeGuid);
}