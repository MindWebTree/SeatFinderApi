using CounsellingApp.Application.DTOs;
using CounsellingApp.Application.Interfaces;
using CounsellingApp.Domain.Enum;
using Microsoft.Extensions.Logging;

namespace CounsellingApp.Application.Services;

public class CounsellingService : ICounsellingService
{
    // How many ranks above the user's own rank still count as a "near miss".
    private const int NearMissRankWindow = 1000;
   // private const int MaxNearMissResults = 5;
    private readonly ILogger<CounsellingService> _logger;

    private readonly ICounsellingRepository _counsellingRepository;

    public CounsellingService(ICounsellingRepository counsellingRepository, ILogger<CounsellingService> logger)
    {
        _counsellingRepository = counsellingRepository;
        _logger = logger;
    }

    //public async Task<SeatFinderResponseDto> SearchSeatsAsync(SeatFinderRequestDto request, Guid userId)
    //{
    //    // The API accepts friendly names ("Govt,Private") - the enum lives only in C#,
    //    // so translate to the numeric codes stored in the DB before calling the repository.
    //    var instituteTypeCodes = ParseInstituteTypeCodes(request.InstituteTypes);

    //    var seats = (await _counsellingRepository.SearchSeatsAsync(
    //     request.Rank, request.Category, instituteTypeCodes, request.CourseIds, request.StateIds, request.Quota)).ToList();

    //    foreach (var seat in seats)
    //    {
    //        if (seat.Round1ClosingRank.HasValue)
    //            seat.RankDifference = seat.Round1ClosingRank.Value - request.Rank;
    //    }

    //    var closedJustBeforeYou = seats
    //        .Where(s => s.Round1ClosingRank.HasValue
    //                    && s.Round1ClosingRank.Value < request.Rank
    //                    && request.Rank - s.Round1ClosingRank.Value <= NearMissRankWindow)
    //        .OrderByDescending(s => s.Round1ClosingRank)
    //        .Take(MaxNearMissResults)
    //        .ToList();

    //    var withinReach = seats
    //        .Where(s => !s.Round1ClosingRank.HasValue || s.Round1ClosingRank.Value >= request.Rank)
    //        .OrderBy(s => s.Round1ClosingRank ?? int.MaxValue)
    //        .ToList();

    //    var response = new SeatFinderResponseDto
    //    {
    //        TotalMatches = seats.Count,
    //        ClosedJustBeforeYou = closedJustBeforeYou,
    //        WithinReach = withinReach
    //    };

    //    // Fire-and-log: every "Find my seats" search gets recorded against the logged-in
    //    // user. A logging failure must never break the actual search response, so it's
    //    // caught and swallowed (just logged as a warning) rather than rethrown.
    //    try
    //    {
    //        await _counsellingRepository.LogSearchAsync(
    //            userId, request.Rank, request.Category, instituteTypeCodes,
    //            request.CourseIds, request.StateIds, request.Quota, response.TotalMatches);
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogWarning(ex, "Failed to log seat search for user {UserId}", userId);
    //    }

    //    return response;
    //}

    public async Task<SeatFinderResponseDto> SearchSeatsAsync(SeatFinderRequestDto request, Guid userId)
    {
        // The API accepts friendly names ("Govt,Private") - the enum lives only in C#,
        // so translate to the numeric codes stored in the DB before calling the repository.
        var instituteTypeCodes = ParseInstituteTypeCodes(request.InstituteTypes);

        var seats = (await _counsellingRepository.SearchSeatsAsync(
         request.Rank, request.Category, instituteTypeCodes, request.CourseIds, request.StateIds, request.Quota)).ToList();

        foreach (var seat in seats)
        {
            if (seat.Round1ClosingRank.HasValue)
                seat.RankDifference = seat.Round1ClosingRank.Value - request.Rank;
        }

        // Every seat whose Round1ClosingRank falls within NearMissRankWindow (1000) ranks
        // BEFORE the user's rank - no Take() limit, so all matching colleges show up,
        // not just the closest 5.
        var closedJustBeforeYou = seats
            .Where(s => s.Round1ClosingRank.HasValue
                        && s.Round1ClosingRank.Value < request.Rank
                        && request.Rank - s.Round1ClosingRank.Value <= NearMissRankWindow)
            .OrderByDescending(s => s.Round1ClosingRank)
            .ToList();

        var withinReach = seats
            .Where(s => !s.Round1ClosingRank.HasValue || s.Round1ClosingRank.Value >= request.Rank)
            .OrderBy(s => s.Round1ClosingRank ?? int.MaxValue)
            .ToList();

        var response = new SeatFinderResponseDto
        {
            TotalMatches = seats.Count,
            ClosedJustBeforeYou = closedJustBeforeYou,
            WithinReach = withinReach
        };

        // Fire-and-log: every "Find my seats" search gets recorded against the logged-in
        // user. A logging failure must never break the actual search response, so it's
        // caught and swallowed (just logged as a warning) rather than rethrown.
        try
        {
            await _counsellingRepository.LogSearchAsync(
                userId, request.Rank, request.Category, instituteTypeCodes,
                request.CourseIds, request.StateIds, request.Quota, response.TotalMatches);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to log seat search for user {UserId}", userId);
        }

        return response;
    }

    public Task<IEnumerable<StateDto>> GetStatesAsync() => _counsellingRepository.GetStatesAsync();

    public Task<IEnumerable<CourseDto>> GetCoursesAsync(string? type, string? clinicaltype) => _counsellingRepository.GetCoursesAsync(type, clinicaltype);


    //private static string? ParseInstituteTypeCodes(string? instituteTypeNames)
    //{
    //    if (string.IsNullOrWhiteSpace(instituteTypeNames)) return null;
    //    var codes = instituteTypeNames
    //        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
    //        .Where(name => Enum.TryParse<InstituteType>(name, true, out _))
    //        .Select(name => (int)Enum.Parse<InstituteType>(name, true));
    //    return codes.Any() ? string.Join(',', codes) : null;
    //}

    private static readonly Dictionary<string, int> InstituteTypeCodes = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Govt"] = 1,
        ["Private"] = 2,
        ["Deemed"] = 3,
        ["DNB Hospital"] = 4,
        ["Self Financed"] = 5
    };

    private static string? ParseInstituteTypeCodes(string? instituteTypeNames)
    {
        if (string.IsNullOrWhiteSpace(instituteTypeNames)) return null;
        var codes = instituteTypeNames
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(name => InstituteTypeCodes.ContainsKey(name))
            .Select(name => InstituteTypeCodes[name]);
        return codes.Any() ? string.Join(',', codes) : null;
    }
}
