using CommunityToolkit.Mvvm.Input;

namespace ReqForge.ViewModels;

public partial class MainViewModel
{
    [RelayCommand]
    private void ToggleTheme()
    {
        IsDarkTheme = !IsDarkTheme;
        App.ApplyCustomTheme(IsDarkTheme);
    }
}
