using CounsellingApp.Application.DTOs;
using CounsellingApp.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace CounsellingApp.API.Controllers;

[ApiController]
[Route("api/counselling")]
[Authorize] 
public class CounsellingController : ControllerBase
{
    private readonly ICounsellingService _counsellingService;

    public CounsellingController(ICounsellingService counsellingService)
    {
        _counsellingService = counsellingService;
    }

    /// <summary>
    /// Core counselling logic: given the logged-in user's rank + category (+ optional filters),
    /// returns seats that closed just before their rank and seats within reach - round 1 to 3.
    /// </summary>
    //[HttpGet("seat-finder")]
    //[AllowAnonymous]
    //public async Task<IActionResult> SeatFinder([FromQuery] SeatFinderRequestDto request)
    //{
    //    var result = await _counsellingService.SearchSeatsAsync(request);
    //    return Ok(new { success = true, data = result });
    //}

    [HttpGet("seat-finder")]
    public async Task<IActionResult> SeatFinder([FromQuery] SeatFinderRequestDto request)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
         ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized(new { success = false, message = "Invalid or missing user identity." });

        var result = await _counsellingService.SearchSeatsAsync(request, userId);
        return Ok(new { success = true, data = result });
    }

    [HttpGet("states")]
    [AllowAnonymous]
    public async Task<IActionResult> GetStates()
    {
        var result = await _counsellingService.GetStatesAsync();
        return Ok(new { success = true, data = result });
    }

    [HttpGet("courses")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCourses([FromQuery] string? type, [FromQuery] string? clinicaltype)
    {
        var result = await _counsellingService.GetCoursesAsync(type, clinicaltype);
        return Ok(new { success = true, data = result });
    }
}
