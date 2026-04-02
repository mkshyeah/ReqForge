using System.Net.WebSockets;
using System.Text;

namespace ReqForge.Services;

public class WebSocketClientService : IDisposable
{
    private ClientWebSocket? _ws;
    private CancellationTokenSource? _cts;

    public event Action<string>? MessageReceived;
    public event Action<string>? StatusChanged;

    public bool IsConnected => _ws?.State == WebSocketState.Open;

    public async Task ConnectAsync(string url)
    {
        _ws = new ClientWebSocket();
        _cts = new CancellationTokenSource();

        try
        {
            await _ws.ConnectAsync(new Uri(url), _cts.Token);
            StatusChanged?.Invoke("Connected");
            _ = Task.Run(() => ReceiveLoopAsync(_cts.Token));
        }
        catch (Exception ex)
        {
            StatusChanged?.Invoke($"Connection failed: {ex.Message}");
        }
    }

    public async Task SendAsync(string message)
    {
        if (_ws?.State != WebSocketState.Open) return;

        var bytes = Encoding.UTF8.GetBytes(message);
        await _ws.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None);
    }

    public async Task DisconnectAsync()
    {
        if (_ws?.State == WebSocketState.Open)
        {
            await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by user", CancellationToken.None);
        }

        _cts?.Cancel();
        StatusChanged?.Invoke("Disconnected");
    }

    private async Task ReceiveLoopAsync(CancellationToken ct)
    {
        var buffer = new byte[4096];
        try
        {
            while (_ws?.State == WebSocketState.Open && !ct.IsCancellationRequested)
            {
                var result = await _ws.ReceiveAsync(buffer, ct);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    StatusChanged?.Invoke("Server closed connection");
                    break;
                }

                var msg = Encoding.UTF8.GetString(buffer, 0, result.Count);
                MessageReceived?.Invoke(msg);
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            StatusChanged?.Invoke($"Error: {ex.Message}");
        }
    }

    public void Dispose()
    {
        _cts?.Cancel();
        _ws?.Dispose();
        _cts?.Dispose();
    }
}