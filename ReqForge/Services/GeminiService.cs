using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace ReqForge.Services;

public enum AiProvider
{
    Groq,
    Gemini
}

public record AiModelInfo(string Id, string DisplayName, AiProvider Provider);

public class AiService
{
    private static readonly HttpClient _http = new() { Timeout = TimeSpan.FromSeconds(60) };

    public string ApiKey { get; set; } = string.Empty;
    public AiProvider Provider { get; set; } = AiProvider.Groq;
    public string Model { get; set; } = "llama-3.3-70b-versatile";

    public static readonly List<AiModelInfo> AvailableModels = new()
    {
        new("llama-3.3-70b-versatile", "Llama 3.3 70B (free, smart)",    AiProvider.Groq),
        new("llama-3.1-8b-instant",    "Llama 3.1 8B (free, fast)",      AiProvider.Groq),
        new("gemini-2.0-flash",        "Gemini 2.0 Flash",               AiProvider.Gemini),
    };

    public async Task<string> AskAsync(string prompt)
    {
        if (string.IsNullOrWhiteSpace(ApiKey))
            return "API key is not set. Enter your API key above.";

        return Provider switch
        {
            AiProvider.Groq => await CallOpenAiCompatibleAsync(prompt, "https://api.groq.com/openai/v1/chat/completions"),
            AiProvider.Gemini => await CallGeminiAsync(prompt),
            _ => "Unknown AI provider"
        };
    }

    private async Task<string> CallOpenAiCompatibleAsync(string prompt, string endpoint)
    {
        var requestBody = new
        {
            model = Model,
            messages = new object[]
            {
                new { role = "system", content = "You are a helpful API debugging assistant. Respond concisely." },
                new { role = "user", content = prompt }
            },
            temperature = 0.3
        };

        var json = JsonSerializer.Serialize(requestBody);
        var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
        request.Headers.Add("Authorization", $"Bearer {ApiKey}");

        HttpResponseMessage response;
        try
        {
            response = await _http.SendAsync(request);
        }
        catch (TaskCanceledException)
        {
            return "Request timed out. Check your internet connection.";
        }
        catch (HttpRequestException ex)
        {
            return $"Network error: {ex.Message}";
        }

        var responseJson = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            return ParseOpenAiError(response.StatusCode, responseJson);

        try
        {
            using var doc = JsonDocument.Parse(responseJson);
            return doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? "Empty response";
        }
        catch (Exception ex)
        {
            return $"Failed to parse response: {ex.Message}";
        }
    }

    private async Task<string> CallGeminiAsync(string prompt)
    {
        var url = $"https://generativelanguage.googleapis.com/v1beta/models/{Model}:generateContent?key={ApiKey}";

        var requestBody = new
        {
            contents = new[]
            {
                new { parts = new[] { new { text = prompt } } }
            }
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        HttpResponseMessage response;
        try
        {
            response = await _http.PostAsync(url, content);
        }
        catch (TaskCanceledException)
        {
            return "Request timed out. Check your internet connection.";
        }
        catch (HttpRequestException ex)
        {
            return $"Network error: {ex.Message}";
        }

        var responseJson = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            return ParseGeminiError(response.StatusCode, responseJson);

        try
        {
            using var doc = JsonDocument.Parse(responseJson);
            return doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString() ?? "Empty response";
        }
        catch (Exception ex)
        {
            return $"Failed to parse response: {ex.Message}";
        }
    }

    private static string ParseOpenAiError(HttpStatusCode statusCode, string responseJson)
    {
        try
        {
            using var doc = JsonDocument.Parse(responseJson);
            var message = doc.RootElement.TryGetProperty("error", out var err)
                ? err.TryGetProperty("message", out var msg) ? msg.GetString() ?? "" : responseJson
                : responseJson;

            return statusCode switch
            {
                HttpStatusCode.TooManyRequests => "Rate limit exceeded. Free tier: 30 req/min. Wait a moment.",
                HttpStatusCode.Unauthorized => "Invalid API key. Get one at console.groq.com",
                _ => $"API error ({(int)statusCode}): {Truncate(message, 300)}"
            };
        }
        catch
        {
            return $"API error ({(int)statusCode}): {Truncate(responseJson, 300)}";
        }
    }

    private static string ParseGeminiError(HttpStatusCode statusCode, string responseJson)
    {
        try
        {
            using var doc = JsonDocument.Parse(responseJson);
            var error = doc.RootElement.GetProperty("error");
            var message = error.GetProperty("message").GetString() ?? "";

            return statusCode switch
            {
                HttpStatusCode.TooManyRequests when message.Contains("free_tier", StringComparison.OrdinalIgnoreCase)
                    => "Free-tier quota exhausted. Wait until tomorrow or use a new API key.",
                HttpStatusCode.TooManyRequests => "Rate limit exceeded. Wait and try again.",
                HttpStatusCode.Unauthorized => "Invalid API key. Get one at https://aistudio.google.com/apikey",
                _ => $"Gemini error ({(int)statusCode}): {Truncate(message, 300)}"
            };
        }
        catch
        {
            return $"Gemini error ({(int)statusCode}): {Truncate(responseJson, 300)}";
        }
    }

    private static string Truncate(string s, int max) =>
        s.Length <= max ? s : s[..max] + "...";
}
