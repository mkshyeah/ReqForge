using System.Windows;
using ReqForge.Models;
using ReqForge.Services;
using ReqForge.ViewModels;

namespace ReqForge;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    
    public MainWindow()
    {
        InitializeComponent();
        var service = new HttpClientService();
        var collectionStorage = new CollectionStorageService();
        var envStorage = new EnvironmentStorageService();
        var authService = new AuthService();
        // Устанавливаем DataContext. Теперь {Binding} знает, где искать свойства.

        DataContext = new MainViewModel(service, collectionStorage, envStorage,authService );
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