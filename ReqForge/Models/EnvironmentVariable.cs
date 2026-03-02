using CommunityToolkit.Mvvm.ComponentModel;

namespace ReqForge.Models;

public partial class EnvironmentVariable : ObservableObject
{
    [ObservableProperty]
    private string _key = string.Empty;

    [ObservableProperty]
    private string _value = string.Empty;
    
    // Конструктор для удобства
    public EnvironmentVariable(string key = "", string value = "")
    {
        Key = key;
        Value = value;
    }
}
