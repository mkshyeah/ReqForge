using CommunityToolkit.Mvvm.ComponentModel;

namespace ReqForge.ViewModels;

public partial class MainViewModel
{
    [ObservableProperty]private bool _isLoggedIn;
    [ObservableProperty]private string _currentUsername = string.Empty;
    [ObservableProperty]private string _loginUserName =  string.Empty;
    [ObservableProperty]private string _loginPassword  = string.Empty;
    [ObservableProperty]private string _authErrorMessage = string.Empty;
    
    []
}