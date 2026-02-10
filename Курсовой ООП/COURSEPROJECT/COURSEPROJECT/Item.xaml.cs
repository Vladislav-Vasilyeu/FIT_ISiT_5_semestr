using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Diagnostics;

namespace COURSEPROJECT
{
    public partial class Item : Window
    {
        private readonly Computer _tariff;
        public Item(Computer tariff)
        {
            InitializeComponent();
            _tariff = tariff;
            DataContext = tariff;
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
            CostChange();
            starttime.SelectionChanged += (s, e) => CostChange();
            endtime.SelectionChanged += (s, e) => CostChange();
            FormattedPrice();
            CorrectRating();
            GenerateRamAndMonitor();
            Lang.LanguageChanged += CostChange;
            this.Closed += (s, e) => Lang.LanguageChanged -= CostChange;
            Lang.LanguageChanged += FormattedPrice;
            this.Closed += (s, e) => Lang.LanguageChanged -= FormattedPrice;
            Lang.LanguageChanged += GenerateRamAndMonitor;
            this.Closed += (s, e) => Lang.LanguageChanged -= GenerateRamAndMonitor;
        }
        bool IsValidTime(string time)
        {
            return Regex.IsMatch(time, @"^(?:[01][0-9]|2[0-3]):[0-5][0-9]$");
        }

