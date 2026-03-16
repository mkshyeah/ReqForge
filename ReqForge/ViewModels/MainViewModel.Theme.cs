using CommunityToolkit.Mvvm.Input;
using MaterialDesignThemes.Wpf;

namespace ReqForge.ViewModels;

public partial class MainViewModel
{
    [RelayCommand]
    private void ToggleTheme()
    {
        IsDarkTheme = !IsDarkTheme;
        var helper = new PaletteHelper();
        var theme = helper.GetTheme();
        theme.SetBaseTheme(IsDarkTheme ? BaseTheme.Dark : BaseTheme.Light);
        helper.SetTheme(theme);
    }
}