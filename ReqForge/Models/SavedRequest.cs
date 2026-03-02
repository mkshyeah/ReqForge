namespace ReqForge.Models;

public class SavedRequest
{
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;

    public List<HeaderItemDto> Headers { get; set; } = new();
    
    public string Body { get; set; } = string.Empty;

}