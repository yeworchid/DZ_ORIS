using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using GameData;

using var udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
var gameState = new GameData.GameState();

var localIP = new IPEndPoint(IPAddress.Parse("0.0.0.0"), 5555);
udpSocket.Bind(localIP);
Console.WriteLine("UDP-сервер запущен...");
 
byte[] data = new byte[256];
EndPoint remoteIp = new IPEndPoint(IPAddress.Any, 0);

while (true)
{
    var result = await udpSocket.ReceiveFromAsync(data, remoteIp);
    var message = Encoding.UTF8.GetString(data, 0, result.ReceivedBytes);

    Console.WriteLine($"Получено {result.ReceivedBytes} байт от {result.RemoteEndPoint}");
    
    if (message.Contains("\\n"))
    {
        message = message.Replace("\\n", "\n");
    }

    if (message.Split("\n")[0] == "{move}")
    {
        var json = message.Split("\n")[1];
        var move = JsonSerializer.Deserialize<GameData.Move>(json);
        Console.WriteLine($"Получен ход: {move}");

        foreach (var player in gameState.Players.Values)
        {
            var playerEndPoint = IPEndPoint.Parse(player.Ip);
            var moveData = Encoding.UTF8.GetBytes($"{{move}}\n{json}");
            await udpSocket.SendToAsync(moveData, playerEndPoint);
        }
    }
    else if (message.Split("\n")[0] == "{name}")
    {
        var json = message.Split("\n")[1];
        var newPlayer = JsonSerializer.Deserialize<GameData.Player>(json);
        
        if (newPlayer != null)
        {
            newPlayer.Ip = result.RemoteEndPoint.ToString();
            
            foreach (var existingPlayer in gameState.Players.Values)
            {
                var existingPlayerData = Encoding.UTF8.GetBytes($"{{name}}\n{JsonSerializer.Serialize(existingPlayer)}");
                await udpSocket.SendToAsync(existingPlayerData, result.RemoteEndPoint);
            }
            
            gameState.Players[newPlayer.Name] = newPlayer;
            Console.WriteLine($"Игрок {newPlayer.Name} подключился");
            
            var newPlayerData = Encoding.UTF8.GetBytes($"{{name}}\n{JsonSerializer.Serialize(newPlayer)}");
            foreach (var existingPlayer in gameState.Players.Values)
            {
                if (existingPlayer.Name != newPlayer.Name)
                {
                    var existingPlayerEndPoint = IPEndPoint.Parse(existingPlayer.Ip);
                    await udpSocket.SendToAsync(newPlayerData, existingPlayerEndPoint);
                }
            }
        }
    }

    data = new byte[256]; 
}
