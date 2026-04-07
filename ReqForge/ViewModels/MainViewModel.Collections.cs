using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using ReqForge.Models;
using ReqForge.Models.DTOs;

namespace ReqForge.ViewModels;

public partial class MainViewModel
{
    [RelayCommand]
    private void CreateCollection()
    {
        if (!IsLoggedIn)
        {
            StatusInfo = "Please login or register first";
            return;
        }

        var dialog = new Views.InputDialog("New Collection", "Enter collection name:");
        if (dialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(dialog.ResponseText))
        {
            var name = dialog.ResponseText.Trim();
            var newColl = new RequestCollection { Name = name };
            _storage.AddCollection(newColl, CurrentUsername);

            Collections = new ObservableCollection<RequestCollection>(
                _storage.LoadAll(CurrentUsername));
            SelectedCollection = Collections.FirstOrDefault(c => c.Name == name);
            ApplyFilter();
        }
    }

    [RelayCommand]
    private void DeleteCollection(RequestCollection? collection)
    {
        if (collection == null) return;

        var result = System.Windows.MessageBox.Show(
            $"Delete collection \"{collection.Name}\" and all its requests?",
            "Confirm Delete",
            System.Windows.MessageBoxButton.YesNo,
            System.Windows.MessageBoxImage.Warning);

        if (result == System.Windows.MessageBoxResult.Yes)
        {
            if (SelectedCollection?.Id == collection.Id)
                SelectedCollection = null;

            _storage.DeleteCollection(collection.Id);
            Collections = new ObservableCollection<RequestCollection>(
                _storage.LoadAll(CurrentUsername));
            ApplyFilter();
        }
    }

    [RelayCommand]
    private void RenameCollection(RequestCollection? collection)
    {
        if (collection == null) return;

        var dialog = new Views.InputDialog("Rename Collection", "Enter new name:", collection.Name);
        if (dialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(dialog.ResponseText))
        {
            collection.Name = dialog.ResponseText.Trim();
            _storage.UpdateCollection(collection);

            Collections = new ObservableCollection<RequestCollection>(
                _storage.LoadAll(CurrentUsername));
            ApplyFilter();
        }
    }

    [RelayCommand]
    private void SaveRequest()
    {
        if (!IsLoggedIn)
        {
            StatusInfo = "Please login or register first";
            return;
        }

        if (string.IsNullOrWhiteSpace(Url))
        {
            StatusInfo = "Enter a URL before saving";
            return;
        }

        if (SelectedCollection == null || SelectedCollection.Id <= 0)
        {
            StatusInfo = "Select a collection first (click on it in the sidebar)";
            return;
        }

        // Verify collection still exists in DB
        var freshCollections = _storage.LoadAll(CurrentUsername);
        if (!freshCollections.Any(c => c.Id == SelectedCollection.Id))
        {
            StatusInfo = "Collection not found. Please select another one.";
            SelectedCollection = null;
            return;
        }

        string requestName;
        try
        {
            var uri = new Uri(Url);
            requestName = $"{SelectedMethod} {uri.AbsolutePath}";
        }
        catch
        {
            requestName = $"{SelectedMethod} (New Request)";
        }

        try
        {
            var request = new SavedRequest
            {
                Name = requestName,
                Url = Url,
                Method = SelectedMethod,
                Body = RequestBody,
                BodyType = SelectedBodyType,
                Headers = Headers
                    .Where(h => !string.IsNullOrWhiteSpace(h.Key))
                    .Select(h => new HeaderItemDto { Key = h.Key, Value = h.Value })
                    .ToList(),
                QueryParams = QueryParams
                    .Where(q => !string.IsNullOrWhiteSpace(q.Key))
                    .Select(q => new QueryParamDto { Key = q.Key, Value = q.Value })
                    .ToList(),
                AuthType = SelectedAuthType,
                BearerToken = BearerToken,
                BasicAuthUsername = BasicAuthUsername,
                BasicAuthPassword = BasicAuthPassword,
                ApiKeyName = ApiKeyName,
                ApiKeyValue = ApiKeyValue
            };

            _storage.AddRequest(request, SelectedCollection.Id);

            var savedCollId = SelectedCollection.Id;
            Collections = new ObservableCollection<RequestCollection>(
                _storage.LoadAll(CurrentUsername));
            SelectedCollection = Collections.FirstOrDefault(c => c.Id == savedCollId);
            ApplyFilter();
            StatusInfo = $"Saved to \"{SelectedCollection?.Name}\"";
        }
        catch (Exception ex)
        {
            StatusInfo = $"Save failed: {ex.InnerException?.Message ?? ex.Message}";
        }
    }

    [RelayCommand]
    private void LoadRequest(SavedRequest? request)
    {
        if (request == null) return;

        Url = request.Url;
        SelectedMethod = request.Method;
        RequestBody = request.Body;
        SelectedBodyType = request.BodyType;

        Headers.Clear();
        foreach (var h in request.Headers)
            Headers.Add(new HeaderItem(h.Key, h.Value));
        if (Headers.Count == 0)
            Headers.Add(new HeaderItem("", ""));

        QueryParams.Clear();
        foreach (var q in request.QueryParams)
            QueryParams.Add(new QueryParam(q.Key, q.Value));

        SelectedAuthType = request.AuthType;
        BearerToken = request.BearerToken;
        BasicAuthUsername = request.BasicAuthUsername;
        BasicAuthPassword = request.BasicAuthPassword;
        ApiKeyName = request.ApiKeyName;
        ApiKeyValue = request.ApiKeyValue;
    }

    [RelayCommand]
    private void DeleteRequest(SavedRequest request)
    {
        if (request == null) return;

        _storage.DeleteRequest(request.Id);

        Collections = new ObservableCollection<RequestCollection>(
            _storage.LoadAll(CurrentUsername));

        if (Headers.Count == 0)
            Headers.Add(new HeaderItem("", ""));

        ApplyFilter();
    }
}
