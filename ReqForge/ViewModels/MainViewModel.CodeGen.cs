using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReqForge.Services;

namespace ReqForge.ViewModels;

public partial class MainViewModel
{
    [ObservableProperty] private string _selectedCodeFormat = "cURL";
    [ObservableProperty] private string _generatedCode = string.Empty;
    
    public List<string> CodeFormats { get; } = new() { "cURL", "C# HttpClient" };

    [RelayCommand]
    private void GenerateCode()
    {
        if (string.IsNullOrWhiteSpace(Url))
        {
            GeneratedCode = "Enter a URL first";
            return;
        }

        GeneratedCode = SelectedCodeFormat switch
        {
            "cURL" => CodeGeneratorService.ToCurl(SelectedMethod, Url, Headers, 
                SelectedBodyType != "none" ? RequestBody : null),
            "C# HttpClient" => CodeGeneratorService.ToCSharpHttpClient(SelectedMethod, Url, Headers,
                SelectedBodyType != "none" ? RequestBody : null),
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