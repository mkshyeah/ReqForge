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
        var newColl = new RequestCollection { Name = $"Collection {Collections.Count + 1}" };
        _storage.AddCollection(newColl, CurrentUsername);

        Collections = new ObservableCollection<RequestCollection>(
            _storage.LoadAll(CurrentUsername));
        ApplyFilter();
    }

    [RelayCommand]
    private void SaveRequest()
    {
        if (string.IsNullOrWhiteSpace(Url)) return;
        if (SelectedCollection == null) return;

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

        Collections = new ObservableCollection<RequestCollection>(
            _storage.LoadAll(CurrentUsername));
        ApplyFilter();
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
