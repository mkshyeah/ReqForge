namespace ReqForge.Models.DTOs;

public class QueryParamDto
{
    public int Id { get; set; }
    public int SavedRequestId { get; set; }

    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}
