using System.Diagnostics;
using System.Net;
using System.Net.Http;
using ReqForge.Models;
using ReqForge.Services.Interfaces;

namespace ReqForge.Services;

public class HttpClientService : IHttpClientService
{
    // Используем статический экземпляр, чтобы избежать исчерпания сокетов
    private static readonly HttpClient _httpClient = new();
    
    public async Task<HttpResponseResult> SendAsync(string method, string url, IEnumerable<HeaderItem> headers, string? body)
    {
        var result = new HttpResponseResult();
        var stopwatch = new Stopwatch();

        try
        {
            //Подготовка запроса 
            using var request = new HttpRequestMessage(new HttpMethod(method), url);
            
            // 1. Сначала обрабатываем тело запроса (Body)
            // Тело обычно не шлют в GET и HEAD
            if (!string.IsNullOrEmpty(body) && method != "GET" && method != "HEAD")
            {
                request.Content = new StringContent(body, System.Text.Encoding.UTF8);
            }
            
            foreach (var header in headers)
            {
                if (string.IsNullOrWhiteSpace(header.Key)) continue;
                
                bool added = request.Headers.TryAddWithoutValidation(header.Key, header.Value);

                if (!added && request.Content != null)
                {
                    if (header.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase))
                    {
                        if (System.Net.Http.Headers.MediaTypeHeaderValue.TryParse(header.Value, out var mediaType))
                        {
                            request.Content.Headers.ContentType = mediaType;
                        }
                    }
                    else
                    {
                        request.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
                    }
                }
            }

            stopwatch.Start();

            // Выполнение запроса
            using (var response = await _httpClient.SendAsync(request))
            {
                stopwatch.Stop();
                
                foreach (var header in response.Headers)
                    result.ResponseHeaders[header.Key] = string.Join("; ", header.Value);
                foreach (var header in response.Content.Headers)
                    result.ResponseHeaders[header.Key] = string.Join("; ", header.Value);
                
                result.StatusCode = response.StatusCode;
                result.ElapsedTime = stopwatch.Elapsed;
                var bytes = await response.Content.ReadAsByteArrayAsync();
                result.Content = System.Text.Encoding.UTF8.GetString(bytes);
                result.ContentLength = bytes.Length;
            }
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            result.StatusCode = HttpStatusCode.BadRequest;
            result.Content = $"Error: {ex.Message}";
            result.ElapsedTime = stopwatch.Elapsed;
        }
        
        return result;
    }
}