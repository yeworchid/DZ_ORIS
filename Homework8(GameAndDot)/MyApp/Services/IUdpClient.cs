using System.Net;
using System.Net.Sockets;

namespace MyApp.Services;

public interface IUdpClient
{
    Task<int> SendAsync(string message, IPEndPoint endPoint);
    Task<int> SendAsync(byte[] data, IPEndPoint endPoint);
    void Dispose();

    Task<UdpReceiveResult> ReceiveAsync();
    Task StartListening(int port, Action<string> onMessageReceived);
    Task StopListening();
}