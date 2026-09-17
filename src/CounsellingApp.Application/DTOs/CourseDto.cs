namespace CounsellingApp.Application.DTOs;

public class CourseDto
{
    public int CourseId { get; set; }
    public Guid CourseGuid { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public string CourseType { get; set; } = string.Empty;
    public string? ClinicalType { get; set; }
}
