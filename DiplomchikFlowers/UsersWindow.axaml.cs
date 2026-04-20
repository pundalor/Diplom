using Avalonia.Controls;
using Avalonia.Interactivity;
using DiplomchikFlowers.Model;
using Microsoft.EntityFrameworkCore;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace DiplomchikFlowers
{
    public partial class UsersWindow : Window
    {
        public ObservableCollection<Customer> Users { get; set; } = new();
        public ObservableCollection<string> Roles { get; set; } = new()
        {
            "Пользователь",
            "Менеджер",
            "Администратор"
        };

        public int CurrentUserId { get; set; }

        private List<Customer> _allUsers = new();
        private bool _isLoading = false;

        public UsersWindow()
        {
            InitializeComponent();
            DataContext = this;
            this.Opened += async (_, __) => await LoadUsers();
        }

        public UsersWindow(int currentUserId) : this()
        {
            CurrentUserId = currentUserId;
        }

        private async Task LoadUsers()
        {
            try
            {
                _isLoading = true;
                using var ctx = new DemoContext();
                _allUsers = await ctx.Customers.ToListAsync();
                ApplyFilter();
            }
            catch (Exception ex)
            {
                await ShowMessage("Ошибка", ex.Message, MsBox.Avalonia.Enums.Icon.Error);
            }
            finally
            {
                _isLoading = false;
            }
        }

        private void ApplyFilter()
        {
            string search = SearchBox?.Text?.ToLower() ?? "";
            Users.Clear();

            var filtered = string.IsNullOrWhiteSpace(search)
                ? _allUsers
                : _allUsers.Where(u =>
                    u.Fullname.ToLower().Contains(search) ||
                    u.Email.ToLower().Contains(search) ||
                    u.Phone.Contains(search)).ToList();

            foreach (var user in filtered)
            {
                user.RoleName = (user.Roleid ?? 3) switch
                {
                    1 => "Администратор",
                    2 => "Менеджер",
                    _ => "Пользователь"
                };
                user.CanDelete = (CurrentUserId > 0 && user.Id != CurrentUserId);
                Users.Add(user);
            }
        }

        private void SearchBox_TextChanged(object? sender, TextChangedEventArgs e) => ApplyFilter();

        private async void AddUser_Click(object? sender, RoutedEventArgs e)
        {
            var win = new UserEditWindow(null);
            await win.ShowDialog(this);
            await LoadUsers();
        }

        private async void EditUser_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is Button { Tag: Customer user })
            {
                var win = new UserEditWindow(user);
                await win.ShowDialog(this);
                await LoadUsers();
            }
        }

        private async void DeleteUser_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is not Button { Tag: Customer user }) return;

            if (user.Id == CurrentUserId)
            {
                await ShowMessage("Нельзя удалить",
                    "Вы не можете удалить самого себя.",
                    MsBox.Avalonia.Enums.Icon.Warning);
                return;
            }

            var confirm = MessageBoxManager.GetMessageBoxStandard(
                "Удаление",
                $"Удалить пользователя {user.Fullname}?\n\n" +
                $"⚠️ Все данные пользователя (заказы, корзина, избранное, отзывы, уведомления) будут безвозвратно удалены.",
                ButtonEnum.YesNo,
                MsBox.Avalonia.Enums.Icon.Warning);

            if (await confirm.ShowAsync() != ButtonResult.Yes) return;

            try
            {
                using var ctx = new DemoContext();

                var dbUser = await ctx.Customers
                    .Include(c => c.Orders)
                    .Include(c => c.Carts)
                    .Include(c => c.Favorites)
                    .Include(c => c.Reviews)
                    .Include(c => c.Notifications)
                    .FirstOrDefaultAsync(c => c.Id == user.Id);

                if (dbUser == null)
                {
                    await ShowMessage("Ошибка", "Пользователь не найден в базе данных", MsBox.Avalonia.Enums.Icon.Error);
                    return;
                }

                ctx.Notifications.RemoveRange(dbUser.Notifications);
                ctx.Reviews.RemoveRange(dbUser.Reviews);
                ctx.Favorites.RemoveRange(dbUser.Favorites);

                foreach (var cart in dbUser.Carts.ToList())
                {
                    var cartItems = ctx.CartItems.Where(ci => ci.CartId == cart.Id).ToList();
                    ctx.CartItems.RemoveRange(cartItems);
                    ctx.Carts.Remove(cart);
                }

                foreach (var order in dbUser.Orders.ToList())
                {
                    var orderItems = ctx.OrderItems.Where(oi => oi.OrderId == order.Id).ToList();
                    ctx.OrderItems.RemoveRange(orderItems);
                    ctx.Orders.Remove(order);
                }

                ctx.Customers.Remove(dbUser);
                await ctx.SaveChangesAsync();

                await ShowMessage("Успешно", $"Пользователь {user.Fullname} удалён", MsBox.Avalonia.Enums.Icon.Success);
                await LoadUsers();
            }
            catch (DbUpdateException ex)
            {
                await ShowMessage("Ошибка удаления",
                    "Не удалось удалить пользователя. Возможно, есть связанные записи в базе данных.\n" +
                    $"Детали: {ex.InnerException?.Message ?? ex.Message}",
                    MsBox.Avalonia.Enums.Icon.Error);
            }
            catch (Exception ex)
            {
                await ShowMessage("Ошибка", $"Произошла ошибка: {ex.Message}", MsBox.Avalonia.Enums.Icon.Error);
            }
        }

        private async void RoleCombo_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (_isLoading) return;
            if (e.AddedItems.Count == 0) return;

            var combo = sender as ComboBox;
            var user = combo?.Tag as Customer;
            if (user == null) return;

            string role = combo.SelectedItem as string;
            int roleId = role switch
            {
                "Администратор" => 1,
                "Менеджер" => 2,
                _ => 3
            };

            using var ctx = new DemoContext();
            var dbUser = await ctx.Customers.FindAsync(user.Id);
            if (dbUser != null && dbUser.Roleid != roleId)
            {
                dbUser.Roleid = roleId;
                await ctx.SaveChangesAsync();
                user.RoleName = role;
            }
        }

        private async void Refresh_Click(object? sender, RoutedEventArgs e) => await LoadUsers();

        private void Back_Click(object? sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async Task ShowMessage(string title, string message, Icon icon)
        {
            var box = MessageBoxManager.GetMessageBoxStandard(title, message, ButtonEnum.Ok, icon);
            await box.ShowAsync();
        }
    }
}