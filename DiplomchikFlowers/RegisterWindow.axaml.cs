using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using BCrypt.Net;
using DiplomchikFlowers.Model;
using Microsoft.EntityFrameworkCore;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DiplomchikFlowers
{
    public partial class RegisterWindow : Window
    {
        public RegisterWindow()
        {
            InitializeComponent();

            FullNameBox.LostFocus += FullNameBox_LostFocus;
            EmailBox.LostFocus += EmailBox_LostFocus;
            PhoneBox.LostFocus += PhoneBox_LostFocus;
            PasswordBox.LostFocus += PasswordBox_LostFocus;
            ConfirmPasswordBox.LostFocus += ConfirmPasswordBox_LostFocus;
        }

        private void FullNameBox_LostFocus(object? sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(FullNameBox.Text) && !IsValidFullName(FullNameBox.Text, out _))
                HighlightError(FullNameBox, true);
            else
                HighlightError(FullNameBox, false);
        }

        private void EmailBox_LostFocus(object? sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(EmailBox.Text) && !IsValidEmail(EmailBox.Text, out _))
                HighlightError(EmailBox, true);
            else
                HighlightError(EmailBox, false);
        }

        private void PhoneBox_LostFocus(object? sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(PhoneBox.Text) && !IsValidPhone(PhoneBox.Text, out _))
                HighlightError(PhoneBox, true);
            else
                HighlightError(PhoneBox, false);
        }

        private void PasswordBox_LostFocus(object? sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(PasswordBox.Text) && !IsValidPassword(PasswordBox.Text, FullNameBox.Text, EmailBox.Text, out _))
                HighlightError(PasswordBox, true);
            else
                HighlightError(PasswordBox, false);
        }

        private void ConfirmPasswordBox_LostFocus(object? sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(ConfirmPasswordBox.Text) && PasswordBox.Text != ConfirmPasswordBox.Text)
                HighlightError(ConfirmPasswordBox, true);
            else
                HighlightError(ConfirmPasswordBox, false);
        }

        private async void Register_Click(object? sender, RoutedEventArgs e)
        {
            HighlightError(FullNameBox, false);
            HighlightError(EmailBox, false);
            HighlightError(PhoneBox, false);
            HighlightError(PasswordBox, false);
            HighlightError(ConfirmPasswordBox, false);

            var errors = new List<string>();

            if (!IsValidFullName(FullNameBox.Text, out var fullNameError))
            {
                HighlightError(FullNameBox, true);
                errors.Add(fullNameError);
            }

            if (!IsValidEmail(EmailBox.Text, out var emailError))
            {
                HighlightError(EmailBox, true);
                errors.Add(emailError);
            }

            if (!IsValidPhone(PhoneBox.Text, out var phoneError))
            {
                HighlightError(PhoneBox, true);
                errors.Add(phoneError);
            }

            if (!IsValidPassword(PasswordBox.Text, FullNameBox.Text, EmailBox.Text, out var passwordError))
            {
                HighlightError(PasswordBox, true);
                errors.Add(passwordError);
            }

            if (PasswordBox.Text != ConfirmPasswordBox.Text)
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

            if (await ctx.Customers.AnyAsync(c => c.Email == EmailBox.Text.Trim().ToLower()))
            {
                HighlightError(EmailBox, true);
                await ShowMessage("Ошибка", "Пользователь с таким email уже существует", MsBox.Avalonia.Enums.Icon.Error);
                return;
            }

            try
            {
                string passwordHash = BCrypt.Net.BCrypt.HashPassword(PasswordBox.Text);

                var newCustomer = new Customer
                {
                    Fullname = FullNameBox.Text.Trim(),
                    Email = EmailBox.Text.Trim().ToLower(),
                    Phone = Regex.Replace(PhoneBox.Text, @"[^\d+]", ""),
                    Password = passwordHash,
                    Roleid = 3,
                };

                ctx.Customers.Add(newCustomer);
                await ctx.SaveChangesAsync();

                await ShowMessage("Успешно", "Регистрация прошла успешно! Теперь вы можете войти.", MsBox.Avalonia.Enums.Icon.Success);
                this.Close();
            }
            catch (Exception ex)
            {
                var exec = ex.ToString();
                await ShowMessage("Ошибка сервера", "Не удалось завершить регистрацию. Попробуйте позже.", MsBox.Avalonia.Enums.Icon.Error);
            }
        }

        private void Cancel_Click(object? sender, RoutedEventArgs e) => this.Close();

        private async Task ShowMessage(string title, string message, MsBox.Avalonia.Enums.Icon icon)
        {
            var box = MessageBoxManager.GetMessageBoxStandard(title, message, ButtonEnum.Ok, icon);
            await box.ShowAsync();
        }

        private bool IsValidFullName(string fullName, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(fullName))
            {
                errorMessage = "ФИО обязательно для заполнения";
                return false;
            }

            fullName = fullName.Trim();

            if (fullName.Length < 2 || fullName.Length > 100)
            {
                errorMessage = "ФИО должно содержать от 2 до 100 символов";
                return false;
            }

            if (!Regex.IsMatch(fullName, @"^[\p{L}\s\-'']+$"))
            {
                errorMessage = "ФИО может содержать только буквы, пробелы, дефисы и апострофы";
                return false;
            }

            var parts = fullName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2)
            {
                errorMessage = "Укажите как минимум имя и фамилию";
                return false;
            }

            return true;
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

        private bool IsValidPhone(string phone, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(phone))
            {
                errorMessage = "Телефон обязателен для заполнения";
                return false;
            }

            var cleaned = Regex.Replace(phone, @"[^\d+]", "");

            if (cleaned.StartsWith("8"))
                cleaned = "+7" + cleaned.Substring(1);

            if (!Regex.IsMatch(cleaned, @"^\+7\d{10}$"))
            {
                errorMessage = "Введите телефон в формате +7 (999) 123-45-67";
                return false;
            }

            return true;
        }

        private bool IsValidPassword(string password, string fullName, string email, out string errorMessage)
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

            if (!string.IsNullOrWhiteSpace(fullName) && password.ToLower().Contains(fullName.Split(' ')[0].ToLower()))
            {
                errorMessage = "Пароль не должен содержать ваше имя";
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
                textBox.BorderThickness = new Thickness(2);
            }
        }
    }
}