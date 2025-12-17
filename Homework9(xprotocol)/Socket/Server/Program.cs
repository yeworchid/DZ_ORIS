using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using GameData;
using GameData.XProtocol;
using GameData.XProtocol.Packets;

using var udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
var gameState = new GameData.GameState();

var localIP = new IPEndPoint(IPAddress.Parse("0.0.0.0"), 5555);
udpSocket.Bind(localIP);
Console.WriteLine("XProtocol сервер запущен...");
 
byte[] data = new byte[1024];
EndPoint remoteIp = new IPEndPoint(IPAddress.Any, 0);

while (true)
{
    var result = await udpSocket.ReceiveFromAsync(data, remoteIp);
    var receivedData = data.Take(result.ReceivedBytes).ToArray();

    Console.WriteLine($"Получено {result.ReceivedBytes} байт от {result.RemoteEndPoint}");
    
    var packet = XPacket.Parse(receivedData);
    
    if (packet == null)
    {
        Console.WriteLine("Неверный пакет");
        data = new byte[1024];
        continue;
    }

    var packetType = XPacketTypeManager.GetTypeFromPacket(packet);
    
    switch (packetType)
    {
        case XPacketType.Handshake:
            ProcessHandshake(packet, result.RemoteEndPoint);
            break;
            
        case XPacketType.PlayerJoin:
            ProcessPlayerJoin(packet, result.RemoteEndPoint);
            break;
            
        case XPacketType.PlayerMove:
            ProcessPlayerMove(packet, result.RemoteEndPoint);
            break;
            
        case XPacketType.Unknown:
            Console.WriteLine("Неизвестный тип пакета");
            break;
    }

    data = new byte[1024]; 
}

void ProcessHandshake(XPacket packet, EndPoint remoteEndPoint)
{
    Console.WriteLine("Получен пакет рукопожатия");
    
    var handshake = XPacketConverter.Deserialize<XPacketHandshake>(packet);
    handshake.MagicHandshakeNumber -= 15;
    
    Console.WriteLine("Отправка ответа на рукопожатие");
    
    var responsePacket = XPacketConverter.Serialize(XPacketType.Handshake, handshake).ToPacket();
    udpSocket.SendToAsync(responsePacket, remoteEndPoint);
}

void ProcessPlayerJoin(XPacket packet, EndPoint remoteEndPoint)
{
    var playerJoin = XPacketConverter.Deserialize<XPacketPlayerJoin>(packet);
    var nameBytes = packet.GetValueRaw(2);
    var playerName = Encoding.UTF8.GetString(nameBytes);
    
    var newPlayer = new Player(playerName)
    {
        Ip = remoteEndPoint.ToString()
    };
    
    foreach (var existingPlayer in gameState.Players.Values)
    {
        var existingPlayerJoin = new XPacketPlayerJoin
        {
            NameLength = existingPlayer.Name.Length
        };
        var existingPacket = XPacketConverter.Serialize(XPacketType.PlayerJoin, existingPlayerJoin);
        existingPacket.SetValueRaw(2, Encoding.UTF8.GetBytes(existingPlayer.Name));
        
        udpSocket.SendToAsync(existingPacket.ToPacket(), remoteEndPoint);
    }
    
    gameState.Players[playerName] = newPlayer;
    Console.WriteLine($"Игрок {playerName} подключился");
    
    var newPlayerJoin = new XPacketPlayerJoin
    {
        NameLength = playerName.Length
    };
    var newPlayerPacket = XPacketConverter.Serialize(XPacketType.PlayerJoin, newPlayerJoin);
    newPlayerPacket.SetValueRaw(2, Encoding.UTF8.GetBytes(playerName));
    var newPlayerData = newPlayerPacket.ToPacket();
    
    foreach (var existingPlayer in gameState.Players.Values)
    {
        if (existingPlayer.Name != playerName)
        {
            var existingPlayerEndPoint = IPEndPoint.Parse(existingPlayer.Ip);
            udpSocket.SendToAsync(newPlayerData, existingPlayerEndPoint);
        }
    }
}

void ProcessPlayerMove(XPacket packet, EndPoint remoteEndPoint)
{
    var move = XPacketConverter.Deserialize<XPacketPlayerMove>(packet);
    Console.WriteLine($"Получен ход: {move.X}, {move.Y}, {move.R}");
    
    var movePacket = XPacketConverter.Serialize(XPacketType.PlayerMove, move).ToPacket();
    
    foreach (var player in gameState.Players.Values)
    {
        var playerEndPoint = IPEndPoint.Parse(player.Ip);
        udpSocket.SendToAsync(movePacket, playerEndPoint);
    }
}
