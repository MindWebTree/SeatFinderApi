using CounsellingApp.Application.DTOs.CollegeFinder;
using CounsellingApp.Application.Interfaces;
using CounsellingApp.Domain.Enum;

using CounsellingApp.Infrastructure.Data;
using Dapper;
using System.Data;

namespace CounsellingApp.Infrastructure.Repositories;

public class CollegeFinderRepository : ICollegeFinderRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public CollegeFinderRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    private static string? G(Guid? value) => value?.ToString("D");

    // AIQ=1, State=2, Private=3, Deemed=4, Trust=5 -> SP _Category
    private static byte? C(CollegeCategory? value) => value is null ? null : (byte)value.Value;

    // List -> "guid1,guid2". Khaali list -> NULL = saare courses
    private static string? Gs(IReadOnlyCollection<Guid> values) =>
        values.Count == 0 ? null : string.Join(",", values.Select(v => v.ToString("D")));

    public async Task<IEnumerable<CfCourseDto>> GetCoursesAsync(string? academicYear)
    {
        using var connection = _connectionFactory.CreateConnection();
        var p = new DynamicParameters();
        p.Add("_AcademicYear", academicYear);
        return await connection.QueryAsync<CfCourseDto>(
            "USP_API_CF_GET_COURSES", p, commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<CfStateDto>> GetStatesAsync(IReadOnlyCollection<Guid> courseGuids, string? academicYear)
    {
        using var connection = _connectionFactory.CreateConnection();
        var p = new DynamicParameters();
        p.Add("_CourseGuids", Gs(courseGuids));
        p.Add("_AcademicYear", academicYear);
        return await connection.QueryAsync<CfStateDto>(
            "USP_API_CF_GET_STATES", p, commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<CfCollegeOptionDto>> GetCollegesAsync(IReadOnlyCollection<Guid> courseGuids,
        Guid? stateGuid, CollegeCategory? category, string? search, string? academicYear)
    {
        using var connection = _connectionFactory.CreateConnection();
        var p = new DynamicParameters();
        p.Add("_CourseGuids", Gs(courseGuids));
        p.Add("_StateGuid", G(stateGuid));
        p.Add("_Category", C(category));
        p.Add("_Search", search);
        p.Add("_AcademicYear", academicYear);
        return await connection.QueryAsync<CfCollegeOptionDto>(
            "USP_API_CF_GET_COLLEGES", p, commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<CfCollegeResultDto>> SearchAsync(IReadOnlyCollection<Guid> courseGuids,
        Guid? stateGuid, CollegeCategory? category, Guid? collegeGuid, string? academicYear, int offset, int limit)
    {
        using var connection = _connectionFactory.CreateConnection();
        var p = new DynamicParameters();
        p.Add("_CourseGuids", Gs(courseGuids));
        p.Add("_StateGuid", G(stateGuid));
        p.Add("_Category", C(category));
        p.Add("_CollegeGuid", G(collegeGuid));
        p.Add("_AcademicYear", academicYear);
        p.Add("_Offset", offset);
        p.Add("_Limit", limit);
        return await connection.QueryAsync<CfCollegeResultDto>(
            "USP_API_CF_SEARCH", p, commandType: CommandType.StoredProcedure);
    }

    public async Task<CfCollegeDetailDto?> GetCollegeDetailAsync(Guid collegeGuid, string? academicYear)
    {
        using var connection = _connectionFactory.CreateConnection();
        var p = new DynamicParameters();
        p.Add("_CollegeGuid", G(collegeGuid));
        p.Add("_AcademicYear", academicYear);

        using var multi = await connection.QueryMultipleAsync(
            "USP_API_CF_GET_COLLEGE_DETAIL", p, commandType: CommandType.StoredProcedure);

        var college = await multi.ReadFirstOrDefaultAsync<CfCollegeInfoDto>();
        if (college is null) return null;

        var courses = (await multi.ReadAsync<CfCollegeCourseDto>()).ToList();
        return new CfCollegeDetailDto
        {
            College = college,
            Courses = courses,
            TotalSeats = courses.Sum(c => c.Seats)
        };
    }
}