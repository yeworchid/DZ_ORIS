using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameData;

public class Move
{
    [JsonPropertyName("x")]
    public int X { get; set; }
    [JsonPropertyName("y")]
    public int Y { get; set; }
    [JsonPropertyName("r")]
    public int R { get; set; }
    [JsonIgnore]
    public int PlayerIp { get; set; }

    public Move(int x, int y, int r)
    {
        X = x;
        Y = y;
        R = r;
    }

    public override string ToString()
    {
        return $"{X}, {Y}, {R}";
    }
}

public class Player
{
    [JsonIgnore]
    public string Ip { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; }

    public Player(string name)
    {
        Name = name;
    }

    public override string ToString()
    {
        return $"{Name}, {Ip}";
    }
}

public class GameState
{
    public Dictionary<string, Player> Players { get; set; }

    public GameState()
    {
        Players = new Dictionary<string, Player>();
    }
}