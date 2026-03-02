using System.Net;

namespace ReqForge.Models;

public class HttpResponseResult
{
    public HttpStatusCode StatusCode { get; set; }
    
    public string Content { get; set; } = string.Empty;
    
    // Время, затраченное на запрос (полезно для отладки API)
    public TimeSpan ElapsedTime { get; set; }
    
    public bool IsSuccess => (int)StatusCode >= 200 && (int)StatusCode <= 299;
    
    public Dictionary<string, string> ResponseHeaders { get; set; } = new();

    public long ContentLength { get; set; }

    public string FullInfo => $"Status: {(int)StatusCode} ({StatusCode}) | " +
                              $"Time: {ElapsedTime.TotalMilliseconds:F0}ms | " +
                              $"Size: {FormatSize(ContentLength)}";

    private string FormatSize(long bytes) => bytes switch
    {
        < 1024 => $"{bytes} B",
        < 1024 * 1024 => $"{bytes / 1024.0:F1} KB",
        _ => $"{bytes / (1024.0 * 1024):F1} MB"
    };
}