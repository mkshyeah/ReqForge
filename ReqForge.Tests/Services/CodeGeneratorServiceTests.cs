using ReqForge.Models;
using ReqForge.Services;

namespace ReqForge.Tests.Services;

public class CodeGeneratorServiceTests
{
    private static readonly HeaderItem[] NoHeaders = Array.Empty<HeaderItem>();

    // ╔═══════════════════════════════════╗
    // ║         cURL (bash)               ║
    // ╚═══════════════════════════════════╝

    [Fact]
    public void CurlBash_SimpleGet()
    {
        var result = CodeGeneratorService.ToCurlBash("GET", "https://api.example.com/users", NoHeaders, null);
        Assert.Contains("curl -X GET", result);
        Assert.Contains("'https://api.example.com/users'", result);
    }

    [Fact]
    public void CurlBash_WithHeaders_UsesSingleQuotes()
    {
        var headers = new[] { new HeaderItem("Authorization", "Bearer token123") };
        var result = CodeGeneratorService.ToCurlBash("GET", "https://example.com", headers, null);

        Assert.Contains("-H 'Authorization: Bearer token123'", result);
        Assert.Contains("\\", result); // backslash line continuation
    }

    [Fact]
    public void CurlBash_WithBody_SingleQuotes()
    {
        var body = "{\"name\":\"John\"}";
        var result = CodeGeneratorService.ToCurlBash("POST", "https://example.com", NoHeaders, body);

        Assert.Contains("-d '", result);
        Assert.Contains("-X POST", result);
    }

    [Fact]
    public void CurlBash_EmptyBody_NoDataFlag()
    {
        var result = CodeGeneratorService.ToCurlBash("GET", "https://example.com", NoHeaders, null);
        Assert.DoesNotContain("-d", result);
    }

    [Fact]
    public void CurlBash_SkipsEmptyHeaderKeys()
    {
        var headers = new[] { new HeaderItem("", "value"), new HeaderItem("Valid", "ok") };
        var result = CodeGeneratorService.ToCurlBash("GET", "https://example.com", headers, null);

        Assert.Contains("'Valid: ok'", result);
        Assert.DoesNotContain("': value'", result);
    }

    [Theory]
    [InlineData("GET")]
    [InlineData("POST")]
    [InlineData("PUT")]
    [InlineData("DELETE")]
    [InlineData("PATCH")]
    public void CurlBash_AllMethods(string method)
    {
        var result = CodeGeneratorService.ToCurlBash(method, "https://example.com", NoHeaders, null);
        Assert.Contains($"-X {method}", result);
    }

    // ╔═══════════════════════════════════╗
    // ║         cURL (Windows)            ║
    // ╚═══════════════════════════════════╝

    [Fact]
    public void CurlWindows_UsesDoubleQuotes()
    {
        var result = CodeGeneratorService.ToCurlWindows("GET", "https://example.com", NoHeaders, null);
        Assert.Contains("\"https://example.com\"", result);
    }

    [Fact]
    public void CurlWindows_UsesCaretContinuation()
    {
        var headers = new[] { new HeaderItem("Accept", "application/json") };
        var result = CodeGeneratorService.ToCurlWindows("GET", "https://example.com", headers, null);

        Assert.Contains("^", result); // caret continuation, not backslash
        Assert.DoesNotContain("\\", result);
    }

    [Fact]
    public void CurlWindows_EscapesQuotesInBody()
    {
        var body = "{\"name\":\"John\"}";
        var result = CodeGeneratorService.ToCurlWindows("POST", "https://example.com", NoHeaders, body);

        Assert.Contains("\\\"name\\\"", result);
        Assert.Contains("-d \"", result);
    }

    [Fact]
    public void CurlWindows_HeadersWithDoubleQuotes()
    {
        var headers = new[] { new HeaderItem("Authorization", "Bearer abc") };
        var result = CodeGeneratorService.ToCurlWindows("GET", "https://example.com", headers, null);

        Assert.Contains("-H \"Authorization: Bearer abc\"", result);
    }

    // ╔═══════════════════════════════════╗
    // ║           PowerShell              ║
    // ╚═══════════════════════════════════╝

    [Fact]
    public void PowerShell_SimpleGet()
    {
        var result = CodeGeneratorService.ToPowerShell("GET", "https://example.com/api", NoHeaders, null);

        Assert.Contains("Invoke-RestMethod", result);
        Assert.Contains("-Uri \"https://example.com/api\"", result);
        Assert.Contains("-Method GET", result);
    }

    [Fact]
    public void PowerShell_WithHeaders_CreatesHashtable()
    {
        var headers = new[]
        {
            new HeaderItem("Authorization", "Bearer token"),
            new HeaderItem("Accept", "application/json")
        };
        var result = CodeGeneratorService.ToPowerShell("GET", "https://example.com", headers, null);

        Assert.Contains("$headers = @{", result);
        Assert.Contains("\"Authorization\" = \"Bearer token\"", result);
        Assert.Contains("\"Accept\" = \"application/json\"", result);
        Assert.Contains("-Headers $headers", result);
    }

    [Fact]
    public void PowerShell_NoHeaders_NoHashtable()
    {
        var result = CodeGeneratorService.ToPowerShell("GET", "https://example.com", NoHeaders, null);

        Assert.DoesNotContain("$headers", result);
    }

    [Fact]
    public void PowerShell_WithBody_UsesHereString()
    {
        var headers = new[] { new HeaderItem("Content-Type", "application/json") };
        var body = "{\"name\":\"test\"}";
        var result = CodeGeneratorService.ToPowerShell("POST", "https://example.com", headers, body);

        Assert.Contains("$body = @'", result);
        Assert.Contains(body, result);
        Assert.Contains("'@", result);
        Assert.Contains("-Body $body", result);
        Assert.Contains("-ContentType \"application/json\"", result);
    }

