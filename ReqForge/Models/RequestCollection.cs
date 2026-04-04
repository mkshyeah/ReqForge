using System.Collections.ObjectModel;

namespace ReqForge.Models;

public class RequestCollection
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int UserId { get; set; }

    public ObservableCollection<SavedRequest> Requests { get; set; } = new();
}