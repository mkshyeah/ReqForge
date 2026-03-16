using System.Collections.ObjectModel;
using System.Text;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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

        public List<string> Methods { get; } = new() { "GET", "POST", "PUT", "PATCH", "DELETE" };
        public ObservableCollection<HeaderItem> Headers { get; } = new();
        
        [ObservableProperty] private ObservableCollection<RequestCollection> _collections = new();
        
        public MainViewModel(IHttpClientService service, ICollectionStorageService storage, IEnvironmentStorageService envStorage)
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
                var env = new RequestEnvironment{Name = dto.Name};
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
            
        }

        [RelayCommand]
        private void DeleteRequest(SavedRequest request)
        {
            if (request == null) return;

            // Ищем в какой коллекции лежит этот запрос
            var collection = Collections.FirstOrDefault(c => c.Requests.Contains(request));
            if (collection != null)
            {
                collection.Requests.Remove(request);
                _storage.SaveAll(Collections.ToList()); // Сохраняем изменения в файл
            }
            
            if (Headers.Count == 0)
                Headers.Add(new HeaderItem("", ""));
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
        
        [RelayCommand]
        private void CreateEnvironment()
        {
            var newEnv = new RequestEnvironment { Name = $"Env {Environments.Count + 1}" };
            newEnv.Variables.Add(new EnvironmentVariable("base_url", "https://api.example.com"));
            Environments.Add(newEnv);
            SaveEnvironments();
        }
        
        [RelayCommand]
        private void AddVariable()
        {
            SelectedEnvironment?.Variables.Add(new EnvironmentVariable("", ""));
            SaveEnvironments();
        }
        
        [RelayCommand]
        private void RemoveVariable(EnvironmentVariable variable)
        {
            if (variable != null)
            {
                SelectedEnvironment?.Variables.Remove(variable);
                SaveEnvironments();
            }
        }
        
        private void SaveEnvironments()
        {
            var dtos = Environments.Select(env => new RequestEnvironmentDto
            {
                Name = env.Name,
                Variables = env.Variables.Select(v => new EnvironmentVariableDto 
                { 
                    Key = v.Key, 
                    Value = v.Value 
                }).ToList()
            }).ToList();
    
            _envStorage.SaveAll(dtos);
        }
        
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
        
        private string ResolveVariables(string input)
        {
            // Если строка пустая или окружение не выбрано — возвращаем как есть
            if (string.IsNullOrEmpty(input) || SelectedEnvironment == null)
                return input;

            var result = input;

            foreach (var variable in SelectedEnvironment.Variables)
            {
                // Проверяем, что ключ переменной не пустой
                if (!string.IsNullOrWhiteSpace(variable.Key))
                {
                    // Ищем {{key}} и заменяем на value
                    // Используем конструкция $$$$"{{{{{variable.Key}}}}}" для экранирования фигурных скобок в строке
                    result = result.Replace($"{{{{{variable.Key}}}}}", variable.Value ?? string.Empty);
                }
            }

            return result;
        }
        
    }
    
}