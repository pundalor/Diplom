using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using DiplomchikFlowers.Model;
using Microsoft.EntityFrameworkCore;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DiplomchikFlowers
{
    public class FavoriteContentConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return (value is bool isFavorite && isFavorite) ? "❤️" : "♡";
        }
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class FavoriteColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return (value is bool isFavorite && isFavorite)
                ? new SolidColorBrush(Color.Parse("#FFE74C3C"))
                : new SolidColorBrush(Color.Parse("#FFCCCCCC"));
        }
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class StockBackgroundConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return (value is bool isInStock && !isInStock)
                ? new SolidColorBrush(Color.Parse("#FFF9F9F9"))
                : new SolidColorBrush(Color.Parse("#FFFFFFFF"));
        }
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class ProductDisplay
    {
        public Product Product { get; set; }
        public bool IsFavorite { get; set; }
        public bool HasDiscount { get; set; }
        public decimal OriginalPrice { get; set; }
        public decimal DiscountedPrice { get; set; }
        public int DiscountPercent { get; set; }

        public bool IsInStock => Product.StockQuantity > 0;
        public bool IsOutOfStock => Product.StockQuantity == 0;
    }

    public partial class CatalogWindow : Window
    {
        private Customer _currentUser;
        private List<Product> _allProducts;
        private List<Product> _filteredProducts;
        private List<ProductDisplay> _currentPageProducts;
        private List<Category> _categories;
        private Category _selectedCategory;
        private bool _showFavoritesOnly = false;
        private bool _showOutOfStock = false;
        private HashSet<int> _favoriteProductIds = new HashSet<int>();
        private Button _suppliersButton;
        private Button _notificationsButton;
        private Button _sendNotificationButton;
        private Dictionary<int, int> _productFavoriteCounts = new();
        private Dictionary<int, double> _productRatings = new();
        private Dictionary<int, int> _productSalesCount = new();

        private int _currentPage = 1;
        private const int PageSize = 20;
        private int _totalPages = 1;

        public bool IsAdmin => _currentUser?.Roleid == 1;
        public bool IsManagerOrAdmin => _currentUser?.Roleid == 2 || IsAdmin;
        public bool IsUser => _currentUser?.Roleid == 3 || _currentUser == null;

        public CatalogWindow()
        {
            InitializeComponent();
        }

        public CatalogWindow(Customer user)
        {
            InitializeComponent();
            _currentUser = user;
            DataContext = this;
        }

        protected override void OnOpened(EventArgs e)
        {
            base.OnOpened(e);

            _suppliersButton = this.FindControl<Button>("SuppliersButton");
            _notificationsButton = this.FindControl<Button>("NotificationsButton");
            _sendNotificationButton = this.FindControl<Button>("SendNotificationButton");

            UpdateUIBasedOnRole();

            LoadCategories();
            LoadFavorites();
            LoadProducts();
        }

        private void UpdateUIBasedOnRole()
        {
            if (AdminPanelButton != null) AdminPanelButton.IsVisible = IsAdmin;
            if (AddProductButton != null) AddProductButton.IsVisible = IsManagerOrAdmin;
            if (CartButton != null) CartButton.IsVisible = true;
            if (FavoritesTabButton != null) FavoritesTabButton.IsVisible = _currentUser != null;
            if (OrdersTabButton != null) OrdersTabButton.IsVisible = IsUser && _currentUser != null;
            if (AllOrdersTabButton != null) AllOrdersTabButton.IsVisible = IsAdmin;
            if (_suppliersButton != null) _suppliersButton.IsVisible = IsManagerOrAdmin;
            if (_sendNotificationButton != null) _sendNotificationButton.IsVisible = IsAdmin;
        }

        private void LoadCategories()
        {
            using var ctx = new DemoContext();
            _categories = ctx.Categories.ToList();
            if (CategoriesItemsControl != null)
                CategoriesItemsControl.ItemsSource = _categories;
        }

        private void LoadFavorites()
        {
            if (_currentUser == null) return;
            using var ctx = new DemoContext();
            _favoriteProductIds = ctx.Favorites
                .Where(f => f.CustomerId == _currentUser.Id)
                .Select(f => f.ProductId)
                .ToHashSet();
        }

        private async void Notifications_Click(object? sender, RoutedEventArgs e)
        {
            if (_currentUser == null)
            {
                await ShowMessage("Внимание", "Войдите в аккаунт", MsBox.Avalonia.Enums.Icon.Warning);
                return;
            }
            var notifWindow = new NotificationsWindow(_currentUser);
            await notifWindow.ShowDialog(this);
        }

        private async void SendNotification_Click(object? sender, RoutedEventArgs e)
        {
            if (!IsAdmin)
            {
                await ShowMessage("Доступ запрещён", "Только администратор может рассылать уведомления", MsBox.Avalonia.Enums.Icon.Warning);
                return;
            }
            var managerWindow = new NotificationManagerWindow(_currentUser);
            await managerWindow.ShowDialog(this);
        }

        private async void OrdersTab_Click(object? sender, RoutedEventArgs e)
        {
            if (_currentUser == null)
            {
                await ShowMessage("Внимание", "Необходимо авторизоваться", MsBox.Avalonia.Enums.Icon.Warning);
                return;
            }
            var ordersWindow = new OrdersWindow(_currentUser);
            await ordersWindow.ShowDialog(this);
        }

        private async void AllOrdersTab_Click(object? sender, RoutedEventArgs e)
        {
            if (!IsAdmin)
            {
                await ShowMessage("Доступ запрещён", "Только администратор может управлять всеми заказами.", MsBox.Avalonia.Enums.Icon.Warning);
                return;
            }
            var ordersWindow = new OrdersWindow(_currentUser);
            await ordersWindow.ShowDialog(this);
        }

        private decimal GetDiscountedPrice(Product product)
        {
            if (product.DiscountPercent > 0 && product.DiscountStartDate.HasValue && product.DiscountEndDate.HasValue)
            {
                var now = DateTime.Now;
                if (now >= product.DiscountStartDate.Value && now <= product.DiscountEndDate.Value)
                {
                    return product.Price * (1 - product.DiscountPercent / 100m);
                }
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

        private void LoadProducts(bool resetPage = true)
        {
            if (SearchBox == null || CountTextBlock == null || ProductsItemsControl == null || EmptyFavoritesPanel == null)
                return;

            if (resetPage) _currentPage = 1;

            using var ctx = new DemoContext();
            _allProducts = ctx.Products
                .Include(p => p.Category)
                .ToList();

            _productFavoriteCounts = ctx.Favorites
                .GroupBy(f => f.ProductId)
                .ToDictionary(g => g.Key, g => g.Count());

            _productRatings = ctx.Reviews
                .GroupBy(r => r.ProductId)
                .ToDictionary(g => g.Key, g => g.Average(r => (double)r.Rating));

            _productSalesCount = ctx.OrderItems
                .GroupBy(oi => oi.ProductId)
                .ToDictionary(g => g.Key, g => g.Sum(oi => oi.Quantity));

            var query = _allProducts.AsEnumerable();

            if (_selectedCategory != null)
                query = query.Where(p => p.CategoryId == _selectedCategory.Id);

            if (!string.IsNullOrWhiteSpace(SearchBox.Text))
            {
                var search = SearchBox.Text.Trim();
                query = query.Where(p => p.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                                         (p.Description != null && p.Description.Contains(search, StringComparison.OrdinalIgnoreCase)));
            }

            if (_showFavoritesOnly && _currentUser != null)
                query = query.Where(p => _favoriteProductIds.Contains(p.Id));

            if (!_showOutOfStock && !_showFavoritesOnly)
            {
                query = query.Where(p => p.StockQuantity > 0);
            }

            _filteredProducts = query.ToList();

            ApplySorting();

            int displayCount = _showOutOfStock ? _filteredProducts.Count : _filteredProducts.Count(p => p.StockQuantity > 0);
            CountTextBlock.Text = $"Найдено товаров: {displayCount}";

            _totalPages = (int)Math.Ceiling((double)_filteredProducts.Count / PageSize);
            if (_totalPages == 0) _totalPages = 1;
            if (_currentPage > _totalPages) _currentPage = _totalPages;

            var pagedProducts = _filteredProducts
                .Skip((_currentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            _currentPageProducts = pagedProducts.Select(p => new ProductDisplay
            {
                Product = p,
                IsFavorite = _favoriteProductIds.Contains(p.Id),
                OriginalPrice = p.Price,
                DiscountedPrice = GetDiscountedPrice(p),
                HasDiscount = IsDiscountActive(p),
                DiscountPercent = p.DiscountPercent
            }).ToList();

            if (_showFavoritesOnly && _filteredProducts.Count == 0)
            {
                ProductsItemsControl.IsVisible = false;
                EmptyFavoritesPanel.IsVisible = true;
            }
            else
            {
                ProductsItemsControl.IsVisible = true;
                EmptyFavoritesPanel.IsVisible = false;
                ProductsItemsControl.ItemsSource = _currentPageProducts;
                UpdateCardButtonsVisibility();
            }

            UpdatePaginationUI();
        }

        private void ApplySorting()
        {
            if (SortComboBox?.SelectedItem is ComboBoxItem selected)
            {
                switch (selected.Content.ToString())
                {
                    case "По цене ↑":
                        _filteredProducts = _filteredProducts.OrderBy(p => p.Price).ToList();
                        break;
                    case "По цене ↓":
                        _filteredProducts = _filteredProducts.OrderByDescending(p => p.Price).ToList();
                        break;
                    case "По названию А→Я":
                        _filteredProducts = _filteredProducts.OrderBy(p => p.Name).ToList();
                        break;
                    case "По названию Я→А":
                        _filteredProducts = _filteredProducts.OrderByDescending(p => p.Name).ToList();
                        break;
                    case "По популярности (избранное)":
                        _filteredProducts = _filteredProducts
                            .OrderByDescending(p => _productFavoriteCounts.GetValueOrDefault(p.Id, 0))
                            .ToList();
                        break;
                    case "По новизне":
                        _filteredProducts = _filteredProducts
                            .OrderByDescending(p => p.Id)
                            .ToList();
                        break;
                    case "По скидке":
                        _filteredProducts = _filteredProducts
                            .OrderByDescending(p => p.DiscountPercent)
                            .ToList();
                        break;
                    case "По рейтингу":
                        _filteredProducts = _filteredProducts
                            .OrderByDescending(p => _productRatings.GetValueOrDefault(p.Id, 0))
                            .ToList();
                        break;
                }
            }
        }

        private void UpdatePaginationUI()
        {
            if (PrevPageButton != null) PrevPageButton.IsEnabled = _currentPage > 1;
            if (NextPageButton != null) NextPageButton.IsEnabled = _currentPage < _totalPages;
            if (PageInfoTextBlock != null) PageInfoTextBlock.Text = $"Страница {_currentPage} из {_totalPages}";
        }

        private void GoToPage(int page)
        {
            if (page < 1 || page > _totalPages) return;
            _currentPage = page;
            LoadProducts(resetPage: false);
        }

        private void PrevPage_Click(object? sender, RoutedEventArgs e) => GoToPage(_currentPage - 1);
        private void NextPage_Click(object? sender, RoutedEventArgs e) => GoToPage(_currentPage + 1);

        private void Search_TextChanged(object? sender, TextChangedEventArgs e) => LoadProducts();
        private void Sort_SelectionChanged(object? sender, SelectionChangedEventArgs e) => LoadProducts();
        private void ShowOutOfStockCheckBox_Changed(object? sender, RoutedEventArgs e)
        {
            _showOutOfStock = ShowOutOfStockCheckBox.IsChecked ?? false;
            LoadProducts();
        }

        private void ResetFilters_Click(object? sender, RoutedEventArgs e)
        {
            if (SearchBox != null) SearchBox.Text = "";
            _selectedCategory = null;
            if (SortComboBox != null) SortComboBox.SelectedIndex = 0;
            _showFavoritesOnly = false;
            _showOutOfStock = false;
            if (ShowOutOfStockCheckBox != null) ShowOutOfStockCheckBox.IsChecked = false;
            LoadProducts();
        }

        private void CategoryChip_Click(object? sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var cat = btn?.Tag as Category;
            if (cat != null)
            {
                _selectedCategory = (_selectedCategory?.Id == cat.Id) ? null : cat;
                LoadProducts();
            }
        }

        private async void AddToCart_Click(object? sender, RoutedEventArgs e)
        {
            if (_currentUser == null)
            {
                await ShowMessage("Внимание", "Для добавления в корзину необходимо авторизоваться", MsBox.Avalonia.Enums.Icon.Warning);
                return;
            }
            var button = sender as Button;
            var product = button?.Tag as Product;
            if (product == null) return;

            if (product.StockQuantity <= 0)
            {
                await ShowMessage("Ошибка", $"Товар '{product.Name}' отсутствует на складе", MsBox.Avalonia.Enums.Icon.Error);
                return;
            }

            using var ctx = new DemoContext();
            var cart = ctx.Carts.FirstOrDefault(c => c.CustomerId == _currentUser.Id);
            if (cart == null)
            {
                cart = new Cart { CustomerId = _currentUser.Id };
                ctx.Carts.Add(cart);
                await ctx.SaveChangesAsync();
            }

            var cartItem = ctx.CartItems.FirstOrDefault(ci => ci.CartId == cart.Id && ci.ProductId == product.Id);
            int newQuantity = (cartItem?.Quantity ?? 0) + 1;

            if (newQuantity > product.StockQuantity)
            {
                await ShowMessage("Ошибка", $"Нельзя добавить больше {product.StockQuantity} шт. товара '{product.Name}'", MsBox.Avalonia.Enums.Icon.Error);
                return;
            }

            if (cartItem == null)
            {
                cartItem = new CartItem { CartId = cart.Id, ProductId = product.Id, Quantity = 1 };
                ctx.CartItems.Add(cartItem);
            }
            else
            {
                cartItem.Quantity++;
            }
            await ctx.SaveChangesAsync();
            await ShowMessage("Добавлено", $"Товар '{product.Name}' добавлен в корзину (теперь в корзине: {newQuantity})", MsBox.Avalonia.Enums.Icon.Success);
        }

        private async void FavoriteButton_Click(object? sender, RoutedEventArgs e)
        {
            if (_currentUser == null)
            {
                await ShowMessage("Внимание", "Войдите в аккаунт, чтобы добавлять в избранное", MsBox.Avalonia.Enums.Icon.Warning);
                return;
            }
            var btn = sender as Button;
            var product = btn?.Tag as Product;
            if (product == null) return;

            using var ctx = new DemoContext();
            var existing = ctx.Favorites.FirstOrDefault(f => f.CustomerId == _currentUser.Id && f.ProductId == product.Id);
            if (existing != null)
            {
                ctx.Favorites.Remove(existing);
                _favoriteProductIds.Remove(product.Id);
                await ShowMessage("Избранное", $"Товар '{product.Name}' удалён из избранного", MsBox.Avalonia.Enums.Icon.Info);
            }
            else
            {
                ctx.Favorites.Add(new Favorite { CustomerId = _currentUser.Id, ProductId = product.Id });
                _favoriteProductIds.Add(product.Id);
                await ShowMessage("Избранное", $"Товар '{product.Name}' добавлен в избранное", MsBox.Avalonia.Enums.Icon.Success);
            }
            await ctx.SaveChangesAsync();

            if (_currentPageProducts != null)
            {
                var display = _currentPageProducts.FirstOrDefault(dp => dp.Product.Id == product.Id);
                if (display != null)
                {
                    display.IsFavorite = _favoriteProductIds.Contains(product.Id);
                    if (ProductsItemsControl.IsVisible)
                    {
                        ProductsItemsControl.ItemsSource = null;
                        ProductsItemsControl.ItemsSource = _currentPageProducts;
                    }
                }
            }
            if (_showFavoritesOnly) LoadProducts(resetPage: false);
        }

        private async void EditProduct_Click(object? sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var product = button?.Tag as Product;
            if (product == null) return;

            var editWindow = new ProductEditWindow(product, _currentUser);
            await editWindow.ShowDialog(this);
            LoadProducts();
        }

        private async void AddProduct_Click(object? sender, RoutedEventArgs e)
        {
            var editWindow = new ProductEditWindow(null, _currentUser);
            await editWindow.ShowDialog(this);
            LoadProducts();
        }

        private async void DeleteProduct_Click(object? sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var product = button?.Tag as Product;
            if (product == null) return;

            using var ctx = new DemoContext();

            var isInOrder = await ctx.OrderItems.AnyAsync(oi => oi.ProductId == product.Id);
            if (isInOrder)
            {
                await ShowMessage("Нельзя удалить",
                    $"Товар '{product.Name}' присутствует в одном или нескольких заказах.\n" +
                    "Удаление невозможно, чтобы сохранить историю покупок.",
                    MsBox.Avalonia.Enums.Icon.Warning);
                return;
            }

            var isInSupply = await ctx.SupplyItems.AnyAsync(si => si.ProductId == product.Id);
            if (isInSupply)
            {
                await ShowMessage("Нельзя удалить",
                    $"Товар '{product.Name}' присутствует в одной или нескольких поставках.\n" +
                    "Удаление невозможно, чтобы сохранить историю поставок.",
                    MsBox.Avalonia.Enums.Icon.Warning);
                return;
            }

            var confirm = MessageBoxManager.GetMessageBoxStandard("Подтверждение",
                $"Удалить товар '{product.Name}'? Все связанные данные (корзина, избранное, поставки, отзывы) будут также удалены.",
                ButtonEnum.YesNo, MsBox.Avalonia.Enums.Icon.Question);
            var result = await confirm.ShowAsync();

            if (result == ButtonResult.Yes)
            {
                if (!string.IsNullOrEmpty(product.ImageUrl))
                {
                    string imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", product.ImageUrl);
                    if (File.Exists(imagePath))
                    {
                        try { File.Delete(imagePath); }
                        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Не удалось удалить файл {imagePath}: {ex.Message}"); }
                    }
                }

                ctx.SupplyItems.RemoveRange(ctx.SupplyItems.Where(si => si.ProductId == product.Id));
                ctx.CartItems.RemoveRange(ctx.CartItems.Where(ci => ci.ProductId == product.Id));
                ctx.Favorites.RemoveRange(ctx.Favorites.Where(f => f.ProductId == product.Id));
                ctx.Reviews.RemoveRange(ctx.Reviews.Where(r => r.ProductId == product.Id));

                ctx.Products.Remove(product);
                await ctx.SaveChangesAsync();

                LoadProducts();
                await ShowMessage("Удалено", "Товар и все связанные данные удалены", MsBox.Avalonia.Enums.Icon.Success);
            }
        }

        private void CatalogTab_Click(object? sender, RoutedEventArgs e)
        {
            _showFavoritesOnly = false;
            LoadProducts();
        }

        private async void FavoritesTab_Click(object? sender, RoutedEventArgs e)
        {
            if (_currentUser == null)
            {
                await ShowMessage("Внимание", "Войдите в аккаунт, чтобы видеть избранное", MsBox.Avalonia.Enums.Icon.Warning);
                return;
            }
            _showFavoritesOnly = true;
            LoadProducts();
        }

        private void GoToCatalogFromFavorites_Click(object? sender, RoutedEventArgs e)
        {
            _showFavoritesOnly = false;
            LoadProducts();
        }

        private async void AdminPanel_Click(object? sender, RoutedEventArgs e)
        {
            if (!IsAdmin)
            {
                await ShowMessage("Доступ запрещён", "Только администратор может управлять пользователями.", MsBox.Avalonia.Enums.Icon.Warning);
                return;
            }
            var usersWindow = new UsersWindow(_currentUser.Id);
            await usersWindow.ShowDialog(this);
        }

        private async void Cart_Click(object? sender, RoutedEventArgs e)
        {
            if (_currentUser == null)
            {
                await ShowMessage("Внимание", "Необходимо авторизоваться", MsBox.Avalonia.Enums.Icon.Warning);
                return;
            }
            var cartWindow = new CartWindow(_currentUser);
            await cartWindow.ShowDialog(this);
        }

        private async void Profile_Click(object? sender, RoutedEventArgs e)
        {
            if (_currentUser == null)
            {
                await ShowMessage("Внимание", "Необходимо авторизоваться", MsBox.Avalonia.Enums.Icon.Warning);
                return;
            }
            var profileWindow = new ProfileWindow(_currentUser);
            await profileWindow.ShowDialog(this);
            LoadFavorites();
        }

        private void Logout_Click(object? sender, RoutedEventArgs e)
        {
            var mainWindow = new MainWindow();
            mainWindow.Show();
            Close();
        }

        private async void Suppliers_Click(object? sender, RoutedEventArgs e)
        {
            if (!IsManagerOrAdmin)
            {
                await ShowMessage("Доступ запрещён", "Только менеджер или администратор могут управлять поставщиками.", MsBox.Avalonia.Enums.Icon.Warning);
                return;
            }
            var supplierWindow = new SupplierWindow(_currentUser);
            await supplierWindow.ShowDialog(this);
        }

        private async Task ShowMessage(string title, string message, Icon icon)
        {
            var box = MessageBoxManager.GetMessageBoxStandard(title, message, ButtonEnum.Ok, icon);
            await box.ShowAsync();
        }

        private void UpdateCardButtonsVisibility()
        {
            if (ProductsItemsControl == null || ProductsItemsControl.ItemsSource == null) return;

            Dispatcher.UIThread.Post(() =>
            {
                var containers = ProductsItemsControl.GetRealizedContainers();
                foreach (var container in containers)
                {
                    var editButton = container.GetVisualDescendants().OfType<Button>()
                        .FirstOrDefault(b => b.Content?.ToString() == "✏️");
                    var deleteButton = container.GetVisualDescendants().OfType<Button>()
                        .FirstOrDefault(b => b.Content?.ToString() == "🗑️");
                    if (editButton != null) editButton.IsVisible = IsManagerOrAdmin;
                    if (deleteButton != null) deleteButton.IsVisible = IsManagerOrAdmin;
                }
            });
        }

        private async void ViewReviews_Click(object? sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var product = btn?.Tag as Product;
            if (product == null) return;

            var reviewsWindow = new ProductReviewsWindow(product, _currentUser);
            await reviewsWindow.ShowDialog(this);
        }

        private async void ProductCard_DoubleTapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            var border = sender as Border;
            var product = border?.Tag as Product;
            if (product == null) return;

            var detailsWindow = new ProductDetailsWindow(product, _currentUser);
            await detailsWindow.ShowDialog(this);

            LoadFavorites();
            LoadProducts();
        }
    }

    public static class VisualExtensions
    {
        public static IEnumerable<Visual> GetVisualDescendants(this Visual visual)
        {
            if (visual == null) yield break;
            foreach (var child in visual.GetVisualChildren())
            {
                yield return child;
                foreach (var descendant in child.GetVisualDescendants())
                    yield return descendant;
            }
        }

        public static IEnumerable<Control> GetRealizedContainers(this ItemsControl itemsControl)
        {
            var panel = itemsControl.GetVisualDescendants().OfType<Panel>().FirstOrDefault();
            if (panel == null) return Enumerable.Empty<Control>();
            return panel.Children.Where(c => c is Control).Cast<Control>();
        }
    }
}