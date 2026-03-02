using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ReqForge.Models;

public partial class RequestEnvironment : ObservableObject
{
    [ObservableProperty]
    private string _name = string.Empty;

    public ObservableCollection<EnvironmentVariable> Variables { get; set; } = new();
}