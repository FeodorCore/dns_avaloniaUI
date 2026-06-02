using System;
using System.IO;
using System.Text.Json;
using Desktop.Models;

namespace Desktop.Services;

public static class SettingsService
{
    private static readonly string SettingsPath = Path.Combine(
        AppContext.BaseDirectory, "connection_settings.json");

    public static ConnectionSettings Load()
    {
        try
        {
            if (File.Exists(SettingsPath))
            {
                var json = File.ReadAllText(SettingsPath);
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                return JsonSerializer.Deserialize<ConnectionSettings>(json, options) ?? new ConnectionSettings();
            }
        }
        catch
        {
            // При ошибке чтения возвращаем настройки по умолчанию
        }
        return new ConnectionSettings();
    }

    public static void Save(ConnectionSettings settings)
    {
        try
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(settings, options);
            File.WriteAllText(SettingsPath, json);
        }
        catch
        {
            // Игнорируем ошибки записи
        }
    }
}