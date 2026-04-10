using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;
using Microsoft.Extensions.DependencyInjection;
using ReqForge.Models;
using ReqForge.ViewModels;

namespace ReqForge;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        if (!DesignerProperties.GetIsInDesignMode(this) && Application.Current is App app)
            DataContext = app.Services.GetRequiredService<MainViewModel>();
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        if (StartupOverlay == null || StartupTitle == null)
            return;

        try
        {
            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(550))
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            StartupTitle.BeginAnimation(OpacityProperty, fadeIn);

            await Task.Delay(1300);

            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(650))
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
            };
            fadeOut.Completed += (_, _) =>
            {
                StartupOverlay.Visibility = Visibility.Collapsed;
                StartupOverlay.IsHitTestVisible = false;
            };
            StartupOverlay.BeginAnimation(OpacityProperty, fadeOut);
        }
        catch
        {
            StartupOverlay.Visibility = Visibility.Collapsed;
            StartupOverlay.IsHitTestVisible = false;
        }
    }

    private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (DataContext is not MainViewModel vm)
            return;

        if (e.NewValue is SavedRequest req)
        {
            vm.LoadRequestCommand.Execute(req);
        }
        else if (e.NewValue is RequestCollection coll)
        {
            vm.SelectedCollection = coll;
        }
    }
}
