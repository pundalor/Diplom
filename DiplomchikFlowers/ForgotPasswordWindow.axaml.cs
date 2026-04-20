using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using BCrypt.Net;
using DiplomchikFlowers.Model;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DiplomchikFlowers
{
    public partial class ForgotPasswordWindow : Window
    {
        public ForgotPasswordWindow()
        {
            InitializeComponent();

            EmailBox.LostFocus += EmailBox_LostFocus;
            NewPasswordBox.LostFocus += NewPasswordBox_LostFocus;
            ConfirmPasswordBox.LostFocus += ConfirmPasswordBox_LostFocus;
        }

        private void EmailBox_LostFocus(object? sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(EmailBox.Text) && !IsValidEmail(EmailBox.Text, out _))
                HighlightError(EmailBox, true);
            else
                HighlightError(EmailBox, false);
        }

        private void NewPasswordBox_LostFocus(object? sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(NewPasswordBox.Text) && !IsValidPassword(NewPasswordBox.Text, out _))
                HighlightError(NewPasswordBox, true);
            else
                HighlightError(NewPasswordBox, false);
        }

        private void ConfirmPasswordBox_LostFocus(object? sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(ConfirmPasswordBox.Text) && NewPasswordBox.Text != ConfirmPasswordBox.Text)
                HighlightError(ConfirmPasswordBox, true);
            else
                HighlightError(ConfirmPasswordBox, false);
        }

        private async void ResetPassword_Click(object? sender, RoutedEventArgs e)
        {
            HighlightError(EmailBox, false);
            HighlightError(NewPasswordBox, false);
            HighlightError(ConfirmPasswordBox, false);

            var errors = new List<string>();

            if (!IsValidEmail(EmailBox.Text, out var emailError))
            {
                HighlightError(EmailBox, true);
                errors.Add(emailError);
            }

            if (!IsValidPassword(NewPasswordBox.Text, out var passwordError))
            {
                HighlightError(NewPasswordBox, true);
                errors.Add(passwordError);
            }

            if (NewPasswordBox.Text != ConfirmPasswordBox.Text)
            {
                HighlightError(ConfirmPasswordBox, true);
                errors.Add("Пароли не совпадают");
            }

            if (errors.Any())
            {
                await ShowMessage("Ошибка валидации", string.Join("\n• ", errors.Prepend("Исправьте следующие ошибки:")), MsBox.Avalonia.Enums.Icon.Error);
                return;
            }

            using var ctx = new DemoContext();

            var customer = ctx.Customers.FirstOrDefault(c => c.Email == EmailBox.Text.Trim().ToLower());
            if (customer == null)
            {
                HighlightError(EmailBox, true);
                await ShowMessage("Ошибка", "Пользователь с таким email не найден", MsBox.Avalonia.Enums.Icon.Error);
                return;
            }

            try
            {
                customer.Password = BCrypt.Net.BCrypt.HashPassword(NewPasswordBox.Text);
                await ctx.SaveChangesAsync();

                await ShowMessage("Успешно", "Пароль успешно изменён! Теперь вы можете войти с новым паролем.", MsBox.Avalonia.Enums.Icon.Success);
                this.Close();
            }
            catch (Exception ex)
            {
                await ShowMessage("Ошибка сервера", "Не удалось изменить пароль. Попробуйте позже.", MsBox.Avalonia.Enums.Icon.Error);
            }
        }

        private void Cancel_Click(object? sender, RoutedEventArgs e) => this.Close();

        private async Task ShowMessage(string title, string message, Icon icon)
        {
            var box = MessageBoxManager.GetMessageBoxStandard(title, message, ButtonEnum.Ok, icon);
            await box.ShowAsync();
        }

        private bool IsValidEmail(string email, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(email))
            {
                errorMessage = "Email обязателен для заполнения";
                return false;
            }

            email = email.Trim();

            if (email.Length > 254)
            {
                errorMessage = "Слишком длинный email";
                return false;
            }

            if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                errorMessage = "Некорректный формат email";
                return false;
            }

            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                if (addr.Address != email) return false;
            }
            catch
            {
                errorMessage = "Некорректный формат email";
                return false;
            }

            return true;
        }

        private bool IsValidPassword(string password, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(password))
            {
                errorMessage = "Пароль обязателен для заполнения";
                return false;
            }

            if (password.Length < 8)
            {
                errorMessage = "Пароль должен содержать не менее 8 символов";
                return false;
            }

            if (password.Length > 128)
            {
                errorMessage = "Пароль слишком длинный";
                return false;
            }

            int criteriaMet = 0;
            if (Regex.IsMatch(password, @"[a-zа-яё]")) criteriaMet++;
            if (Regex.IsMatch(password, @"[A-ZА-ЯЁ]")) criteriaMet++;
            if (Regex.IsMatch(password, @"\d")) criteriaMet++;
            if (Regex.IsMatch(password, @"[^a-zA-Z0-9а-яА-Яё]")) criteriaMet++;

            if (criteriaMet < 3)
            {
                errorMessage = "Пароль должен содержать буквы (заглавные и строчные), цифры и спецсимволы";
                return false;
            }

            var commonPasswords = new[] { "password", "123456", "qwerty", "пароль", "111111" };
            if (commonPasswords.Contains(password.ToLower()))
            {
                errorMessage = "Этот пароль слишком простой, выберите более надёжный";
                return false;
            }

            return true;
        }

        private void HighlightError(TextBox textBox, bool hasError)
        {
            if (hasError)
            {
                textBox.BorderBrush = new SolidColorBrush(Colors.Red);
                textBox.BorderThickness = new Thickness(2);
            }
            else
            {
                textBox.BorderBrush = new SolidColorBrush(Color.Parse("#FFD0B4D4"));
                textBox.BorderThickness = new Thickness(1);
            }
        }
    }
}