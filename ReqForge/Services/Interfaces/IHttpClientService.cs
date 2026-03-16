using ReqForge.Models;

namespace ReqForge.Services.Interfaces;

public interface IHttpClientService
{
    Task<HttpResponseResult> SendAsync(string method, string url, IEnumerable<HeaderItem> headers, string? body);
}