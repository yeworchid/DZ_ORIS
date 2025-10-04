using System.Net;
using System.Text;
using System.Text.Json;
using MiniHttpServer.Shared;

namespace MiniHttpServer.Services;

public class HttpServer
{
    private readonly string _settingsPath = "settings.json";
    private SettingsModel? _settings;
    private HttpListener? _listener;
    private bool _isRunning;

    public async Task StartAsync()
    {
        try
        {
            if (!LoadSettings())
                return;

            if (!ValidateStaticFiles())
                return;

            _listener = new HttpListener();
            _listener.Prefixes.Add($"http://{_settings!.Domain}:{_settings.Port}/");
            _listener.Start();
            _isRunning = true;

            Console.WriteLine($"[INFO] сервер запущен по адресу: {_listener.Prefixes.First()}");

            _ = Task.Run(ListenForCommands);
            await ListenRequestsAsync();
        }
        catch (JsonException)
        {
            Console.WriteLine("[ERROR] ошибка в файле настроек settings.json");
        }
        catch (Exception ex)
        {
            Console.WriteLine("[ERROR] " + ex.Message);
        }
    }

    private bool LoadSettings()
    {
        if (!File.Exists(_settingsPath))
        {
            Console.WriteLine("[WARNING] settings.json не найден, сервер не запущен");
            return false;
        }

        string settingsJson = File.ReadAllText(_settingsPath);
        _settings = JsonSerializer.Deserialize<SettingsModel>(settingsJson);

        if (_settings == null ||
            string.IsNullOrEmpty(_settings.StaticDirectoryPath) ||
            string.IsNullOrEmpty(_settings.Domain) ||
            string.IsNullOrEmpty(_settings.Port))
        {
            Console.WriteLine("[WARNING] настройки сервера некорректны, сервер не запущен");
            return false;
        }

        if (!_settings.StaticDirectoryPath.EndsWith('/'))
            _settings.StaticDirectoryPath += '/';

        Console.WriteLine("[INFO] настройки успешно загружены:" +
            $"\n - путь к статическим файлам: {_settings.StaticDirectoryPath}" +
            $"\n - домен: {_settings.Domain}" +
            $"\n - порт: {_settings.Port}");

        return true;
    }

    private bool ValidateStaticFiles()
    {
        if (!Directory.Exists(_settings!.StaticDirectoryPath) ||
            !File.Exists(Path.Combine(_settings.StaticDirectoryPath, "index.html")))
        {
            Console.WriteLine("[WARNING] папка статических файлов не найдена или в ней отсутствует index.html, сервер не запущен");
            return false;
        }
        return true;
    }

    private async Task ListenRequestsAsync()
    {
        while (_isRunning)
        {
            try
            {
                Console.WriteLine("[INFO] сервер ожидает запрос...");
                var context = await _listener!.GetContextAsync();

                Console.WriteLine("[INFO] получен запрос: " + context.Request.RawUrl);

                await HandleRequestAsync(context);
            }
            catch (ObjectDisposedException)
            {
                // сервер остановлен вручную
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine("[ERROR] " + ex.Message);
                Stop();
                break;
            }
        }
    }

    private async Task HandleRequestAsync(HttpListenerContext context)
    {
        var response = context.Response;
        string filePath = Path.Combine(_settings!.StaticDirectoryPath, "index.html");
        string responseText = File.ReadAllText(filePath);
        byte[] buffer = Encoding.UTF8.GetBytes(responseText);

        response.ContentLength64 = buffer.Length;

        await using Stream output = response.OutputStream;
        await output.WriteAsync(buffer);
        await output.FlushAsync();

        Console.WriteLine("[INFO] запрос успешно обработан");
    }

    private async Task ListenForCommands()
    {
        while (true)
        {
            string? command = Console.ReadLine();
            if (command == "/stop")
            {
                Stop();
                Console.WriteLine("[INFO] сервер остановлен командой /stop");
                break;
            }
        }
    }

    public void Stop()
    {
        if (!_isRunning) return;

        _isRunning = false;
        _listener?.Stop();
    }
}