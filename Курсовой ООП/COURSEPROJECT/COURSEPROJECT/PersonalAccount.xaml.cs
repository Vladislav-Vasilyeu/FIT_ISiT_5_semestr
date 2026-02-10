using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static System.Net.Mime.MediaTypeNames;
using System.Diagnostics;

namespace COURSEPROJECT
{
    /// <summary>
    /// Логика взаимодействия для PersonalAccount.xaml
    /// </summary>
    public partial class PersonalAccount : Window
    {
        public ICommand SaveInfoCommand { get; }

        // сохраняем делегат для корректной отписки
        private readonly Action _languageChangedHandler;

        public PersonalAccount()
        {
            InitializeComponent();
            DataContext = this;
            SaveInfoCommand = new RelayCommand(SaveInfo);
            try
            {
                this.Cursor = new Cursor(System.Windows.Application.GetResourceStream(new Uri("/images/cursor.cur", UriKind.Relative)).Stream);
            }
            catch (Exception ex)
            {
                MessageBox.Show(Lang.lang == "en"
                   ? $"Cursor loading error: {ex.Message}"
                   : $"Ошибка загрузки курсора: {ex.Message}");
            }
            _ = GenerateInfoAsync();
            if (Global.CurrentUser == "admin")
            {
                GetStat.Visibility = Visibility.Visible;
            }

            _languageChangedHandler = async () => await GenerateInfoAsync();
            Lang.LanguageChanged += _languageChangedHandler;
            this.Closed += (s, e) => Lang.LanguageChanged -= _languageChangedHandler;
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

        // Асинхронная версия GenerateInfo
        public async Task GenerateInfoAsync()
        {
            try
            {
                List<string> info = database.GetInfoAboutUser(Global.CurrentUser);
                LoginText.Text = info.Count > 1 ? info[1] : string.Empty;

                var jsonList = await database.ReadElementsInTableOrdersAsync();
                if (int.TryParse(info.FirstOrDefault(), out int userId))
                {
                    jsonList = jsonList.Where(x => x.UserId == userId).ToList();
                }
                else
                {
                    jsonList = new List<Order>();
                }

                int countActive = 0;
                int totalMinutes = 0;

                // Обновляем UI на UI-потоке
                await Dispatcher.InvokeAsync(async () =>
                {
                    itemsuser.Children.Clear();
                    foreach (Order item in jsonList)
                    {
                        if (item.Status == "active")
                        {
                            WrapPanel wrap = new WrapPanel();
                            TextBlock text = new TextBlock();
                            text.TextWrapping = TextWrapping.Wrap;
                            text.Text = Lang.lang == "en"
                                ? $"Computer: {item.ComputerId}, Date: {item.DateOrder}, Status: {item.Status}, Time: {item.StartTime} - {item.EndTime}"
                                : $"Компьютер: {item.ComputerId}, Дата: {item.DateOrder}, Статус: {item.Status}, Время: {item.StartTime} - {item.EndTime}";

                            wrap.Children.Add(text);
                            Button btndel = new Button();
                            btndel.Content = Lang.lang == "en" ? "Delete record" : "Удалить запись";

                            btndel.Click += async (sender, args) =>
                            {
                                if (!IsTimeInRange(TimeSpan.Parse(item.StartTime), TimeSpan.Parse(item.EndTime)))
                                {
                                    if (TimeSpan.TryParse(item.StartTime, out TimeSpan start) &&
                                        TimeSpan.TryParse(item.EndTime, out TimeSpan end))
                                    {
                                        var comps = await database.ReadElementsInTableComputersAsync();
                                        Computer comp = comps.FirstOrDefault(x => x.Id == item.ComputerId);
                                        double totalCost = (end - start).TotalHours * comp.PricePerHour;
                                        double roundedCost = Math.Round(totalCost, 2);

                                        string currency = Lang.lang == "en" ? " byn." : " руб.";
                                        string message = roundedCost >= 0
                                            ? $"{roundedCost}{currency} {(Lang.lang == "en" ? "returned for order" : "вернулось за заказ")}"
                                            : $"0{currency} {(Lang.lang == "en" ? "returned for order" : "вернулось за заказ")}.";

                                        MessageBox.Show(message);
                                    }
                                    else
                                    {
                                        string currency = Lang.lang == "en" ? " byn." : " руб.";
                                        MessageBox.Show($"0{currency} {(Lang.lang == "en" ? "returned for order" : "вернулось за заказ")}.");
                                    }

                                    item.Status = "cancelled";
                                    await database.UpdateElementInTableOrders(item.Id, item.UserId, item.ComputerId, item.Status, item.DateOrder, item.StartTime, item.EndTime);
                                    // Перегенерируем UI
                                    await GenerateInfoAsync();
                                }
                                else
                                {
                                    MessageBox.Show(Lang.lang == "en"
                                        ? "Cannot cancel an order that has already started"
                                        : "Нельзя отменить заказ, время которого уже наступило");
                                }
                            };

                            wrap.Children.Add(btndel);
                            itemsuser.Children.Add(wrap);
                        }

                        if (item.Status == "completed")
                        {
                            countActive++;
                            if (TimeSpan.TryParse(item.StartTime, out TimeSpan startTime) &&
                                TimeSpan.TryParse(item.EndTime, out TimeSpan endTime))
                            {
                                totalMinutes += (int)(endTime - startTime).TotalMinutes;
                            }
                        }
                    }

                    CountOrdersActive.Text = countActive.ToString();
                    allTime.Text = totalMinutes.ToString();
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine("GenerateInfoAsync error: " + ex.Message);
            }
        }

        public void SaveInfo(object parametr)
        {
            if (string.IsNullOrWhiteSpace(Password.Password))
            {
                MessageBox.Show(Lang.lang == "en"
                    ? "Password cannot be empty"
                    : "Пароль не может быть пустым");
                return;
            }

            if (Password.Password.Length < 8 || Password.Password.Length > 100)
            {
                MessageBox.Show(Lang.lang == "en"
                    ? "Password must be between 8 and 99 characters"
                    : "Пароль должен содержать от 8 до 99 символов");
                return;
            }

            database.SaveChanges(LoginText.Text, Password.Password);
            this.Close();
        }

        private async void GenerateStatsAdmin(object sender, EventArgs ev)
        {
            if (Global.CurrentUser == "admin")
            {
                try
                {
                    var usersTask = database.ReadElementsInTableUsersAsync();
                    var ordersTask = database.ReadElementsInTableOrdersAsync();
                    var reviewsTask = database.ReadElementsInTableReviewsAsync();

                    await Task.WhenAll(usersTask, ordersTask, reviewsTask);

                    int allusers = usersTask.Result.Count;
                    int allorders = ordersTask.Result.Count;
                    int allreviews = reviewsTask.Result.Count;

                    string message = Lang.lang == "en"
                        ? $"=== System Statistics ===\n" +
                          $"Total users: {allusers}\n" +
                          $"Total orders: {allorders}\n" +
                          $"Total reviews: {allreviews}"
                        : $"=== Статистика системы ===\n" +
                          $"Всего пользователей: {allusers}\n" +
                          $"Всего заказов: {allorders}\n" +
                          $"Всего отзывов: {allreviews}";

                    MessageBox.Show(message,
                        Lang.lang == "en" ? "System Statistics" : "Статистика системы",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(Lang.lang == "en"
                        ? $"Error generating statistics: {ex.Message}"
                        : $"Ошибка при формировании статистики: {ex.Message}",
                        Lang.lang == "en" ? "Error" : "Ошибка",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }
    }
}
