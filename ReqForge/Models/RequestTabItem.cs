using CommunityToolkit.Mvvm.ComponentModel;

namespace ReqForge.Models;

public partial class RequestTabItem : ObservableObject
{
    public Guid Id { get; init; } = Guid.NewGuid();
    [ObservableProperty] private string _title = "New Request";
}
