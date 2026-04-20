using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using BCrypt.Net;
using DiplomchikFlowers.Model;
using Microsoft.EntityFrameworkCore;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DiplomchikFlowers
{
    public partial class ProfileWindow : Window
    {
        private Customer _currentUser;
        private string? _selectedAvatarPath;
        private string? _oldAvatarFileName;
        private bool _avatarRemoved = false;

        public ProfileWindow(Customer user)
        {
            InitializeComponent();
            _currentUser = user;
            LoadUserData();

            FullNameBox.LostFocus += FullNameBox_LostFocus;
            EmailBox.LostFocus += EmailBox_LostFocus;
            PhoneBox.LostFocus += PhoneBox_LostFocus;
            NewPasswordBox.LostFocus += NewPasswordBox_LostFocus;
            ConfirmPasswordBox.LostFocus += ConfirmPasswordBox_LostFocus;
        }

        private void LoadUserData()
        {
            FullNameBox.Text = _currentUser.Fullname;
            EmailBox.Text = _currentUser.Email;
            PhoneBox.Text = _currentUser.Phone;
            CityBox.Text = _currentUser.City;
            AddressBox.Text = _currentUser.Address;
            _oldAvatarFileName = _currentUser.ImagePath;

            if (!string.IsNullOrEmpty(_currentUser.ImagePath))
            {
                string avatarPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", _currentUser.ImagePath);
                if (File.Exists(avatarPath))
                {
                    AvatarImage.Source = new Bitmap(avatarPath);
                    RemoveAvatarButton.IsVisible = true;
                }
            }
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

        private void NewPasswordBox_LostFocus(object? sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(NewPasswordBox.Text) && !IsValidPassword(NewPasswordBox.Text, FullNameBox.Text, EmailBox.Text, out _))
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

        private async void UploadAvatar_Click(object? sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Title = "Выберите аватар",
                AllowMultiple = false,
                Filters = new List<FileDialogFilter>
                {
                    new FileDialogFilter { Name = "Изображения", Extensions = { "jpg", "jpeg", "png", "bmp", "gif" } }
                }
            };
            var result = await dialog.ShowAsync(this);
            if (result != null && result.Length > 0)
            {
                var fileInfo = new FileInfo(result[0]);
                if (fileInfo.Length > 2 * 1024 * 1024)
                {
                    await ShowError("Размер изображения не должен превышать 2 МБ");
                    return;
                }
                _selectedAvatarPath = result[0];
                _avatarRemoved = false;
                try
                {
                    AvatarImage.Source = new Bitmap(_selectedAvatarPath);
                }
                catch (Exception ex)
                {
                    await ShowError($"Не удалось загрузить изображение: {ex.Message}");
                    _selectedAvatarPath = null;
                    return;
                }
                RemoveAvatarButton.IsVisible = true;
            }
        }

        private async void RemoveAvatar_Click(object? sender, RoutedEventArgs e)
        {
            _selectedAvatarPath = null;
            _avatarRemoved = true;
            AvatarImage.Source = null;
            RemoveAvatarButton.IsVisible = false;
        }

        private async void Save_Click(object? sender, RoutedEventArgs e)
        {
            HighlightError(FullNameBox, false);
            HighlightError(EmailBox, false);
            HighlightError(PhoneBox, false);
            HighlightError(NewPasswordBox, false);
            HighlightError(ConfirmPasswordBox, false);
            ErrorTextBlock.IsVisible = false;
            SuccessTextBlock.IsVisible = false;

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

            string currentPassword = CurrentPasswordBox.Text;
            string newPassword = NewPasswordBox.Text;
            string confirmPassword = ConfirmPasswordBox.Text;

            bool passwordChangeRequested = !string.IsNullOrEmpty(currentPassword) ||
                                           !string.IsNullOrEmpty(newPassword) ||
                                           !string.IsNullOrEmpty(confirmPassword);

            if (passwordChangeRequested)
            {
                if (string.IsNullOrEmpty(currentPassword))
                {
                    errors.Add("Введите текущий пароль для смены");
                }
                if (string.IsNullOrEmpty(newPassword))
                {
                    errors.Add("Введите новый пароль");
                }
                else if (!IsValidPassword(newPassword, FullNameBox.Text, EmailBox.Text, out var pwdError))
                {
                    HighlightError(NewPasswordBox, true);
                    errors.Add(pwdError);
                }
                if (newPassword != confirmPassword)
                {
                    HighlightError(ConfirmPasswordBox, true);
                    errors.Add("Пароли не совпадают");
                }
                if (!string.IsNullOrEmpty(currentPassword) && !string.IsNullOrEmpty(newPassword))
                {
                    if (!BCrypt.Net.BCrypt.Verify(currentPassword, _currentUser.Password))
                    {
                        errors.Add("Неверный текущий пароль");
                    }
                }
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

            using var ctx = new DemoContext();
            bool emailExists = await ctx.Customers.AnyAsync(c => c.Email == email && c.Id != _currentUser.Id);
            if (emailExists)
            {
                HighlightError(EmailBox, true);
                await ShowMessage("Ошибка", "Пользователь с таким email уже существует", MsBox.Avalonia.Enums.Icon.Error);
                return;
            }

            string? finalAvatarPath = _oldAvatarFileName;
            if (_selectedAvatarPath != null)
            {
                string avatarsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");
                if (!Directory.Exists(avatarsDir))
                    Directory.CreateDirectory(avatarsDir);

                string ext = Path.GetExtension(_selectedAvatarPath);
                string newFileName = $"avatar_{_currentUser.Id}_{Guid.NewGuid():N}{ext}";
                string destPath = Path.Combine(avatarsDir, newFileName);
                File.Copy(_selectedAvatarPath, destPath, overwrite: true);
                finalAvatarPath = newFileName;
                if (!string.IsNullOrEmpty(_oldAvatarFileName))
                {
                    string oldPath = Path.Combine(avatarsDir, _oldAvatarFileName);
                    if (File.Exists(oldPath))
                        File.Delete(oldPath);
                }
            }
            else if (_avatarRemoved)
            {
                finalAvatarPath = null;
                if (!string.IsNullOrEmpty(_oldAvatarFileName))
                {
                    string oldPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", _oldAvatarFileName);
                    if (File.Exists(oldPath))
                        File.Delete(oldPath);
                }
            }

            var customerToUpdate = await ctx.Customers.FindAsync(_currentUser.Id);
            if (customerToUpdate == null)
            {
                await ShowMessage("Ошибка", "Пользователь не найден", MsBox.Avalonia.Enums.Icon.Error);
                return;
            }

            customerToUpdate.Fullname = fullname;
            customerToUpdate.Email = email;
            customerToUpdate.Phone = phone;
            customerToUpdate.City = city;
            customerToUpdate.Address = address;
            customerToUpdate.ImagePath = finalAvatarPath;
            customerToUpdate.UpdatedAt = DateTime.Now;
            if (passwordChangeRequested && !string.IsNullOrEmpty(newPassword))
                customerToUpdate.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);

            ctx.Entry(customerToUpdate).State = EntityState.Modified;
            await ctx.SaveChangesAsync();

            _currentUser.Fullname = fullname;
            _currentUser.Email = email;
            _currentUser.Phone = phone;
            _currentUser.City = city;
            _currentUser.Address = address;
            _currentUser.ImagePath = finalAvatarPath;
            _currentUser.UpdatedAt = DateTime.Now;
            if (passwordChangeRequested && !string.IsNullOrEmpty(newPassword))
                _currentUser.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);

            _oldAvatarFileName = finalAvatarPath;
            _selectedAvatarPath = null;
            _avatarRemoved = false;

            SuccessTextBlock.Text = "Данные успешно обновлены";
            SuccessTextBlock.IsVisible = true;

            CurrentPasswordBox.Text = "";
            NewPasswordBox.Text = "";
            ConfirmPasswordBox.Text = "";
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
                textBox.BorderBrush = new SolidColorBrush(Color.Parse("#FFE1D5E6"));
                textBox.BorderThickness = new Thickness(1);
            }
        }

        private async Task ShowError(string message)
        {
            ErrorTextBlock.Text = message;
            ErrorTextBlock.IsVisible = true;
            var box = MessageBoxManager.GetMessageBoxStandard("Ошибка", message, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowAsync();
        }

        private async Task ShowMessage(string title, string message, MsBox.Avalonia.Enums.Icon icon)
        {
            var box = MessageBoxManager.GetMessageBoxStandard(title, message, ButtonEnum.Ok, icon);
            await box.ShowAsync();
        }
    }
}