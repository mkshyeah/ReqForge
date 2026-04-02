namespace ReqForge.Models;

public class RequestHistoryItem
{
    public string Method { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public string ElapsedTime { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
}