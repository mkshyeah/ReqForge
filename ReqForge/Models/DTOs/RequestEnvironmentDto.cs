namespace ReqForge.Models.DTOs;

public class RequestEnvironmentDto
{
    public int Id { get; set; }
    public int UserId { get; set; }

    public string Name { get; set; } = string.Empty;
    public List<EnvironmentVariableDto> Variables { get; set; } = new();
}