namespace CounsellingApp.Domain.Entities;

public class Course
{
    public int Id { get; set; }
    public Guid Guid { get; set; }
    public string Name { get; set; } = string.Empty;

    /// <summary>MD, MS, or Diploma.</summary>
    public string Type { get; set; } = string.Empty;
}
