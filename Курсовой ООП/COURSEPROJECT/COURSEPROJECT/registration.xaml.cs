using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace COURSEPROJECT 
{
    /// <summary>
    /// Interaction logic for registration.xaml
    /// </summary>
    public partial class registration : Window
    {
        public ICommand AddNewUserCommand { get; }
        public registration()
        {
            InitializeComponent();
            DataContext = this;

            AddNewUserCommand = new RelayCommand(async (p) => await AddNewUserAsync());
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
        }

        private static readonly HashSet<string> ForbiddenUsernames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "user", "пользователь",
            "admin", "админ",
            "administrator", "администратор"
        };

        public async Task AddNewUserAsync()
        {
            if (ForbiddenUsernames.Contains(Login.Text.Trim()))
            {
                MessageBox.Show(Lang.lang == "en"
                    ? "This is a reserved name and cannot be used"
                    : "Это зарезервированное имя, его нельзя использовать");
                return;
            }

            if (string.IsNullOrWhiteSpace(Login.Text) || string.IsNullOrWhiteSpace(Password1.Password))
            {
                MessageBox.Show(Lang.lang == "en"
                    ? "Login and password cannot be empty"
                    : "Логин и пароль не могут быть пустыми");
                return;
            }

            if (Login.Text.Length < 5 || Login.Text.Length > 100)
            {
                MessageBox.Show(Lang.lang == "en"
                    ? "Login must contain between 5 and 99 characters"
                    : "Логин должен содержать от 5 до 99 символов");
                return;
            }

            if (Password1.Password.Length < 8 || Password1.Password.Length > 100)
            {
                MessageBox.Show(Lang.lang == "en"
                    ? "Password must contain between 8 and 99 characters"
                    : "Пароль должен содержать от 8 до 99 символов");
                return;
            }

            if (Password1.Password != Password2.Password)
            {
                MessageBox.Show(Lang.lang == "en"
                    ? "Passwords do not match"
                    : "Пароли не совпадают");
                return;
            }

            var users = await database.ReadElementsInTableUsersAsync();
            if (users.Any(u => u.Name.Equals(Login.Text, StringComparison.OrdinalIgnoreCase)))
            {
                MessageBox.Show(Lang.lang == "en" ? "User already exists" : "Пользователь с таким именем уже существует");
                return;
            }

            try
            {
                await database.AddElementInTableUsers(Login.Text, Password1.Password);
                // Возвращаемся на UI-поток для закрытия окна
                await Dispatcher.InvokeAsync(() =>
                {
                    MessageBox.Show(Lang.lang == "en" ? "Registration successful" : "Регистрация прошла успешно");
                    this.Close();
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine("AddNewUserAsync error: " + ex.Message);
                await Dispatcher.InvokeAsync(() =>
                {
                    MessageBox.Show(Lang.lang == "en" ? $"Error: {ex.Message}" : $"Ошибка: {ex.Message}");
                });
            }
        }
    }
}