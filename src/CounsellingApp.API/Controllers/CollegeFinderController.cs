using CounsellingApp.Application.DTOs.CollegeFinder;
using CounsellingApp.Application.Interfaces;
using CounsellingApp.Domain.Enum;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CounsellingApp.API.Controllers;

[ApiController]
[Route("api/college-finder")]
[AllowAnonymous]
public class CollegeFinderController : ControllerBase
{
    private readonly ICollegeFinderService _service;

    public CollegeFinderController(ICollegeFinderService service)
    {
        _service = service;
    }

    [HttpGet("courses")]
    public async Task<IActionResult> GetCourses() =>
        Ok(new { success = true, data = await _service.GetCoursesAsync() });

    [HttpGet("states")]
    public async Task<IActionResult> GetStates([FromQuery] List<Guid>? courseGuids) =>
        Ok(new { success = true, data = await _service.GetStatesAsync(courseGuids) });

    // ?category=AIQ / State / Private / Deemed / Trust
    [HttpGet("colleges")]
    public async Task<IActionResult> GetColleges([FromQuery] List<Guid>? courseGuids, [FromQuery] Guid? stateGuid,
        [FromQuery] CollegeCategory? category, [FromQuery] string? search)
    {

        return Ok(new { success = true, data = await _service.GetCollegesAsync(courseGuids, stateGuid, category, search) });
    }

    // ?courseGuid=...&stateGuid=...&category=AIQ&page=1&pageSize=20
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] CfSearchRequestDto request)
    {
        try
        {
            return Ok(new { success = true, data = await _service.SearchAsync(request) });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpGet("colleges/{collegeGuid:guid}")]
    public async Task<IActionResult> GetCollegeDetail(Guid collegeGuid)
    {
        var detail = await _service.GetCollegeDetailAsync(collegeGuid);
        return detail is null
            ? NotFound(new { success = false, message = "College not found." })
            : Ok(new { success = true, data = detail });
    }
}