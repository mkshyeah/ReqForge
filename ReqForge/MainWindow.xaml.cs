using System.ComponentModel;
using System.Windows;
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

    private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        var vm = (MainViewModel)DataContext;
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
