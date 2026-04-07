using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReqForge.Services;

namespace ReqForge.ViewModels;

public partial class MainViewModel
{
    [ObservableProperty] private string _selectedCodeFormat = "cURL (bash)";
    [ObservableProperty] private string _generatedCode = string.Empty;

    public List<string> CodeFormats { get; } = new()
    {
        "cURL (bash)",
        "cURL (Windows)",
        "PowerShell",
        "C# HttpClient"
    };

    [RelayCommand]
    private void GenerateCode()
    {
        if (string.IsNullOrWhiteSpace(Url))
        {
            GeneratedCode = "Enter a URL first";
            return;
        }

        var body = SelectedBodyType != "none" ? RequestBody : null;

        GeneratedCode = SelectedCodeFormat switch
        {
            "cURL (bash)" => CodeGeneratorService.ToCurlBash(SelectedMethod, Url, Headers, body),
            "cURL (Windows)" => CodeGeneratorService.ToCurlWindows(SelectedMethod, Url, Headers, body),
            "PowerShell" => CodeGeneratorService.ToPowerShell(SelectedMethod, Url, Headers, body),
            "C# HttpClient" => CodeGeneratorService.ToCSharpHttpClient(SelectedMethod, Url, Headers, body),
            _ => string.Empty
        };
    }

    [RelayCommand]
    private void CopyGeneratedCode()
    {
        if (!string.IsNullOrEmpty(GeneratedCode))
            System.Windows.Clipboard.SetText(GeneratedCode);
    }
}
