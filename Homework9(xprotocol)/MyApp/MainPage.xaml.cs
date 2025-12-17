using Microsoft.Maui.Graphics;
using MyApp.Models;
using MyApp.Client;
using MyApp.XProtocol.Packets;

namespace MyApp;

public partial class MainPage : ContentPage
{
    private readonly List<Move> _moves = new();
    private readonly XProtocolClient _client;
    private string _myName;

    public MainPage()
    {
        InitializeComponent();
        
        _client = new XProtocolClient("192.168.1.101", 5555);
        _client.OnHandshakeComplete += OnHandshakeComplete;
        _client.OnPlayerJoined += OnPlayerJoined;
        _client.OnPlayerMoved += OnPlayerMoved;

        GameField.Drawable = new GameDrawable(_moves);
        
        _ = _client.ConnectAsync();
    }

    private void OnHandshakeComplete()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            System.Diagnostics.Debug.WriteLine("Рукопожатие успешно");
        });
    }

    private void OnPlayerJoined(XPacketPlayerJoin packet, string playerName)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (playerName != _myName)
            {
                Player2Name.Text = playerName;
            }
        });
    }

    private void OnPlayerMoved(XPacketPlayerMove movePacket)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            var move = new Move(movePacket.X, movePacket.Y, movePacket.R);
            AddMove(move);
            UpdateMovesHistory();
            GameField.Invalidate();
        });
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
        var x = Random.Shared.Next(80, 241);
        var y = Random.Shared.Next(80, 241);
        var r = 1;
        
        await _client.SendPlayerMove(x, y, r);
    }

    private async void OnTestUdpClicked(object sender, EventArgs e)
    {
        await _client.ConnectAsync();
    }

    private async void OnSetNameClicked(object sender, EventArgs e)
    {
        string name = NameEntry.Text;
        if (string.IsNullOrWhiteSpace(name)) return;
        
        _myName = name;
        Player1Name.Text = name;
        
        await _client.SendPlayerJoin(name);
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


