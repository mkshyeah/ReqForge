namespace ReqForge.Models;

public class User
{
    public int Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Salt { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public List<RequestCollection> Collections { get; set; } = new();
    public List<RequestHistoryItem> History { get; set; } = new();
}