        // Поместил обработчик в async void — безопасно для UI-обработчика события
        public async void WriteInfoByItem(object sender, EventArgs e)
        {
            try
            {
                // читаем заказы асинхронно
                var items = await database.ReadElementsInTableOrdersAsync();

                // получаем id пользователя (синхронный метод оставлен в базе)
                var userIdStr = database.GetUserIdByName(Global.CurrentUser);
                if (!int.TryParse(userIdStr, out int currentUserId))
                {
                    MessageBox.Show(Lang.lang == "en" ? "User not found" : "Пользователь не найден");
                    return;
                }

                int userActiveBookings = items.Count(item =>
                    item.UserId == currentUserId &&
                    item.Status == "active");

                if (!IsValidTime(starttime.Text) || !IsValidTime(endtime.Text))
                {
                    MessageBox.Show(Lang.lang == "en" ? "Invalid time format" : "Неверный формат времени");
                    return;
                }

                if (userActiveBookings >= 3)
                {
                    MessageBox.Show(Lang.lang == "en"
                        ? "You can have no more than 3 active bookings!"
                        : "Вы можете иметь не более 3 активных бронирований!");
                    return;
                }

                if (!selectedDate.SelectedDate.HasValue)
                {
                    MessageBox.Show(Lang.lang == "en"
                        ? "Please select a date!"
                        : "Пожалуйста, выберите дату!");
                    return;
                }

                DateTime selectedDateValue = selectedDate.SelectedDate.Value.Date;
                DateTime currentDate = DateTime.Now.Date;
                DateTime maxAllowedDate = currentDate.AddDays(14);

                if (selectedDateValue < currentDate)
                {
                    MessageBox.Show(Lang.lang == "en"
                        ? "Date cannot be in the past!"
                        : "Дата не может быть в прошлом!");
                    return;
                }

                if (selectedDateValue > maxAllowedDate)
                {
                    MessageBox.Show(Lang.lang == "en"
                        ? "Booking is available only for 2 weeks in advance!"
                        : "Бронирование доступно только на 2 недели вперед!");
                    return;
                }

                if (!TimeSpan.TryParse(starttime?.Text, out TimeSpan start) ||
                    !TimeSpan.TryParse(endtime?.Text, out TimeSpan end))
                {
                    MessageBox.Show(Lang.lang == "en"
                        ? "Invalid time format!"
                        : "Некорректный формат времени!");
                    return;
                }

                if (selectedDateValue == currentDate && start <= DateTime.Now.TimeOfDay)
                {
                    MessageBox.Show(Lang.lang == "en"
                        ? "Booking start time must be later than current time!"
                        : "Время начала бронирования должно быть позже текущего времени!");
                    return;
                }

                if (start >= end)
                {
                    MessageBox.Show(Lang.lang == "en"
                        ? "Start time must be earlier than end time!"
                        : "Время начала должно быть меньше времени окончания!");
                    return;
                }

                var selectedDateStr = selectedDateValue.ToString("dd.MM.yyyy");
                if (!int.TryParse(computerid?.Text, out int currentComputerId))
                {
                    MessageBox.Show(Lang.lang == "en" ? "Invalid computer id" : "Некорректный ID компьютера");
                    return;
                }

                // безопасная проверка конфликта времени — пропускаем записи с некорректными временными полями
                bool isComputerBusy = items.Any(item =>
                {
                    if (item.ComputerId != currentComputerId) return false;
                    if (item.DateOrder != selectedDateStr) return false;
                    if (item.Status != "active") return false;

                    if (!TimeSpan.TryParse(item.StartTime, out TimeSpan exStart)) return false;
                    if (!TimeSpan.TryParse(item.EndTime, out TimeSpan exEnd)) return false;

                    return IsTimeOverlap(exStart, exEnd, start, end);
                });

                if (isComputerBusy)
                {
                    MessageBox.Show(Lang.lang == "en"
                        ? "This computer is already booked for the selected time!"
                        : "Этот компьютер уже занят на выбранное время!");
                    return;
                }

                bool hasTimeConflict = items.Any(item =>
                {
                    if (item.UserId != currentUserId) return false;
                    if (item.DateOrder != selectedDateStr) return false;
                    if (item.Status != "active") return false;

                    if (!TimeSpan.TryParse(item.StartTime, out TimeSpan exStart)) return false;
                    if (!TimeSpan.TryParse(item.EndTime, out TimeSpan exEnd)) return false;

                    return IsTimeOverlap(exStart, exEnd, start, end);
                });

                if (hasTimeConflict)
                {
                    MessageBox.Show(Lang.lang == "en"
                        ? "You already have a booking for this time!"
                        : "У вас уже есть бронирование на это время!");
                    return;
                }

                // формируем заказ (Id не задаём — ID генерирует БД)
                Order el = new Order
                {
                    UserId = currentUserId,
                    ComputerId = currentComputerId,
                    Status = "active",
                    DateOrder = selectedDateStr,
                    StartTime = starttime?.Text ?? string.Empty,
                    EndTime = endtime?.Text ?? string.Empty
                };

                // сохраняем асинхронно
                await database.AddElementsInTableOrders(el.UserId, el.ComputerId, el.Status, el.DateOrder, el.StartTime, el.EndTime).ConfigureAwait(false);

                // показать результат на UI-потоке
                await Dispatcher.InvokeAsync(() =>
                {
                    MessageBox.Show(Lang.lang == "en"
                        ? "Record successfully added!"
                        : "Запись успешно добавлена!");
                    this.Close();
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine("WriteInfoByItem error: " + ex.Message);
                MessageBox.Show(Lang.lang == "en" ? $"Error: {ex.Message}" : $"Ошибка: {ex.Message}");
            }
        }
        private void CostChange() {
            if (TimeSpan.TryParse(starttime?.Text, out TimeSpan start) &&
                    TimeSpan.TryParse(endtime?.Text, out TimeSpan end))
            {
                double totalCost = (end - start).TotalHours * _tariff.PricePerHour;
                double roundedCost = Math.Round(totalCost, 2);
                if (roundedCost >= 0)
                {
                    costpc.Text = roundedCost.ToString() + (Lang.lang == "en" ? " byn." : " руб.");
                }
                else
                {
                    costpc.Text = "0" + (Lang.lang == "en" ? " byn." : " руб.");
                }
            }
            else {
                costpc.Text = "0" + (Lang.lang == "en" ? " byn." : " руб.");
            }
                
        }
        public void FormattedPrice() { 
            price.Text = _tariff.PricePerHour + " " + (Lang.lang == "en" ? "byn." : "руб.");
        }
        public void CorrectRating() {
            rating.Text = Math.Round(_tariff.Rating, 2).ToString();
        }
        public void GenerateRamAndMonitor() { 
            monitor.Text = _tariff.Monitor + " " + (Lang.lang == "en" ? "Hz" : "Гц");
            ram.Text = _tariff.Ram + " " + (Lang.lang == "en" ? "Gb" : "Гб");
        }
        private bool IsTimeOverlap(TimeSpan existingStart, TimeSpan existingEnd, TimeSpan newStart, TimeSpan newEnd)
        {
            return (newStart < existingEnd && newEnd > existingStart);
        }
        private void NumericTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            foreach (char c in e.Text)
            {
                if (!char.IsDigit(c) && c != ':')
                {
                    e.Handled = true;
                    return;
                }
            }

            TextBox textBox = sender as TextBox;
            string newText = textBox.Text.Insert(textBox.CaretIndex, e.Text);

            if (newText.Count(c => c == ':') > 1)
            {
                e.Handled = true;
            }
        }
    }
}
