using System.Net;
using System.Net.Sockets;
using System.Text;
using MyApp.XProtocol;
using MyApp.XProtocol.Packets;

namespace MyApp.Client;

public class XProtocolClient
{
    private Socket _udpSocket;
    private EndPoint _serverEndPoint;
    private int _handshakeMagic;
    private bool _isConnected;
    
    public event Action<XPacketPlayerJoin, string> OnPlayerJoined;
    public event Action<XPacketPlayerMove> OnPlayerMoved;
    public event Action OnHandshakeComplete;

    public XProtocolClient(string serverIp, int serverPort)
    {
        _udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        _serverEndPoint = new IPEndPoint(IPAddress.Parse(serverIp), serverPort);
    }

    public async Task ConnectAsync()
    {
        var rand = new Random();
        _handshakeMagic = rand.Next();

        var handshake = new XPacketHandshake
        {
            MagicHandshakeNumber = _handshakeMagic
        };

        var packet = XPacketConverter.Serialize(XPacketType.Handshake, handshake).ToPacket();
        await _udpSocket.SendToAsync(packet, _serverEndPoint);

        StartReceiving();
    }

    public async Task SendPlayerJoin(string playerName)
    {
        var playerJoin = new XPacketPlayerJoin
        {
            NameLength = playerName.Length
        };

        var packet = XPacketConverter.Serialize(XPacketType.PlayerJoin, playerJoin);
        packet.SetValueRaw(2, Encoding.UTF8.GetBytes(playerName));

        await _udpSocket.SendToAsync(packet.ToPacket(), _serverEndPoint);
    }

    public async Task SendPlayerMove(int x, int y, int r)
    {
        var move = new XPacketPlayerMove
        {
            X = x,
            Y = y,
            R = r
        };

        var packet = XPacketConverter.Serialize(XPacketType.PlayerMove, move).ToPacket();
        await _udpSocket.SendToAsync(packet, _serverEndPoint);
    }

    private async void StartReceiving()
    {
        byte[] buffer = new byte[1024];
        EndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);

        while (true)
        {
            try
            {
                var result = await _udpSocket.ReceiveFromAsync(buffer, remoteEndPoint);
                var receivedData = buffer.Take(result.ReceivedBytes).ToArray();

                var packet = XPacket.Parse(receivedData);
                if (packet != null)
                {
                    ProcessPacket(packet);
                }

                buffer = new byte[1024];
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка получения: {ex.Message}");
                break;
            }
        }
    }

    private void ProcessPacket(XPacket packet)
    {
        var packetType = XPacketTypeManager.GetTypeFromPacket(packet);

        switch (packetType)
        {
            case XPacketType.Handshake:
                ProcessHandshake(packet);
                break;

            case XPacketType.PlayerJoin:
                ProcessPlayerJoin(packet);
                break;

            case XPacketType.PlayerMove:
                ProcessPlayerMove(packet);
                break;
        }
    }

    private void ProcessHandshake(XPacket packet)
    {
        var handshake = XPacketConverter.Deserialize<XPacketHandshake>(packet);

        if (_handshakeMagic - handshake.MagicHandshakeNumber == 15)
        {
            _isConnected = true;
            OnHandshakeComplete?.Invoke();
        }
    }

    private void ProcessPlayerJoin(XPacket packet)
    {
        var playerJoin = XPacketConverter.Deserialize<XPacketPlayerJoin>(packet);
        var nameBytes = packet.GetValueRaw(2);
        var playerName = Encoding.UTF8.GetString(nameBytes);

        OnPlayerJoined?.Invoke(playerJoin, playerName);
    }

    private void ProcessPlayerMove(XPacket packet)
    {
        var move = XPacketConverter.Deserialize<XPacketPlayerMove>(packet);
        OnPlayerMoved?.Invoke(move);
    }

    public void Disconnect()
    {
        _udpSocket?.Close();
        _udpSocket?.Dispose();
    }
}