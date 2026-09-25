using CounsellingApp.Domain.Enum;


namespace CounsellingApp.Application.DTOs.CollegeFinder;

public class CfCourseDto
{
    public Guid CourseGuid { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public string? ShortName { get; set; }
    public string CourseType { get; set; } = string.Empty;
    public int CollegeCount { get; set; }
    public int TotalSeats { get; set; }
}

public class CfStateDto
{
    public Guid StateGuid { get; set; }
    public string StateName { get; set; } = string.Empty;
    public int CollegeCount { get; set; }
    public int TotalSeats { get; set; }
}

public class CfCollegeOptionDto
{
    public Guid CollegeGuid { get; set; }
    public string CollegeName { get; set; } = string.Empty;
    public string? City { get; set; }
    public string StateName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;   
    public int Seats { get; set; }                          
    public int AiqSeats { get; set; }
    public int StateSeats { get; set; }
    public int CourseCount { get; set; }
}

public class CfCollegeResultDto
{
    public Guid CollegeGuid { get; set; }
    public string CollegeName { get; set; } = string.Empty;
    public string? City { get; set; }
    public string StateName { get; set; } = string.Empty;
    public Guid CourseGuid { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public string? CourseShortName { get; set; }
    public int Seats { get; set; }
    public int AiqSeats { get; set; }
    public int StateSeats { get; set; }
    public int InstituteSeats { get; set; }
    public string CounsellingBody { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? NmcStatus { get; set; }
    public string? University { get; set; }

    [System.Text.Json.Serialization.JsonIgnore] public int TotalCount { get; set; }
    [System.Text.Json.Serialization.JsonIgnore] public int TotalSeats { get; set; }
    [System.Text.Json.Serialization.JsonIgnore] public int TotalColleges { get; set; }
}

public class CfSearchRequestDto
{
    public List<Guid>? CourseGuids { get; set; }
    public Guid? StateGuid { get; set; }
    public CollegeCategory? Category { get; set; }        
    public Guid? CollegeGuid { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class CfSearchResponseDto
{
    public int TotalCount { get; set; }
    public int TotalColleges { get; set; }
    public int TotalSeats { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public bool HasMore { get; set; }
    public IEnumerable<CfCollegeResultDto> Items { get; set; } = [];
}

public class CfCollegeInfoDto
{
    public Guid CollegeGuid { get; set; }
    public string NmcCode { get; set; } = string.Empty;
    public string CollegeName { get; set; } = string.Empty;
    public string? City { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string StateName { get; set; } = string.Empty;
    public string Ownership { get; set; } = string.Empty;
    public int? YearOfInception { get; set; }
}

public class CfCollegeCourseDto
{
    public Guid CourseGuid { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public string? CourseShortName { get; set; }
    public string CourseType { get; set; } = string.Empty;
    public int Seats { get; set; }
    public int AiqSeats { get; set; }
    public int StateSeats { get; set; }
    public int InstituteSeats { get; set; }
    public string CounsellingBody { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? University { get; set; }
    public string? NmcStatus { get; set; }
    public DateTime? LastLopDate { get; set; }
}

public class CfCollegeDetailDto
{
    public CfCollegeInfoDto College { get; set; } = new();
    public IEnumerable<CfCollegeCourseDto> Courses { get; set; } = [];
    public int TotalSeats { get; set; }
}