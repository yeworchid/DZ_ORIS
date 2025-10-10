using System.Net;
using System.Text;
using System.Text.Json;
using MiniHttpServer.Shared;

namespace MiniHttpServer.Services;

public class HttpServer
{
    private static HttpServer? _instance;
    private static readonly object _lock = new();

    public static HttpServer Instance // реализуем паттерн Singleton
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                        _instance = new HttpServer();
                }
            }
            return _instance;
        }
    }

    private HttpServer() { }

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
        var request = context.Request;
        
        Console.WriteLine("[INFO] получен запрос: " + request.RawUrl);
        
        var localPath = request.Url?.LocalPath;

        if (localPath == "/") // если запрошен корневой путь, отдаем index.html
        {
            localPath = "/index.html";
        }
        else if (!File.Exists(_settings.StaticDirectoryPath + localPath?.TrimStart('/'))) // если файл не найден, отдаем 404
        {
            Console.WriteLine("[INFO] запрос обработан с ошибкой 404: файл не найден");

            context.Response.StatusCode = 404;

            byte[] notFound = Encoding.UTF8.GetBytes("404 Not Found");
            await context.Response.OutputStream.WriteAsync(notFound);   
            context.Response.Close();
            return;
        }
        
        var response = context.Response;
        var filePath = Path.Combine(_settings!.StaticDirectoryPath, localPath?.TrimStart('/'));
        
        response.ContentType = MimeTypes.GetMimeType(filePath); // определяем тип файла
        
        byte[] buffer = File.ReadAllBytes(filePath); // читаем файл в массив байтов
        response.ContentLength64 = buffer.Length;

        using Stream output = response.OutputStream;
        await output.WriteAsync(buffer); // записываем массив байтов в поток ответа
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