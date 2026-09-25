using CounsellingApp.Application.DTOs.CollegeFinder;
using CounsellingApp.Application.Interfaces;
using CounsellingApp.Domain.Enum;


namespace CounsellingApp.Application.Services;

public class CollegeFinderService : ICollegeFinderService
{
    private const int MaxPageSize = 100;
    private readonly ICollegeFinderRepository _repository;

    public CollegeFinderService(ICollegeFinderRepository repository)
    {
        _repository = repository;
    }

    public Task<IEnumerable<CfCourseDto>> GetCoursesAsync() =>
        _repository.GetCoursesAsync(null);

    public Task<IEnumerable<CfStateDto>> GetStatesAsync(IEnumerable<Guid>? courseGuids) =>
    _repository.GetStatesAsync(Clean(courseGuids), null);

    public Task<IEnumerable<CfCollegeOptionDto>> GetCollegesAsync(IEnumerable<Guid>? courseGuids, Guid? stateGuid,
        CollegeCategory? category, string? search) =>
        _repository.GetCollegesAsync(Clean(courseGuids), stateGuid, category, search?.Trim(), null);

    public async Task<CfSearchResponseDto> SearchAsync(CfSearchRequestDto request)
    {
        var page = Math.Max(request.Page, 1);
        var pageSize = Math.Clamp(request.PageSize, 1, MaxPageSize);
        var offset = (page - 1) * pageSize;

        var items = (await _repository.SearchAsync(Clean(request.CourseGuids), request.StateGuid,
            request.Category, request.CollegeGuid, null, offset, pageSize)).ToList();

        var first = items.FirstOrDefault();
        var totalCount = first?.TotalCount ?? 0;

        return new CfSearchResponseDto
        {
            TotalCount = totalCount,
            TotalColleges = first?.TotalColleges ?? 0,
            TotalSeats = first?.TotalSeats ?? 0,
            Page = page,
            PageSize = pageSize,
            HasMore = offset + items.Count < totalCount,
            Items = items
        };
    }
    public Task<CfCollegeDetailDto?> GetCollegeDetailAsync(Guid collegeGuid) =>
        _repository.GetCollegeDetailAsync(collegeGuid, null);

    private static IReadOnlyCollection<Guid> Clean(IEnumerable<Guid>? guids) =>
       guids?.Where(g => g != Guid.Empty).Distinct().ToList() ?? [];
}