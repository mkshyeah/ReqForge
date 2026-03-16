using CommunityToolkit.Mvvm.Input;
using ReqForge.Models;

namespace ReqForge.ViewModels;

public partial class MainViewModel
{
    [RelayCommand]
    private void CreateCollection()
    {
        var newColl = new RequestCollection { Name = $"Collection {Collections.Count + 1}" };
        Collections.Add(newColl);
        _storage.SaveAll(Collections.ToList());
    }

    [RelayCommand]
    private void SaveRequest()
    {
        if(string.IsNullOrWhiteSpace(Url)) return;
        if(SelectedCollection == null) return;

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

        var requestDto = new SavedRequest
        {
            Name = requestName,
            Url = Url,
            Method = SelectedMethod,
            Body = RequestBody,
            Headers = Headers
                .Where(h => !string.IsNullOrWhiteSpace(h.Key))
                .Select(h => new HeaderItemDto { Key = h.Key, Value = h.Value })
                .ToList()
        };
        
        SelectedCollection.Requests.Add(requestDto);
        _storage.SaveAll(Collections.ToList());
    }

    [RelayCommand]
    private void LoadRequest(SavedRequest? request)
    {
        if (request == null) return;
        
        Url = request.Url;
        SelectedMethod = request.Method;
        RequestBody = request.Body;
        
        Headers.Clear();

        foreach (var h in request.Headers)
            Headers.Add(new HeaderItem(h.Key, h.Value));

        if (Headers.Count == 0)
            Headers.Add(new HeaderItem("", ""));
    }

    [RelayCommand]
    private void DeleteRequest(SavedRequest request)
    {
        if(request==null) return;

        var collection = Collections.FirstOrDefault(c => c.Requests.Contains(request));
        if (collection != null)
        {
            collection.Requests.Remove(request);
            _storage.SaveAll(Collections.ToList());
        }

        if (Headers.Count == 0)
        {
            Headers.Add(new HeaderItem("", ""));
        }
    }
}