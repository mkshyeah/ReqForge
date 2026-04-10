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

        var resolvedUrl = ResolveVariables(Url);
        var resolvedHeaders = Headers
            .Where(h => !string.IsNullOrWhiteSpace(h.Key))
            .Select(h => new Models.HeaderItem(ResolveVariables(h.Key), ResolveVariables(h.Value)))
            .ToList();

        string? body = null;
        switch (SelectedBodyType)
        {
            case "json":
            case "raw":
                body = ResolveVariables(RequestBody);
                break;
            case "form-data":
                body = string.Join("&", FormDataItems
                    .Where(f => !string.IsNullOrWhiteSpace(f.Key))
                    .Select(f =>
                        $"{Uri.EscapeDataString(ResolveVariables(f.Key))}={Uri.EscapeDataString(ResolveVariables(f.Value))}"));
                break;
        }

        GeneratedCode = SelectedCodeFormat switch
        {
            "cURL (bash)" => CodeGeneratorService.ToCurlBash(SelectedMethod, resolvedUrl, resolvedHeaders, body),
            "cURL (Windows)" => CodeGeneratorService.ToCurlWindows(SelectedMethod, resolvedUrl, resolvedHeaders, body),
            "PowerShell" => CodeGeneratorService.ToPowerShell(SelectedMethod, resolvedUrl, resolvedHeaders, body),
            "C# HttpClient" => CodeGeneratorService.ToCSharpHttpClient(SelectedMethod, resolvedUrl, resolvedHeaders, body),
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
