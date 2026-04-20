using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using DiplomchikFlowers.Model;
using Microsoft.EntityFrameworkCore;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace DiplomchikFlowers
{
    public partial class ProductDetailsWindow : Window, INotifyPropertyChanged
    {
        private Product _product;
        private Customer _currentUser;
        private bool _isFavorite;
        private decimal _discountedPrice;
        private bool _hasDiscount;
        private int _discountPercent;
        private double _averageRating;
        private int _reviewsCount;
        private ObservableCollection<ReviewDisplay> _recentReviews = new();

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public Product Product
        {
            get => _product;
            set { _product = value; OnPropertyChanged(); }
        }

        public decimal DiscountedPrice
        {
            get => _discountedPrice;
            set { _discountedPrice = value; OnPropertyChanged(); }
        }

        public bool HasDiscount
        {
            get => _hasDiscount;
            set { _hasDiscount = value; OnPropertyChanged(); }
        }

        public int DiscountPercent
        {
            get => _discountPercent;
            set { _discountPercent = value; OnPropertyChanged(); }
        }

        private string _favoriteButtonText = "♡ В избранное";
        public string FavoriteButtonText
        {
            get => _favoriteButtonText;
            set { _favoriteButtonText = value; OnPropertyChanged(); }
        }

        public IBrush StockColor => (Product?.StockQuantity ?? 0) > 0
            ? new SolidColorBrush(Color.Parse("#27AE60"))
            : new SolidColorBrush(Color.Parse("#E74C3C"));

        public double AverageRating
        {
            get => _averageRating;
            set { _averageRating = value; OnPropertyChanged(); }
        }

        public int ReviewsCount
        {
            get => _reviewsCount;
            set { _reviewsCount = value; OnPropertyChanged(); }
        }

        public ObservableCollection<ReviewDisplay> RecentReviews
        {
            get => _recentReviews;
            set { _recentReviews = value; OnPropertyChanged(); }
        }

        public class ReviewDisplay
        {
            public string CustomerName { get; set; } = "";
            public int Rating { get; set; }
            public string Comment { get; set; } = "";
            public DateTime CreatedAt { get; set; }
            public string DisplayComment => string.IsNullOrWhiteSpace(Comment) ? "(без комментария)" : Comment;
        }

        public ProductDetailsWindow()
        {
            InitializeComponent();
        }

        public ProductDetailsWindow(Product product, Customer currentUser)
        {
            InitializeComponent();
            _product = product;
            _currentUser = currentUser;
            DataContext = this;

            LoadProductData();
            LoadFavoriteStatus();
            LoadReviews();
        }

        private void LoadProductData()
        {
            Product = _product;
            _hasDiscount = IsDiscountActive(_product);
            if (_hasDiscount)
            {
                _discountPercent = _product.DiscountPercent;
                _discountedPrice = GetDiscountedPrice(_product);
            }
            else
            {
                _discountedPrice = _product.Price;
            }
            OnPropertyChanged(nameof(DiscountedPrice));
            OnPropertyChanged(nameof(HasDiscount));
            OnPropertyChanged(nameof(DiscountPercent));
            OnPropertyChanged(nameof(StockColor));
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

        private async void LoadFavoriteStatus()
        {
            if (_currentUser == null)
            {
                FavoriteButtonText = "♡ В избранное";
                return;
            }
            using var ctx = new DemoContext();
            _isFavorite = await ctx.Favorites.AnyAsync(f => f.CustomerId == _currentUser.Id && f.ProductId == _product.Id);
            FavoriteButtonText = _isFavorite ? "❤️ В избранном" : "♡ В избранное";
        }

        private async void LoadReviews()
        {
            using var ctx = new DemoContext();
            var reviews = await ctx.Reviews
                .Where(r => r.ProductId == _product.Id)
                .Include(r => r.Customer)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            ReviewsCount = reviews.Count;
            AverageRating = reviews.Any() ? reviews.Average(r => r.Rating) : 0;

            RecentReviews.Clear();
            foreach (var rev in reviews.Take(5))
            {
                RecentReviews.Add(new ReviewDisplay
                {
                    CustomerName = rev.Customer.Fullname,
                    Rating = rev.Rating,
                    Comment = rev.Comment ?? "",
                    CreatedAt = (DateTime)rev.CreatedAt
                });
            }
        }

        private async void AddToCart_Click(object? sender, RoutedEventArgs e)
        {
            if (_currentUser == null)
            {
                await ShowMessage("Внимание", "Авторизуйтесь", MsBox.Avalonia.Enums.Icon.Warning);
                return;
            }

            using var ctx = new DemoContext();

            int productId = Product.Id;

            var product = await ctx.Products.FindAsync(productId);
            if (product == null)
            {
                await ShowMessage("Ошибка", "Товар не найден", MsBox.Avalonia.Enums.Icon.Error);
                return;
            }

            if (product.StockQuantity <= 0)
            {
                await ShowMessage("Нет в наличии", "Товар закончился", MsBox.Avalonia.Enums.Icon.Warning);
                return;
            }

            var cart = await ctx.Carts.FirstOrDefaultAsync(c => c.CustomerId == _currentUser.Id);
            if (cart == null)
            {
                cart = new Cart { CustomerId = _currentUser.Id };
                ctx.Carts.Add(cart);
                await ctx.SaveChangesAsync();
            }

            var cartItem = await ctx.CartItems
                .FirstOrDefaultAsync(ci => ci.CartId == cart.Id && ci.ProductId == productId);

            int currentInCart = cartItem?.Quantity ?? 0;

            if (currentInCart + 1 > product.StockQuantity)
            {
                await ShowMessage("Ограничение",
                    $"На складе только {product.StockQuantity} шт. Вы уже добавили {currentInCart} шт.",
                    MsBox.Avalonia.Enums.Icon.Warning);
                return;
            }

            if (cartItem == null)
            {
                ctx.CartItems.Add(new CartItem
                {
                    CartId = cart.Id,
                    ProductId = productId,
                    Quantity = 1
                });
            }
            else
            {
                cartItem.Quantity++;
            }

            await ctx.SaveChangesAsync();

            await ShowMessage("Добавлено",
                $"Товар добавлен в корзину. Теперь в корзине: {currentInCart + 1} шт.",
                MsBox.Avalonia.Enums.Icon.Success);
        }

        private async void ToggleFavorite_Click(object? sender, RoutedEventArgs e)
        {
            if (_currentUser == null)
            {
                await ShowMessage("Внимание", "Войдите в аккаунт", MsBox.Avalonia.Enums.Icon.Warning);
                return;
            }

            using var ctx = new DemoContext();

            int productId = Product.Id;
            int userId = _currentUser.Id;

            var existing = await ctx.Favorites
                .FirstOrDefaultAsync(f => f.CustomerId == userId && f.ProductId == productId);

            if (existing != null)
            {
                ctx.Favorites.Remove(existing);
                _isFavorite = false;
                FavoriteButtonText = "♡ В избранное";
            }
            else
            {
                ctx.Favorites.Add(new Favorite
                {
                    CustomerId = userId,
                    ProductId = productId
                });

                _isFavorite = true;
                FavoriteButtonText = "❤️ В избранном";
            }

            await ctx.SaveChangesAsync();
        }

        private async void WriteReview_Click(object? sender, RoutedEventArgs e)
        {
            if (_currentUser == null)
            {
                await ShowMessage("Внимание", "Войдите в аккаунт, чтобы оставить отзыв", MsBox.Avalonia.Enums.Icon.Warning);
                return;
            }
            var reviewsWindow = new ProductReviewsWindow(_product, _currentUser);
            await reviewsWindow.ShowDialog(this);
            LoadReviews();
        }

        private void Back_Click(object? sender, RoutedEventArgs e)
        {
            Close();
        }

        private async void ViewAllReviews_Click(object? sender, RoutedEventArgs e)
        {
            var reviewsWindow = new ProductReviewsWindow(_product, _currentUser);
            await reviewsWindow.ShowDialog(this);
            LoadReviews();
        }

        private async Task ShowMessage(string title, string message, Icon icon)
        {
            var box = MessageBoxManager.GetMessageBoxStandard(title, message, ButtonEnum.Ok, icon);
            await box.ShowAsync();
        }
    }
}