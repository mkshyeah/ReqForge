using System.Windows;
using ReqForge.Data;
using ReqForge.Models;
using ReqForge.Services;
using ReqForge.ViewModels;

namespace ReqForge;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        var db = new AppDbContext();
        db.Database.EnsureCreated();

        var httpService = new HttpClientService();
        var collectionStorage = new CollectionStorageService(db);
        var envStorage = new EnvironmentStorageService(db);
        var authService = new AuthService(db);
        var historyService = new RequestHistoryService(db);

        DataContext = new MainViewModel(httpService, collectionStorage, envStorage, authService, historyService);
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
