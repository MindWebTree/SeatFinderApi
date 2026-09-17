using CounsellingApp.Domain.Enum;

namespace CounsellingApp.Domain.Entities;

public class College
{
    public int Id { get; set; }
    public Guid Guid { get; set; }
    public string Name { get; set; } = string.Empty;

    /// <summary>Govt, Private, or Deemed.</summary>
    public InstituteType InstituteType { get; set; }

    /// <summary>INT - relates to States, a regular dual-key table.</summary>
    public int StateId { get; set; }
    public bool IsActive { get; set; }
}
