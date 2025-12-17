using System.Net;
using System.Text.Json;
using Microsoft.Maui.Graphics;
using MyApp.Services;
using MyApp.Models;

namespace MyApp;

public partial class MainPage : ContentPage
{
    private readonly List<Move> _moves = new();
    private readonly IUdpClient _udpClient;

    public MainPage(IUdpClient udpClient)
    {
        InitializeComponent();
        _udpClient = udpClient;
        udpClient.StartListening(5556, ProcessMessage);

        GameField.Drawable = new GameDrawable(_moves);
    }

    public void AddMove(Move move)
    {
        _moves.Add(move);
        UpdateMovesHistory();
    }

    private void UpdateMovesHistory()
    {
        MovesHistory.Children.Clear();
        foreach (var move in _moves)
        {
            MovesHistory.Children.Add(new Label { Text = $"Ход игрока ?: ({move.X}, {move.Y})" });
        }
    }

    private async void OnNewGameClicked(object sender, EventArgs e)
    {
        var move = new Move(Random.Shared.Next(80, 241), Random.Shared.Next(80, 241), 1);
        
        var endPoint = new IPEndPoint(IPAddress.Parse("192.168.1.101"), 5555);
        var message = "{move}\n" + JsonSerializer.Serialize(new { x = move.X, y = move.Y, r = move.R });
        
        await _udpClient.SendAsync(message, endPoint);
    }

    private async void OnTestUdpClicked(object sender, EventArgs e)
    {
        string testMessage = $"Тест UDP {DateTime.Now:HH:mm:ss}";
        var endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5555);
        await _udpClient.SendAsync(testMessage, endPoint);
    }

    private async void OnSetNameClicked(object sender, EventArgs e)
    {
        string name = NameEntry.Text;
        if (string.IsNullOrWhiteSpace(name)) return;
        
        Player1Name.Text = name;
        
        var endPoint = new IPEndPoint(IPAddress.Parse("192.168.1.101"), 5555);
        var message = "{name}\n" + JsonSerializer.Serialize(new { name = name });
        
        await _udpClient.SendAsync(message, endPoint);
    }

    public void ProcessMessage(string message)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (message.Split("\n")[0] == "{move}")
            {
                var json = message.Split("\n")[1];
                var move = JsonSerializer.Deserialize<Move>(json);

                if (move != null)
                {
                    AddMove(move);
                    UpdateMovesHistory();
                    GameField.Invalidate();
                }
            }
            else if (message.Split("\n")[0] == "{name}")
            {
                var json = message.Split("\n")[1];
                var player = JsonSerializer.Deserialize<Player>(json);

                if (player != null && player.Name != Player1Name.Text)
                {
                    Player2Name.Text = player.Name;
                }
            }
        });
    }
}

public class GameDrawable : IDrawable
{
    private readonly List<Move> _moves;

    public GameDrawable(List<Move> moves) => _moves = moves;

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        canvas.FillColor = Colors.White;
        canvas.FillRectangle(dirtyRect);

        canvas.StrokeColor = Colors.Red;
        canvas.StrokeSize = 4;

        foreach (var point in _moves) canvas.DrawCircle(point.X, point.Y, point.R);
    }
}


