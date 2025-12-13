using System;
using System.IO;
using System.Text.Json;

namespace Kontr;

public class Settings
{
    public int processing_interval_seconds { get; set; }
    public int max_error_retries { get; set; }
    public string connection_string { get; set; }

    public override string ToString()
    {
        return $"Интервал: {processing_interval_seconds}\nКоличество попыток: {max_error_retries}";
    }
}

public class SettingsManager
{
    private string _configPath;
    private Settings _settings;

    public SettingsManager(string configPath = "config.json")
    {
        _configPath = configPath;
        _settings = new Settings();
    }

    public Settings GetSettings()
    {
        return _settings;
    }

    public void LoadSettings()
    {
        try
        {
            if (File.Exists(_configPath))
            {
                string json = File.ReadAllText(_configPath);
                _settings = JsonSerializer.Deserialize<Settings>(json);
                Console.WriteLine("Настройки загружены успешно");
            }
            else
            {
                Console.WriteLine("Файл конфига не найден, используем дефолтные значения");
                _settings = new Settings
                {
                    processing_interval_seconds = 300,
                    max_error_retries = 5,
                    connection_string = "Host=localhost;Port=5432;Database=invoice_db;Username=postgres;Password=postgres"
                };
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Ошибка при загрузке настроек: " + ex.Message);
        }
    }

    public void SaveSettings()
    {
        try
        {
            string json = JsonSerializer.Serialize(_settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_configPath, json);
            Console.WriteLine("Настройки сохранены");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Ошибка при сохранении: " + ex.Message);
        }
    }
}
