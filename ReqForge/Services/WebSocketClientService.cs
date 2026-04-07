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
        if (string.IsNullOrWhiteSpace(url) || url is "ws://" or "wss://")
        {
            StatusChanged?.Invoke("Enter a valid WebSocket URL");
            return;
        }

        if (!url.StartsWith("ws://", StringComparison.OrdinalIgnoreCase) &&
            !url.StartsWith("wss://", StringComparison.OrdinalIgnoreCase))
        {
            StatusChanged?.Invoke("URL must start with ws:// or wss://");
            return;
        }

        Uri uri;
        try
        {
            uri = new Uri(url);
        }
        catch (UriFormatException)
        {
            StatusChanged?.Invoke("Invalid URL format");
            return;
        }

        await CleanupPreviousConnection();

        _ws = new ClientWebSocket();
        _cts = new CancellationTokenSource();

        _ws.Options.SetRequestHeader("Origin", $"{uri.Scheme.Replace("ws", "http")}://{uri.Host}");
        _ws.Options.SetRequestHeader("User-Agent", "ReqForge/1.0");
        _ws.Options.KeepAliveInterval = TimeSpan.FromSeconds(30);

        try
        {
            StatusChanged?.Invoke("Connecting...");
            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token, timeoutCts.Token);

            await _ws.ConnectAsync(uri, linkedCts.Token);
            StatusChanged?.Invoke($"Connected to {uri.Host}");
            _ = Task.Run(() => ReceiveLoopAsync(_cts.Token));
        }
        catch (OperationCanceledException)
        {
            StatusChanged?.Invoke("Connection timed out (15s). Check URL or network.");
        }
        catch (WebSocketException ex)
        {
            var inner = ex.InnerException?.Message ?? ex.Message;
            StatusChanged?.Invoke($"WebSocket error: {inner}");
        }
        catch (Exception ex)
        {
            StatusChanged?.Invoke($"Connection failed: {ex.Message}");
        }
    }

    public async Task SendAsync(string message)
    {
        if (_ws?.State != WebSocketState.Open)
        {
            StatusChanged?.Invoke("Not connected — press Connect first");
            return;
        }

        try
        {
            var bytes = Encoding.UTF8.GetBytes(message);
            await _ws.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None);
        }
        catch (WebSocketException ex)
        {
            StatusChanged?.Invoke($"Send failed: {ex.Message}");
        }
    }

    public async Task DisconnectAsync()
    {
        if (_ws?.State == WebSocketState.Open)
        {
            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by user", cts.Token);
            }
            catch { /* connection may already be broken */ }
        }

        _cts?.Cancel();
        StatusChanged?.Invoke("Disconnected");
    }

    private async Task CleanupPreviousConnection()
    {
        if (_ws != null)
        {
            if (_ws.State is WebSocketState.Open or WebSocketState.Connecting)
            {
                try
                {
                    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
                    await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Reconnecting", cts.Token);
                }
                catch { /* ignore */ }
            }

            _ws.Dispose();
            _ws = null;
        }

        if (_cts != null)
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
        }
    }

    private async Task ReceiveLoopAsync(CancellationToken ct)
    {
        var buffer = new byte[8192];
        var messageBuffer = new StringBuilder();

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

                messageBuffer.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));

                if (result.EndOfMessage)
                {
                    MessageReceived?.Invoke(messageBuffer.ToString());
                    messageBuffer.Clear();
                }
            }
        }
        catch (OperationCanceledException) { }
        catch (WebSocketException ex)
        {
            StatusChanged?.Invoke($"Connection lost: {ex.Message}");
        }
        catch (Exception ex)
        {
            StatusChanged?.Invoke($"Receive error: {ex.Message}");
        }
    }

    public void Dispose()
    {
        _cts?.Cancel();
        _ws?.Dispose();
        _cts?.Dispose();
    }
}
