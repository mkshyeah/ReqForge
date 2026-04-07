using System.Windows;
using System.Windows.Media;
using MaterialDesignThemes.Wpf;

namespace ReqForge;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        ApplyCustomTheme();
    }

    public static void ApplyCustomTheme(bool isDark = false)
    {
        var helper = new PaletteHelper();
        var theme = helper.GetTheme();
        theme.SetBaseTheme(isDark ? BaseTheme.Dark : BaseTheme.Light);
        theme.SetPrimaryColor(Color.FromRgb(66, 13, 9));
        theme.SetSecondaryColor(Color.FromRgb(201, 149, 107));
        helper.SetTheme(theme);
    }
}
