using System.Text;
using ReqForge.Models;

namespace ReqForge.Services;

public static class CodeGeneratorService
{
    public static string ToCurlBash(string method, string url,
        IEnumerable<HeaderItem> headers, string? body)
    {
        var sb = new StringBuilder($"curl -X {method} '{url}'");
        foreach (var h in headers.Where(h => !string.IsNullOrWhiteSpace(h.Key)))
            sb.Append($" \\\n  -H '{h.Key}: {h.Value}'");
        if (!string.IsNullOrEmpty(body))
            sb.Append($" \\\n  -d '{body.Replace("'", "'\\''")}'");
        return sb.ToString();
    }

    public static string ToCurlWindows(string method, string url,
        IEnumerable<HeaderItem> headers, string? body)
    {
        var sb = new StringBuilder($"curl -X {method} \"{url}\"");
        foreach (var h in headers.Where(h => !string.IsNullOrWhiteSpace(h.Key)))
            sb.Append($" ^\n  -H \"{h.Key}: {h.Value}\"");
        if (!string.IsNullOrEmpty(body))
        {
            var escaped = body.Replace("\"", "\\\"");
            sb.Append($" ^\n  -d \"{escaped}\"");
        }
        return sb.ToString();
    }

    public static string ToPowerShell(string method, string url,
        IEnumerable<HeaderItem> headers, string? body)
    {
        var sb = new StringBuilder();
        var activeHeaders = headers.Where(h => !string.IsNullOrWhiteSpace(h.Key)).ToList();

        if (activeHeaders.Count > 0)
        {
            sb.AppendLine("$headers = @{");
            foreach (var h in activeHeaders)
                sb.AppendLine($"    \"{h.Key}\" = \"{h.Value}\"");
            sb.AppendLine("}");
            sb.AppendLine();
        }

        if (!string.IsNullOrEmpty(body))
        {
            sb.AppendLine("$body = @'");
            sb.AppendLine(body);
            sb.AppendLine("'@");
            sb.AppendLine();
        }

        var ps = method.ToUpper() switch
        {
            "GET" => "Invoke-RestMethod",
            "DELETE" => "Invoke-RestMethod",
            _ => "Invoke-RestMethod"
        };

        sb.Append($"{ps} -Uri \"{url}\" -Method {method}");

        if (activeHeaders.Count > 0)
            sb.Append(" -Headers $headers");

        var contentType = activeHeaders
            .FirstOrDefault(h => h.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase))
            ?.Value;

        if (!string.IsNullOrEmpty(body))
        {
            sb.Append(" -Body $body");
            if (!string.IsNullOrEmpty(contentType))
                sb.Append($" -ContentType \"{contentType}\"");
        }

        return sb.ToString();
    }

    public static string ToCSharpHttpClient(string method, string url,
        IEnumerable<HeaderItem> headers, string? body)
    {
        var sb = new StringBuilder();
        var activeHeaders = headers.Where(h => !string.IsNullOrWhiteSpace(h.Key)).ToList();

        var contentType = activeHeaders
            .FirstOrDefault(h => h.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase))
            ?.Value ?? "application/json";

        sb.AppendLine("using var client = new HttpClient();");
        sb.AppendLine();

        foreach (var h in activeHeaders)
        {
            if (h.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase))
                continue;
            if (h.Key.Equals("Accept", StringComparison.OrdinalIgnoreCase))
            {
                sb.AppendLine($"client.DefaultRequestHeaders.Accept.Add(");
                sb.AppendLine($"    new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue(\"{h.Value}\"));");
            }
            else if (h.Key.Equals("Authorization", StringComparison.OrdinalIgnoreCase))
            {
                var parts = h.Value.Split(' ', 2);
                if (parts.Length == 2)
                    sb.AppendLine($"client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(\"{parts[0]}\", \"{parts[1]}\");");
                else
                    sb.AppendLine($"client.DefaultRequestHeaders.TryAddWithoutValidation(\"Authorization\", \"{h.Value}\");");
            }
            else
            {
                sb.AppendLine($"client.DefaultRequestHeaders.TryAddWithoutValidation(\"{h.Key}\", \"{h.Value}\");");
            }
        }

        if (activeHeaders.Any(h => !h.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase)))
            sb.AppendLine();

        if (!string.IsNullOrEmpty(body))
        {
            var escaped = body.Replace("\"", "\"\"");
            sb.AppendLine($"var jsonBody = @\"{escaped}\";");
            sb.AppendLine();
            sb.AppendLine($"var content = new StringContent(jsonBody, Encoding.UTF8, \"{contentType}\");");
            sb.AppendLine();

            var csMethod = method.ToUpper() switch
            {
                "POST" => "PostAsync",
                "PUT" => "PutAsync",
                "PATCH" => "PatchAsync",
                _ => null
            };

            if (csMethod != null)
            {
                sb.AppendLine($"var response = await client.{csMethod}(\"{url}\", content);");
            }
            else
            {
                sb.AppendLine($"using var request = new HttpRequestMessage(new HttpMethod(\"{method}\"), \"{url}\");");
                sb.AppendLine("request.Content = content;");
                sb.AppendLine("var response = await client.SendAsync(request);");
            }
        }
        else
        {
            var csMethod = method.ToUpper() switch
            {
                "GET" => "GetAsync",
                "DELETE" => "DeleteAsync",
                _ => null
            };

            if (csMethod != null)
            {
                sb.AppendLine($"var response = await client.{csMethod}(\"{url}\");");
            }
            else
            {
                sb.AppendLine($"using var request = new HttpRequestMessage(new HttpMethod(\"{method}\"), \"{url}\");");
                sb.AppendLine("var response = await client.SendAsync(request);");
            }
        }

        sb.AppendLine("var responseBody = await response.Content.ReadAsStringAsync();");
        sb.AppendLine();
        sb.AppendLine("Console.WriteLine(responseBody);");

        return sb.ToString();
    }
}
