namespace ReqForge.Models;

public class RequestEnvironmentDto
{
    public string Name { get; set; } = string.Empty;
    public List<EnvironmentVariableDto> Variables { get; set; } = new();
}