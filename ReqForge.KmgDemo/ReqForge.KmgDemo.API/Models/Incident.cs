namespace ReqForge.KmgDemo.Models;

public class Incident
{
    public int Id { get; set; }
    public int FieldId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Severity { get; set; } = "low"; // low, medium, high
    public string Status { get; set; } = "open";  // open, in_progress, resolved
    public DateTime CreatedAt { get; set; }
}