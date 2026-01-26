using System;
using System.IO;
using System.Text.Json;

namespace Garage.Services
{
    public class UserSettings
    {
        public string Theme { get; set; } = "Light";
    }

    public static class UserSettingsService
    {
        private static string SettingsPath
        {
            get
            {
                var dir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "Garage");
                return Path.Combine(dir, "settings.json");
            }
        }

        public static UserSettings Load()
        {
            try
            {
                if (!File.Exists(SettingsPath))
                    return new UserSettings();

                var json = File.ReadAllText(SettingsPath);
                var settings = JsonSerializer.Deserialize<UserSettings>(json);
                return settings ?? new UserSettings();
            }
            catch
            {
                // En cas de JSON corrompu, on retombe sur des valeurs par défaut.
                return new UserSettings();
            }
        }

        public static void Save(UserSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            var dir = Path.GetDirectoryName(SettingsPath);
            if (!string.IsNullOrWhiteSpace(dir))
                Directory.CreateDirectory(dir);

            var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(SettingsPath, json);
        }
    }
}
