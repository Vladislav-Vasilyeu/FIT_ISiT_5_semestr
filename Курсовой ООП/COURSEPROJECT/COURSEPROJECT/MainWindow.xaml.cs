using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Text.Json.Serialization;
using System.Runtime.CompilerServices;
using System.ComponentModel;
using System.Windows.Threading;
using System.Diagnostics;
using MaterialDesignThemes.Wpf;
using System.Net.Http;
using System.Web;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace COURSEPROJECT
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;

        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }

    public static class LaunchedGames
    {
        public static string Id;
        public static List<string> PathsGames = new List<string>();
    }

    public static class Global
    {
        public static string CurrentUser = Lang.lang == "en" ? "User" : "Пользователь";
    };

    public partial class MainWindow : Window
    {
        public DispatcherTimer timer;
        public DispatcherTimer timerupd;

        public ICommand OpenAuthorizationCommand { get; }
        public ICommand OpenEditingCommand { get; }
        public ICommand OpenFilteringCommand { get; }
        public ICommand OpenAddCommand { get; }
        public ICommand ReseachPCCommand { get; }
        public ICommand ChangeThemeBtnCommand { get; }
        public ICommand OpenPersonalCommand { get; }
        public ICommand OpenMapbookingCommand { get; }
        public ICommand OpenGamesCommand { get; }
        public ICommand OpenAdminTableCommand { get; }

        // для debounce поиска
        private CancellationTokenSource _searchCts;

        public MainWindow()
        {
            InitializeComponent();

            DataContext = this;

            OpenAuthorizationCommand = new RelayCommand(OpenAuthorization);
            ReseachPCCommand = new RelayCommand(ReseachPC);
            ChangeThemeBtnCommand = new RelayCommand(ChangeThemeBtn);
            OpenPersonalCommand = new RelayCommand(OpenPersonal);
            OpenMapbookingCommand = new RelayCommand(OpenMapbooking);
            OpenGamesCommand = new RelayCommand(OpenGames);
            OpenAdminTableCommand = new RelayCommand(OpenAdminTable);

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

            // первоначальная загрузка
            generatePCfromFile();

            // Таймер для лёгких UI-обновлений (не частый)
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1); // уменьшил частоту (бывшие 0.1s слишком часто)
            timer.Tick += Timer_Tick;
            timer.Start();

            // Таймер обновления рейтингов — выполняем реже и в фоне
            timerupd = new DispatcherTimer();
            timerupd.Interval = TimeSpan.FromMinutes(1); // выполняется раз в минуту
            timerupd.Tick += async (s, e) => await UpdateAllComputersRatingAsync();
            timerupd.Start();

            DependencyPropertyDescriptor descriptor =
                DependencyPropertyDescriptor.FromProperty(TextBlock.TextProperty, typeof(TextBlock));

            descriptor.AddValueChanged(UserName, (sender, args) =>
            {
                if (UserName.Text != "Пользователь" && UserName.Text != "User")
                {
                    UserNameView.Text = UserName.Text;
                }
            });

            // Подписка на изменение текста поиска — debounce
            if (ResearchPCInformation != null)
            {
                ResearchPCInformation.TextChanged += ResearchPCInformation_TextChanged;
            }

            Music.PlayMusic();
            CommandBindings.Add
                (new CommandBinding(CustomCommand.OpenProfileWindow, OpenPersonalEvent));
            CommandBindings.Add
                (new CommandBinding(CustomCommand.SwitchTheme, ChangeThemeBtnEvent));
            this.AddHandler(ComputerClickedEvent, new RoutedEventHandler(OnComputerClicked));
            this.Closing += MainWindow_Closing;

            
        }

        [DllImport("Dwmapi.dll")]
        public static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        public static readonly RoutedEvent ComputerClickedEvent =
            EventManager.RegisterRoutedEvent(
                "ComputerClicked",
                RoutingStrategy.Bubble,
                typeof(RoutedEventHandler),
                typeof(MainWindow));

        public event RoutedEventHandler ComputerClicked
        {
            add => AddHandler(ComputerClickedEvent, value);
            remove => RemoveHandler(ComputerClickedEvent, value);
        }

        private void ChangeThemeBtn(object parametr)
        {
            var currentTheme = Properties.Settings.Default.Theme;
            var newTheme = currentTheme == "StyleTheme1" ? "StyleTheme2" : "StyleTheme1";
            ThemeManager.ApplyTheme(newTheme);
        }
        private void ChangeThemeBtnEvent(object sender, EventArgs e)
        {
            var currentTheme = Properties.Settings.Default.Theme;
            var newTheme = currentTheme == "StyleTheme1" ? "StyleTheme2" : "StyleTheme1";
            ThemeManager.ApplyTheme(newTheme);
        }

        // Асинхронная безопасная переработка обновления рейтингов
        public async Task UpdateAllComputersRatingAsync()
        {
            try
            {
                // Снимем тяжёлую агрегацию на фон
                var computerRatings = await Task.Run(() =>
                {
                    List<Reviews> allReviews = database.ReadElementsInTableReviews();
                    var grouped = allReviews
                        .GroupBy(r => r.ComputerId)
                        .Select(g => new
                        {
                            ComputerId = g.Key,
                            AverageRating = (float)Math.Round(g.Average(r => r.Rating), 2),
                            ReviewCount = g.Count()
                        })
                        .ToList();
                    return grouped;
                });

                // Получаем список компьютеров один раз в фоне
                var allComputers = await Task.Run(() => database.ReadElementsInTableComputers());

                // Выполняем обновления БД в фоне последовательно (можно улучшить батчем)
                foreach (var computer in allComputers)
                {
                    var ratingInfo = computerRatings.FirstOrDefault(cr => cr.ComputerId == computer.Id);
                    float newRating = ratingInfo?.AverageRating ?? 0f;

                    // Используем существующий асинхронный метод обновления
                    await database.UpdateElementInTableComputers(
                        computer.Id,
                        computer.Name,
                        computer.Description,
                        computer.Cpu,
                        computer.Gpu,
                        computer.Ram,
                        computer.Storage,
                        computer.Monitor,
                        newRating,
                        (float)computer.PricePerHour,
                        computer.GraphicData);
                }
            }
            catch (Exception ex)
            {
                // Логируем в Output, показывать MessageBox слишком агрессивно для периодической задачи
                Debug.WriteLine("UpdateAllComputersRatingAsync error: " + ex.Message);
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            // лёгкие операции — не дергаем БД здесь напрямую
            if (UserName.Text != Global.CurrentUser)
            {
                UserName.Text = Global.CurrentUser;
            }
            IsAdmin();
            isRegUser();

            // Проверка статусов заказов делается асинхронно
            _ = CheckOrdersAndNotifyAsync(); // fire-and-forget — ошибки логируются внутри
        }

        private void IsAdmin()
        {
            if (Global.CurrentUser == "admin")
            {
                AdminDBTable.Visibility = Visibility.Visible;
            }
            else
            {
                AdminDBTable.Visibility = Visibility.Collapsed;
            }
        }
        private void isRegUser()
        {
            if (Global.CurrentUser == "Пользователь" || Global.CurrentUser == "User")
            {
                UserBooking.Visibility = Visibility.Collapsed;
                ToolPersonal.Visibility = Visibility.Collapsed;
            }
            else
            {
                UserBooking.Visibility = Visibility.Visible;
                ToolPersonal.Visibility = Visibility.Visible;
            }
        }

        // Асинхронно получить заказы и выполнить UI-операции корректно на UI-потоке
        private async Task CheckOrdersAndNotifyAsync()
        {
            try
            {
                var orders = await Task.Run(() => database.ReadElementsInTableOrders());
                DateTime now = DateTime.Now;

                // Перенести минимальную логику на UI-поток, чтобы безопасно создавать окна/MessageBox
                await Dispatcher.InvokeAsync(() =>
                {
                    foreach (var item in orders)
                    {
                        if (item.Status == "active" && DateTime.TryParse($"{item.DateOrder} {item.EndTime}", out DateTime endDateTime))
                        {
                            DateTime.TryParse(item.StartTime, out DateTime startTime);
                            DateTime.TryParse(item.EndTime, out DateTime endTime);
                            bool isCurrentUserBooking = Global.CurrentUser == database.GetUserNameById(item.UserId);

                            if (DateTime.TryParse(item.DateOrder, out DateTime rd) &&
                                TimeSpan.TryParse(item.StartTime, out TimeSpan st) &&
                                TimeSpan.TryParse(item.EndTime, out TimeSpan et))
                            {
                                DateTime sdt = rd.Date + st;
                                DateTime edt = rd.Date + et;

                                bool isc = Global.CurrentUser == database.GetUserNameById(item.UserId);
                                if (isc && now >= sdt && now <= sdt.AddSeconds(2))
                                {
                                    if (App.LaunchingGamesWindow != null)
                                    {
                                        App.LaunchingGamesWindow.Close();
                                        App.LaunchingGamesWindow = null;
                                    }
                                }
                            }

                            if (now > endDateTime)
                            {
                                item.Status = "completed";
                                var review = Application.Current.Windows.OfType<WritingReview>().FirstOrDefault();
                                if (isCurrentUserBooking)
                                {
                                    if (review == null)
                                    {
                                        review = new WritingReview(item);
                                        review.Show();
                                    }
                                    else
                                    {
                                        review.Activate();
                                        if (review.WindowState == WindowState.Minimized)
                                            review.WindowState = WindowState.Normal;
                                    }
                                    // Обновление статуса в БД выполняем асинхронно (fire-and-forget)
                                    _ = database.UpdateElementInTableOrders(item.Id, item.UserId, item.ComputerId, item.Status, item.DateOrder, item.StartTime, item.EndTime);
                                }
                            }

                            if (now > endDateTime && Global.CurrentUser == database.GetUserNameById(item.UserId))
                            {
                                item.Status = "completed";

                                // Корректное завершение процессов лучше вынести отдельно; здесь - минимальная логика
                                if (!string.IsNullOrEmpty(LaunchedGames.Id))
                                {
                                    int launchedGameId;
                                    if (int.TryParse(LaunchedGames.Id, out launchedGameId))
                                    {
                                        foreach (string expectedPath in LaunchedGames.PathsGames)
                                        {
                                            foreach (Process proc in Process.GetProcessesByName(System.IO.Path.GetFileNameWithoutExtension(expectedPath)))
                                            {
                                                try
                                                {
                                                    // сначала мягкое завершение
                                                    try
                                                    {
                                                        proc.CloseMainWindow();
                                                        if (!proc.WaitForExit(2000))
                                                        {
                                                            proc.Kill();
                                                            proc.WaitForExit();
                                                        }
                                                    }
                                                    catch
                                                    {
                                                        proc.Kill();
                                                        proc.WaitForExit();
                                                    }
                                                }
                                                catch (Exception ex)
                                                {
                                                    Debug.WriteLine(Lang.lang == "en"
                                                        ? $"Error terminating process: {ex.Message}"
                                                        : $"Ошибка при завершении процесса: {ex.Message}");
                                                }
                                            }
                                        }
                                    }
                                }

                                if (App.LaunchingGamesWindow != null)
                                {
                                    App.LaunchingGamesWindow.Close();
                                    App.LaunchingGamesWindow = null;
                                }

                                var review = Application.Current.Windows.OfType<WritingReview>().FirstOrDefault();
                                if (isCurrentUserBooking)
                                {
                                    if (review == null)
                                    {
                                        review = new WritingReview(item);
                                        review.Show();
                                    }
                                    else
                                    {
                                        review.Activate();
                                        if (review.WindowState == WindowState.Minimized)
                                            review.WindowState = WindowState.Normal;
                                    }
                                    _ = database.UpdateElementInTableOrders(item.Id, item.UserId, item.ComputerId, item.Status, item.DateOrder, item.StartTime, item.EndTime);
                                }
                            }
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine("CheckOrdersAndNotifyAsync error: " + ex.Message);
            }
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            CloseAllGames();
        }

        private void CloseAllGames()
        {
            if (!string.IsNullOrEmpty(LaunchedGames.Id))
            {
                int launchedGameId;
                if (int.TryParse(LaunchedGames.Id, out launchedGameId))
                {
                    foreach (string expectedPath in LaunchedGames.PathsGames)
                    {
                        foreach (Process proc in Process.GetProcessesByName(System.IO.Path.GetFileNameWithoutExtension(expectedPath)))
                        {
                            try
                            {
                                // сначала попытаться корректно закрыть
                                try
                                {
                                    proc.CloseMainWindow();
                                    if (!proc.WaitForExit(2000))
                                    {
                                        proc.Kill();
                                        proc.WaitForExit();
                                    }
                                }
                                catch
                                {
                                    proc.Kill();
                                    proc.WaitForExit();
                                }
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine(Lang.lang == "en"
                                    ? $"Process termination error: {ex.Message}"
                                    : $"Ошибка при завершении процесса: {ex.Message}");
                            }
                        }
                    }
                }
            }
        }

        private void OpenAuthorization(object parameter)
        {
            var window = Application.Current.Windows.OfType<authorization>().FirstOrDefault();

            if (window == null)
            {
                window = new authorization();
                window.Show();
            }
            else
            {
                window.Activate();
                if (window.WindowState == WindowState.Minimized)
                    window.WindowState = WindowState.Normal;
            }
        }
        private void OpenPersonal(object parameter)
        {
            if (Global.CurrentUser != "Пользователь" && Global.CurrentUser != "User")
            {

                var window = Application.Current.Windows.OfType<PersonalAccount>().FirstOrDefault();

                if (window == null)
                {
                    window = new PersonalAccount();
                    window.Show();
                }
                else
                {
                    window.Activate();
                    if (window.WindowState == WindowState.Minimized)
                        window.WindowState = WindowState.Normal;
                }
            }
        }
        private void OpenPersonalEvent(object sender, EventArgs e)
        {
            if (Global.CurrentUser != "Пользователь" && Global.CurrentUser != "User")
            {
                var window = Application.Current.Windows.OfType<PersonalAccount>().FirstOrDefault();

                if (window == null)
                {
                    window = new PersonalAccount();
                    window.Show();
                }
                else
                {
                    window.Activate();
                    if (window.WindowState == WindowState.Minimized)
                        window.WindowState = WindowState.Normal;
                }
            }
        }
        private void OpenMapbooking(object parameter)
        {
            var window = Application.Current.Windows.OfType<mapbooking>().FirstOrDefault();

            if (window == null)
            {
                window = new mapbooking();
                window.Show();
            }
            else
            {
                window.Activate();
                if (window.WindowState == WindowState.Minimized)
                    window.WindowState = WindowState.Normal;
            }
        }
        private void OpenGames(object parameter)
        {
            var window = Application.Current.Windows.OfType<LaunchingGames>().FirstOrDefault();

            if (window == null)
            {
                window = new LaunchingGames();
                App.LaunchingGamesWindow = window;
                window.Show();
            }
            else
            {
                window.Activate();
                if (window.WindowState == WindowState.Minimized)
                    window.WindowState = WindowState.Normal;
            }
        }
        private void OpenAdminTable(object parameter)
        {
            var window = Application.Current.Windows.OfType<AdminTable>().FirstOrDefault();

            if (window == null)
            {
                window = new AdminTable();
                window.Show();
            }
            else
            {
                window.Activate();
                if (window.WindowState == WindowState.Minimized)
                    window.WindowState = WindowState.Normal;
            }
        }

        private void ReseachPC(object parameter)
        {
            
            _ = RefreshComputersAsync();
        }

        public void ReseachPCPublic()
        {
            
            _ = RefreshComputersAsync();
        }

        
        private async Task RefreshComputersAsync()
        {
            try
            {
                string searchText = string.Empty;
                await Dispatcher.InvokeAsync(() => searchText = ResearchPCInformation?.Text?.Trim() ?? string.Empty);

                
                var list = await Task.Run(() => database.ReadElementsInTableComputers());

                var filtered = list.Where(item =>
                {
                    if (string.IsNullOrEmpty(searchText)) return true;

                    return (item.Name?.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) ?? -1) >= 0 ||
                           (item.Description?.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) ?? -1) >= 0 ||
                           (item.Cpu?.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) ?? -1) >= 0 ||
                           (item.Gpu?.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) ?? -1) >= 0 ||
                           (item.Ram?.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) ?? -1) >= 0 ||
                           (item.Storage?.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) ?? -1) >= 0 ||
                           (item.Monitor?.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) ?? -1) >= 0 ||
                           (item.Rating.ToString().IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0) ||
                           (item.PricePerHour.ToString().IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0);
                }).ToList();

                // обновляем UI на UI-потоке
                await Dispatcher.InvokeAsync(() =>
                {
                    ContainerPc.Children.Clear();
                    int column = 0;
                    int row = 0;
                    foreach (Computer item in filtered)
                    {
                        List<string> array = new List<string>
                        {
                            item.Name,
                            item.Description,
                            item.Cpu,
                            item.Gpu,
                            item.Ram,
                            item.Storage,
                            item.Monitor,
                            Math.Round(item.Rating, 2).ToString(),
                            item.PricePerHour.ToString() + (Lang.lang == "en" ? " byn." : " руб.")
                        };

                        ContainerPc.Children.Add(GenerateComputerInfoBorder(array.ToArray(), row, column));
                        if (column + 1 > 2)
                        {
                            column = 0;
                            row++;
                        }
                        else
                        {
                            column++;
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine("RefreshComputersAsync error: " + ex.Message);
            }
        }

        private void generatePCfromFile()
        {
            // начальная синхронная загрузка при старте; при больших объёмах лучше вызвать RefreshComputersAsync
            List<Computer> list = database.ReadElementsInTableComputers();

            int column = 0;
            int row = 0;

            foreach (Computer item in list)
            {
                List<string> array = new List<string>
                {
                    item.Name,
                    item.Description,
                    item.Cpu,
                    item.Gpu,
                    item.Ram,
                    item.Storage,
                    item.Monitor,
                    Math.Round(item.Rating, 2).ToString(),
                    item.PricePerHour.ToString() + (Lang.lang == "en" ? " byn." : " руб.")
                };

                ContainerPc.Children.Add(GenerateComputerInfoBorder(array.ToArray(), row, column));
                if (column + 1 > 2)
                {
                    column = 0;
                    row++;
                }
                else
                {
                    column++;
                }
            }
        }

        private void ResearchPCInformation_TextChanged(object sender, TextChangedEventArgs e)
        {
            // debounce: отменяем предыдущую задачу и запускаем новое обновление через 350ms
            try
            {
                _searchCts?.Cancel();
                _searchCts?.Dispose();
            }
            catch { }

            _searchCts = new CancellationTokenSource();
            var token = _searchCts.Token;

            Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(350, token);
                    if (token.IsCancellationRequested) return;
                    await RefreshComputersAsync();
                }
                catch (TaskCanceledException) { /* ожидалось */ }
                catch (Exception ex)
                {
                    Debug.WriteLine("Research debounce error: " + ex.Message);
                }
            }, token);
        }

        

        private void OnComputerClicked(object sender, RoutedEventArgs e)
        {
            if (Global.CurrentUser != "Пользователь" && Global.CurrentUser != "User")
            {
                string computerId = e.Source.ToString();
                var border = e.OriginalSource as Border;
                string computerIdFromTag = border?.Tag?.ToString();
                var window = Application.Current.Windows.OfType<mapbooking>().FirstOrDefault();

                if (window == null)
                {
                    window = new mapbooking(computerIdFromTag);
                    window.Show();
                }
                else
                {
                    window.Activate();
                    if (window.WindowState == WindowState.Minimized)
                        window.WindowState = WindowState.Normal;
                }

                e.Handled = true;
            }
        }

        public Border GenerateComputerInfoBorder(string[] texts, int row, int column)
        {
            Border border = new Border
            {
                Margin = new Thickness(10),
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#8001464b")),
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#8001464b")),
                BorderThickness = new Thickness(1),
                Cursor = Cursors.Hand,
                CornerRadius = new CornerRadius(10),
                Tag = texts[0]
            };

            border.MouseDown += (sender, e) =>
            {
                var clickedBorder = sender as Border;
                if (clickedBorder != null)
                {
                    var args = new RoutedEventArgs(ComputerClickedEvent, clickedBorder)
                    {
                        Source = clickedBorder.Tag
                    };
                    clickedBorder.RaiseEvent(args);
                    e.Handled = true;
                }
            };

            StackPanel stackPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Background = Brushes.Transparent
            };
            int counterItem = -1;
            foreach (string text in texts)
            {
                counterItem++;
                TextBlock textBlock = new TextBlock
                {
                    TextAlignment = TextAlignment.Center,
                    FontSize = 20,
                    Foreground = Brushes.White,
                    Margin = new Thickness(2),
                    TextWrapping = TextWrapping.Wrap,
                    HorizontalAlignment = HorizontalAlignment.Stretch
                };
                if (counterItem == 1 && text.ToLower().Contains("компьютер"))
                {
                    string dynamicText = Application.Current.FindResource("PC") as string;
                    string combinedText = $"{dynamicText}";
                    textBlock.Text = combinedText;
                }
                else if (counterItem == 2)
                {
                    string dynamicText = Application.Current.FindResource("cpu") as string;
                    string combinedText = $"{dynamicText}: {text}";
                    textBlock.Text = combinedText;
                }
                else if (counterItem == 3)
                {
                    string dynamicText = Application.Current.FindResource("gpu") as string;
                    string combinedText = $"{dynamicText}: {text}";
                    textBlock.Text = combinedText;
                }
                else if (counterItem == 4)
                {
                    string dynamicText = Application.Current.FindResource("ram") as string;
                    string combinedText = $"{dynamicText}: {text + (Lang.lang == "en" ? "Gb" : "Гб")}";
                    textBlock.Text = combinedText;
                }
                else if (counterItem == 5)
                {
                    string dynamicText = Application.Current.FindResource("storage") as string;
                    string combinedText = $"{dynamicText}: {text}";
                    textBlock.Text = combinedText;
                }
                else if (counterItem == 6)
                {
                    string dynamicText = Application.Current.FindResource("monitor") as string;
                    string combinedText = $"{dynamicText}: {text + (Lang.lang == "en" ? "Hz" : "Гц")}";
                    textBlock.Text = combinedText;
                }
                else if (counterItem == 7)
                {
                    string dynamicText = Application.Current.FindResource("rating") as string;
                    string combinedText = $"{dynamicText}: {Math.Round(double.Parse(text), 2)}";
                    textBlock.Text = combinedText;
                }
                else
                {
                    textBlock.Text = text;
                }
                stackPanel.Children.Add(textBlock);
            }

            border.Child = stackPanel;

            Grid.SetRow(border, row);
            Grid.SetColumn(border, column);

            return border;
        }

        private void SwitchToRussian(object sender, EventArgs e)
        {
            LocalizationManager.SwitchLanguage("ru-RU");
            Lang.lang = "ru";
        }
        private void SwitchToEnglish(object sender, EventArgs e)
        {
            LocalizationManager.SwitchLanguage("en-US");
            Lang.lang = "en";
        }
        public void SwitchMusicStatus(object sender, RoutedEventArgs e)
        {
            Music.ToggleMusic();
        }

    }

    static public class Music
    {
        static public MediaPlayer player = new MediaPlayer();
        static public bool isPlay = true;

        static public void PlayMusic()
        {
            try
            {
                try
                {
                    player.Stop();
                    player.Close();
                }
                catch { }

                string relativePath = @"sounds\music.mp3";
                string fullPath = System.IO.Path.GetFullPath(relativePath);

                if (!File.Exists(fullPath))
                {
                    MessageBox.Show(Lang.lang == "en"
                        ? $"File not found!\nLook here: {fullPath}"
                        : $"Файл не найден!\nИщите здесь: {fullPath}");
                    return;
                }

                player.MediaEnded -= LoopMusic;
                player.MediaFailed -= OnMediaFailed;

                player.Open(new Uri(fullPath, UriKind.Absolute));
                player.Volume = 0.1;
                player.MediaEnded += LoopMusic;
                player.MediaFailed += OnMediaFailed;

                player.Play();
                isPlay = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(Lang.lang == "en"
                    ? $"Initialization error: {ex.Message}"
                    : $"Ошибка инициализации: {ex.Message}");
            }
        }

        private static void LoopMusic(object sender, EventArgs e)
        {
            player.Position = TimeSpan.Zero;
            player.Play();
        }

        private static void OnMediaFailed(object sender, ExceptionEventArgs e)
        {
            MessageBox.Show(Lang.lang == "en"
                ? $"Media error: {e.ErrorException?.Message ?? "Unknown"}"
                : $"Ошибка медиа: {e.ErrorException?.Message ?? "Неизвестная ошибка"}");
        }

        static public void ToggleMusic()
        {
            if (isPlay)
            {
                player.Stop();
                isPlay = false;
            }
            else
            {
                player.Play();
                isPlay = true;
            }
        }
        static public void SetValue(int value)
        {
            player.Volume = value / 100f;
        }
    }

}
