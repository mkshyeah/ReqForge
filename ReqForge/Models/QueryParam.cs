using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ReqForge.Models;

public partial class QueryParam : ObservableObject
{
    [ObservableProperty]
    private string _key = string.Empty;
    
    [ObservableProperty]
    private string _value = string.Empty;

    public QueryParam() { }

    public QueryParam(string key, string value)
    {
        Key = key;
        Value = value;
    }
}