namespace ReqForge.Models.DTOs;

public class EnvironmentVariableDto
{
    public int Id { get; set; }
    public int EnvironmentId { get; set; }

    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}