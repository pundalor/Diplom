using Avalonia.Controls;
using Avalonia.Interactivity;
using DiplomchikFlowers.Model;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System.Linq;
using System.Threading.Tasks;

namespace DiplomchikFlowers
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            PasswordBox.PasswordChar = '*';
            ShowPasswordCheckBox.Checked += (s, e) => PasswordBox.PasswordChar = '\0';
            ShowPasswordCheckBox.Unchecked += (s, e) => PasswordBox.PasswordChar = '*';
        }

        private async void Auth_Click(object? sender, RoutedEventArgs e)
        {
            string password = PasswordBox.Text; 

            if (string.IsNullOrWhiteSpace(LoginBox.Text))
            {
                await ShowError("Поле почты не может быть пустым");
                return;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                await ShowError("Поле пароля не может быть пустым");
                return;
            }

            if (!IsValidEmail(LoginBox.Text))
            {
                await ShowError("Введите корректный адрес электронной почты");
                return;
            }

            using var ctx = new DemoContext();
            var login = LoginBox.Text.Trim();
            var user = ctx.Customers.FirstOrDefault(x => x.Email == login);

            if (user != null && BCrypt.Net.BCrypt.Verify(password, user.Password))
            {
                var catalog = new CatalogWindow(user);
                catalog.Show();
                this.Close();
            }
            else
            {
                await ShowError("Неверный email или пароль");
            }
        }

        private void Guest_Click(object? sender, RoutedEventArgs e)
        {
            var catalog = new CatalogWindow(null);
            catalog.Show();
            this.Close();
        }

        private void ForgotPassword_Click(object? sender, RoutedEventArgs e)
        {
            var forgotWindow = new ForgotPasswordWindow();
            forgotWindow.ShowDialog(this);
        }

        private void Exit_Click(object? sender, RoutedEventArgs e) => this.Close();

        private async Task ShowError(string message)
        {
            var errorBox = MessageBoxManager.GetMessageBoxStandard("Ошибка", message, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await errorBox.ShowAsync();
        }

        private void Register_Click(object? sender, RoutedEventArgs e)
        {
            var registerWindow = new RegisterWindow();
            registerWindow.ShowDialog(this);
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}