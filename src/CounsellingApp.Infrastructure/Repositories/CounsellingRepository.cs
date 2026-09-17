using System.Data;
using CounsellingApp.Application.DTOs;
using CounsellingApp.Application.Interfaces;
using CounsellingApp.Infrastructure.Data;
using Dapper;

namespace CounsellingApp.Infrastructure.Repositories;

/// <summary>
/// courses / states / colleges / seat_allotments are all regular dual-key tables,
/// so filters here are plain INT ids - no Guid/DbType.Guid needed in this repository.
/// </summary>
public class CounsellingRepository : ICounsellingRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public CounsellingRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<SeatDto>> SearchSeatsAsync(
     int rank, string category, string? instituteTypeCodes, string? courseIds, string? stateIds, string? quota)
    {
        using var connection = _connectionFactory.CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("_Rank", rank);
        parameters.Add("_Category", category);
        parameters.Add("_InstituteTypes", instituteTypeCodes);
        parameters.Add("_CourseIds", courseIds);
        parameters.Add("_StateIds", stateIds);
        parameters.Add("_Quota", quota);

        return await connection.QueryAsync<SeatDto>(
            "USP_API_USER_SEARCH_SEATS", parameters, commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<StateDto>> GetStatesAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<StateDto>("USP_API_GET_STATE", commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<CourseDto>> GetCoursesAsync(string? type, string? clinicaltype)
    {
        using var connection = _connectionFactory.CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("_type", type);
        parameters.Add("_clinicaltype", clinicaltype);
        return await connection.QueryAsync<CourseDto>("USP_API_GET_COUSRE", parameters, commandType: CommandType.StoredProcedure);
    }


    public async Task LogSearchAsync(Guid userId, int rank, string category, string? instituteTypeCodes,
    string? courseIds, string? stateIds, string? quota, int totalMatches)
    {
        using var connection = _connectionFactory.CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("_UserId", userId, DbType.Guid);
        parameters.Add("_CandidateRank", rank);
        parameters.Add("_Category", category);
        parameters.Add("_InstituteTypes", instituteTypeCodes);
        parameters.Add("_CourseIds", courseIds);
        parameters.Add("_StateIds", stateIds);
        parameters.Add("_Quota", quota);
        parameters.Add("_TotalMatches", totalMatches);

        await connection.ExecuteAsync("USP_API_LOG_SEAT_SEARCH", parameters, commandType: CommandType.StoredProcedure);
    }
}
