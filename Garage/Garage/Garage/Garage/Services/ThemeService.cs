using System;
using System.Linq;
using System.Windows;

namespace Garage.Services
{
    public enum AppTheme
    {
        Light,
        Dark
    }

    public static class ThemeService
    {
        public static AppTheme CurrentTheme { get; private set; } = AppTheme.Light;

        public static void LoadAndApply()
        {
            var settings = UserSettingsService.Load();
            var theme = ParseTheme(settings.Theme);
            Apply(theme);
        }

        public static void Apply(AppTheme theme)
        {
            var app = Application.Current;
            if (app == null)
                return;

            var source = theme == AppTheme.Dark
                ? new Uri("Themes/DarkTheme.xaml", UriKind.Relative)
                : new Uri("Themes/LightTheme.xaml", UriKind.Relative);

            var newDict = new ResourceDictionary { Source = source };

            // Remplace le dictionnaire de thème (LightTheme/DarkTheme) dans les MergedDictionaries.
            var dictionaries = app.Resources.MergedDictionaries;
            var existingThemeDict = dictionaries.FirstOrDefault(d =>
                d.Source != null && (
                    d.Source.OriginalString.Contains("Themes/LightTheme.xaml") ||
                    d.Source.OriginalString.Contains("Themes/DarkTheme.xaml")));

            if (existingThemeDict != null)
                dictionaries.Remove(existingThemeDict);

            dictionaries.Insert(0, newDict);

            CurrentTheme = theme;

            // Persistance
            var settings = UserSettingsService.Load();
            settings.Theme = theme.ToString();
            UserSettingsService.Save(settings);
        }

        private static AppTheme ParseTheme(string value)
        {
            if (string.Equals(value, "Dark", StringComparison.OrdinalIgnoreCase))
                return AppTheme.Dark;

            return AppTheme.Light;
        }
    }
}
