using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
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
    public partial class SupplierEditWindow : Window
    {
        private Supplier _supplier;
        private bool _isNew;

        public SupplierEditWindow(Supplier supplier)
        {
            InitializeComponent();
            _supplier = supplier ?? new Supplier();
            _isNew = supplier == null;
            Title = _isNew ? "Добавление поставщика" : "Редактирование поставщика";

            if (!_isNew)
            {
                NameBox.Text = _supplier.Name;
                ContactPersonBox.Text = _supplier.ContactPerson;
                PhoneBox.Text = _supplier.Phone;
                EmailBox.Text = _supplier.Email;
                CityBox.Text = _supplier.City;
                AddressBox.Text = _supplier.Address;
            }

            NameBox.LostFocus += NameBox_LostFocus;
            ContactPersonBox.LostFocus += ContactPersonBox_LostFocus;
            PhoneBox.LostFocus += PhoneBox_LostFocus;
            EmailBox.LostFocus += EmailBox_LostFocus;
        }

        private void NameBox_LostFocus(object? sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(NameBox.Text) && !IsValidSupplierName(NameBox.Text, out _))
                HighlightError(NameBox, NameError);
            else
                ClearError(NameBox, NameError);
        }

        private void ContactPersonBox_LostFocus(object? sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(ContactPersonBox.Text) && !IsValidContactPerson(ContactPersonBox.Text, out _))
                HighlightError(ContactPersonBox, ContactPersonError);
            else
                ClearError(ContactPersonBox, ContactPersonError);
        }

        private void PhoneBox_LostFocus(object? sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(PhoneBox.Text) && !IsValidPhone(PhoneBox.Text, out _))
                HighlightError(PhoneBox, PhoneError);
            else
                ClearError(PhoneBox, PhoneError);
        }

        private void EmailBox_LostFocus(object? sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(EmailBox.Text) && !IsValidEmail(EmailBox.Text, out _))
                HighlightError(EmailBox, EmailError);
            else
                ClearError(EmailBox, EmailError);
        }

        private async void Save_Click(object? sender, RoutedEventArgs e)
        {
            ClearError(NameBox, NameError);
            ClearError(ContactPersonBox, ContactPersonError);
            ClearError(PhoneBox, PhoneError);
            ClearError(EmailBox, EmailError);

            var errors = new List<string>();

            if (!IsValidSupplierName(NameBox.Text, out var nameError))
            {
                HighlightError(NameBox, NameError);
                errors.Add(nameError);
            }

            if (!IsValidContactPerson(ContactPersonBox.Text, out var contactError))
            {
                HighlightError(ContactPersonBox, ContactPersonError);
                errors.Add(contactError);
            }

            if (!IsValidPhone(PhoneBox.Text, out var phoneError))
            {
                HighlightError(PhoneBox, PhoneError);
                errors.Add(phoneError);
            }

            if (!string.IsNullOrWhiteSpace(EmailBox.Text) && !IsValidEmail(EmailBox.Text, out var emailError))
            {
                HighlightError(EmailBox, EmailError);
                errors.Add(emailError);
            }

            if (errors.Any())
            {
                await ShowMessage("Ошибка валидации", string.Join("\n• ", errors.Prepend("Исправьте следующие ошибки:")), MsBox.Avalonia.Enums.Icon.Error);
                return;
            }

            string name = NameBox.Text?.Trim() ?? "";
            string contact = ContactPersonBox.Text?.Trim() ?? "";
            string phone = Regex.Replace(PhoneBox.Text, @"[^\d+]", "");
            string email = string.IsNullOrWhiteSpace(EmailBox.Text) ? null : EmailBox.Text?.Trim().ToLower();
            string city = string.IsNullOrWhiteSpace(CityBox.Text) ? null : CityBox.Text?.Trim();
            string address = string.IsNullOrWhiteSpace(AddressBox.Text) ? null : AddressBox.Text?.Trim();

            using var ctx = new DemoContext();
            bool nameExists = _isNew
                ? await ctx.Suppliers.AnyAsync(s => s.Name == name)
                : await ctx.Suppliers.AnyAsync(s => s.Name == name && s.Id != _supplier.Id);

            if (nameExists)
            {
                HighlightError(NameBox, NameError);
                await ShowMessage("Ошибка", "Поставщик с таким названием уже существует", MsBox.Avalonia.Enums.Icon.Error);
                return;
            }

            if (_isNew)
            {
                _supplier = new Supplier
                {
                    Name = name,
                    ContactPerson = contact,
                    Phone = phone,
                    Email = email,
                    City = city,
                    Address = address,
                    CreatedAt = DateTime.Now
                };
                ctx.Suppliers.Add(_supplier);
            }
            else
            {
                var db = await ctx.Suppliers.FindAsync(_supplier.Id);
                if (db == null)
                {
                    await ShowMessage("Ошибка", "Поставщик не найден", MsBox.Avalonia.Enums.Icon.Error);
                    return;
                }
                db.Name = name;
                db.ContactPerson = contact;
                db.Phone = phone;
                db.Email = email;
                db.City = city;
                db.Address = address;
                db.UpdatedAt = DateTime.Now;
            }

            await ctx.SaveChangesAsync();
            await ShowMessage("Успешно", "Данные сохранены", MsBox.Avalonia.Enums.Icon.Success);
            Close();
        }

        private void Cancel_Click(object? sender, RoutedEventArgs e) => Close();

        private bool IsValidSupplierName(string name, out string errorMessage)
        {
            errorMessage = string.Empty;
            if (string.IsNullOrWhiteSpace(name))
            {
                errorMessage = "Название поставщика обязательно";
                return false;
            }
            name = name.Trim();
            if (name.Length < 3 || name.Length > 150)
            {
                errorMessage = "Название должно содержать от 3 до 150 символов";
                return false;
            }
            if (!Regex.IsMatch(name, @"^[\p{L}\p{N}\s\.,\-&']+$"))
            {
                errorMessage = "Название содержит недопустимые символы";
                return false;
            }
            return true;
        }

        private bool IsValidContactPerson(string contact, out string errorMessage)
        {
            errorMessage = string.Empty;
            if (string.IsNullOrWhiteSpace(contact))
            {
                errorMessage = "Контактное лицо обязательно";
                return false;
            }
            contact = contact.Trim();
            if (contact.Length < 2 || contact.Length > 100)
            {
                errorMessage = "ФИО должно содержать от 2 до 100 символов";
                return false;
            }
            if (!Regex.IsMatch(contact, @"^[\p{L}\s\-'']+$"))
            {
                errorMessage = "ФИО может содержать только буквы, пробелы, дефисы и апострофы";
                return false;
            }
            var parts = contact.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2)
            {
                errorMessage = "Укажите как минимум имя и фамилию";
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

        private bool IsValidEmail(string email, out string errorMessage)
        {
            errorMessage = string.Empty;
            if (string.IsNullOrWhiteSpace(email))
            {
                return true;
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

        private void HighlightError(TextBox textBox, TextBlock errorBlock)
        {
            textBox.BorderBrush = new SolidColorBrush(Colors.Red);
            textBox.BorderThickness = new Thickness(2);
            if (errorBlock != null)
            {
                errorBlock.IsVisible = true;
            }
        }

        private void ClearError(TextBox textBox, TextBlock errorBlock)
        {
            textBox.BorderBrush = new SolidColorBrush(Color.Parse("#FFE5E5E5"));
            textBox.BorderThickness = new Thickness(1);
            if (errorBlock != null)
            {
                errorBlock.IsVisible = false;
            }
        }

        private async Task ShowMessage(string title, string message, MsBox.Avalonia.Enums.Icon icon)
        {
            var box = MessageBoxManager.GetMessageBoxStandard(title, message, ButtonEnum.Ok, icon);
            await box.ShowAsync();
        }
    }
}