using CommunityToolkit.Mvvm.ComponentModel;

namespace ReqForge.Models;

public partial class ResponseTest : ObservableObject
{
    [ObservableProperty] private string _testType = "StatusEquals";
    [ObservableProperty] private string _expectedValue = "200";
}