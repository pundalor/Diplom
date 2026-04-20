using Avalonia.Controls;
using Avalonia.Interactivity;
using DiplomchikFlowers.Model;
using Microsoft.EntityFrameworkCore;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace DiplomchikFlowers
{
    public partial class SupplierWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            Debug.WriteLine($"[PropertyChanged] {propertyName}");
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private ObservableCollection<Supplier> _allSuppliers = new();
        public ObservableCollection<Supplier> Suppliers { get; } = new();
        public ObservableCollection<Supply> Supplies { get; } = new();

        private Supplier _selectedSupplier;
        public Supplier SelectedSupplier
        {
            get => _selectedSupplier;
            set
            {
                if (_selectedSupplier == value) return;
                _selectedSupplier = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsSupplierSelected));
                if (value != null)
                    _ = LoadSupplies(value.Id);
                else
                    Supplies.Clear();
            }
        }

        public bool IsSupplierSelected => SelectedSupplier != null;

        public SupplierWindow()
        {
            Debug.WriteLine("[SupplierWindow] Constructor start");
            InitializeComponent();
            DataContext = this;
            this.Opened += async (_, _) => await LoadSuppliers();
            Debug.WriteLine("[SupplierWindow] Constructor end");
        }

        public SupplierWindow(Customer currentUser) : this() { }

        private async Task LoadSuppliers()
        {
            try
            {
                using var ctx = new DemoContext();
                var list = await ctx.Suppliers.ToListAsync();
                _allSuppliers.Clear();
                foreach (var s in list) _allSuppliers.Add(s);
                ApplyFilter();
            }
            catch (Exception ex)
            {
                await ShowMessage("Ошибка", $"Не удалось загрузить поставщиков: {ex.Message}", MsBox.Avalonia.Enums.Icon.Error);
            }
        }

        private async Task LoadSupplies(int supplierId)
        {
            try
            {
                using var ctx = new DemoContext();
                var list = await ctx.Supplies.Where(s => s.SupplierId == supplierId).ToListAsync();
                Supplies.Clear();
                foreach (var s in list) Supplies.Add(s);
            }
            catch (Exception ex)
            {
                await ShowMessage("Ошибка", ex.Message, MsBox.Avalonia.Enums.Icon.Error);
            }
        }

        private void SearchBox_TextChanged(object? sender, TextChangedEventArgs e) => ApplyFilter();

        private void ApplyFilter()
        {
            var searchText = SearchBox?.Text?.Trim().ToLower() ?? "";
            Suppliers.Clear();
            IEnumerable<Supplier> source = string.IsNullOrWhiteSpace(searchText)
                ? _allSuppliers
                : _allSuppliers.Where(s => s.Name.ToLower().Contains(searchText) || (s.City != null && s.City.ToLower().Contains(searchText)));
            foreach (var s in source) Suppliers.Add(s);
        }

        private async void AddSupply_Click(object? sender, RoutedEventArgs e)
        {
            if (SelectedSupplier == null)
            {
                await ShowMessage("Внимание", "Сначала выберите поставщика", MsBox.Avalonia.Enums.Icon.Warning);
                return;
            }
            var win = new SupplyEditWindow(null, SelectedSupplier.Id);
            await win.ShowDialog(this);
            await LoadSupplies(SelectedSupplier.Id);
        }

        private async void ViewSupplyDetails_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is Button { Tag: Supply supply })
            {
                var win = new SupplyDetailsWindow(supply);
                await win.ShowDialog(this);
            }
        }

        private async void EditSupply_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is Button { Tag: Supply supply })
            {
                var win = new SupplyEditWindow(supply);
                await win.ShowDialog(this);
                await LoadSupplies(supply.SupplierId);
            }
        }

        private async void DeleteSupply_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is not Button { Tag: Supply supply }) return;

            var confirm = await MessageBoxManager.GetMessageBoxStandard(
                "Удаление поставки",
                $"Удалить поставку №{supply.Id}? Это также удалит все товары из поставки и скорректирует остатки.",
                ButtonEnum.YesNo,
                MsBox.Avalonia.Enums.Icon.Warning).ShowAsync();

            if (confirm != ButtonResult.Yes) return;

            using var ctx = new DemoContext();

            var items = await ctx.SupplyItems
                .Include(si => si.Product)
                .Where(i => i.SupplyId == supply.Id)
                .ToListAsync();

            foreach (var item in items)
            {
                var product = item.Product ?? await ctx.Products.FindAsync(item.ProductId);
                if (product != null && product.StockQuantity < item.Quantity)
                {
                    await ShowMessage("Ошибка",
                        $"Невозможно удалить поставку: товар «{product.Name}» имеет остаток {product.StockQuantity} шт., " +
                        $"а поставка содержит {item.Quantity} шт. Удаление приведёт к отрицательному остатку.\n\n" +
                        "Возможно, товар уже был продан. Проверьте историю продаж перед удалением поставки.",
                        MsBox.Avalonia.Enums.Icon.Error);
                    return;
                }
            }
            foreach (var item in items)
            {
                var product = item.Product ?? await ctx.Products.FindAsync(item.ProductId);
                if (product != null)
                {
                    product.StockQuantity -= item.Quantity;
                }
            }
            ctx.SupplyItems.RemoveRange(items);
            ctx.Supplies.Remove(supply);

            await ctx.SaveChangesAsync();

            await LoadSupplies(supply.SupplierId);
            await ShowMessage("Удалено", "Поставка удалена, остатки товаров скорректированы", MsBox.Avalonia.Enums.Icon.Success);
        }

        private async void AddSupplier_Click(object? sender, RoutedEventArgs e)
        {
            var win = new SupplierEditWindow(null);
            await win.ShowDialog(this);
            await LoadSuppliers();
        }

        private async void EditSupplier_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is Button { Tag: Supplier supplier })
            {
                var win = new SupplierEditWindow(supplier);
                await win.ShowDialog(this);
                await LoadSuppliers();
            }
        }

        private async void DeleteSupplier_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is not Button { Tag: Supplier supplier }) return;
            var confirm = await MessageBoxManager.GetMessageBoxStandard(
                "Удаление поставщика",
                $"Удалить поставщика \"{supplier.Name}\"? Это действие нельзя отменить.",
                ButtonEnum.YesNo,
                MsBox.Avalonia.Enums.Icon.Warning).ShowAsync();

            if (confirm != ButtonResult.Yes) return;

            using var ctx = new DemoContext();
            var hasSupplies = await ctx.Supplies.AnyAsync(s => s.SupplierId == supplier.Id);
            if (hasSupplies)
            {
                await ShowMessage("Ошибка", "Нельзя удалить поставщика, у которого есть связанные поставки.", MsBox.Avalonia.Enums.Icon.Error);
                return;
            }

            ctx.Suppliers.Remove(supplier);
            await ctx.SaveChangesAsync();

            await LoadSuppliers();
            if (SelectedSupplier?.Id == supplier.Id)
                SelectedSupplier = null;
            Supplies.Clear();

            await ShowMessage("Удалено", "Поставщик удалён", MsBox.Avalonia.Enums.Icon.Success);
        }

        private void Back_Click(object? sender, RoutedEventArgs e)
        {
            this.Close(); 
        }

        private async void Refresh_Click(object? sender, RoutedEventArgs e) => await LoadSuppliers();

        private async Task ShowMessage(string title, string message, Icon icon)
        {
            var box = MessageBoxManager.GetMessageBoxStandard(title, message, ButtonEnum.Ok, icon);
            await box.ShowAsync();
        }
    }
}