    [Fact]
    public void PowerShell_NoBody_NoBodyParam()
    {
        var result = CodeGeneratorService.ToPowerShell("GET", "https://example.com", NoHeaders, null);

        Assert.DoesNotContain("$body", result);
        Assert.DoesNotContain("-Body", result);
    }

    [Theory]
    [InlineData("GET")]
    [InlineData("POST")]
    [InlineData("PUT")]
    [InlineData("DELETE")]
    [InlineData("PATCH")]
    public void PowerShell_AllMethods(string method)
    {
        var result = CodeGeneratorService.ToPowerShell(method, "https://example.com", NoHeaders, null);
        Assert.Contains($"-Method {method}", result);
    }

    // ╔═══════════════════════════════════╗
    // ║         C# HttpClient             ║
    // ╚═══════════════════════════════════╝

    [Fact]
    public void CSharp_SimpleGet_UsesGetAsync()
    {
        var result = CodeGeneratorService.ToCSharpHttpClient("GET", "https://api.example.com", NoHeaders, null);

        Assert.Contains("new HttpClient()", result);
        Assert.Contains("client.GetAsync(\"https://api.example.com\")", result);
        Assert.Contains("response.Content.ReadAsStringAsync()", result);
        Assert.Contains("Console.WriteLine(responseBody)", result);
    }

    [Fact]
    public void CSharp_Delete_UsesDeleteAsync()
    {
        var result = CodeGeneratorService.ToCSharpHttpClient("DELETE", "https://example.com/1", NoHeaders, null);
        Assert.Contains("client.DeleteAsync(", result);
    }

    [Fact]
    public void CSharp_PostWithBody_UsesPostAsync()
    {
        var body = "{\"name\":\"test\"}";
        var result = CodeGeneratorService.ToCSharpHttpClient("POST", "https://example.com", NoHeaders, body);

        Assert.Contains("client.PostAsync(", result);
        Assert.Contains("new StringContent(jsonBody, Encoding.UTF8", result);
        Assert.Contains("var jsonBody = @\"", result);
    }

    [Fact]
    public void CSharp_PutWithBody_UsesPutAsync()
    {
        var result = CodeGeneratorService.ToCSharpHttpClient("PUT", "https://example.com", NoHeaders, "{\"a\":1}");
        Assert.Contains("client.PutAsync(", result);
    }

    [Fact]
    public void CSharp_PatchWithBody_UsesPatchAsync()
    {
        var result = CodeGeneratorService.ToCSharpHttpClient("PATCH", "https://example.com", NoHeaders, "{\"a\":1}");
        Assert.Contains("client.PatchAsync(", result);
    }

    [Fact]
    public void CSharp_AcceptHeader_UsesTypedApi()
    {
        var headers = new[] { new HeaderItem("Accept", "application/json") };
        var result = CodeGeneratorService.ToCSharpHttpClient("GET", "https://example.com", headers, null);

        Assert.Contains("DefaultRequestHeaders.Accept.Add", result);
        Assert.Contains("MediaTypeWithQualityHeaderValue", result);
    }

    [Fact]
    public void CSharp_AuthorizationHeader_UsesTypedApi()
    {
        var headers = new[] { new HeaderItem("Authorization", "Bearer abc123") };
        var result = CodeGeneratorService.ToCSharpHttpClient("GET", "https://example.com", headers, null);

        Assert.Contains("DefaultRequestHeaders.Authorization", result);
        Assert.Contains("AuthenticationHeaderValue(\"Bearer\", \"abc123\")", result);
    }

    [Fact]
    public void CSharp_ContentTypeSkipped_InHeaders()
    {
        var headers = new[]
        {
            new HeaderItem("Content-Type", "application/json"),
            new HeaderItem("X-Custom", "test")
        };
        var result = CodeGeneratorService.ToCSharpHttpClient("POST", "https://example.com", headers, "{\"a\":1}");

        Assert.DoesNotContain("TryAddWithoutValidation(\"Content-Type\"", result);
        Assert.Contains("TryAddWithoutValidation(\"X-Custom\", \"test\")", result);
    }

    [Fact]
    public void CSharp_ContentType_UsedInStringContent()
    {
        var headers = new[] { new HeaderItem("Content-Type", "text/xml") };
        var body = "<root/>";
        var result = CodeGeneratorService.ToCSharpHttpClient("POST", "https://example.com", headers, body);

        Assert.Contains("\"text/xml\"", result);
    }

    [Fact]
    public void CSharp_NoContentType_DefaultsToJson()
    {
        var result = CodeGeneratorService.ToCSharpHttpClient("POST", "https://example.com", NoHeaders, "{\"a\":1}");
        Assert.Contains("\"application/json\"", result);
    }

    [Fact]
    public void CSharp_NullBody_NoStringContent()
    {
        var result = CodeGeneratorService.ToCSharpHttpClient("GET", "https://example.com", NoHeaders, null);
        Assert.DoesNotContain("StringContent", result);
        Assert.DoesNotContain("jsonBody", result);
    }

    [Fact]
    public void CSharp_UnknownMethodWithBody_UsesSendAsync()
    {
        var result = CodeGeneratorService.ToCSharpHttpClient("OPTIONS", "https://example.com", NoHeaders, "{\"a\":1}");
        Assert.Contains("SendAsync", result);
        Assert.Contains("new HttpMethod(\"OPTIONS\")", result);
    }
}
