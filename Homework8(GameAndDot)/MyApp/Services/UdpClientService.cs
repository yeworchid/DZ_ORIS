using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MyApp.Services;

public class UdpClientService : IUdpClient, IDisposable
{
    private UdpClient? _udpClient;
    private CancellationTokenSource? _cancellationTokenSource;
    private bool _disposed = false;

    public async Task<int> SendAsync(string message, IPEndPoint endPoint)
    {
        byte[] data = Encoding.UTF8.GetBytes(message);
        return await SendAsync(data, endPoint);
    }

    public async Task<int> SendAsync(byte[] data, IPEndPoint endPoint)
    {
        if (_udpClient == null)
        {
            throw new InvalidOperationException("UDP client not initialized. Call StartListening first.");
        }
        return await _udpClient.SendAsync(data, endPoint);
    }

    public async Task<UdpReceiveResult> ReceiveAsync()
    {
        if (_udpClient == null)
        {
            throw new InvalidOperationException("UDP client not initialized. Call StartListening first.");
        }
        return await _udpClient.ReceiveAsync();
    }

    public async Task StartListening(int port, Action<string> onMessageReceived)
    {
        await StopListening();
        
        _udpClient = new UdpClient(port);
        _cancellationTokenSource = new CancellationTokenSource();
        
        _ = Task.Run(async () =>
        {
            try
            {
                while (!_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    var result = await _udpClient.ReceiveAsync();
                    var message = Encoding.UTF8.GetString(result.Buffer);
                    onMessageReceived?.Invoke(message);
                }
            }
            catch (ObjectDisposedException) { }
            catch (SocketException) { }
        }, _cancellationTokenSource.Token);
    }

    public async Task StopListening()
    {
        _cancellationTokenSource?.Cancel();
        _udpClient?.Close();
        _udpClient?.Dispose();
        _udpClient = null;
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = null;
        
        await Task.Delay(100);
    }

    public void Dispose()
    {
        if (_disposed) return;
        
        _disposed = true;
        _cancellationTokenSource?.Cancel();
        _udpClient?.Dispose();
        _cancellationTokenSource?.Dispose();
    }
}