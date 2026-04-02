using ReqForge.Models.DTOs;

namespace ReqForge.Models;

public class SavedRequest
{
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;

    public List<HeaderItemDto> Headers { get; set; } = new();
    
    public string Body { get; set; } = string.Empty;
    
    
    // Авторизация
    public string AuthType { get; set; } = "None";       // None, BearerToken, BasicAuth, ApiKey
    public string BearerToken { get; set; } = string.Empty;
    public string BasicAuthUsername { get; set; } = string.Empty;
    public string BasicAuthPassword { get; set; } = string.Empty;
    public string ApiKeyName { get; set; } = string.Empty;
    public string ApiKeyValue { get; set; } = string.Empty;
    
}