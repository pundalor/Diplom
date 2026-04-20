using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using DiplomchikFlowers.Model;
using Microsoft.EntityFrameworkCore;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DiplomchikFlowers
{
    public partial class CartWindow : Window
    {
        private Customer _currentUser;
        private List<CartItemDisplay> _cartItems;
        private List<CartItemDisplay> _filteredItems;

        public CartWindow()
        {
            InitializeComponent();
        }

        public CartWindow(Customer user)
        {
            QuestPDF.Settings.License = LicenseType.Community;
            InitializeComponent();
            _currentUser = user;
            LoadCart();
        }

        private void LoadCart()
        {
            using var ctx = new DemoContext();
            var cart = ctx.Carts.Include(c => c.CartItems)
                                .ThenInclude(ci => ci.Product)
                                .FirstOrDefault(c => c.CustomerId == _currentUser.Id);
            if (cart == null || !cart.CartItems.Any())
            {
                _cartItems = new List<CartItemDisplay>();
                CartItemsControl.IsVisible = false;
                EmptyCartPanel.IsVisible = true;
                TotalText.Text = "0 ₽";
                return;
            }
            else
            {
                CartItemsControl.IsVisible = true;
                EmptyCartPanel.IsVisible = false;
            }

            _cartItems = cart.CartItems.Select(ci => new CartItemDisplay
            {
                CartItemId = ci.Id,
                Product = ci.Product,
                Quantity = ci.Quantity,
                HasDiscount = IsDiscountActive(ci.Product),
                DisplayPrice = GetDiscountedPrice(ci.Product)
            }).ToList();

            ApplyFilter();
            UpdateTotal();
        }


        private void GoToCatalog_Click(object? sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ApplyFilter()
        {
            if (string.IsNullOrWhiteSpace(SearchBox?.Text))
                _filteredItems = _cartItems.ToList();
            else
            {
                var search = SearchBox.Text.Trim().ToLower();
                _filteredItems = _cartItems.Where(i => i.Product.Name.ToLower().Contains(search)).ToList();
            }
            CartItemsControl.ItemsSource = _filteredItems;
        }

        private decimal GetDiscountedPrice(Product product)
        {
            if (product.DiscountPercent > 0 && product.DiscountStartDate.HasValue && product.DiscountEndDate.HasValue)
            {
                var now = DateTime.Now;
                if (now >= product.DiscountStartDate.Value && now <= product.DiscountEndDate.Value)
                    return product.Price * (1 - product.DiscountPercent / 100m);
            }
            return product.Price;
        }

        private bool IsDiscountActive(Product product)
        {
            if (product.DiscountPercent > 0 && product.DiscountStartDate.HasValue && product.DiscountEndDate.HasValue)
            {
                var now = DateTime.Now;
                return now >= product.DiscountStartDate.Value && now <= product.DiscountEndDate.Value;
            }
            return false;
        }

        private void UpdateTotal()
        {
            decimal total = _cartItems.Sum(i => i.DisplayPrice * i.Quantity);
            TotalText.Text = $"{total:C}";
        }

        private async void ClearCart_Click(object? sender, RoutedEventArgs e)
        {
            var confirm = MessageBoxManager.GetMessageBoxStandard("Очистка корзины", "Вы уверены, что хотите очистить корзину?", ButtonEnum.YesNo, MsBox.Avalonia.Enums.Icon.Question);
            var result = await confirm.ShowAsync();
            if (result == ButtonResult.Yes)
            {
                using var ctx = new DemoContext();
                var cart = ctx.Carts.Include(c => c.CartItems).FirstOrDefault(c => c.CustomerId == _currentUser.Id);
                if (cart != null)
                {
                    ctx.CartItems.RemoveRange(cart.CartItems);
                    await ctx.SaveChangesAsync();
                }
                LoadCart();
                await ShowMessage("Корзина очищена", "Все товары удалены из корзины.", MsBox.Avalonia.Enums.Icon.Info);
            }
        }

        private async void Checkout_Click(object? sender, RoutedEventArgs e)
        {
            if (_cartItems.Count == 0)
            {
                await ShowMessage("Корзина пуста", "Невозможно оформить заказ.", MsBox.Avalonia.Enums.Icon.Warning);
                return;
            }

            string currentAddress = _currentUser.Address;
            string currentCity = _currentUser.City;
            var addressDialog = new AddressDialog(currentCity, currentAddress);
            var result = await addressDialog.ShowDialog<AddressDialogResult?>(this);
            if (result == null)
                return;

            string city = result.City;
            string fullAddress = result.FullAddress;

            using var ctx = new DemoContext();

            foreach (var item in _cartItems)
            {
                var product = await ctx.Products.FindAsync(item.Product.Id);
                if (product.StockQuantity < item.Quantity)
                {
                    await ShowMessage("Недостаточно товара", $"Товара '{product.Name}' осталось только {product.StockQuantity} шт.", MsBox.Avalonia.Enums.Icon.Warning);
                    return;
                }
            }

            var order = new Order
            {
                CustomerId = _currentUser.Id,
                OrderDate = DateTime.Now,
                StatusId = 1,
                TotalAmount = _cartItems.Sum(i => i.DisplayPrice * i.Quantity),
                ShippingAddress = fullAddress,
                City = city,
                Notes = "Спасибо за покупку!"
            };
            ctx.Orders.Add(order);
            await ctx.SaveChangesAsync();

            foreach (var item in _cartItems)
            {
                var product = await ctx.Products.FindAsync(item.Product.Id);
                product.StockQuantity -= item.Quantity;

                var orderItem = new OrderItem
                {
                    OrderId = order.Id,
                    ProductId = item.Product.Id,
                    Quantity = item.Quantity,
                    PriceAtTime = item.DisplayPrice,
                    DiscountApplied = item.HasDiscount ? item.Product.DiscountPercent : 0
                };
                ctx.OrderItems.Add(orderItem);
            }

            var cart = ctx.Carts.Include(c => c.CartItems).FirstOrDefault(c => c.CustomerId == _currentUser.Id);
            if (cart != null)
                ctx.CartItems.RemoveRange(cart.CartItems);

            await ctx.SaveChangesAsync();

            GeneratePdfReceipt(order, _cartItems);

            await ShowMessage("Заказ оформлен", $"Номер заказа: {order.Id}. Чек сохранён в папке 'Загрузки'.", MsBox.Avalonia.Enums.Icon.Success);
            LoadCart();
            Close();
        }

        private void GeneratePdfReceipt(Order order, List<CartItemDisplay> items)
        {
            string downloadsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
            if (!Directory.Exists(downloadsPath))
                Directory.CreateDirectory(downloadsPath);

            string fileName = $"Receipt_{order.Id}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
            string filePath = Path.Combine(downloadsPath, fileName);

            global::QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(12).FontFamily(Fonts.Arial));

                    page.Header()
                        .AlignCenter()
                        .Text("ЧЕК ПОКУПКИ")
                        .SemiBold().FontSize(24).FontColor(Colors.BlueGrey.Darken4);

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(column =>
                        {
                            column.Item().Text(text =>
                            {
                                text.Span($"Заказ №{order.Id}").SemiBold();
                                text.Span($" от {order.OrderDate:dd.MM.yyyy HH:mm}");
                            });
                            column.Item().PaddingBottom(5);

                            column.Item().Text($"Покупатель: {_currentUser.Fullname}");
                            column.Item().Text($"Email: {_currentUser.Email}");
                            column.Item().Text($"Телефон: {_currentUser.Phone}");
                            column.Item().Text($"Адрес доставки: {order.ShippingAddress}, {order.City}");
                            column.Item().PaddingBottom(10);

                            column.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(3);
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(2);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().PaddingBottom(5).Text("Товар").SemiBold().FontSize(14);
                                    header.Cell().PaddingBottom(5).Text("Цена").SemiBold().FontSize(14);
                                    header.Cell().PaddingBottom(5).Text("Кол-во").SemiBold().FontSize(14);
                                    header.Cell().PaddingBottom(5).Text("Сумма").SemiBold().FontSize(14);
                                });

                                foreach (var item in items)
                                {
                                    table.Cell().Element(CellStyle).Text(item.Product.Name);
                                    table.Cell().Element(CellStyle).Text($"{item.DisplayPrice:C}");
                                    table.Cell().Element(CellStyle).Text(item.Quantity.ToString());
                                    table.Cell().Element(CellStyle).Text($"{(item.DisplayPrice * item.Quantity):C}");
                                }

                                static IContainer CellStyle(IContainer container)
                                {
                                    return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);
                                }
                            });

                            column.Item().PaddingTop(10).AlignRight().Text($"Итого к оплате: {order.TotalAmount:C}").SemiBold().FontSize(14);
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text("Спасибо за покупку!")
                        .FontSize(10).FontColor(Colors.Grey.Darken1);
                });
            }).GeneratePdf(filePath);

            System.Diagnostics.Process.Start("explorer.exe", downloadsPath);
        }

        private async void IncrementQuantity_Click(object? sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var display = btn?.Tag as CartItemDisplay;
            if (display == null) return;

            using var ctx = new DemoContext();
            var cartItem = await ctx.CartItems.FindAsync(display.CartItemId);
            if (cartItem != null)
            {
                var product = await ctx.Products.FindAsync(cartItem.ProductId);
                if (cartItem.Quantity + 1 > product.StockQuantity)
                {
                    await ShowMessage("Недостаточно товара", $"На складе осталось только {product.StockQuantity} шт.", MsBox.Avalonia.Enums.Icon.Warning);
                    return;
                }
                cartItem.Quantity++;
                await ctx.SaveChangesAsync();
            }
            LoadCart();
        }

        private async void DecrementQuantity_Click(object? sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var display = btn?.Tag as CartItemDisplay;
            if (display == null) return;

            using var ctx = new DemoContext();
            var cartItem = await ctx.CartItems.FindAsync(display.CartItemId);
            if (cartItem != null)
            {
                if (cartItem.Quantity > 1)
                    cartItem.Quantity--;
                else
                    ctx.CartItems.Remove(cartItem);
                await ctx.SaveChangesAsync();
            }
            LoadCart();
        }

        private async void RemoveItem_Click(object? sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var display = btn?.Tag as CartItemDisplay;
            if (display == null) return;

            using var ctx = new DemoContext();
            var cartItem = await ctx.CartItems.FindAsync(display.CartItemId);
            if (cartItem != null)
            {
                ctx.CartItems.Remove(cartItem);
                await ctx.SaveChangesAsync();
            }
            LoadCart();
        }

        private void SearchBox_TextChanged(object? sender, TextChangedEventArgs e) => ApplyFilter();

        private void Back_Click(object? sender, RoutedEventArgs e) => Close();

        private async Task ShowMessage(string title, string message, Icon icon)
        {
            var box = MessageBoxManager.GetMessageBoxStandard(title, message, ButtonEnum.Ok, icon);
            await box.ShowAsync();
        }
    }

    public class CartItemDisplay
    {
        public int CartItemId { get; set; }
        public Product Product { get; set; }
        public int Quantity { get; set; }
        public bool HasDiscount { get; set; }
        public decimal DisplayPrice { get; set; }
        public decimal TotalPrice => DisplayPrice * Quantity;
    }

    public class AddressDialogResult
    {
        public string City { get; set; }
        public string Street { get; set; }
        public string House { get; set; }
        public string Apartment { get; set; }
        public string FullAddress => $"{Street}, д. {House}" + (string.IsNullOrWhiteSpace(Apartment) ? "" : $", кв. {Apartment}");
    }

    public class AddressDialog : Window
    {
        private TextBox cityBox;
        private TextBox streetBox;
        private TextBox houseBox;
        private TextBox apartmentBox;

        public AddressDialog(string currentCity, string currentAddress)
        {
            Title = "Адрес доставки";
            Width = 450;
            Height = 350;
            this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            CanResize = false;

            string currentStreet = "", currentHouse = "", currentApartment = "";
            if (!string.IsNullOrWhiteSpace(currentAddress))
            {
                var parts = currentAddress.Split(new[] { ", д. ", ", кв. " }, StringSplitOptions.None);
                if (parts.Length >= 1) currentStreet = parts[0];
                if (parts.Length >= 2) currentHouse = parts[1];
                if (parts.Length >= 3) currentApartment = parts[2];
            }

            var panel = new StackPanel { Margin = new Thickness(20) };

            panel.Children.Add(new TextBlock { Text = "Город *", Margin = new Thickness(0, 0, 0, 5) });
            cityBox = new TextBox { Watermark = "Введите город", Text = currentCity, Margin = new Thickness(0, 0, 0, 15) };
            panel.Children.Add(cityBox);

            panel.Children.Add(new TextBlock { Text = "Улица *", Margin = new Thickness(0, 0, 0, 5) });
            streetBox = new TextBox { Watermark = "Введите улицу", Text = currentStreet, Margin = new Thickness(0, 0, 0, 15) };
            panel.Children.Add(streetBox);

            panel.Children.Add(new TextBlock { Text = "Дом *", Margin = new Thickness(0, 0, 0, 5) });
            houseBox = new TextBox
            {
                Watermark = "Введите номер дома",
                Text = currentHouse,
                Margin = new Thickness(0, 0, 0, 15)
            };
            houseBox.TextChanged += NumericTextBox_TextChanged;
            panel.Children.Add(houseBox);

            panel.Children.Add(new TextBlock { Text = "Квартира / офис", Margin = new Thickness(0, 0, 0, 5) });
            apartmentBox = new TextBox
            {
                Watermark = "Введите номер квартиры (необязательно)",
                Text = currentApartment,
                Margin = new Thickness(0, 0, 0, 20)
            };
            apartmentBox.TextChanged += NumericTextBox_TextChanged;
            panel.Children.Add(apartmentBox);

            var buttons = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 10
            };

            buttons.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center;

            var okButton = new Button { Content = "ОК", Width = 80 };
            okButton.Click += async (s, e) => await Save();
            var cancelButton = new Button { Content = "Отмена", Width = 80 };
            cancelButton.Click += (s, e) => Close(null);
            buttons.Children.Add(okButton);
            buttons.Children.Add(cancelButton);
            panel.Children.Add(buttons);

            Content = panel;
        }

        /// <summary>
        /// Обработчик TextChanged для ограничения ввода только цифр
        /// </summary>
        private void NumericTextBox_TextChanged(object? sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                string originalText = textBox.Text ?? "";
                string digitsOnly = Regex.Replace(originalText, @"[^\d]", "");
                if (digitsOnly != originalText)
                {
                    int caretIndex = Math.Min(textBox.CaretIndex, digitsOnly.Length);

                    textBox.Text = digitsOnly;
                    textBox.CaretIndex = caretIndex;
                }
            }
        }

        private async Task Save()
        {
            string city = cityBox.Text?.Trim();
            string street = streetBox.Text?.Trim();
            string house = houseBox.Text?.Trim();

            if (string.IsNullOrWhiteSpace(city) || string.IsNullOrWhiteSpace(street) || string.IsNullOrWhiteSpace(house))
            {
                var msg = MessageBoxManager.GetMessageBoxStandard("Ошибка", "Пожалуйста, заполните город, улицу и номер дома.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Warning);
                await msg.ShowAsync();
                return;
            }

            string apartment = apartmentBox.Text?.Trim() ?? "";
            var result = new AddressDialogResult
            {
                City = city,
                Street = street,
                House = house,
                Apartment = apartment
            };
            Close(result);
        }

        private void Close(AddressDialogResult result)
        {
            base.Close(result);
        }
    }
}