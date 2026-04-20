using Avalonia.Controls;
using Avalonia.Interactivity;
using DiplomchikFlowers.Model;
using Microsoft.EntityFrameworkCore;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace DiplomchikFlowers
{
    public partial class NotificationsWindow : Window
    {
        private Customer _currentUser;
        private ObservableCollection<Notification> _notifications;

        public NotificationsWindow()
        {
            InitializeComponent();
        }

        public NotificationsWindow(Customer user)
        {
            InitializeComponent();
            _currentUser = user;
            LoadNotifications();
        }

        private async void LoadNotifications()
        {
            using var ctx = new DemoContext();
            var list = await ctx.Notifications
                .Where(n => n.CustomerId == _currentUser.Id)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
            _notifications = new ObservableCollection<Notification>(list);
            NotificationsItemsControl.ItemsSource = _notifications;
            UpdateEmptyState();
        }

        private void UpdateEmptyState()
        {
            bool hasItems = _notifications != null && _notifications.Any();
            if (NotificationsItemsControl != null)
                NotificationsItemsControl.IsVisible = hasItems;
            if (EmptyPanel != null)
                EmptyPanel.IsVisible = !hasItems;
        }

        private async void Refresh_Click(object? sender, RoutedEventArgs e)
        {
            LoadNotifications();
        }

        private async void MarkAllRead_Click(object? sender, RoutedEventArgs e)
        {
            using var ctx = new DemoContext();
            var unread = await ctx.Notifications
                .Where(n => n.CustomerId == _currentUser.Id && !n.IsRead)
                .ToListAsync();
            foreach (var n in unread)
                n.IsRead = true;
            await ctx.SaveChangesAsync();
            LoadNotifications();
            await ShowMessage("Готово", "Все уведомления отмечены прочитанными", MsBox.Avalonia.Enums.Icon.Info);
        }

        private void Back_Click(object? sender, RoutedEventArgs e)
        {
            Close();
        }

        private async void DeleteNotification_Click(object? sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var notification = btn?.Tag as Notification;
            if (notification == null) return;

            var confirm = await MessageBoxManager.GetMessageBoxStandard("Удаление", "Удалить уведомление?", ButtonEnum.YesNo, MsBox.Avalonia.Enums.Icon.Warning).ShowAsync();
            if (confirm == ButtonResult.Yes)
            {
                using var ctx = new DemoContext();
                ctx.Notifications.Remove(notification);
                await ctx.SaveChangesAsync();
                LoadNotifications();
            }
        }

        private async Task ShowMessage(string title, string message, Icon icon)
        {
            var box = MessageBoxManager.GetMessageBoxStandard(title, message, ButtonEnum.Ok, icon);
            await box.ShowAsync();
        }
    }
}