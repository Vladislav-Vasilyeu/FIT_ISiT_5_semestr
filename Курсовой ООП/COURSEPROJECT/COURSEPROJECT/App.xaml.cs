using System;
using System.Linq;
using System.Windows;

namespace COURSEPROJECT
{
    public static class Lang
    {
        public static string _lang = "";
        public static event Action LanguageChanged; 

        public static string lang
        {
            get => _lang;
            set
            {
                if (_lang != value)
                {
                    _lang = value;
                    LanguageChanged?.Invoke();
                }
            }
        }
    }
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var settings = COURSEPROJECT.Properties.Settings.Default;

            string theme = !string.IsNullOrEmpty(settings.Theme) ? settings.Theme : "StyleTheme1";
            ThemeManager.ApplyTheme(theme, false); 
            string culture = !string.IsNullOrEmpty(settings.Strings) ? settings.Strings : "en-US";
            LocalizationManager.SwitchLanguage(culture, false);
            Lang.lang = culture.Substring(0, 2);
            database.InitDataBase();
           
        }
        public static void CloseAllWindowsExceptMain()
        {
            var windowsToClose = Application.Current.Windows.OfType<Window>()
                                      .Where(w => w != Application.Current.MainWindow)
                                      .ToList();
            if (windowsToClose.Count == 0)
                return;

            foreach (Window window in windowsToClose)
            {
                window.Close();
            }
        }
        public static LaunchingGames LaunchingGamesWindow { get; set; }
    }

    public static class ThemeManager
    {
        public static void ApplyTheme(string themeName, bool saveSettings = true)
        {
            try
            {
                var app = Application.Current;
                var oldTheme = app.Resources.MergedDictionaries
                    .FirstOrDefault(d => d.Source?.ToString().Contains("styles/StyleTheme") == true);

                if (oldTheme != null)
                {
                    app.Resources.MergedDictionaries.Remove(oldTheme);
                }
                var newTheme = new ResourceDictionary
                {
                    Source = new Uri($"styles/{themeName}.xaml", UriKind.Relative)
                };
                app.Resources.MergedDictionaries.Add(newTheme);
                if (saveSettings)
                {
                    Properties.Settings.Default.Theme = themeName;
                    Properties.Settings.Default.Save();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(Lang.lang == "en" ? $"Error loading theme: {ex.Message}" : $"Ошибка загрузки темы: {ex.Message}");
            }
        }
    }

    public static class LocalizationManager
    {
        public static void SwitchLanguage(string culture, bool saveSettings = true)
        {
            try
            {
                var app = Application.Current;

                var oldLang = app.Resources.MergedDictionaries
                    .FirstOrDefault(d => d.Source?.ToString().StartsWith("Strings.") == true);

                if (oldLang != null)
                {
                    app.Resources.MergedDictionaries.Remove(oldLang);
                }

                var newLang = new ResourceDictionary
                {
                    Source = new Uri($"Strings.{culture}.xaml", UriKind.Relative)
                };
                app.Resources.MergedDictionaries.Add(newLang);

                if (saveSettings)
                {
                    Properties.Settings.Default.Strings = culture;
                    Properties.Settings.Default.Save();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(Lang.lang == "en" ? $"Error loading language: {ex.Message}" : $"Ошибка загрузки языка: {ex.Message}");
            }
        }
    }
}