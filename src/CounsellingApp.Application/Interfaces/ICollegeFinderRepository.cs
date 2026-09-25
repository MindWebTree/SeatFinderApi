using CounsellingApp.Application.DTOs.CollegeFinder;
using CounsellingApp.Domain.Enum;


namespace CounsellingApp.Application.Interfaces;

public interface ICollegeFinderRepository
{
    Task<IEnumerable<CfCourseDto>> GetCoursesAsync(string? academicYear);
    Task<IEnumerable<CfStateDto>> GetStatesAsync(IReadOnlyCollection<Guid> courseGuids, string? academicYear);
    Task<IEnumerable<CfCollegeOptionDto>> GetCollegesAsync(IReadOnlyCollection<Guid> courseGuids, Guid? stateGuid,
        CollegeCategory? category, string? search, string? academicYear);
    Task<IEnumerable<CfCollegeResultDto>> SearchAsync(IReadOnlyCollection<Guid> courseGuids, Guid? stateGuid,
        CollegeCategory? category, Guid? collegeGuid, string? academicYear, int offset, int limit);
    Task<CfCollegeDetailDto?> GetCollegeDetailAsync(Guid collegeGuid, string? academicYear);
}