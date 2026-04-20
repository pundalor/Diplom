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
    public partial class SupplyEditWindow : Window
    {
        public Supply EditingSupply { get; set; }
        public ObservableCollection<ProductRow> ProductRows { get; set; }
        private List<Product> _allProducts = new();
        public Supplier? SelectedSupplier { get; set; }

        public class ProductRow
        {
            public Product? SelectedProduct { get; set; }
            public int Quantity { get; set; }
            public decimal? PurchasePrice { get; set; }
            public ObservableCollection<Product> Products { get; set; } = new();
        }

        public SupplyEditWindow()
        {
            InitializeComponent();
        }

        public SupplyEditWindow(Supply supply, int supplierId = 0)
        {
            InitializeComponent();

            var defaultDate = DateOnly.FromDateTime(DateTime.Today);
            EditingSupply = supply ?? new Supply
            {
                SupplyDate = defaultDate,
                SupplierId = supplierId
            };

            ProductRows = new ObservableCollection<ProductRow>();
            DataContext = this;

            var date = EditingSupply.SupplyDate;
            SupplyDatePicker.SelectedDate = new DateTimeOffset(date.Year, date.Month, date.Day, 0, 0, 0, TimeSpan.Zero);
            NotesBox.Text = EditingSupply.Notes;

            this.Opened += async (_, __) => await LoadDataAsync(supply, supplierId);
        }

        private async Task LoadDataAsync(Supply? supply, int supplierId)
        {
            await LoadSuppliers();
            await LoadProducts();

            if (supply?.Id > 0)
                await LoadSupplyItems();

            if (supplierId > 0 && SelectedSupplier == null)
            {
                SelectedSupplier = (SupplierComboBox.ItemsSource as List<Supplier>)
                    ?.FirstOrDefault(s => s.Id == supplierId);
                SupplierComboBox.SelectedItem = SelectedSupplier;
            }
        }

        private async Task LoadSuppliers()
        {
            using var ctx = new DemoContext();
            var suppliers = await ctx.Suppliers.ToListAsync();
            SupplierComboBox.ItemsSource = suppliers;

            if (EditingSupply.SupplierId > 0)
            {
                SelectedSupplier = suppliers.FirstOrDefault(s => s.Id == EditingSupply.SupplierId);
                SupplierComboBox.SelectedItem = SelectedSupplier;
            }
        }

        private async Task LoadProducts()
        {
            using var ctx = new DemoContext();
            _allProducts = await ctx.Products.OrderBy(p => p.Name).ToListAsync();
        }

        private async Task LoadSupplyItems()
        {
            using var ctx = new DemoContext();
            var items = await ctx.SupplyItems
                .Where(i => i.SupplyId == EditingSupply.Id)
                .ToListAsync();

            foreach (var item in items)
            {
                var product = _allProducts.FirstOrDefault(p => p.Id == item.ProductId);
                var row = new ProductRow
                {
                    SelectedProduct = product,
                    Quantity = item.Quantity,
                    PurchasePrice = item.PurchasePrice,
                    Products = new ObservableCollection<Product>(_allProducts)
                };
                ProductRows.Add(row);
            }
        }

        private void AddProductRow_Click(object? sender, RoutedEventArgs e)
        {
            var selectedIds = ProductRows
                .Where(r => r.SelectedProduct != null)
                .Select(r => r.SelectedProduct!.Id)
                .ToHashSet();

            var availableProducts = new ObservableCollection<Product>(
                _allProducts.Where(p => !selectedIds.Contains(p.Id))
            );

            ProductRows.Add(new ProductRow { Products = availableProducts });
        }

        private void RemoveProductRow_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is ProductRow row)
                ProductRows.Remove(row);
        }

        private async void Save_Click(object? sender, RoutedEventArgs e)
        {
            bool isValid = true;
            SelectedSupplier = SupplierComboBox.SelectedItem as Supplier;

            if (SelectedSupplier == null)
            {
                SupplierError.Text = "Выберите поставщика";
                SupplierError.IsVisible = true;
                isValid = false;
            }
            else SupplierError.IsVisible = false;

            if (SupplyDatePicker.SelectedDate == null)
            {
                DateError.Text = "Укажите дату поставки";
                DateError.IsVisible = true;
                isValid = false;
            }
            else DateError.IsVisible = false;

            if (ProductRows.Count == 0)
            {
                ProductsError.Text = "Добавьте хотя бы один товар";
                ProductsError.IsVisible = true;
                isValid = false;
            }
            else
            {
                foreach (var row in ProductRows)
                {
                    if (row.SelectedProduct == null)
                    {
                        ProductsError.Text = "Выберите товар для каждой строки";
                        ProductsError.IsVisible = true;
                        isValid = false;
                        break;
                    }
                    if (row.Quantity <= 0)
                    {
                        ProductsError.Text = "Количество товара должно быть больше 0";
                        ProductsError.IsVisible = true;
                        isValid = false;
                        break;
                    }
                    if (row.PurchasePrice == null || row.PurchasePrice <= 0)
                    {
                        ProductsError.Text = "Укажите корректную цену закупки";
                        ProductsError.IsVisible = true;
                        isValid = false;
                        break;
                    }
                }
                if (isValid) ProductsError.IsVisible = false;
            }

            if (!isValid) return;

            using var ctx = new DemoContext();

            bool isNew = EditingSupply.Id == 0;

            List<SupplyItem> oldItems = new();
            if (!isNew)
            {
                oldItems = await ctx.SupplyItems
                    .Where(i => i.SupplyId == EditingSupply.Id)
                    .ToListAsync();
            }
            var oldQuantities = oldItems.ToDictionary(i => i.ProductId, i => i.Quantity);

            var stockChanges = new Dictionary<int, int>();

            EditingSupply.SupplierId = SelectedSupplier!.Id;
            EditingSupply.SupplyDate = DateOnly.FromDateTime(SupplyDatePicker.SelectedDate!.Value.DateTime);
            EditingSupply.Notes = NotesBox.Text;
            EditingSupply.TotalAmount = ProductRows.Sum(r => r.Quantity * r.PurchasePrice);

            if (isNew)
                ctx.Supplies.Add(EditingSupply);
            else
                ctx.Supplies.Update(EditingSupply);

            await ctx.SaveChangesAsync(); 

            if (!isNew)
            {
                foreach (var oldItem in oldItems)
                {
                    ctx.SupplyItems.Remove(oldItem);
                    if (!stockChanges.ContainsKey(oldItem.ProductId))
                        stockChanges[oldItem.ProductId] = 0;
                    stockChanges[oldItem.ProductId] -= oldItem.Quantity;
                }
            }

            foreach (var row in ProductRows)
            {
                var newItem = new SupplyItem
                {
                    SupplyId = EditingSupply.Id,
                    ProductId = row.SelectedProduct!.Id,
                    Quantity = row.Quantity,
                    PurchasePrice = row.PurchasePrice
                };
                ctx.SupplyItems.Add(newItem);

                if (!stockChanges.ContainsKey(row.SelectedProduct.Id))
                    stockChanges[row.SelectedProduct.Id] = 0;
                stockChanges[row.SelectedProduct.Id] += row.Quantity;
            }

            foreach (var kvp in stockChanges)
            {
                var product = await ctx.Products.FindAsync(kvp.Key);
                if (product != null)
                {
                    int newStock = product.StockQuantity + kvp.Value;

                    if (newStock < 0)
                    {
                        await ShowMessage("Ошибка",
                            $"Недостаточно товара \"{product.Name}\" на складе для корректировки.\n" +
                            $"Текущий остаток: {product.StockQuantity}, изменение: {kvp.Value}",
                            MsBox.Avalonia.Enums.Icon.Error);
                        return;
                    }

                    product.StockQuantity = newStock;
                }
            }

            await ctx.SaveChangesAsync();

            await ShowMessage("Успех", "Поставка сохранена", MsBox.Avalonia.Enums.Icon.Success);
            Close();
        }

        private async Task ShowMessage(string title, string message, Icon icon)
        {
            var box = MessageBoxManager.GetMessageBoxStandard(title, message, ButtonEnum.Ok, icon);
            await box.ShowAsync();
        }

        private void Cancel_Click(object? sender, RoutedEventArgs e) => Close();

        private void Back_Click(object? sender, RoutedEventArgs e) => Close();
    }
}