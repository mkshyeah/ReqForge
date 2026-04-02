using System.Collections.ObjectModel;
using System.Text;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaterialDesignThemes.Wpf;
using ReqForge.Models;
using ReqForge.Models.DTOs;
using ReqForge.Services;
using ReqForge.Services.Interfaces;

namespace ReqForge.ViewModels
{
    // partial обязателен — генератор допишет вторую половину класса за тебя
    public partial class MainViewModel : ObservableObject
    {
        private readonly IHttpClientService _service;
        private readonly ICollectionStorageService _storage;
        private readonly IEnvironmentStorageService _envStorage;
        private readonly IAuthService _authService;

        [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(SendCommand))]
        private string _url = string.Empty;

        [ObservableProperty] private string _selectedMethod = "GET";

        [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(SendCommand))]
        private bool _isLoading;

        [ObservableProperty] private string _requestBody = string.Empty;
        [ObservableProperty] private string _selectedBodyType = "none";
        [ObservableProperty] private string _statusInfo = string.Empty;
        [ObservableProperty] private string _responseBody = string.Empty;
        [ObservableProperty] private string _responseHeadersText = string.Empty;
        [ObservableProperty] private RequestCollection? _selectedCollection;
        [ObservableProperty] private RequestEnvironment? _selectedEnvironment;
        [ObservableProperty] private ObservableCollection<RequestEnvironment> _environments = new();
        [ObservableProperty] private bool _isDarkTheme = false;

        public List<string> Methods { get; } = new() { "GET", "POST", "PUT", "PATCH", "DELETE" };
        public List<string> BodyTypes { get; } = new() { "none", "json", "form-data", "raw" };
        public ObservableCollection<HeaderItem> Headers { get; } = new();
        public ObservableCollection<QueryParam> QueryParams { get; } = new();
        public ObservableCollection<QueryParam> FormDataItems { get; } = new();
        public ObservableCollection<RequestHistoryItem> RequestHistory { get; } = new();

        [ObservableProperty] private ObservableCollection<RequestCollection> _collections = new();

        public MainViewModel(IHttpClientService service, ICollectionStorageService storage,
            IEnvironmentStorageService envStorage, IAuthService authService)
        {
            _service = service;
            _storage = storage;
            _envStorage = envStorage;
            _authService = authService;

            IsLoggedIn = _authService.IsLoggedIn;
            CurrentUsername = _authService.CurrentUsername ?? string.Empty;
            AuthErrorMessage = string.Empty;
            
            

            // Загружаем коллекции при старте
            Collections = new ObservableCollection<RequestCollection>();

            // Загружаем окружение
            var envDtos = _envStorage.LoadAll();
            foreach (var dto in envDtos)
            {
                var env = new RequestEnvironment { Name = dto.Name };
                foreach (var vDto in dto.Variables)
                {
                    env.Variables.Add(new EnvironmentVariable(vDto.Key, vDto.Value));
                }

                Environments.Add(env);
            }

            if (Headers.Count == 0) Headers.Add(new HeaderItem("", ""));
        }


        // Генератор создаст свойство SendCommand
        [RelayCommand(CanExecute = nameof(CanSend))]
        private async Task Send()
        {
            try
            {
                IsLoading = true;
                StatusInfo = "Отправка запроса...";
                ResponseBody = string.Empty;
                ResponseHeadersText = string.Empty;

                var resolvedUrl = ResolveVariables(Url);
                
                var activeParams = QueryParams
                    .Where(p => !string.IsNullOrWhiteSpace(p.Key))
                    .Select(p => $"{Uri.EscapeDataString(ResolveVariables(p.Key))}={Uri.EscapeDataString((ResolveVariables(p.Value)))}")
                    .ToList();

                if (activeParams.Count > 0)
                {
                    var separator = resolvedUrl.Contains('?') ? "&" : "?";
                    resolvedUrl += separator + string.Join("&", activeParams);
                }
                
                string? resolvedBody = null;
                string? contentType = null;

                switch (SelectedBodyType)
                {
                    case "json":
                        resolvedBody = ResolveVariables(RequestBody);
                        contentType = "application/json";
                        break;
                    case "raw":
                        resolvedBody = ResolveVariables(RequestBody);
                        contentType = "text/plain";
                        break;
                    case "form-data":
                        resolvedBody = string.Join("&", FormDataItems
                            .Where(f => !string.IsNullOrWhiteSpace(f.Key))
                            .Select(f => $"{Uri.EscapeDataString(ResolveVariables(f.Key))}={Uri.EscapeDataString(ResolveVariables(f.Value))}"));
                        contentType = "application/x-www-form-urlencoded";
                        break;
                }

                var resolvedHeaders = new ObservableCollection<HeaderItem>(
                    Headers.Select(h => new HeaderItem(
                        ResolveVariables(h.Key),
                        ResolveVariables(h.Value)))
                );

                if (contentType != null && !resolvedHeaders.Any(h =>
                        h.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase)))
                {
                    resolvedHeaders.Add(new HeaderItem("Content-Type", contentType));
                }

                var result = await _service.SendAsync(SelectedMethod, resolvedUrl, resolvedHeaders, resolvedBody);

                StatusInfo = result.FullInfo;

                ResponseHeadersText = string.Join("\n",
                    result.ResponseHeaders.Select(h => $"{h.Key} : {h.Value}"));


                ResponseBody = TryFormatJson(result.Content);
                
                RequestHistory.Insert(0, new RequestHistoryItem
                {
                    Method = SelectedMethod,
                    Url = resolvedUrl,
                    StatusCode = (int)result.StatusCode,
                    ElapsedTime = result.ElapsedTime.TotalMilliseconds.ToString("F0") + " ms",
                    SentAt = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                StatusInfo = "Ошибка";
                ResponseBody = ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private void AddHeader()
        {
            Headers.Add(new HeaderItem("", ""));
        }

        [RelayCommand]
        private void AddQueryParam()
        {
            QueryParams.Add(new QueryParam("",""));
        }

        [RelayCommand]
        private void RemoveQueryParam(QueryParam param)
        {
            if (param != null)
            {
                QueryParams.Remove(param);
            }
        }

        [RelayCommand]
        private void AddFormDataItem()
        {
            FormDataItems.Add(new QueryParam("", ""));
        }

        [RelayCommand]
        private void RemoveFormDataItem(QueryParam item)
        {
            if (item != null)
                FormDataItems.Remove(item);
        }

        [RelayCommand]
        private void RemoveHeader(HeaderItem header)
        {
            if (header != null)
            {
                Headers.Remove(header);
            }
        }

        // Help-classes

        private bool CanSend() => !IsLoading && !string.IsNullOrWhiteSpace(Url);

        private string TryFormatJson(string content)
        {
            var trimmed = content.Trim();
            if (!trimmed.StartsWith("{") && !trimmed.StartsWith("["))
                return content;

            try
            {
                using var doc = JsonDocument.Parse(content);
                return JsonSerializer.Serialize(doc, _jsonOptions);

            }
            catch
            {
                return content;
            }
        }

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        [RelayCommand]
        private void DeleteEnvironment(RequestEnvironment env)
        {
            if (env == null) return;
            Environments.Remove(env);
            if (SelectedEnvironment == env) SelectedEnvironment = null;
            SaveEnvironments();
        }
        
        [RelayCommand]
        private void CopyResponse()
        {
            if (!string.IsNullOrEmpty(ResponseBody))
                System.Windows.Clipboard.SetText(ResponseBody);
        }
    }
    
}