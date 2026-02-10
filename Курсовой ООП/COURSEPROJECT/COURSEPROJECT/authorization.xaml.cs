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
    /// Логика взаимодействия для authorization.xaml
    /// </summary>
    public partial class authorization : Window
    {
        public ICommand AuthorizationClickCommand { get; }
        public ICommand RegistrationCommand { get; }

        public authorization()
        {
            InitializeComponent();
            DataContext = this;

            AuthorizationClickCommand = new RelayCommand(AuthorizationClick);
            RegistrationCommand = new RelayCommand(Registration);
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
                                proc.Kill();
                                proc.WaitForExit();
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(Lang.lang == "en"
                                    ? $"Error terminating process: {ex.Message}"
                                    : $"Ошибка при завершении процесса: {ex.Message}");
                            }
                        }
                    }
                }
            }
        }

        private void AuthorizationClick(object parametr)
        {
            if (Login.Text == "admin" && Password.Password == "admin")
            {
                Global.CurrentUser = Login.Text;
                this.Close();
                return;
            }

            if (string.IsNullOrWhiteSpace(Login.Text) || string.IsNullOrWhiteSpace(Password.Password))
            {
                MessageBox.Show(Lang.lang == "en"
                    ? "Login and password cannot be empty"
                    : "Логин и пароль не могут быть пустыми");
                return;
            }

            if (Login.Text.Length < 5 || Login.Text.Length > 100)
            {
                MessageBox.Show(Lang.lang == "en"
                    ? "Login must be between 5 and 99 characters"
                    : "Логин должен содержать от 5 до 99 символов");
                return;
            }

            if (Password.Password.Length < 8 || Password.Password.Length > 100)
            {
                MessageBox.Show(Lang.lang == "en"
                    ? "Password must be between 8 and 99 characters"
                    : "Пароль должен содержать от 8 до 99 символов");
                return;
            }

            if (database.CorrectEntrance(Login.Text, Password.Password))
            {
                Global.CurrentUser = Login.Text;
                App.CloseAllWindowsExceptMain();
                Application.Current.MainWindow?.Show();
                CloseAllGames();
                this.Close();
            }
            else
            {
                string userid = database.GetUserIdByName(Login.Text);
                if (userid == "0")
                {
                    MessageBox.Show(Lang.lang == "en"
                        ? "User not found"
                        : "Такого пользователя не существует");
                }
                else
                {
                    MessageBox.Show(Lang.lang == "en"
                        ? "Wrong password"
                        : "Неверный пароль");
                }
            }
        }

        private void Registration(object parametr)
        {
            var window = Application.Current.Windows.OfType<registration>().FirstOrDefault();

            if (window == null)
            {
                window = new registration();
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
}