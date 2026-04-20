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
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DiplomchikFlowers
{
    public partial class UserEditWindow : Window
    {
        private Customer _user;
        private bool _isNew;

        public UserEditWindow(Customer? user)
        {
            InitializeComponent();
            _user = user ?? new Customer();
            _isNew = user == null;

            Title = _isNew ? "Добавление пользователя" : "Редактирование пользователя";

            if (!_isNew)
            {
                FullNameBox.Text = _user.Fullname;
                EmailBox.Text = _user.Email;
                PhoneBox.Text = _user.Phone;
                CityBox.Text = _user.City;
                AddressBox.Text = _user.Address;
                string roleName = _user.Roleid switch
                {
                    1 => "Администратор",
                    2 => "Менеджер",
                    _ => "Пользователь"
                };
                foreach (ComboBoxItem item in RoleCombo.Items)
                {
                    if (item.Content?.ToString() == roleName)
                    {
                        RoleCombo.SelectedItem = item;
                        break;
                    }
                }
                PasswordBox.Watermark = "Оставьте пустым, чтобы не менять";
                PasswordBlock.Text = "Новый пароль (если нужно сменить)";
            }

            FullNameBox.LostFocus += FullNameBox_LostFocus;
            EmailBox.LostFocus += EmailBox_LostFocus;
            PhoneBox.LostFocus += PhoneBox_LostFocus;
            PasswordBox.LostFocus += PasswordBox_LostFocus;
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

        private async void Save_Click(object? sender, RoutedEventArgs e)
        {
            HighlightError(FullNameBox, false);
            HighlightError(EmailBox, false);
            HighlightError(PhoneBox, false);
            HighlightError(PasswordBox, false);

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

            if (!_isNew && !string.IsNullOrWhiteSpace(PasswordBox.Text))
            {
                if (!IsValidPassword(PasswordBox.Text, FullNameBox.Text, EmailBox.Text, out var passwordError))
                {
                    HighlightError(PasswordBox, true);
                    errors.Add(passwordError);
                }
            }
            else if (_isNew && string.IsNullOrWhiteSpace(PasswordBox.Text))
            {
                HighlightError(PasswordBox, true);
                errors.Add("Пароль обязателен для нового пользователя");
            }

            if (errors.Any())
            {
                await ShowMessage("Ошибка валидации", string.Join("\n• ", errors.Prepend("Исправьте следующие ошибки:")), MsBox.Avalonia.Enums.Icon.Error);
                return;
            }

            string fullname = FullNameBox.Text?.Trim() ?? "";
            string email = EmailBox.Text?.Trim().ToLower() ?? "";
            string phone = Regex.Replace(PhoneBox.Text, @"[^\d+]", "");
            string city = CityBox.Text?.Trim() ?? "";
            string address = AddressBox.Text?.Trim() ?? "";

            var selectedRoleItem = RoleCombo.SelectedItem as ComboBoxItem;
            int roleId = int.Parse(selectedRoleItem?.Tag?.ToString() ?? "3");

            using var ctx = new DemoContext();
            bool emailExists = await ctx.Customers.AnyAsync(c => c.Email == email && c.Id != _user.Id);
            if (emailExists)
            {
                HighlightError(EmailBox, true);
                await ShowMessage("Ошибка", "Пользователь с таким email уже существует", MsBox.Avalonia.Enums.Icon.Error);
                return;
            }

            if (_isNew)
            {
                _user = new Customer
                {
                    Fullname = fullname,
                    Email = email,
                    Phone = phone,
                    City = city,
                    Address = address,
                    Roleid = roleId,
                    Password = BCrypt.Net.BCrypt.HashPassword(PasswordBox.Text)
                };
                ctx.Customers.Add(_user);
            }
            else
            {
                var dbUser = await ctx.Customers.FindAsync(_user.Id);
                if (dbUser == null)
                {
                    await ShowMessage("Ошибка", "Пользователь не найден", MsBox.Avalonia.Enums.Icon.Error);
                    return;
                }
                dbUser.Fullname = fullname;
                dbUser.Email = email;
                dbUser.Phone = phone;
                dbUser.City = city;
                dbUser.Address = address;
                dbUser.Roleid = roleId;
                dbUser.UpdatedAt = DateTime.Now;
                if (!string.IsNullOrWhiteSpace(PasswordBox.Text))
                    dbUser.Password = BCrypt.Net.BCrypt.HashPassword(PasswordBox.Text);
            }

            await ctx.SaveChangesAsync();
            await ShowMessage("Успешно", "Пользователь сохранён", MsBox.Avalonia.Enums.Icon.Success);
            Close();
        }

        private void Cancel_Click(object? sender, RoutedEventArgs e) => Close();

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
                var addr = new MailAddress(email);
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

        private async Task ShowMessage(string title, string message, MsBox.Avalonia.Enums.Icon icon)
        {
            var box = MessageBoxManager.GetMessageBoxStandard(title, message, ButtonEnum.Ok, icon);
            await box.ShowAsync();
        }
    }
}