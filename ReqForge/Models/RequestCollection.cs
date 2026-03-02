using System.Collections.ObjectModel;

namespace ReqForge.Models;

public class RequestCollection
{
    public string Name { get; set; } = string.Empty;
    public ObservableCollection<SavedRequest> Requests { get; set; } = new();
}