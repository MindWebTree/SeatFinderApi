using System.ComponentModel.DataAnnotations;

namespace CounsellingApp.Application.DTOs;

public class RegisterRequestDto
{
    [Required, StringLength(150, MinimumLength = 2)]
    public string FullName { get; set; } = string.Empty;

    /// <summary>At least one of Email/PhoneNumber is required - checked in AuthService
    /// since DataAnnotations can't easily express "at least one of these two".</summary>
    [EmailAddress, StringLength(150)]
    public string? Email { get; set; }

    [Phone]
    public string? PhoneNumber { get; set; }

    public int? StateId { get; set; }

    /// <summary>Optional NEET rank, so it can double as a counselling profile.</summary>
    public int? NeetRank { get; set; }

    /// <summary>Open, OBC, SC, ST, EWS, PwD.</summary>
    public string? Category { get; set; }
}