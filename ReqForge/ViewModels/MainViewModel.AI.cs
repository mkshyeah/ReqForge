using System.Collections.ObjectModel;
using System.Text;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReqForge.Models;
using ReqForge.Services;

namespace ReqForge.ViewModels;

public partial class MainViewModel
{
    private readonly AiService _ai = new();

    [ObservableProperty] private string _aiApiKey = string.Empty;
    [ObservableProperty] private string _aiExplanation = string.Empty;
    [ObservableProperty] private bool _isAiLoading;
    [ObservableProperty] private string _aiBodyPrompt = string.Empty;
    [ObservableProperty] private string _curlInput = string.Empty;
    [ObservableProperty] private AiModelInfo _selectedAiModel = AiService.AvailableModels[0];

    public List<AiModelInfo> AiModels => AiService.AvailableModels;

    partial void OnAiApiKeyChanged(string value)
    {
        _ai.ApiKey = value;
    }

    partial void OnSelectedAiModelChanged(AiModelInfo value)
    {
        if (value == null) return;
        _ai.Provider = value.Provider;
        _ai.Model = value.Id;
    }

    [RelayCommand]
    private async Task ExplainResponse()
    {
        if (string.IsNullOrWhiteSpace(ResponseBody))
        {
            AiExplanation = "Send a request first to get a response.";
            return;
        }

        IsAiLoading = true;
        AiExplanation = "Thinking...";

        var prompt = new StringBuilder();
        prompt.AppendLine("You are an API debugging assistant. Explain the following HTTP response concisely in Russian.");
        prompt.AppendLine("Include: what the status code means, what the response body contains, and any potential issues.");
        prompt.AppendLine();
        prompt.AppendLine($"Status: {StatusInfo}");
        prompt.AppendLine($"Headers:\n{ResponseHeadersText}");
        prompt.AppendLine($"Body:\n{ResponseBody[..Math.Min(ResponseBody.Length, 3000)]}");

        try
        {
            AiExplanation = await _ai.AskAsync(prompt.ToString());
        }
        catch (Exception ex)
        {
            AiExplanation = $"Error: {ex.Message}";
        }
        finally
        {
            IsAiLoading = false;
        }
    }

    [RelayCommand]
    private async Task GenerateJsonBody()
    {
        if (string.IsNullOrWhiteSpace(AiBodyPrompt))
        {
            AiExplanation = "Describe what JSON you need.";
            return;
        }

        IsAiLoading = true;
        AiExplanation = "Generating JSON...";

        var prompt = $"""
            Generate a valid JSON body based on this description. 
            Return ONLY the raw JSON, no markdown, no explanation, no code fences.
            Description: {AiBodyPrompt}
            """;

        try
        {
            var result = await _ai.AskAsync(prompt);
            var trimmed = result.Trim();

            if (trimmed.StartsWith("```"))
            {
                var lines = trimmed.Split('\n');
                trimmed = string.Join("\n", lines.Skip(1).TakeWhile(l => !l.StartsWith("```")));
            }

            RequestBody = trimmed;
            SelectedBodyType = "json";
            AiExplanation = "JSON body generated and applied to Body tab.";
        }
        catch (Exception ex)
        {
            AiExplanation = $"Error: {ex.Message}";
        }
        finally
        {
            IsAiLoading = false;
        }
    }

    [RelayCommand]
    private async Task ParseCurl()
    {
        if (string.IsNullOrWhiteSpace(CurlInput))
        {
            AiExplanation = "Paste a cURL command first.";
            return;
        }

        IsAiLoading = true;
        AiExplanation = "Parsing cURL...";

        var prompt = $$"""
            Parse this cURL command and return a JSON object with these exact fields:
            - "method": HTTP method (string)
            - "url": the URL (string)
            - "headers": array of {"key": "...", "value": "..."}
            - "body": request body if present (string or null)
            - "bodyType": "json", "form-data", "raw", or "none"
            
            Return ONLY valid JSON, no markdown, no explanation, no code fences.
            
            cURL command:
            {{CurlInput}}
            """;

        try
        {
            var result = await _ai.AskAsync(prompt);
            var trimmed = result.Trim();

            if (trimmed.StartsWith("```"))
            {
                var lines = trimmed.Split('\n');
                trimmed = string.Join("\n", lines.Skip(1).TakeWhile(l => !l.StartsWith("```")));
            }

            using var doc = JsonDocument.Parse(trimmed);
            var root = doc.RootElement;

            if (root.TryGetProperty("method", out var method))
                SelectedMethod = method.GetString()?.ToUpper() ?? "GET";

            if (root.TryGetProperty("url", out var url))
                Url = url.GetString() ?? string.Empty;

            Headers.Clear();
            if (root.TryGetProperty("headers", out var headers))
            {
                foreach (var h in headers.EnumerateArray())
                {
                    var k = h.GetProperty("key").GetString() ?? "";
                    var v = h.GetProperty("value").GetString() ?? "";
                    Headers.Add(new HeaderItem(k, v));
                }
            }
            if (Headers.Count == 0)
                Headers.Add(new HeaderItem("", ""));

            if (root.TryGetProperty("body", out var body) && body.ValueKind != JsonValueKind.Null)
                RequestBody = body.GetString() ?? string.Empty;

            if (root.TryGetProperty("bodyType", out var bt))
                SelectedBodyType = bt.GetString() ?? "none";

            AiExplanation = "cURL parsed! Method, URL, headers and body have been applied.";
        }
        catch (Exception ex)
        {
            AiExplanation = $"Failed to parse cURL: {ex.Message}";
        }
        finally
        {
            IsAiLoading = false;
        }
    }
}
