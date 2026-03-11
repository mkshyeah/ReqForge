using System.Collections.ObjectModel;
using System.Text;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaterialDesignThemes.Wpf;
using ReqForge.Models;
using ReqForge.Services;

namespace ReqForge.ViewModels
{
    // partial обязателен — генератор допишет вторую половину класса за тебя
    public partial class MainViewModel : ObservableObject
    {
        private readonly IHttpClientService _service;
        private readonly ICollectionStorageService _storage;
        private readonly IEnvironmentStorageService _envStorage;

        [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(SendCommand))]
        private string _url = string.Empty;

        [ObservableProperty] private string _selectedMethod = "GET";

        [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(SendCommand))]
        private bool _isLoading;

        [ObservableProperty] private string _requestBody = string.Empty;
        [ObservableProperty] private string _statusInfo = string.Empty;
        [ObservableProperty] private string _responseBody = string.Empty;
        [ObservableProperty] private string _responseHeadersText = string.Empty;
        [ObservableProperty] private RequestCollection? _selectedCollection;
        [ObservableProperty] private RequestEnvironment? _selectedEnvironment;
        [ObservableProperty] private ObservableCollection<RequestEnvironment> _environments = new();
        [ObservableProperty] private bool _isDarkTheme = false;

        public List<string> Methods { get; } = new() { "GET", "POST", "PUT", "PATCH", "DELETE" };
        public ObservableCollection<HeaderItem> Headers { get; } = new();

        [ObservableProperty] private ObservableCollection<RequestCollection> _collections = new();

        public MainViewModel(IHttpClientService service, ICollectionStorageService storage,
            IEnvironmentStorageService envStorage)
        {
            _service = service;
            _storage = storage;
            _envStorage = envStorage;

            // Загружаем коллекции при старте
            Collections = new ObservableCollection<RequestCollection>(_storage.LoadAll());

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
                var resolvedBody = ResolveVariables(RequestBody);

                var resolvedHeaders = new ObservableCollection<HeaderItem>(
                    Headers.Select(h => new HeaderItem(
                        ResolveVariables(h.Key),
                        ResolveVariables(h.Value)))
                );

                var result = await _service.SendAsync(SelectedMethod, resolvedUrl, resolvedHeaders, resolvedBody);

                StatusInfo = result.FullInfo;

                ResponseHeadersText = string.Join("\n",
                    result.ResponseHeaders.Select(h => $"{h.Key} : {h.Value}"));


                ResponseBody = TryFormatJson(result.Content);
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
        
    }
    
}