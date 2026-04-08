using System.Windows;
using System.Windows.Media;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ReqForge.Data;
using ReqForge.Services;
using ReqForge.Services.Interfaces;
using ReqForge.ViewModels;
using MaterialDesignThemes.Wpf;

namespace ReqForge;

public partial class App : Application
{
    public IServiceProvider Services { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        Services = ConfigureServices();
        
        var db = Services.GetRequiredService<AppDbContext>();
        db.Database.EnsureCreated();
        
        ApplyCustomTheme();
        
        var mainWindow = Services.GetRequiredService<MainWindow>();
        MainWindow = mainWindow;
        mainWindow.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        if (Services is IDisposable disposableServices)
            disposableServices.Dispose();
        
        base.OnExit(e);
    }

    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();
        
        services.AddSingleton<AppDbContext>();
        services.AddSingleton<IHttpClientService, HttpClientService>();
        services.AddSingleton<ICollectionStorageService, CollectionStorageService>();
        services.AddSingleton<IEnvironmentStorageService, EnvironmentStorageService>();
        services.AddSingleton<IAuthService, AuthService>();
        services.AddSingleton<IRequestHistoryService, RequestHistoryService>();
        services.AddSingleton<MainViewModel>();
        services.AddSingleton<MainWindow>();
        
        return services.BuildServiceProvider();
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
