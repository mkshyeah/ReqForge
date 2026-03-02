using CommunityToolkit.Mvvm.ComponentModel;

namespace ReqForge.Models;

public partial class HeaderItem : ObservableObject
{
    [ObservableProperty]
    private string _key = string.Empty;
    
    [ObservableProperty]
    private string _value = string.Empty;
    
    // Конструктор без параметров (нужен для XAML и коллекций)
    public HeaderItem() { }
    
    // Конструктор для быстрого создания (удобно в коде)
    public HeaderItem(string key, string value)
    {
        Key = key;
        Value = value;
    }
}