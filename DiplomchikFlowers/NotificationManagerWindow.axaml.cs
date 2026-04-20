using Avalonia.Controls;
using Avalonia.Controls.Chrome;
using Avalonia.Interactivity;
using DiplomchikFlowers.Model;
using Microsoft.EntityFrameworkCore;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DiplomchikFlowers
{
    public partial class NotificationManagerWindow : Window
    {
        private Customer _currentAdmin;


        public NotificationManagerWindow()
        {
            InitializeComponent();
           
        }

        public NotificationManagerWindow(Customer admin)
        {
            InitializeComponent();
            _currentAdmin = admin;
            RecipientComboBox.SelectionChanged += RecipientComboBox_SelectionChanged;
            LoadUsers();
        }

        private void RecipientComboBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            var selected = RecipientComboBox.SelectedItem as ComboBoxItem;
            bool isSpecificUser = selected?.Content?.ToString() == "Конкретному пользователю";
            UserSelectorPanel.IsVisible = isSpecificUser;
        }

        private async void LoadUsers()
        {
            using var ctx = new DemoContext();
            var users = await ctx.Customers.ToListAsync();
            UserComboBox.ItemsSource = users;
        }

        private async void Send_Click(object? sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TitleBox.Text))
            {
                await ShowMessage("Ошибка", "Введите заголовок", MsBox.Avalonia.Enums.Icon.Error);
                return;
            }
            if (string.IsNullOrWhiteSpace(MessageBox.Text))
            {
                await ShowMessage("Ошибка", "Введите текст сообщения", MsBox.Avalonia.Enums.Icon.Error);
                return;
            }

            bool isToAll = (RecipientComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() == "Всем пользователям";

            using var ctx = new DemoContext();
            if (isToAll)
            {
                var allUsers = await ctx.Customers.ToListAsync();
                foreach (var user in allUsers)
                {
                    var notif = new Notification
                    {
                        CustomerId = user.Id,
                        Title = TitleBox.Text.Trim(),
                        Message = MessageBox.Text.Trim(),
                        CreatedAt = DateTime.Now,
                        IsRead = false
                    };
                    ctx.Notifications.Add(notif);
                }
            }
            else
            {
                var selectedUser = UserComboBox.SelectedItem as Customer;
                if (selectedUser == null)
                {
                    await ShowMessage("Ошибка", "Выберите пользователя", MsBox.Avalonia.Enums.Icon.Error);
                    return;
                }
                var notif = new Notification
                {
                    CustomerId = selectedUser.Id,
                    Title = TitleBox.Text.Trim(),
                    Message = MessageBox.Text.Trim(),
                    CreatedAt = DateTime.Now,
                    IsRead = false
                };
                ctx.Notifications.Add(notif);
            }

            await ctx.SaveChangesAsync();
            await ShowMessage("Успех", "Уведомление отправлено", MsBox.Avalonia.Enums.Icon.Success);
            Close();
        }

        private void Cancel_Click(object? sender, RoutedEventArgs e) => Close();

        private async Task ShowMessage(string title, string message, Icon icon)
        {
            var box = MessageBoxManager.GetMessageBoxStandard(title, message, ButtonEnum.Ok, icon);
            await box.ShowAsync();
        }
    }
}