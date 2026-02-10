using Microsoft.Xaml.Behaviors.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static MaterialDesignThemes.Wpf.Theme.ToolBar;
using System.Text.RegularExpressions;
using System.IO.Compression;
using System.Windows.Controls.Primitives;
using System.Diagnostics;
using System.IO.Compression;

namespace COURSEPROJECT
{
    public partial class LaunchingGames : Window
    {
        public LaunchingGames()
        {
            InitializeComponent();
            try
            {
                this.Cursor = new Cursor(Application.GetResourceStream(new Uri("/images/cursor.cur", UriKind.Relative)).Stream);
            }
            catch (Exception ex)
            {
                MessageBox.Show(Lang.lang == "en"
                   ? $"Cursor loading error: {ex.Message}"
                   : $"Ошибка загрузки курсора: {ex.Message}");
            }
            _ = GeneratePCFromFileAsync();
        }
        private int rows = 0;
        private int columns = 0;
        private void AnimatedGif_Loaded(object sender, RoutedEventArgs e)
        {
            var animation = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.5));
            AnimatedGif.BeginAnimation(OpacityProperty, animation);
        }

        public async Task<Border> GenerateInformation(string[] texts, int currentRow, int currentColumn)
        {
            Border border = new Border
            {
                Margin = new Thickness(10),
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#8001464b")),
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#8001464b")),
                CornerRadius = new CornerRadius(10),
            };

            Grid grid = new Grid
            {
                Background = Brushes.Transparent
            };

            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            string title = texts[1];
            string img = texts[2];
            string description = texts[3];
            string link = texts[4];

            StackPanel infoPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Margin = new Thickness(0, 0, 0, 10)
            };

            TextBlock titleBlock = new TextBlock
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                Text = title,
                Margin = new Thickness(0, 10, 0, 0),
                FontWeight = FontWeights.Bold,
                FontSize = 24,
                Foreground = Brushes.White,
                TextWrapping = TextWrapping.Wrap
            };

            Border imageBorder = new Border
            {
                CornerRadius = new CornerRadius(10),
                Width = 200,
                Height = 200,
                ClipToBounds = true,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 5, 0, 0),
                Child = new Image
                {
                    Stretch = Stretch.UniformToFill,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                }
            };

            try
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(img, UriKind.RelativeOrAbsolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();

                if (bitmap.Width > 1)
                {
                    ((Image)imageBorder.Child).Source = bitmap;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(Lang.lang == "en"
                    ? $"Image loading error: {ex.Message}"
                    : $"Ошибка загрузки изображения: {ex.Message}");
            }

            TextBlock descBlock = new TextBlock
            {
                Text = description,
                FontSize = 14,
                Foreground = Brushes.White,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 5, 0, 0),
                MaxWidth = 200
            };

            infoPanel.Children.Add(titleBlock);
            infoPanel.Children.Add(imageBorder);
            infoPanel.Children.Add(descBlock);

            Button launchButton = new Button
            {
                Content = await TranslateGoogleAsync("Запустить", Lang.lang),
                Tag = link,
                HorizontalAlignment = HorizontalAlignment.Center,
                Padding = new Thickness(10, 5, 10, 5),
                Margin = new Thickness(0, 0, 0, 10),
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF007ACC")),
                Foreground = Brushes.White,
                Cursor = Cursors.Hand,
                Width = 200,
                BorderThickness = new Thickness(0)
            };

            launchButton.Click += async (sender, e) =>
{
    try
    {
        // 1) проверка авторизации
        if (Global.CurrentUser == "Пользователь" || Global.CurrentUser == "User")
        {
            MessageBox.Show(Lang.lang == "en"
                ? "Please authorize to launch games."
                : "Пожалуйста, авторизуйтесь, чтобы запускать игры.");
            return;
        }

        // 2) проверка разрешения играть сейчас (активное бронирование)
        bool canPlay = await UserCanPlayAsync(Global.CurrentUser);

        // Разрешаем администратору запускать игры даже без активного бронирования
        if (!canPlay && !Global.CurrentUser.Equals("admin", StringComparison.OrdinalIgnoreCase))
        {
            MessageBox.Show(Lang.lang == "en"
                ? "You don't have an active booking right now."
                : "У вас сейчас нет активного бронирования для запуска игры.");
            return;
        }

        if (!(sender is Button button) || !(button.Tag is string url) || string.IsNullOrWhiteSpace(url))
        {
            MessageBox.Show(Lang.lang == "en"
                ? "Game path is missing."
                : "Путь к игре не указан.");
            return;
        }

        // 3) Попытка запустить: если это URL -> открыть в браузере; иначе — проверить файл
        try
        {
            // абсолютный путь (если относительный)
            string pathCandidate = url;
            string fullPath = pathCandidate;
            try
            {
                fullPath = System.IO.Path.GetFullPath(pathCandidate);
            }
            catch { /* оставляем как есть если Path.GetFullPath бросит */ }

            if (Uri.IsWellFormedUriString(url, UriKind.Absolute))
            {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                LaunchedGames.PathsGames.Add(url);
                LaunchedGames.Id = await UserProcessPlayAsync(Global.CurrentUser);
                return;
            }

            if (!File.Exists(fullPath))
            {
                MessageBox.Show(Lang.lang == "en"
                    ? $"File not found: {fullPath}"
                    : $"Файл не найден: {fullPath}");
                return;
            }

            var psi = new ProcessStartInfo(fullPath)
            {
                UseShellExecute = true,
                WorkingDirectory = System.IO.Path.GetDirectoryName(fullPath)
            };

            Process proc = Process.Start(psi);
            if (proc != null)
            {
                LaunchedGames.PathsGames.Add(fullPath);
                LaunchedGames.Id = await UserProcessPlayAsync(Global.CurrentUser);
            }
            else
            {
                MessageBox.Show(Lang.lang == "en"
                    ? "Failed to start process."
                    : "Не удалось запустить процесс.");
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(Lang.lang == "en"
                ? $"Error starting game: {ex.Message}"
                : $"Ошибка при запуске игры: {ex.Message}");
        }
    }
    catch (Exception ex)
    {
        Debug.WriteLine("Launch button click error: " + ex.Message);
    }
};

            Grid.SetRow(infoPanel, 0);
            Grid.SetRow(launchButton, 1);

            grid.Children.Add(infoPanel);
            grid.Children.Add(launchButton);

            border.Child = grid;

            return border;
        }

        private void SearchClick(object sender, EventArgs e)
        {
            _ = GeneratePCFromFileAsync();
        }

        private async Task<bool> UserCanPlayAsync(string name)
        {
            try
            {
                var userid = database.GetUserIdByName(Global.CurrentUser);
                if (!int.TryParse(userid, out int uid)) return false;

                var orders = await database.ReadElementsInTableOrdersAsync();
                foreach (Order item in orders)
                {
                    if (item.UserId == uid && item.Status == "active")
                    {
                        if (DateTime.TryParse($"{item.DateOrder} {item.StartTime}", out DateTime startDateTime) &&
                            DateTime.TryParse($"{item.DateOrder} {item.EndTime}", out DateTime endDateTime))
                        {
                            DateTime now = DateTime.Now;
                            if (now >= startDateTime && now <= endDateTime)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("UserCanPlayAsync error: " + ex.Message);
            }
            return false;
        }

        private async Task<string> UserProcessPlayAsync(string name)
        {
            try
            {
                var userid = database.GetUserIdByName(Global.CurrentUser);
                if (!int.TryParse(userid, out int userId)) return "";

                var orders = await database.ReadElementsInTableOrdersAsync();
                foreach (Order item in orders)
                {
                    if (item.UserId == userId && item.Status == "active")
                    {
                        if (!string.IsNullOrEmpty(item.DateOrder) &&
                            !string.IsNullOrEmpty(item.StartTime) &&
                            !string.IsNullOrEmpty(item.EndTime))
                        {
                            if (DateTime.TryParse($"{item.DateOrder} {item.StartTime}", out DateTime startDateTime) &&
                                DateTime.TryParse($"{item.DateOrder} {item.EndTime}", out DateTime endDateTime))
                            {
                                DateTime now = DateTime.Now;
                                if (now >= startDateTime && now <= endDateTime)
                                {
                                    return item.Id.ToString();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("UserProcessPlayAsync error: " + ex.Message);
            }
            return "";
        }

        public async Task<string> TranslateGoogleAsync(string text, string toLang = "ru")
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            try
            {
                using (var client = new WebClient())
                {
                    client.Headers.Add("User-Agent", "Mozilla/5.0");
                    client.Encoding = Encoding.UTF8;
                    client.Proxy = null;
                    client.Headers.Add("Accept-Encoding", "gzip");

                    string url = "https://translate.googleapis.com/translate_a/single?" +
                               $"client=gtx&sl=auto&tl={toLang}&dt=t&q={Uri.EscapeDataString(text)}";

                    byte[] responseData = await client.DownloadDataTaskAsync(url);

                    if (responseData.Length > 1 && responseData[0] == 0x1F && responseData[1] == 0x8B)
                    {
                        using (var stream = new MemoryStream(responseData))
                        using (var gzip = new GZipStream(stream, CompressionMode.Decompress))
                        using (var reader = new StreamReader(gzip))
                        {
                            string response = reader.ReadToEnd();
                            return ParseTranslation(response);
                        }
                    }
                    else
                    {
                        string response = Encoding.UTF8.GetString(responseData);
                        return ParseTranslation(response);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("TranslateGoogleAsync error: " + ex.Message);
                return text;
            }
        }

        private bool IsGameOrApp(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return false;

            try
            {
                string lower = url.ToLowerInvariant();

                // 1) Локальные файлы/ярлыки/исполняемые файлы
                string[] execExt = { ".exe", ".bat", ".lnk", ".msi", ".appx" };
                if (execExt.Any(ext => lower.EndsWith(ext)))
                    return true;

                // 2) Явное упоминание слова "game"/"игра" в имени или пути
                if (lower.Contains("game") || lower.Contains("игра") || lower.Contains("игр"))
                    return true;

                // 3) URL-хосты известных игровых площадок или пути, содержащие game/play
                if (Uri.IsWellFormedUriString(url, UriKind.Absolute))
                {
                    var uri = new Uri(url);
                    string host = uri.Host.ToLowerInvariant();
                    string path = uri.AbsolutePath.ToLowerInvariant();

                    string[] gamingHosts = { "store.steampowered.com", "steamcommunity.com", "epicgames.com", "gog.com", "itch.io", "humblebundle.com", "origin.com", "uplay.com" };
                    if (gamingHosts.Any(h => host.Contains(h) || host.EndsWith(h)))
                        return true;

                    if (path.Contains("/game") || path.Contains("/games") || path.Contains("/play"))
                        return true;

                    // если в конце URL имя файла с расширением
                    string fileName = System.IO.Path.GetFileName(path);
                    if (!string.IsNullOrEmpty(fileName) && execExt.Any(ext => fileName.EndsWith(ext)))
                        return true;
                }
            }
            catch
            {
                // в случае исключения считаем, что это не игра (без краха)
            }

            return false;
        }
        private static readonly string[] GamingGpuKeywords = { "RTX", "GTX", "RX", "Radeon", "GeForce" };

        private async Task<bool> IsUserHaveNoActiveAsync()
        {
            try
            {
                var userIdStr = database.GetUserIdByName(Global.CurrentUser);
                if (string.IsNullOrEmpty(userIdStr)) return true;

                var today = DateTime.Today.ToString("dd.MM.yyyy");

                var orders = await database.ReadElementsInTableOrdersAsync();
                var activeOrder = orders.FirstOrDefault(o =>
                    o.UserId == int.Parse(userIdStr) &&
                    o.Status == "active" &&
                    o.DateOrder == today &&
                    TimeSpan.TryParse(o.StartTime, out TimeSpan s) &&
                    TimeSpan.TryParse(o.EndTime, out TimeSpan en) &&
                    IsTimeInRange(s, en));

                return activeOrder == null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("IsUserHaveNoActiveAsync error: " + ex.Message);
                return true;
            }
        }

        private async Task<bool> IsGamingComputerAsync(string userName)
        {
            try
            {
                var userId = database.GetUserIdByName(userName);
                if (string.IsNullOrEmpty(userId)) return false;

                var today = DateTime.Today.ToString("dd.MM.yyyy");

                var orders = await database.ReadElementsInTableOrdersAsync();
                var activeOrder = orders.FirstOrDefault(o =>
                    o.UserId == int.Parse(userId) &&
                    o.Status == "active" &&
                    o.DateOrder == today &&
                    TimeSpan.TryParse(o.StartTime, out TimeSpan s) &&
                    TimeSpan.TryParse(o.EndTime, out TimeSpan en) &&
                    IsTimeInRange(s, en));

                if (activeOrder == null) return false;

                var computers = await database.ReadElementsInTableComputersAsync();
                var computer = computers.FirstOrDefault(c => c.Id == activeOrder.ComputerId);
                if (computer == null) return false;

                return GamingGpuKeywords.Any(keyword =>
                    computer.Gpu.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("IsGamingComputerAsync error: " + ex.Message);
                return false;
            }
        }

        public bool IsTimeInRange(TimeSpan startTime, TimeSpan endTime)
        {
            TimeSpan currentTime = DateTime.Now.TimeOfDay;

            if (startTime <= endTime)
            {
                return currentTime >= startTime && currentTime <= endTime;
            }
            else
            {
                return currentTime >= startTime || currentTime <= endTime;
            }
        }

        private string ParseTranslation(string response)
        {
            try
            {
                int start = response.IndexOf("\"", StringComparison.Ordinal) + 1;
                int end = response.IndexOf("\"", start);
                return end > start ? response.Substring(start, end - start) : response;
            }
            catch
            {
                return response;
            }
        }

        private async Task GeneratePCFromFileAsync()
        {
            try
            {
                Items.Children.Clear();
                var list = await database.ReadElementsInTableApplicationGameAsync();
                Items.Rows = (int)Math.Ceiling((double)list.Count / 4);

                foreach (ApplicationGame item in list)
                {
                    if (Global.CurrentUser == "Пользователь" || Global.CurrentUser == "User" || await IsUserHaveNoActiveAsync())
                    {
                        string translatedDesc = await TranslateGoogleAsync(item.Description, Lang.lang);

                        List<string> array = new List<string>
                        {
                            item.Id.ToString(),
                            item.Name,
                            item.IMG,
                            translatedDesc,
                            item.URL
                        };

                        if (string.IsNullOrEmpty(ResearchPCInformation.Text))
                        {
                            var border = await GenerateInformation(array.ToArray(), rows, columns);
                            Items.Children.Add(border);
                        }
                        else if (item.Name.IndexOf(ResearchPCInformation.Text, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            var border = await GenerateInformation(array.ToArray(), rows, columns);
                            Items.Children.Add(border);
                        }
                    }
                    else
                    {
                        // Когда есть активное бронирование, показываем игры всегда
                        // Для игровых компьютеров показываем только игры, для обычных - все приложения и игры
                        bool isGamingComputer = await IsGamingComputerAsync(Global.CurrentUser);
                        bool isGame = IsGameOrApp(item.URL);
                        
                        // Если игровой компьютер - показываем только игры
                        // Если обычный компьютер - показываем все (игры и приложения)
                        if (isGamingComputer && !isGame)
                        {
                            continue; // Пропускаем не-игры на игровом компьютере
                        }
                        
                        string translatedDesc = await TranslateGoogleAsync(item.Description, Lang.lang);

                        List<string> array = new List<string>
                        {
                            item.Id.ToString(),
                            item.Name,
                            item.IMG,
                            translatedDesc,
                            item.URL
                        };

                        if (string.IsNullOrEmpty(ResearchPCInformation.Text))
                        {
                            var border = await GenerateInformation(array.ToArray(), rows, columns);
                            Items.Children.Add(border);
                        }
                        else if (item.Name.IndexOf(ResearchPCInformation.Text, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            var border = await GenerateInformation(array.ToArray(), rows, columns);
                            Items.Children.Add(border);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("GeneratePCFromFileAsync error: " + ex.Message);
            }
        }
    }
}
