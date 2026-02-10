using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
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
using System.Xml.Linq;
using System.Diagnostics;

namespace COURSEPROJECT
{
    /// <summary>
    /// Логика взаимодействия для AdminTable.xaml
    /// </summary>
    public partial class AdminTable : Window
    {
        private string TableInWin = "Computers";

        public AdminTable()
        {
            InitializeComponent();
            // асинхронно загружаем таблицу
            _ = LoadComputersAsync();
            TableBox.SelectionChanged += async (s, e) =>
            {
                await LoadComputersAsync();
                TableParam.Text = "";
            };
            TableParam.SelectionChanged += async (s, e) => await LoadComputersAsync();
            inputuser.TextChanged += async (s, e) => await LoadComputersAsync();
            inputcomputer.TextChanged += async (s, e) => await LoadComputersAsync();
        }
        private void AddNewRow_Click(object sender, RoutedEventArgs e)
        {
            DataView view = (DataView)dataGrid.ItemsSource;
            DataRow newRow = view.Table.NewRow();
            view.Table.Rows.Add(newRow);
            dataGrid.ScrollIntoView(newRow);
        }
        private void DeleteRowButton_Click(object sender, RoutedEventArgs e)
        {
            if (dataGrid.SelectedItem != null)
            {
                var rowView = (DataRowView)dataGrid.SelectedItem;
                rowView.Row.Delete();
            }
        }
        private static readonly Regex TimeRegex = new Regex(@"^([01][0-9]|2[0-3]):[0-5][0-9]$");

        public static bool IsValidTimeFormat(string time)
        {
            return !string.IsNullOrEmpty(time) && TimeRegex.IsMatch(time);
        }

        // thin wrapper — event handler остаётся async void, логика в async Task для тестируемости
        private async void GetUpdateRows(object sender, EventArgs e)
        {
            try
            {
                await HandleUpdateRowsAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("GetUpdateRows wrapper error: " + ex.Message);
            }
        }

