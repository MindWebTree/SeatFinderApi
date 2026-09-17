namespace CounsellingApp.Application.DTOs;

public class StateDto
{
    public int StateId { get; set; }
    public Guid StateGuid { get; set; }
    public string StateName { get; set; } = string.Empty;
}
