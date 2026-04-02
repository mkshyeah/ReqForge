using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReqForge.Services;

namespace ReqForge.ViewModels;

public partial class MainViewModel
{
    private readonly WebSocketClientService _wsService = new();
    [ObservableProperty] private string _wsUrl = "ws://";
    [ObservableProperty] private string _wsMessageToSend = string.Empty;
    [ObservableProperty] private string _wsStatus = "Disconnected";
    [ObservableProperty] private bool _wsIsConnected;

    public ObservableCollection<string> WsMessages { get; } = new();

    private void InitWebSocket()
    {
        _wsService.MessageReceived += msg =>
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                WsMessages.Add($"[IN]  {DateTime.Now:HH:mm:ss}  {msg}");
            });
        };

        _wsService.StatusChanged += status =>
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                WsStatus = status;
                WsIsConnected = _wsService.IsConnected;
            });
        };
    }

    [RelayCommand]
    private async Task WsConnect()
    {
        if (string.IsNullOrWhiteSpace(WsUrl)) return;
        WsMessages.Clear();
        await _wsService.ConnectAsync(WsUrl);
    }
    
    [RelayCommand]
    private async Task WsDisconnect()
    {
        await _wsService.DisconnectAsync();
    }
    
    [RelayCommand]
    private async Task WsSend()
    {
        if (string.IsNullOrWhiteSpace(WsMessageToSend)) return;
        await _wsService.SendAsync(WsMessageToSend);
        WsMessages.Add($"[OUT] {DateTime.Now:HH:mm:ss}  {WsMessageToSend}");
        WsMessageToSend = string.Empty;
    }
    
    [RelayCommand]
    private void WsClearMessages()
    {
        WsMessages.Clear();
    }
}