        private async Task HandleUpdateRowsAsync()
        {
            dataGrid.CommitEdit();

            DataView dataView = (DataView)dataGrid.ItemsSource;
            if (dataView == null) return;

            DataTable dataTable = dataView.Table;

            var modifiedRows = dataTable.Rows
                .Cast<DataRow>()
                .Where(row => row.RowState == DataRowState.Modified)
                .ToList();

            var deletedRows = dataTable.GetChanges(DataRowState.Deleted)?.Rows.Cast<DataRow>() ?? Enumerable.Empty<DataRow>();
            var addedRows = dataTable.Rows
                .Cast<DataRow>()
                .Where(row => row.RowState == DataRowState.Added)
                .ToList();

            if (modifiedRows.Count > 0 || deletedRows.Any() || addedRows.Count > 0)
            {
                foreach (DataRow row in addedRows)
                {
                    try
                    {
                        if (TableInWin == "Users")
                        {
                            string name = row["name"].ToString();
                            if (name.Length == 0)
                            {
                                MessageBox.Show(Lang.lang == "en" ? "Login cannot be empty" : "Логин не может быть пустым");
                                return;
                            }
                            if (name.Length > 99 || name.Length < 5)
                            {
                                MessageBox.Show(Lang.lang == "en"
                                    ? "Login must be between 5 and 99 characters"
                                    : "Логин должен быть от 5 до 99 символов");
                                return;
                            }
                            string password = row["password"].ToString();
                            if (password.Length == 0)
                            {
                                MessageBox.Show(Lang.lang == "en" ? "Password cannot be empty" : "Пароль не может быть пустым");
                                return;
                            }
                            if (password.Length > 99 || password.Length < 8)
                            {
                                MessageBox.Show(Lang.lang == "en"
                                    ? "Password must be between 8 and 99 characters"
                                    : "Пароль должен быть от 8 до 99 символов");
                                return;
                            }

                            await database.AddElementInTableUsers(name, password);
                            MessageBox.Show(Lang.lang == "en" ? "New user added" : "Добавлен новый пользователь");
                        }
                        else if (TableInWin == "Computers")
                        {
                            var comps = await database.ReadElementsInTableComputersAsync();
                            string name = row["name"].ToString();
                            if (name.Length == 0)
                            {
                                MessageBox.Show(Lang.lang == "en" ? "Name cannot be empty" : "Название не может быть пустым");
                                return;
                            }

                            string description = row["description"].ToString();
                            if (description.Length == 0)
                            {
                                MessageBox.Show(Lang.lang == "en" ? "Description cannot be empty" : "Описание не может быть пустым");
                                return;
                            }

                            string cpu = row["cpu"].ToString();
                            if (cpu.Length == 0)
                            {
                                MessageBox.Show(Lang.lang == "en" ? "CPU cannot be empty" : "Процессор не может быть пустым");
                                return;
                            }

                            string gpu = row["gpu"].ToString();
                            if (gpu.Length == 0)
                            {
                                MessageBox.Show(Lang.lang == "en" ? "GPU cannot be empty" : "Видеокарта не может быть пустым");
                                return;
                            }

                            string ram = row["ram"].ToString();
                            if (ram.Length == 0)
                            {
                                MessageBox.Show(Lang.lang == "en" ? "RAM cannot be empty" : "Оперативная память не может быть пустым");
                                return;
                            }
                            if (!int.TryParse(ram, out int x))
                            {
                                MessageBox.Show(Lang.lang == "en" ? "RAM must be an integer number" : "Оперативная память должна быть целым числом");
                                return;
                            }

                            string storage = row["storage"].ToString();
                            if (storage.Length == 0)
                            {
                                MessageBox.Show(Lang.lang == "en" ? "Storage cannot be empty" : "Накопитель не может быть пустым");
                                return;
                            }
                            string monitor = row["monitor"].ToString();
                            if (monitor.Length == 0)
                            {
                                MessageBox.Show(Lang.lang == "en" ? "Monitor cannot be empty" : "Монитор не может быть пустым");
                                return;
                            }
                            if (!Regex.IsMatch(monitor, @"^[0-9\s\"".]*$"))
                            {
                                MessageBox.Show(Lang.lang == "en"
                                    ? "Monitor value can only contain numbers, spaces, double quotes and dots."
                                    : "Значение монитора может содержать только цифры, пробелы, кавычки и точки.");
                                return;
                            }
                            if (!float.TryParse(row["rating"].ToString(), out float rating))
                            {
                                MessageBox.Show(Lang.lang == "en" ? "Error: Invalid rating value" : "Ошибка: Некорректное значение рейтинга");
                                return;
                            }
                            if (rating < 0 || rating > 5)
                            {
                                MessageBox.Show(Lang.lang == "en" ? "Rating must be between 0 and 5" : "Рейтинг должен быть от 0 до 5");
                                return;
                            }

                            if (!float.TryParse(row["PricePerHour"].ToString(), out float pricePerHour))
                            {
                                MessageBox.Show(Lang.lang == "en" ? "Error: Invalid price per hour" : "Ошибка: Некорректное значение цены за час");
                                return;
                            }
                            if (pricePerHour < 0)
                            {
                                MessageBox.Show(Lang.lang == "en" ? "Error: Price cannot be negative" : "Ошибка: Цена не может быть отрицательной");
                                return;
                            }
                            byte[] graphicData = null;
                            object graphicDataObj = row["GraphicData"];

                            if (graphicDataObj != DBNull.Value)
                            {
                                if (graphicDataObj is byte[])
                                {
                                    graphicData = (byte[])graphicDataObj;
                                }
                                else
                                {
                                    try
                                    {
                                        graphicData = Convert.FromBase64String(graphicDataObj.ToString());
                                    }
                                    catch
                                    {
                                        MessageBox.Show(Lang.lang == "en" ? "Graphic data is corrupted or in wrong format" : "Графические данные повреждены или в неверном формате");
                                        return;
                                    }
                                }
                            }
                            if (comps.Count > 17)
                            {
                                MessageBox.Show(Lang.lang == "en" ? "Maximum number of computers reached" : "Достигнуто максимальное количество компьютеров");
                            }
                            else
                            {
                                await database.AddElementInTableComputers(name, description, cpu, gpu, ram, storage, monitor, rating, pricePerHour, graphicData);
                                MessageBox.Show(Lang.lang == "en" ? "New computer added" : "Добавлен новый компьютер");
                            }
                        }
                        else if (TableInWin == "Orders")
                        {
                            try
                            {
                                int userId = Convert.ToInt32(row["UserId"]);
                                var users = await database.ReadElementsInTableUsersAsync();
                                var userExists = users.Any(x => x.Id == userId);
                                if (!userExists)
                                {
                                    MessageBox.Show(Lang.lang == "en" ? "Error: User with specified ID not found" : "Ошибка: Пользователь с указанным ID не найден");
                                    return;
                                }

                                int computerId = Convert.ToInt32(row["ComputerId"]);
                                var computers = await database.ReadElementsInTableComputersAsync();
                                var computerExists = computers.Any(x => x.Id == computerId);
                                if (!computerExists)
                                {
                                    MessageBox.Show(Lang.lang == "en" ? "Error: Computer with specified ID not found" : "Ошибка: Компьютер с указанным ID не найден");
                                    return;
                                }
                                string status = row["status"].ToString().ToLower();
                                string dateOrder = row["DateOrder"].ToString();
                                string startTimeStr = row["StartTime"].ToString().Trim();
                                string endTimeStr = row["EndTime"].ToString().Trim();

                                if (status != "completed" && status != "active" && status != "cancelled")
                                {
                                    MessageBox.Show(Lang.lang == "en" ? "Invalid status value. Allowed values: completed, active, cancelled" : "Недопустимое значение статуса. Допустимые значения: completed, active, cancelled");
                                    return;
                                }

                                if (!DateTime.TryParseExact(dateOrder, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime orderDate))
                                {
                                    MessageBox.Show(Lang.lang == "en" ? "Invalid date format. Use DD.MM.YYYY" : "Неверный формат даты. Используйте ДД.ММ.ГГГГ");
                                    return;
                                }

                                DateTime today = DateTime.Today;
                                DateTime maxDate = today.AddDays(14);

                                if (orderDate < today)
                                {
                                    MessageBox.Show(Lang.lang == "en" ? "Order date cannot be earlier than today" : "Дата заказа не может быть раньше сегодняшнего дня");
                                    return;
                                }

                                if (orderDate > maxDate)
                                {
                                    MessageBox.Show(Lang.lang == "en" ? "Order date cannot be more than 2 weeks ahead" : "Дата заказа не может быть более чем на 2 недели вперед");
                                    return;
                                }

                                if (!IsValidTimeFormat(startTimeStr))
                                {
                                    MessageBox.Show(Lang.lang == "en" ? "Invalid start time format. Use HH:MM (00:00 - 23:59)" : "Неверный формат времени начала. Используйте ЧЧ:ММ (00:00 - 23:59)");
                                    return;
                                }

                                if (!IsValidTimeFormat(endTimeStr))
                                {
                                    MessageBox.Show(Lang.lang == "en" ? "Invalid end time format. Use HH:MM (00:00 - 23:59)" : "Неверный формат времени окончания. Используйте ЧЧ:ММ (00:00 - 23:59)");
                                    return;
                                }

                                TimeSpan startTime = TimeSpan.Parse(startTimeStr);
                                TimeSpan endTime = TimeSpan.Parse(endTimeStr);

                                if (endTime <= startTime)
                                {
                                    MessageBox.Show(Lang.lang == "en" ? "End time must be later than start time" : "Время окончания должно быть позже времени начала");
                                    return;
                                }

                                DateTime now = DateTime.Now;
                                DateTime selectedStartDateTime = orderDate.Date + startTime;

                                if (selectedStartDateTime < now)
                                {
                                    MessageBox.Show(Lang.lang == "en" ? "Start time cannot be in the past" : "Время начала не может быть в прошлом");
                                    return;
                                }

                                await database.AddElementsInTableOrders(userId, computerId, status, dateOrder, startTimeStr, endTimeStr);
                                MessageBox.Show(Lang.lang == "en" ? "New order added" : "Добавлен новый заказ");
                            }
                            catch (FormatException ex)
                            {
                                MessageBox.Show(Lang.lang == "en" ? $"Data format error: {ex.Message}" : $"Ошибка формата данных: {ex.Message}");
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(Lang.lang == "en" ? $"Unexpected error while adding order: {ex.Message}" : $"Неожиданная ошибка при добавлении заказа: {ex.Message}");
                            }
                        }
                        else if (TableInWin == "Reviews")
                        {
                            int userId = Convert.ToInt32(row["UserId"]);
                            var users = await database.ReadElementsInTableUsersAsync();
                            var userExists = users.Any(x => x.Id == userId);
                            if (!userExists)
                            {
                                MessageBox.Show(Lang.lang == "en" ? "Error: User with specified ID not found" : "Ошибка: Пользователь с указанным ID не найден");
                                return;
                            }

                            int computerId = Convert.ToInt32(row["ComputerId"]);
                            var computers = await database.ReadElementsInTableComputersAsync();
                            var computerExists = computers.Any(x => x.Id == computerId);
                            if (!computerExists)
                            {
                                MessageBox.Show(Lang.lang == "en" ? "Error: Computer with specified ID not found" : "Ошибка: Компьютер с указанным ID не найден");
                                return;
                            }
                            if (!float.TryParse(row["rating"].ToString(), out float rating))
                            {
                                MessageBox.Show(Lang.lang == "en" ? "Error: Invalid rating value" : "Ошибка: Некорректное значение рейтинга");
                                return;
                            }
                            if (rating < 0 || rating > 5)
                            {
                                MessageBox.Show(Lang.lang == "en" ? "Rating must be between 0 and 5" : "Рейтинг должен быть от 0 до 5");
                                return;
                            }
                            string comment = row["Comment"].ToString();
                            if (comment.Length > 500)
                            {
                                MessageBox.Show(Lang.lang == "en" ? "Comment should not exceed 500 characters" : "Комментарий не должен превышать 500 символов");
                                return;
                            }
                            if (comment.Length == 0)
                            {
                                MessageBox.Show(Lang.lang == "en" ? "Comment cannot be empty" : "Комментарий не должен быть пустым");
                                return;
                            }
                            string createAt = row["CreateAt"].ToString();
                            if (!DateTime.TryParseExact(createAt, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime orderDate))
                            {
                                MessageBox.Show(Lang.lang == "en" ? "Invalid date format. Use DD.MM.YYYY" : "Неверный формат даты. Используйте ДД.ММ.ГГГГ");
                                return;
                            }

                            if (orderDate.Date != DateTime.Today)
                            {
                                MessageBox.Show(Lang.lang == "en" ? "Creation date must be today" : "Дата создания должна быть сегодняшней");
                                return;
                            }

                            await database.AddElementsInTableReviews(userId, computerId, rating, comment, createAt);
                            MessageBox.Show(Lang.lang == "en" ? "New review added" : "Добавлен новый отзыв");
                        }
                        else if (TableInWin == "ApplicationGame")
                        {
                            try
                            {
                                string name = row["Name"].ToString().Trim();
                                if (string.IsNullOrEmpty(name))
                                {
                                    MessageBox.Show(Lang.lang == "en" ? "Name cannot be empty" : "Название не может быть пустым");
                                    return;
                                }
                                string img = row["IMG"].ToString().Trim();
                                if (string.IsNullOrEmpty(img))
                                {
                                    MessageBox.Show(Lang.lang == "en" ? "Image path cannot be empty" : "Путь к изображению не может быть пустым");
                                    return;
                                }


                                string description = row["Description"].ToString().Trim();
                                if (string.IsNullOrEmpty(description))
                                {
                                    MessageBox.Show(Lang.lang == "en" ? "Description cannot be empty" : "Описание не может быть пустым");
                                    return;
                                }

                                string url = row["URl"].ToString().Trim();
                                if (string.IsNullOrEmpty(url))
                                {
                                    MessageBox.Show(Lang.lang == "en" ? "URL cannot be empty" : "URL не может быть пустым");
                                    return;
                                }

                                if (!img.EndsWith(".png", StringComparison.OrdinalIgnoreCase) &&
                                    !img.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) &&
                                    !img.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase))
                                {
                                    MessageBox.Show(Lang.lang == "en" ? "Image must be in .png or .jpg format"
                                                                    : "Изображение должно быть в формате .png или .jpg");
                                    return;
                                }

                                if (!url.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                                {
                                    MessageBox.Show(Lang.lang == "en" ? "URL must point to .exe file" : "URL должен указывать на .exe файл");
                                    return;
                                }

                                await database.AddElementsInTableApplicationGame(name, img, description, url);
                                MessageBox.Show(Lang.lang == "en" ? "Application successfully added" : "Приложение успешно добавлено");
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(Lang.lang == "en" ? $"Error: {ex.Message}" : $"Ошибка: {ex.Message}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(Lang.lang == "en" ? $"Error while adding: {ex.Message}" : $"Ошибка при добавлении: {ex.Message}");
                    }
                }
                foreach (DataRow row in modifiedRows)
                {
                    try
                    {
                        if (TableInWin == "Users")
                        {
                            int id = Convert.ToInt32(row["id"]);
                            string name = row["name"].ToString();
                            if (string.IsNullOrEmpty(name))
                            {
                                MessageBox.Show(Lang.lang == "en" ? "Login cannot be empty" : "Логин не может быть пустым");
                                return;
                            }
                            if (name.Length > 99 || name.Length < 5)
                            {
                                MessageBox.Show(Lang.lang == "en"
                                    ? "Login must be between 5 and 99 characters"
                                    : "Логин должен быть от 5 до 99 символов");
                                return;
                            }

                            string password = row["password"].ToString();
                            if (string.IsNullOrEmpty(password))
                            {
                                MessageBox.Show(Lang.lang == "en" ? "Password cannot be empty" : "Пароль не может быть пустым");
                                return;
                            }
                            if (password.Length > 99 || password.Length < 8)
                            {
                                MessageBox.Show(Lang.lang == "en"
                                    ? "Password must be between 8 and 99 characters"
                                    : "Пароль должен быть от 8 до 99 символов");
                                return;
                            }
                            if (HashCode.IsHash(password))
                            {
                                MessageBox.Show(
                                    Lang.lang == "en"
                        ? "The password field contains a hash. Please enter a new password, not an existing hash."
                        : "В поле пароля обнаружен хэш. Введите новый пароль, а не существующий хэш."
                                );
                                return;
                            }

                            await database.UpdateElementInTableUsers(id, name, password);
                            MessageBox.Show(Lang.lang == "en" ? $"User data (ID: {id}) updated successfully!" : $"Данные пользователя (ID: {id}) успешно обновлены!");
                        }
                        else if (TableInWin == "Computers")
                        {
                            int id = Convert.ToInt32(row["id"]);
                            string name = row["name"].ToString();
                            if (string.IsNullOrEmpty(name))
                            {
                                MessageBox.Show(Lang.lang == "en" ? "Name cannot be empty" : "Название не может быть пустым");
                                return;
                            }

                            string description = row["description"].ToString();
                            if (string.IsNullOrEmpty(description))
                            {
                                MessageBox.Show(Lang.lang == "en" ? "Description cannot be empty" : "Описание не может быть пустым");
                                return;
                            }

                            string cpu = row["cpu"].ToString();
                            if (string.IsNullOrEmpty(cpu))
                            {
                                MessageBox.Show(Lang.lang == "en" ? "CPU cannot be empty" : "Процессор не может быть пустым");
                                return;
                            }

                            string gpu = row["gpu"].ToString();
                            if (string.IsNullOrEmpty(gpu))
                            {
                                MessageBox.Show(Lang.lang == "en" ? "GPU cannot be empty" : "Видеокарта не может быть пустым");
                                return;
                            }

                            string ram = row["ram"].ToString();
                            if (string.IsNullOrEmpty(ram))
                            {
                                MessageBox.Show(Lang.lang == "en" ? "RAM cannot be empty" : "Оперативная память не может быть пустым");
                                return;
                            }
                            if (!int.TryParse(ram, out int x))
                            {
                                MessageBox.Show(Lang.lang == "en" ? "RAM must be an integer number" : "Оперативная память должна быть целым числом");
                                return;
                            }

                            string storage = row["storage"].ToString();
                            if (string.IsNullOrEmpty(storage))
                            {
                                MessageBox.Show(Lang.lang == "en" ? "Storage cannot be empty" : "Накопитель не может быть пустым");
                                return;
                            }

                            string monitor = row["monitor"].ToString();
                            if (string.IsNullOrEmpty(monitor))
                            {
                                MessageBox.Show(Lang.lang == "en" ? "Monitor cannot be empty" : "Монитор не может быть пустым");
                                return;
                            }
                            if (!Regex.IsMatch(monitor, @"^[0-9\s\"".]*$"))
                            {
                                MessageBox.Show(Lang.lang == "en"
                                    ? "Monitor value can only contain numbers, spaces, double quotes and dots."
                                    : "Значение монитора может содержать только цифры, пробелы, кавычки и точки.");
                                return;
                            }

                            if (!float.TryParse(row["rating"].ToString(), out float rating))
                            {
                                MessageBox.Show(Lang.lang == "en" ? "Error: Invalid rating value" : "Ошибка: Некорректное значение рейтинга");
                                return;
                            }
                            if (rating < 0 || rating > 5)
                            {
                                MessageBox.Show(Lang.lang == "en" ? "Rating must be between 0 and 5" : "Рейтинг должен быть от 0 до 5");
                                return;
                            }

                            if (!float.TryParse(row["PricePerHour"].ToString(), out float pricePerHour))
                            {
                                MessageBox.Show(Lang.lang == "en" ? "Error: Invalid price per hour" : "Ошибка: Некорректное значение цены за час");
                                return;
                            }
                            if (pricePerHour < 0)
                            {
                                MessageBox.Show(Lang.lang == "en" ? "Error: Price cannot be negative" : "Ошибка: Цена не может быть отрицательной");
                                return;
                            }

                            byte[] graphicData = null;
                            object graphicDataObj = row["GraphicData"];
                            if (graphicDataObj != DBNull.Value)
                            {
                                if (graphicDataObj is byte[])
                                {
                                    graphicData = (byte[])graphicDataObj;
                                }
                                else
                                {
                                    try
                                    {
                                        graphicData = Convert.FromBase64String(graphicDataObj.ToString());
                                    }
                                    catch
                                    {
                                        MessageBox.Show(Lang.lang == "en" ? "Graphic data is corrupted or in wrong format" : "Графические данные повреждены или в неверном формате");
                                        return;
                                    }
                                }
                            }

                            await database.UpdateElementInTableComputers(id, name, description, cpu, gpu, ram, storage, monitor, rating, pricePerHour, graphicData);
                            MessageBox.Show(Lang.lang == "en" ? $"Computer data (ID: {id}) updated successfully!" : $"Данные компьютера (ID: {id}) успешно обновлены!");
                        }
                        else if (TableInWin == "Orders")
                        {
                            try
                            {
                                int id = Convert.ToInt32(row["Id"]);
                                int userId = Convert.ToInt32(row["UserId"]);
                                var users = await database.ReadElementsInTableUsersAsync();
                                var userExists = users.Any(x => x.Id == userId);
                                if (!userExists)
                                {
                                    MessageBox.Show(Lang.lang == "en" ? "Error: User with specified ID not found" : "Ошибка: Пользователь с указанным ID не найден");
                                    return;
                                }

                                int computerId = Convert.ToInt32(row["ComputerId"]);
                                var computers = await database.ReadElementsInTableComputersAsync();
                                var computerExists = computers.Any(x => x.Id == computerId);
                                if (!computerExists)
                                {
                                    MessageBox.Show(Lang.lang == "en" ? "Error: Computer with specified ID not found" : "Ошибка: Компьютер с указанным ID не найден");
                                    return;
                                }

                                string status = row["status"].ToString().ToLower();
                                if (status != "completed" && status != "active" && status != "cancelled")
                                {
                                    MessageBox.Show(Lang.lang == "en" ? "Invalid status value. Allowed values: completed, active, cancelled" : "Недопустимое значение статуса. Допустимые значения: completed, active, cancelled");
                                    return;
                                }

                                string dateOrder = row["DateOrder"].ToString();
                                if (!DateTime.TryParseExact(dateOrder, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime orderDate))
                                {
                                    MessageBox.Show(Lang.lang == "en" ? "Invalid date format. Use DD.MM.YYYY" : "Неверный формат даты. Используйте ДД.ММ.ГГГГ");
                                    return;
                                }

                                DateTime today = DateTime.Today;
                                DateTime firstDayOfCurrentMonth = new DateTime(today.Year, today.Month, 1);
                                DateTime maxDate = today.AddDays(14);

                                if (orderDate < firstDayOfCurrentMonth)
                                {
                                    MessageBox.Show(Lang.lang == "en" ? $"Order date cannot be earlier than the current month ({firstDayOfCurrentMonth:dd.MM.yyyy})"
                                                                    : $"Дата заказа не может быть раньше текущего месяца ({firstDayOfCurrentMonth:dd.MM.yyyy})");
                                    return;
                                }

                                if (orderDate > maxDate)
                                {
                                    MessageBox.Show(Lang.lang == "en" ? "Order date cannot be more than 2 weeks ahead" : "Дата заказа не может быть более чем на 2 недели вперед");
                                    return;
                                }

                                string startTimeStr = row["StartTime"].ToString().Trim();
                                string endTimeStr = row["EndTime"].ToString().Trim();

                                if (!IsValidTimeFormat(startTimeStr))
                                {
                                    MessageBox.Show(Lang.lang == "en" ? "Invalid start time format. Use HH:MM (00:00 - 23:59)" : "Неверный формат времени начала. Используйте ЧЧ:ММ (00:00 - 23:59)");
                                    return;
                                }
                                if (!IsValidTimeFormat(endTimeStr))
                                {
                                    MessageBox.Show(Lang.lang == "en" ? "Invalid end time format. Use HH:MM (00:00 - 23:59)" : "Неверный формат времени окончания. Используйте ЧЧ:ММ (00:00 - 23:59)");
                                    return;
                                }

                                TimeSpan startTime = TimeSpan.Parse(startTimeStr);
                                TimeSpan endTime = TimeSpan.Parse(endTimeStr);
                                if (endTime <= startTime)
                                {
                                    MessageBox.Show(Lang.lang == "en" ? "End time must be later than start time" : "Время окончания должно быть позже времени начала");
                                    return;
                                }

                                DateTime now = DateTime.Now;
                                DateTime selectedStartDateTime = orderDate.Date + startTime;

                                await database.UpdateElementInTableOrders(id, userId, computerId, status, dateOrder, startTimeStr, endTimeStr);
                                MessageBox.Show(Lang.lang == "en" ? $"Order data (ID: {id}) updated successfully!" : $"Данные заказа (ID: {id}) успешно обновлены!");
                            }
                            catch (FormatException ex)
                            {
                                MessageBox.Show(Lang.lang == "en" ? $"Data format error: {ex.Message}" : $"Ошибка формата данных: {ex.Message}");
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(Lang.lang == "en" ? $"Unexpected error while updating order: {ex.Message}" : $"Неожиданная ошибка при обновлении заказа: {ex.Message}");
                            }
                        }
                        else if (TableInWin == "Reviews")
                        {
                            int id = Convert.ToInt32(row["Id"]);
                            int userId = Convert.ToInt32(row["UserId"]);
                            var users = await database.ReadElementsInTableUsersAsync();
                            var userExists = users.Any(x => x.Id == userId);
                            if (!userExists)
                            {
                                MessageBox.Show(Lang.lang == "en" ? "Error: User with specified ID not found" : "Ошибка: Пользователь с указанным ID не найден");
                                return;
                            }

                            int computerId = Convert.ToInt32(row["ComputerId"]);
                            var computers = await database.ReadElementsInTableComputersAsync();
                            var computerExists = computers.Any(x => x.Id == computerId);
                            if (!computerExists)
                            {
                                MessageBox.Show(Lang.lang == "en" ? "Error: Computer with specified ID not found" : "Ошибка: Компьютер с указанным ID не найден");
                                return;
                            }

                            if (!float.TryParse(row["rating"].ToString(), out float rating))
                            {
                                MessageBox.Show(Lang.lang == "en" ? "Error: Invalid rating value" : "Ошибка: Некорректное значение рейтинга");
                                return;
                            }
                            if (rating < 0 || rating > 5)
                            {
                                MessageBox.Show(Lang.lang == "en" ? "Rating must be between 0 and 5" : "Рейтинг должен быть от 0 до 5");
                                return;
                            }

                            string comment = row["Comment"].ToString();
                            if (string.IsNullOrEmpty(comment))
                            {
                                MessageBox.Show(Lang.lang == "en" ? "Comment cannot be empty" : "Комментарий не может быть пустым");
                                return;
                            }
                            if (comment.Length > 500)
                            {
                                MessageBox.Show(Lang.lang == "en" ? "Comment should not exceed 500 characters" : "Комментарий не должен превышать 500 символов");
                                return;
                            }

                            string createAt = row["CreateAt"].ToString();
                            if (!DateTime.TryParseExact(createAt, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime createDate))
                            {
                                MessageBox.Show(Lang.lang == "en" ? "Invalid date format. Use DD.MM.YYYY" : "Неверный формат даты. Используйте ДД.ММ.ГГГГ");
                                return;
                            }
                            DateTime today = DateTime.Today;
                            if (createDate > today)
                            {
                                MessageBox.Show(Lang.lang == "en" ? "Review date cannot be in the future" : "Дата отзыва не может быть в будущем");
                                return;
                            }
                            await database.UpdateElementInTableReviews(id, userId, computerId, rating, comment, createAt);
                            MessageBox.Show(Lang.lang == "en" ? $"Review data (ID: {id}) updated successfully!" : $"Данные отзыва (ID: {id}) успешно обновлены!");
                        }
                        else if (TableInWin == "ApplicationGame")
                        {
                            try
                            {
                                int id = Convert.ToInt32(row["Id"]);
                                string name = row["Name"].ToString().Trim();
                                if (string.IsNullOrEmpty(name))
                                {
                                    MessageBox.Show(Lang.lang == "en" ? "Name cannot be empty" : "Название не может быть пустым");
                                    return;
                                }

                                string img = row["IMG"].ToString().Trim();
                                if (string.IsNullOrEmpty(img))
                                {
                                    MessageBox.Show(Lang.lang == "en" ? "Image path cannot be empty" : "Путь к изображению не может быть пустым");
                                    return;
                                }


                                string description = row["Description"].ToString().Trim();
                                if (string.IsNullOrEmpty(description))
                                {
                                    MessageBox.Show(Lang.lang == "en" ? "Description cannot be empty" : "Описание не может быть пустым");
                                    return;
                                }

                                string url = row["URl"].ToString().Trim();
                                if (string.IsNullOrEmpty(url))
                                {
                                    MessageBox.Show(Lang.lang == "en" ? "URL cannot be empty" : "URL не может быть пустым");
                                    return;
                                }
                                if (!img.EndsWith(".png", StringComparison.OrdinalIgnoreCase) &&
                                    !img.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) &&
                                    !img.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase))
                                {
                                    MessageBox.Show(Lang.lang == "en" ? "Image must be in .png or .jpg format"
                                                                    : "Изображение должно быть в формате .png или .jpg");
                                    return;
                                }
                                if (!url.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                                {
                                    MessageBox.Show(Lang.lang == "en" ? "URL must point to .exe file" : "URL должен указывать на .exe файл");
                                    return;
                                }

                                await database.UpdateElementInTableApplicationGame(id, name, img, description, url);
                                MessageBox.Show(Lang.lang == "en" ? $"Application data (ID: {id}) updated successfully!" : $"Данные приложения (ID: {id}) успешно обновлены!");
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(Lang.lang == "en" ? $"Error while updating application: {ex.Message}" : $"Ошибка при обновлении приложения: {ex.Message}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(Lang.lang == "en" ? $"Error while updating: {ex.Message}" : $"Ошибка при обновлении: {ex.Message}");
                    }
                }
                foreach (DataRow row in deletedRows)
                {
                    try
                    {
                        if (!row.Table.Columns.Contains("id"))
                        {
                            MessageBox.Show(Lang.lang == "en" ? "Error: 'id' column missing in table" : "Ошибка: отсутствует столбец 'id' в таблице");
                            return;
                        }

                        if (!row.HasVersion(DataRowVersion.Original))
                        {
                            MessageBox.Show(Lang.lang == "en" ? "Error: original row version missing" : "Ошибка: отсутствует оригинальная версия строки");
                            return;
                        }

                        object idValue = row["id", DataRowVersion.Original];

                        if (idValue == null || idValue == DBNull.Value)
                        {
                            MessageBox.Show(Lang.lang == "en" ? "Error: ID cannot be NULL" : "Ошибка: ID не может быть NULL");
                            return;
                        }

                        if (!int.TryParse(idValue.ToString(), out int id) || id <= 0)
                        {
                            MessageBox.Show(Lang.lang == "en" ? "Error: Invalid ID. Expected positive integer" : "Ошибка: Некорректный ID. Ожидается положительное целое число");
                            return;
                        }

                        if (TableInWin == "Users")
                        {
                            await database.DeleteElementInTableUsers(id);
                            MessageBox.Show(Lang.lang == "en" ? $"User (ID: {id}) deleted!" : $"Пользователь (ID: {id}) удален!");
                        }
                        else if (TableInWin == "Computers")
                        {
                            await database.DeleteElementInTableComputers(id);
                            MessageBox.Show(Lang.lang == "en" ? $"Computer (ID: {id}) deleted!" : $"Компьютер (ID: {id}) удален!");
                        }
                        else if (TableInWin == "Orders")
                        {
                            await database.DeleteElementInTableOrders(id);
                            MessageBox.Show(Lang.lang == "en" ? $"Order (ID: {id}) deleted!" : $"Заказ (ID: {id}) удален!");
                        }
                        else if (TableInWin == "Reviews")
                        {
                            await database.DeleteElementInTableReviews(id);
                            MessageBox.Show(Lang.lang == "en" ? $"Review (ID: {id}) deleted!" : $"Отзыв (ID: {id}) удален!");
                        }
                        else if (TableInWin == "ApplicationGame")
                        {
                            await database.DeleteElementInTableApplicationGame(id);
                            MessageBox.Show(Lang.lang == "en" ? $"Application (ID: {id}) deleted!" : $"Приложение (ID: {id}) удалено!");
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(Lang.lang == "en" ? $"Error while deleting: {ex.Message}" : $"Ошибка при удалении: {ex.Message}");
                    }
                }

                dataTable.AcceptChanges();

                // После всех изменений безопасно обновляем таблицу
                await LoadComputersAsync();
            }
            else
            {
                MessageBox.Show(Lang.lang == "en" ? "No changes to save" : "Нет изменений для сохранения");
            }

            await UpdateRatingAsync();
        }

        private async Task UpdateRatingAsync()
        {
            try
            {
                var allOrders = await database.ReadElementsInTableOrdersAsync();

                foreach (var _item in allOrders)
                {
                    int count = 0;
                    float result = 0;

                    var allReviews = await database.ReadElementsInTableReviewsAsync();

                    foreach (var rev in allReviews)
                    {
                        if (rev.ComputerId == _item.ComputerId)
                        {
                            count++;
                            result += (float)rev.Rating;
                        }
                    }

                    if (count != 0)
                    {
                        result = (float)Math.Round(result / count, 2);
                        var comps = await database.ReadElementsInTableComputersAsync();
                        var comp = comps.FirstOrDefault(x => x.Id == _item.ComputerId);
                        if (comp != null)
                        {
                            await database.UpdateElementInTableComputers(comp.Id, comp.Name, comp.Description, comp.Cpu
                                , comp.Gpu, comp.Ram, comp.Storage, comp.Monitor, result, (float)comp.PricePerHour, comp.GraphicData);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private async Task LoadComputersAsync()
        {
            dataGrid.AutoGeneratingColumn -= DataGrid_AutoGeneratingColumn;
            dataGrid.AutoGeneratingColumn += DataGrid_AutoGeneratingColumn;

            // безопасно читаем выбор на UI-потоке
            var selectedItem = await Dispatcher.InvokeAsync(() => TableBox.SelectedItem);

            try
            {
                if (selectedItem == TableBox.Items[0])
                {
                    TableInWin = "Users";
                    var dt = database.GetUsersTableWhere(inputuser.Text);
                    dataGrid.ItemsSource = dt.DefaultView;
                    ((DataView)dataGrid.ItemsSource).Table.AcceptChanges();
                    TableParam.Visibility = Visibility.Collapsed;
                    inputuser.Visibility = Visibility.Visible;
                    inputcomputer.Visibility = Visibility.Collapsed;
                }
                else if (selectedItem == TableBox.Items[1])
                {
                    inputcomputer.Visibility = Visibility.Visible;
                    inputuser.Visibility = Visibility.Collapsed;
                    TableInWin = "Computers";
                    if (TableParam.SelectedItem == TableParam.Items[0])
                    {
                        dataGrid.ItemsSource = database.GetComputersTableWhere(inputcomputer.Text, Lang.lang == "en" ? "Powerful processors" : "Мощные процессоры").DefaultView;
                        ((DataView)dataGrid.ItemsSource).Table.AcceptChanges();
                    }
                    else if (TableParam.SelectedItem == TableParam.Items[1])
                    {
                        dataGrid.ItemsSource = database.GetComputersTableWhere(inputcomputer.Text, Lang.lang == "en" ? "Powerful graphics cards" : "Мощные видеокарты").DefaultView;
                        ((DataView)dataGrid.ItemsSource).Table.AcceptChanges();
                    }
                    else if (TableParam.SelectedItem == TableParam.Items[2])
                    {
                        dataGrid.ItemsSource = database.GetComputersTableWhere(inputcomputer.Text, Lang.lang == "en" ? "Expensive" : "Дорогие").DefaultView;
                        ((DataView)dataGrid.ItemsSource).Table.AcceptChanges();
                    }
                    else
                    {
                        dataGrid.ItemsSource = database.GetComputersTableWhere(inputcomputer.Text).DefaultView;
                        ((DataView)dataGrid.ItemsSource).Table.AcceptChanges();
                    }
                    TableParam.Visibility = Visibility.Visible;
                    comp1.Visibility = Visibility.Visible;
                    comp2.Visibility = Visibility.Visible;
                    comp3.Visibility = Visibility.Visible;
                    ord1.Visibility = Visibility.Collapsed;
                    ord2.Visibility = Visibility.Collapsed;
                    ord3.Visibility = Visibility.Collapsed;
                }
                else if (selectedItem == TableBox.Items[2])
                {
                    inputcomputer.Visibility = Visibility.Collapsed;
                    inputuser.Visibility = Visibility.Collapsed;
                    TableInWin = "Orders";
                    if (TableParam.SelectedItem == TableParam.Items[3])
                    {
                        dataGrid.ItemsSource = database.GetOrdersTable(Lang.lang == "en" ? "Completed" : "Завершенные").DefaultView;
                        ((DataView)dataGrid.ItemsSource).Table.AcceptChanges();
                    }
                    else if (TableParam.SelectedItem == TableParam.Items[4])
                    {
                        dataGrid.ItemsSource = database.GetOrdersTable(Lang.lang == "en" ? "Active" : "Активные").DefaultView;
                        ((DataView)dataGrid.ItemsSource).Table.AcceptChanges();
                    }
                    else if (TableParam.SelectedItem == TableParam.Items[5])
                    {
                        dataGrid.ItemsSource = database.GetOrdersTable(Lang.lang == "en" ? "Cancelled" : "Отмененные").DefaultView;
                        ((DataView)dataGrid.ItemsSource).Table.AcceptChanges();
                    }
                    else
                    {
                        dataGrid.ItemsSource = database.GetOrdersTable().DefaultView;
                        ((DataView)dataGrid.ItemsSource).Table.AcceptChanges();
                    }
                    TableParam.Visibility = Visibility.Visible;
                    comp1.Visibility = Visibility.Collapsed;
                    comp2.Visibility = Visibility.Collapsed;
                    comp3.Visibility = Visibility.Collapsed;
                    ord1.Visibility = Visibility.Visible;
                    ord2.Visibility = Visibility.Visible;
                    ord3.Visibility = Visibility.Visible;
                }
                else if (selectedItem == TableBox.Items[3])
                {
                    TableInWin = "Reviews";
                    dataGrid.ItemsSource = database.GetReviewsTable().DefaultView;
                    ((DataView)dataGrid.ItemsSource).Table.AcceptChanges();
                    TableParam.Visibility = Visibility.Collapsed;
                    inputuser.Visibility = Visibility.Collapsed;
                    inputcomputer.Visibility = Visibility.Collapsed;
                    comp1.Visibility = Visibility.Collapsed;
                    comp2.Visibility = Visibility.Collapsed;
                    comp3.Visibility = Visibility.Collapsed;
                    ord1.Visibility = Visibility.Collapsed;
                    ord2.Visibility = Visibility.Collapsed;
                    ord3.Visibility = Visibility.Collapsed;
                }
                else if (selectedItem == TableBox.Items[4])
                {
                    TableInWin = "ApplicationGame";
                    dataGrid.ItemsSource = database.GetApplicationGameTable().DefaultView;
                    ((DataView)dataGrid.ItemsSource).Table.AcceptChanges();
                    TableParam.Visibility = Visibility.Collapsed;
                    inputuser.Visibility = Visibility.Collapsed;
                    inputcomputer.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("LoadComputersAsync error: " + ex.Message);
            }
        }
        private void DataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.PropertyName == "Id" || e.PropertyName == "id")
            {
                e.Column.IsReadOnly = true;
                if (e.Column is DataGridTextColumn textColumn)
                {
                    textColumn.Foreground = Brushes.Gray;
                }
            }
        }
    }

    
}
