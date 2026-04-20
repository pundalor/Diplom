using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using DiplomchikFlowers.Model;
using Microsoft.EntityFrameworkCore;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DiplomchikFlowers
{
    public class OrderStatusBrushConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value switch
            {
                1 => new SolidColorBrush(Color.Parse("#FFF39C12")),
                2 => new SolidColorBrush(Color.Parse("#FF3498DB")),
                3 => new SolidColorBrush(Color.Parse("#FF9B59B6")),
                4 => new SolidColorBrush(Color.Parse("#FF27AE60")),
                5 => new SolidColorBrush(Color.Parse("#FFE74C3C")),
                6 => new SolidColorBrush(Color.Parse("#FF95A5A6")),
                _ => new SolidColorBrush(Color.Parse("#FF777777"))
            };
        }
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class OrderStatusTextConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value switch
            {
                1 => "Ожидает оплаты",
                2 => "Оплачен",
                3 => "В доставке",
                4 => "Доставлен",
                5 => "Отменён",
                6 => "Возврат",
                _ => "Неизвестно"
            };
        }
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class OrderDisplay
    {
        public Order Order { get; set; }
        public string OrderNumber => $"#{Order.Id:D5}";
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public string ShippingAddress { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public int StatusId { get; set; }
        public string StatusName { get; set; }
        public int ItemsCount { get; set; }
    }

    public partial class OrdersWindow : Window
    {
        public ObservableCollection<OrderDisplay> Orders { get; set; } = new();
        private List<Order> _allOrders = new();
        private List<OrderDisplay> _filteredOrders = new();
        private List<OrderStatus> _allStatuses = new();
        private Customer _currentUser;
        private int _currentPage = 1;
        private const int PageSize = 15;
        private int _totalPages = 1;
        private int? _statusFilter = null;

        public bool IsAdmin => _currentUser?.Roleid == 1;
        public bool IsManager => _currentUser?.Roleid == 2;
        public bool CanManageOrders => IsAdmin || IsManager;
        public bool CanDeleteOrders => IsAdmin;
        public bool CanExport => IsAdmin;
        public bool CanViewStats => IsAdmin || IsManager;

        private static readonly HashSet<int> DeductedStatuses = new() { 2, 3, 4 };
        private static readonly HashSet<int> ReturnStatuses = new() { 5, 6 };

        public OrdersWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        public OrdersWindow(Customer currentUser) : this()
        {
            _currentUser = currentUser;
            this.Opened += async (_, __) => await LoadOrders();
        }

        protected override void OnOpened(EventArgs e)
        {
            base.OnOpened(e);
            UpdateUIBasedOnRole();
        }

        private void UpdateUIBasedOnRole()
        {
            if (this.FindControl<Button>("DeleteButton") is Button deleteBtn)
                deleteBtn.IsVisible = CanDeleteOrders;

            if (this.FindControl<Button>("ExportButton") is Button exportBtn)
                exportBtn.IsVisible = CanExport;

            if (this.FindControl<Button>("StatsButton") is Button statsBtn)
                statsBtn.IsVisible = CanViewStats;
        }

        private async Task LoadOrders()
        {
            try
            {
                using var ctx = new DemoContext();

                _allStatuses = await ctx.OrderStatuses.OrderBy(s => s.SortOrder).ToListAsync();
                IQueryable<Order> query = ctx.Orders
                    .Include(o => o.Customer)
                    .Include(o => o.Status)
                    .Include(o => o.OrderItems);

                if (!IsAdmin && _currentUser != null)
                {
                    query = query.Where(o => o.CustomerId == _currentUser.Id);
                }

                _allOrders = await query.OrderByDescending(o => o.OrderDate).ToListAsync();

                PopulateStatusFilter();
                ApplyFilters();
            }
            catch (Exception ex)
            {
                await ShowMessage("Ошибка", $"Не удалось загрузить заказы: {ex.Message}", MsBox.Avalonia.Enums.Icon.Error);
            }
        }

        private void PopulateStatusFilter()
        {
            StatusFilterCombo.Items.Clear();
            StatusFilterCombo.Items.Add(new ComboBoxItem { Content = "Все статусы", Tag = -1, IsSelected = true });

            foreach (var status in _allStatuses)
            {
                StatusFilterCombo.Items.Add(new ComboBoxItem
                {
                    Content = status.Name,
                    Tag = status.Id
                });
            }
        }

        private void ApplyFilters()
        {
            string search = SearchBox?.Text?.ToLower().Trim() ?? "";
            var query = _allOrders.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(o =>
                    (o.Customer.Fullname != null && o.Customer.Fullname.ToLower().Contains(search)) ||
                    (o.Customer.Email != null && o.Customer.Email.ToLower().Contains(search)) ||
                    (o.Customer.Phone != null && o.Customer.Phone.Contains(search)) ||
                    (o.ShippingAddress != null && o.ShippingAddress.ToLower().Contains(search)));
            }

            if (_statusFilter.HasValue && _statusFilter.Value > 0)
            {
                query = query.Where(o => o.StatusId == _statusFilter.Value);
            }

            _filteredOrders = query.ToList()
                .Select(o => new OrderDisplay
                {
                    Order = o,
                    CustomerName = o.Customer.Fullname ?? "",
                    CustomerEmail = o.Customer.Email ?? "",
                    ShippingAddress = $"{o.City ?? ""}, {o.ShippingAddress ?? ""}".Trim().Trim(','),
                    OrderDate = o.OrderDate ?? o.CreatedAt ?? DateTime.MinValue,
                    TotalAmount = o.TotalAmount,
                    StatusId = o.StatusId,
                    StatusName = o.Status != null ? o.Status.Name : "Неизвестно",
                    ItemsCount = o.OrderItems != null ? o.OrderItems.Sum(oi => oi.Quantity) : 0
                }).ToList();

            _totalPages = Math.Max(1, (int)Math.Ceiling((double)_filteredOrders.Count / PageSize));
            if (_currentPage > _totalPages) _currentPage = _totalPages;

            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            CountTextBlock.Text = $"Найдено заказов: {_filteredOrders.Count}";
            PageInfoTextBlock.Text = $"Страница {_currentPage} из {_totalPages}";
            PrevPageButton.IsEnabled = _currentPage > 1;
            NextPageButton.IsEnabled = _currentPage < _totalPages;

            var pagedOrders = _filteredOrders
                .Skip((_currentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            Orders.Clear();
            foreach (var order in pagedOrders)
            {
                Orders.Add(order);
            }
            OrdersItemsControl.ItemsSource = Orders;
        }

        private void SearchBox_TextChanged(object? sender, TextChangedEventArgs e)
        {
            _currentPage = 1;
            ApplyFilters();
        }

        private void StatusFilterCombo_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (StatusFilterCombo?.SelectedItem is ComboBoxItem item)
            {
                _statusFilter = int.Parse(item.Tag?.ToString() ?? "-1");
                _currentPage = 1;
                ApplyFilters();
            }
        }

        private async void Refresh_Click(object? sender, RoutedEventArgs e)
        {
            e.Handled = true;
            await LoadOrders();
            await ShowMessage("Обновлено", "Список заказов обновлён", MsBox.Avalonia.Enums.Icon.Success);
        }

        private void Back_Click(object? sender, RoutedEventArgs e)
        {
            e.Handled = true;
            Close();
        }

        private void PrevPage_Click(object? sender, RoutedEventArgs e)
        {
            e.Handled = true;
            if (_currentPage > 1)
            {
                _currentPage--;
                UpdateDisplay();
            }
        }

        private void NextPage_Click(object? sender, RoutedEventArgs e)
        {
            e.Handled = true;
            if (_currentPage < _totalPages)
            {
                _currentPage++;
                UpdateDisplay();
            }
        }

        private async void ViewOrder_Click(object? sender, RoutedEventArgs e)
        {
            e.Handled = true;
            if (sender is Button { Tag: OrderDisplay orderDisplay })
            {
                await ShowOrderDetails(orderDisplay.Order);
            }
        }

        private async Task ShowOrderDetails(Order order)
        {
            using var ctx = new DemoContext();
            var fullOrder = await ctx.Orders
                .Include(o => o.Customer)
                .Include(o => o.Status)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == order.Id);

            if (fullOrder == null) return;

            var logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", "rose.ico");
            string logoText = File.Exists(logoPath) ? "🌹" : "🌸";

            var itemsRows = fullOrder.OrderItems.Select(oi =>
            {
                var productName = oi.Product != null ? oi.Product.Name : $"Товар #{oi.ProductId}";
                var subtotal = oi.Quantity * oi.PriceAtTime;
                return $"   ├─ {productName}\n      {oi.Quantity} шт. × {oi.PriceAtTime:C} = {subtotal:C}";
            });
            var itemsText = string.Join("\n", itemsRows);

            var customerName = fullOrder.Customer != null ? fullOrder.Customer.Fullname : "Неизвестно";
            var customerEmail = fullOrder.Customer != null ? fullOrder.Customer.Email : "";
            var customerPhone = fullOrder.Customer != null ? fullOrder.Customer.Phone : "";
            var statusName = fullOrder.Status != null ? fullOrder.Status.Name : "Неизвестно";
            var orderDate = fullOrder.OrderDate ?? fullOrder.CreatedAt ?? DateTime.MinValue;
            var city = fullOrder.City ?? "";
            var address = fullOrder.ShippingAddress ?? "";
            var notes = fullOrder.Notes ?? "";

            var statusBadge = fullOrder.StatusId switch
            {
                1 => "⏳",
                2 => "✅",
                3 => "🚚",
                4 => "📦",
                5 => "❌",
                6 => "↩️",
                _ => "❓"
            };

            var detailsText = $"{logoText}  FlowerShop  {logoText}\n" +
                $"━━━━━━━━━━━━━━━━━━━━━━\n" +
                $"📋 Заказ #{fullOrder.Id:D5}\n" +
                $"━━━━━━━━━━━━━━━━━━━━━━\n\n" +
                $"👤 Клиент:\n" +
                $"   {customerName}\n" +
                $"   ✉ {customerEmail}\n" +
                $"   ☎ {customerPhone}\n\n" +
                $"📅 Дата: {orderDate:dd.MM.yyyy HH:mm}\n" +
                $"{statusBadge} Статус: {statusName}\n" +
                $"📍 Адрес: {city}, {address}\n\n" +
                $"🛒 Товары:\n{itemsText}\n\n" +
                $"━━━━━━━━━━━━━━━━━━━━━━\n" +
                $"💰 Итого: {fullOrder.TotalAmount:C}\n" +
                $"━━━━━━━━━━━━━━━━━━━━━━";

            if (!string.IsNullOrWhiteSpace(notes))
            {
                detailsText += $"\n\n📝 Заметки:\n   {notes.Replace("\n", "\n   ")}";
            }

            await ShowMessage("Детали заказа", detailsText, MsBox.Avalonia.Enums.Icon.Info);
        }

        private async Task<bool> ProcessOrderCancellation(Order order, int newStatusId)
        {
            if (ReturnStatuses.Contains(newStatusId) && !ReturnStatuses.Contains(order.StatusId))
            {
                using var ctx = new DemoContext();

                var dbOrder = await ctx.Orders
                    .Include(o => o.OrderItems)
                    .FirstOrDefaultAsync(o => o.Id == order.Id);

                if (dbOrder == null) return false;

                foreach (var item in dbOrder.OrderItems.ToList())
                {
                    var product = await ctx.Products.FindAsync(item.ProductId);
                    if (product != null)
                    {
                        product.StockQuantity += item.Quantity;
                        product.UpdatedAt = DateTime.Now;
                    }
                }

                ctx.OrderItems.RemoveRange(dbOrder.OrderItems);

                await ctx.SaveChangesAsync();
                return true;
            }
            return true;
        }

        private async void EditStatus_Click(object? sender, RoutedEventArgs e)
        {
            e.Handled = true;
            if (sender is not Button { Tag: OrderDisplay orderDisplay }) return;

            if (!CanManageOrders)
            {
                await ShowMessage("Доступ запрещён", "Только администратор и менеджер могут изменять статусы заказов.", MsBox.Avalonia.Enums.Icon.Warning);
                return;
            }

            var statusWindow = new StatusSelectionWindow(_allStatuses, orderDisplay.StatusId);
            var newStatusId = await statusWindow.ShowDialog<int?>(this);

            if (newStatusId.HasValue)
            {
                try
                {
                    bool stockProcessed = await ProcessOrderCancellation(orderDisplay.Order, newStatusId.Value);

                    if (!stockProcessed)
                    {
                        await ShowMessage("Ошибка", "Не удалось обработать возврат товаров", MsBox.Avalonia.Enums.Icon.Error);
                        return;
                    }

                    using var ctx = new DemoContext();
                    var dbOrder = await ctx.Orders.FindAsync(orderDisplay.Order.Id);
                    if (dbOrder != null)
                    {
                        dbOrder.StatusId = newStatusId.Value;
                        dbOrder.UpdatedAt = DateTime.Now;
                        await ctx.SaveChangesAsync();
                        await LoadOrders();

                        if (ReturnStatuses.Contains(newStatusId.Value))
                        {
                            await ShowMessage("Успешно",
                                $"Статус заказа обновлён на «{_allStatuses.FirstOrDefault(s => s.Id == newStatusId.Value)?.Name}».\n" +
                                $"Товары возвращены на склад, позиции заказа удалены.",
                                MsBox.Avalonia.Enums.Icon.Success);
                        }
                        else
                        {
                            await ShowMessage("Успешно", "Статус заказа обновлён", MsBox.Avalonia.Enums.Icon.Success);
                        }
                    }
                }
                catch (Exception ex)
                {
                    await ShowMessage("Ошибка", $"Не удалось обновить статус: {ex.Message}", MsBox.Avalonia.Enums.Icon.Error);
                }
            }
        }

        private async void DeleteOrder_Click(object? sender, RoutedEventArgs e)
        {
            e.Handled = true;
            if (sender is not Button { Tag: OrderDisplay orderDisplay }) return;

            if (!CanDeleteOrders)
            {
                await ShowMessage("Доступ запрещён", "Только администратор может удалять заказы.", MsBox.Avalonia.Enums.Icon.Warning);
                return;
            }

            var confirm = await MessageBoxManager.GetMessageBoxStandard(
                "Удаление заказа",
                $"Удалить заказ №{orderDisplay.Order.Id} на сумму {orderDisplay.TotalAmount:C}?\n\n" +
                $"⚠️ Это действие нельзя отменить. Все позиции заказа будут удалены.",
                ButtonEnum.YesNo,
                MsBox.Avalonia.Enums.Icon.Warning).ShowAsync();

            if (confirm != ButtonResult.Yes) return;

            try
            {
                using var ctx = new DemoContext();
                var dbOrder = await ctx.Orders
                    .Include(o => o.OrderItems)
                    .FirstOrDefaultAsync(o => o.Id == orderDisplay.Order.Id);

                if (dbOrder == null)
                {
                    await ShowMessage("Ошибка", "Заказ не найден", MsBox.Avalonia.Enums.Icon.Error);
                    return;
                }

                foreach (var item in dbOrder.OrderItems.ToList())
                {
                    var product = await ctx.Products.FindAsync(item.ProductId);
                    if (product != null)
                    {
                        product.StockQuantity += item.Quantity;
                        product.UpdatedAt = DateTime.Now;
                    }
                }

                ctx.OrderItems.RemoveRange(dbOrder.OrderItems);
                ctx.Orders.Remove(dbOrder);
                await ctx.SaveChangesAsync();

                await LoadOrders();
                await ShowMessage("Удалено", $"Заказ #{orderDisplay.Order.Id:D5} удалён. Товары возвращены на склад.", MsBox.Avalonia.Enums.Icon.Success);
            }
            catch (Exception ex)
            {
                await ShowMessage("Ошибка", $"Не удалось удалить заказ: {ex.Message}", MsBox.Avalonia.Enums.Icon.Error);
            }
        }

        private async void ShowStats_Click(object? sender, RoutedEventArgs e)
        {
            e.Handled = true;
            using var ctx = new DemoContext();
            var stats = await ctx.Orders
                .GroupBy(o => o.StatusId)
                .Select(g => new { StatusId = g.Key, Count = g.Count(), Total = g.Sum(o => o.TotalAmount) })
                .ToListAsync();

            var totalRevenue = _allOrders.Where(o => o.StatusId == 2).Sum(o => o.TotalAmount);

            var logoText = "🌹";

            var statsText = $"{logoText}  Статистика заказов  {logoText}\n" +
                $"━━━━━━━━━━━━━━━━━━━━━━\n\n";

            foreach (var stat in stats.OrderBy(s => s.StatusId))
            {
                var status = _allStatuses.FirstOrDefault(st => st.Id == stat.StatusId);
                var statusName = status != null ? status.Name : $"Статус #{stat.StatusId}";
                var statusIcon = stat.StatusId switch
                {
                    1 => "⏳",
                    2 => "✅",
                    3 => "🚚",
                    4 => "📦",
                    5 => "❌",
                    6 => "↩️",
                    _ => "📋"
                };
                statsText += $"{statusIcon} {statusName}:\n   {stat.Count} заказов • {stat.Total:C}\n\n";
            }

            statsText += $"━━━━━━━━━━━━━━━━━━━━━━\n" +
                $"💰 Выручка (оплачено): {totalRevenue:C}\n" +
                $"━━━━━━━━━━━━━━━━━━━━━━";

            await ShowMessage("Статистика", statsText, MsBox.Avalonia.Enums.Icon.Info);
        }

        private async void Export_Click(object? sender, RoutedEventArgs e)
        {
            e.Handled = true;
            if (!CanExport)
            {
                await ShowMessage("Доступ запрещён", "Только администратор может экспортировать данные.", MsBox.Avalonia.Enums.Icon.Warning);
                return;
            }

            try
            {
                var csv = "ID;Дата;Клиент;Email;Телефон;Адрес;Статус;Сумма;Товаров\n";
                foreach (var order in _filteredOrders)
                {
                    using var ctx = new DemoContext();
                    var customer = await ctx.Customers.FindAsync(order.Order.CustomerId);
                    var status = _allStatuses.FirstOrDefault(s => s.Id == order.StatusId);
                    var statusName = status != null ? status.Name : "Неизвестно";
                    var phone = customer != null ? customer.Phone : "";

                    csv += $"{order.Order.Id};{order.OrderDate:dd.MM.yyyy HH:mm};{order.CustomerName};{order.CustomerEmail};{phone};{order.ShippingAddress};{statusName};{order.TotalAmount};{order.ItemsCount}\n";
                }

                var saveDialog = new SaveFileDialog
                {
                    Title = "Экспорт заказов",
                    DefaultExtension = ".csv",
                    Filters = new List<FileDialogFilter>
                    {
                        new FileDialogFilter { Name = "CSV файл", Extensions = { "csv" } }
                    }
                };

                var path = await saveDialog.ShowAsync(this);
                if (!string.IsNullOrEmpty(path))
                {
                    await System.IO.File.WriteAllTextAsync(path, csv, System.Text.Encoding.UTF8);
                    await ShowMessage("Экспорт", $"Заказы экспортированы в {path}", MsBox.Avalonia.Enums.Icon.Success);
                }
            }
            catch (Exception ex)
            {
                await ShowMessage("Ошибка", $"Не удалось экспортировать: {ex.Message}", MsBox.Avalonia.Enums.Icon.Error);
            }
        }

        private async Task ShowMessage(string title, string message, MsBox.Avalonia.Enums.Icon icon)
        {
            var box = MessageBoxManager.GetMessageBoxStandard(title, message, ButtonEnum.Ok, icon);
            await box.ShowAsync();
        }
    }
}