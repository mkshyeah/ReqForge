using System.Text;
using ReqForge.Models;

namespace ReqForge.Services;

public static class CodeGeneratorService
{
    public static string ToCurl(string method, string url,
        IEnumerable<HeaderItem> headers, string? body)
    {
        var sb = new StringBuilder($"curl -X {method} \"{url}\"");
        foreach (var h in headers.Where(h => !string.IsNullOrWhiteSpace(h.Key)))
            sb.Append($" \\\n  -H \"{h.Key}: {h.Value}\"");
        if (!string.IsNullOrEmpty(body))
            sb.Append($" \\\n  -d '{body}'");
        return sb.ToString();
    }
    public static string ToCSharpHttpClient(string method, string url,
        IEnumerable<HeaderItem> headers, string? body)
    {
        var sb = new StringBuilder();
        sb.AppendLine("using var client = new HttpClient();");
        sb.AppendLine($"using var request = new HttpRequestMessage(new HttpMethod(\"{method}\"), \"{url}\");");
        foreach (var h in headers.Where(h => !string.IsNullOrWhiteSpace(h.Key)))
        {
            if (h.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase))
                continue;
            sb.AppendLine($"request.Headers.TryAddWithoutValidation(\"{h.Key}\", \"{h.Value}\");");
        }
        if (!string.IsNullOrEmpty(body))
        {
            var contentType = headers
                .FirstOrDefault(h => h.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase))
                ?.Value ?? "application/json";
            sb.AppendLine($"request.Content = new StringContent(@\"{body.Replace("\"", "\"\"")}\", Encoding.UTF8, \"{contentType}\");");
        }
        sb.AppendLine("var response = await client.SendAsync(request);");
        sb.AppendLine("var content = await response.Content.ReadAsStringAsync();");
        return sb.ToString();
    }
}
