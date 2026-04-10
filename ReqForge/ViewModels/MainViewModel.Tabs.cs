using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using ReqForge.Models;
using ReqForge.Models.DTOs;

namespace ReqForge.ViewModels;

public partial class MainViewModel
{
    private sealed class RequestTabDraft
    {
        public string Url { get; set; } = string.Empty;
        public string SelectedMethod { get; set; } = "GET";
        public string RequestBody { get; set; } = string.Empty;
        public string SelectedBodyType { get; set; } = "none";
        public string SelectedAuthType { get; set; } = "None";
        public string BearerToken { get; set; } = string.Empty;
        public string BasicAuthUsername { get; set; } = string.Empty;
        public string BasicAuthPassword { get; set; } = string.Empty;
        public string ApiKeyName { get; set; } = string.Empty;
        public string ApiKeyValue { get; set; } = string.Empty;
        public List<HeaderItemDto> Headers { get; set; } = new();
        public List<QueryParamDto> QueryParams { get; set; } = new();
        public List<QueryParamDto> FormData { get; set; } = new();
    }

    private void InitRequestTabs()
    {
        var first = new RequestTabItem { Title = "GET New Request" };
        RequestTabs = new ObservableCollection<RequestTabItem> { first };
        _tabDrafts[first.Id] = CaptureCurrentDraft();
        _lastSelectedTabId = first.Id;
        SelectedRequestTab = first;
    }

    [RelayCommand]
    private void NewRequestTab()
    {
        SaveCurrentTabDraft();

        var tab = new RequestTabItem { Title = "GET New Request" };
        _tabDrafts[tab.Id] = new RequestTabDraft();
        RequestTabs.Add(tab);
        SelectedRequestTab = tab;
    }

    [RelayCommand]
    private void CloseRequestTab(RequestTabItem? tab)
    {
        if (tab == null) return;

        if (RequestTabs.Count == 1)
        {
            _tabDrafts[tab.Id] = new RequestTabDraft();
            ApplyDraft(_tabDrafts[tab.Id]);
            tab.Title = "GET New Request";
            return;
        }

        var index = RequestTabs.IndexOf(tab);
        var nextIndex = Math.Max(0, index - 1);

        RequestTabs.Remove(tab);
        _tabDrafts.Remove(tab.Id);

        SelectedRequestTab = RequestTabs[nextIndex];
    }

    partial void OnSelectedRequestTabChanged(RequestTabItem? value)
    {
        if (value == null || _isApplyingTabState) return;

        SaveCurrentTabDraft();

        if (!_tabDrafts.TryGetValue(value.Id, out var draft))
        {
            draft = new RequestTabDraft();
            _tabDrafts[value.Id] = draft;
        }

        ApplyDraft(draft);
        _lastSelectedTabId = value.Id;
        UpdateSelectedTabTitle();
    }

    private void SaveCurrentTabDraft()
    {
        if (_isApplyingTabState) return;
        if (_lastSelectedTabId == null) return;
        _tabDrafts[_lastSelectedTabId.Value] = CaptureCurrentDraft();
    }

    private RequestTabDraft CaptureCurrentDraft()
    {
        return new RequestTabDraft
        {
            Url = Url,
            SelectedMethod = SelectedMethod,
            RequestBody = RequestBody,
            SelectedBodyType = SelectedBodyType,
            SelectedAuthType = SelectedAuthType,
            BearerToken = BearerToken,
            BasicAuthUsername = BasicAuthUsername,
            BasicAuthPassword = BasicAuthPassword,
            ApiKeyName = ApiKeyName,
            ApiKeyValue = ApiKeyValue,
            Headers = Headers
                .Where(h => !string.IsNullOrWhiteSpace(h.Key) || !string.IsNullOrWhiteSpace(h.Value))
                .Select(h => new HeaderItemDto { Key = h.Key, Value = h.Value })
                .ToList(),
            QueryParams = QueryParams
                .Where(q => !string.IsNullOrWhiteSpace(q.Key) || !string.IsNullOrWhiteSpace(q.Value))
                .Select(q => new QueryParamDto { Key = q.Key, Value = q.Value })
                .ToList(),
            FormData = FormDataItems
                .Where(q => !string.IsNullOrWhiteSpace(q.Key) || !string.IsNullOrWhiteSpace(q.Value))
                .Select(q => new QueryParamDto { Key = q.Key, Value = q.Value })
                .ToList()
        };
    }

    private void ApplyDraft(RequestTabDraft draft)
    {
        _isApplyingTabState = true;
        try
        {
            Url = draft.Url;
            SelectedMethod = draft.SelectedMethod;
            RequestBody = draft.RequestBody;
            SelectedBodyType = draft.SelectedBodyType;
            SelectedAuthType = draft.SelectedAuthType;
            BearerToken = draft.BearerToken;
            BasicAuthUsername = draft.BasicAuthUsername;
            BasicAuthPassword = draft.BasicAuthPassword;
            ApiKeyName = draft.ApiKeyName;
            ApiKeyValue = draft.ApiKeyValue;

            Headers.Clear();
            foreach (var h in draft.Headers)
                Headers.Add(new HeaderItem(h.Key, h.Value));
            if (Headers.Count == 0) Headers.Add(new HeaderItem("", ""));

            QueryParams.Clear();
            foreach (var q in draft.QueryParams)
                QueryParams.Add(new QueryParam(q.Key, q.Value));

            FormDataItems.Clear();
            foreach (var q in draft.FormData)
                FormDataItems.Add(new QueryParam(q.Key, q.Value));
        }
        finally
        {
            _isApplyingTabState = false;
        }
    }

    private void UpdateSelectedTabTitle()
    {
        if (SelectedRequestTab == null) return;

        var title = string.IsNullOrWhiteSpace(Url)
            ? "New Request"
            : TryGetUrlPathOrHost(Url);
        SelectedRequestTab.Title = $"{SelectedMethod} {title}";
    }

    private static string TryGetUrlPathOrHost(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return url.Length > 24 ? url[..24] + "..." : url;

        var path = uri.PathAndQuery.Trim('/');
        if (string.IsNullOrWhiteSpace(path))
            return uri.Host;

        return path.Length > 24 ? path[..24] + "..." : path;
    }
}
