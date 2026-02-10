using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    /// <summary>
    /// Interaction logic for WritingReview.xaml
    /// </summary>
    public partial class WritingReview : Window
    {
        private readonly Order _item;
        private readonly Func<Task> _langHandler;

        public WritingReview(Order item)
        {
            InitializeComponent();
            _item = item;
            DataContext = item;
            _ = InitReviewAsync();

            // сохранённый делегат для корректной отписки
            _langHandler = async () => await InitReviewAsync();
            Lang.LanguageChanged += () => _ = _langHandler();
            this.Closed += (s, e) => Lang.LanguageChanged -= () => _ = _langHandler();
        }

        private async Task InitReviewAsync()
        {
            try
            {
                username.Text = database.GetUserNameById(_item.UserId);

                // безопасный парсинг времени
                TimeSpan startTime = TimeSpan.Zero;
                TimeSpan endTime = TimeSpan.Zero;
                TimeSpan.TryParse(_item.StartTime, out startTime);
                TimeSpan.TryParse(_item.EndTime, out endTime);
                TimeSpan duration = endTime - startTime;
                double durationDouble = Math.Max(0, duration.TotalHours);

                // читаем компьютеры асинхронно
                var computers = await database.ReadElementsInTableComputersAsync();
                var pc = computers.FirstOrDefault(x => x.Id == _item.ComputerId);

                string costStr = "0" + (Lang.lang == "en" ? " byn." : " руб.");
                if (pc != null)
                {
                    double cost = Math.Round((durationDouble * pc.PricePerHour), 2);
                    costStr = $"{cost}{(Lang.lang == "en" ? " byn." : " руб.")}";
                }

                await Dispatcher.InvokeAsync(() => cost.Text = costStr);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("InitReviewAsync error: " + ex.Message);
            }
        }

        public async void SaveReview(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(textReview.Text))
                {
                    MessageBox.Show(Lang.lang == "en"
                        ? "Cannot submit an empty review"
                        : "Нельзя оставить пустой отзыв");
                    return;
                }

                if (textReview.Text.Trim().Length > 500)
                {
                    MessageBox.Show(Lang.lang == "en"
                        ? "Review cannot exceed 500 characters"
                        : "Отзыв должен содержать не более 500 символов");
                    return;
                }

                await database.AddElementsInTableReviews(_item.UserId, _item.ComputerId, (float)stats.Value, textReview.Text.Trim(), DateTime.Now.ToString("dd.MM.yyyy"));

                // пересчёт рейтинга в фоне
                var allReviews = await database.ReadElementsInTableReviewsAsync();
                var reviewsForComputer = allReviews.Where(r => r.ComputerId == _item.ComputerId).ToList();

                if (reviewsForComputer.Count > 0)
                {
                    float result = (float)Math.Round(reviewsForComputer.Average(r => r.Rating), 2);

                    var computers = await database.ReadElementsInTableComputersAsync();
                    var comp = computers.FirstOrDefault(x => x.Id == _item.ComputerId);
                    if (comp != null)
                    {
                        await database.UpdateElementInTableComputers(comp.Id, comp.Name, comp.Description, comp.Cpu, comp.Gpu, comp.Ram, comp.Storage, comp.Monitor, result, (float)comp.PricePerHour, comp.GraphicData);
                    }
                }

                await Dispatcher.InvokeAsync(() =>
                {
                    MessageBox.Show(Lang.lang == "en"
                        ? "Your review has been saved!"
                        : "Ваш отзыв сохранен!");
                    this.Close();
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine("SaveReview error: " + ex.Message);
                if (ex.InnerException != null)
                {
                    Debug.WriteLine("Inner exception: " + ex.InnerException.Message);
                    Debug.WriteLine("Stack trace: " + ex.InnerException.StackTrace);
                }
                
                string errorMessage = ex.Message;
                if (ex.InnerException != null)
                {
                    errorMessage += $"\n{ex.InnerException.Message}";
                }
                
                await Dispatcher.InvokeAsync(() =>
                {
                    MessageBox.Show(Lang.lang == "en"
                        ? $"Error saving review: {errorMessage}"
                        : $"Ошибка при сохранении отзыва: {errorMessage}");
                });
            }
        }
    }
}