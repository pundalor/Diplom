using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Interactivity;
using DiplomchikFlowers.Model;
using Microsoft.EntityFrameworkCore;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace DiplomchikFlowers
{
    public class StarTextConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is int number)
            {
                return GetStarForm(number);
            }
            if (value is double doubleNumber)
            {
                return GetStarForm((int)Math.Round(doubleNumber));
            }
            return "звезд";
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();

        private string GetStarForm(int number)
        {
            int lastDigit = number % 10;
            int lastTwoDigits = number % 100;

            if (lastTwoDigits >= 11 && lastTwoDigits <= 14)
                return "звезд";

            return lastDigit switch
            {
                1 => "звезда",
                2 or 3 or 4 => "звезды",
                _ => "звезд"
            };
        }
    }

    public partial class ProductReviewsWindow : Window, INotifyPropertyChanged
    {
        private Product _product;
        private Customer _currentUser;
        private ObservableCollection<ReviewDisplay> _reviews = new();
        private double _averageRating;

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public class ReviewDisplay
        {
            public int Id { get; set; }
            public int ProductId { get; set; }
            public int CustomerId { get; set; }
            public int Rating { get; set; }
            public string? Comment { get; set; }
            public DateTime CreatedAt { get; set; }
            public string CustomerName { get; set; } = "";
            public bool IsOwner { get; set; }
            public bool CanDelete { get; set; }
            public string DisplayComment => string.IsNullOrWhiteSpace(Comment) ? "(без комментария)" : Comment;
        }

        public Product Product { get; set; }

        public double AverageRating
        {
            get => _averageRating;
            set
            {
                if (_averageRating != value)
                {
                    _averageRating = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool CanReview => _currentUser != null;

        public ProductReviewsWindow()
        {
            InitializeComponent();
        }

        public ProductReviewsWindow(Product product, Customer currentUser)
        {
            InitializeComponent();
            _product = product;
            _currentUser = currentUser;
            Product = product;
            DataContext = this;
            LoadReviews();
        }

        private void Back_Click(object? sender, RoutedEventArgs e) => Close();

        private async void LoadReviews()
        {
            using var ctx = new DemoContext();
            var reviews = await ctx.Reviews
                .Where(r => r.ProductId == _product.Id)
                .Include(r => r.Customer)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            _reviews.Clear();
            foreach (var rev in reviews)
            {
                var display = new ReviewDisplay
                {
                    Id = rev.Id,
                    ProductId = rev.ProductId,
                    CustomerId = rev.CustomerId,
                    Rating = rev.Rating,
                    Comment = rev.Comment,
                    CreatedAt = (DateTime)rev.CreatedAt,
                    CustomerName = rev.Customer.Fullname,
                    IsOwner = _currentUser != null && rev.CustomerId == _currentUser.Id,
                    CanDelete = (_currentUser != null && rev.CustomerId == _currentUser.Id) || (_currentUser?.Roleid == 1)
                };
                _reviews.Add(display);
            }

            if (_reviews.Any())
                AverageRating = Math.Round(_reviews.Average(r => r.Rating), 2);
            else
                AverageRating = 0;

            ReviewsItemsControl.ItemsSource = _reviews;
        }

        private async void SendReview_Click(object? sender, RoutedEventArgs e)
        {
            if (_currentUser == null) return;

            var selectedRating = RatingComboBox.SelectedItem as ComboBoxItem;
            int rating = int.Parse(selectedRating?.Content?.ToString() ?? "5");
            string comment = CommentBox.Text?.Trim();

            using var ctx = new DemoContext();

            var existingReview = await ctx.Reviews
                .FirstOrDefaultAsync(r => r.ProductId == _product.Id && r.CustomerId == _currentUser.Id);

            if (existingReview != null)
            {
                var updateConfirm = await MessageBoxManager.GetMessageBoxStandard(
                    "Обновить отзыв",
                    "Вы уже оставляли отзыв на этот товар. Хотите обновить его?",
                    ButtonEnum.YesNo,
                    MsBox.Avalonia.Enums.Icon.Question).ShowAsync();

                if (updateConfirm == ButtonResult.Yes)
                {
                    existingReview.Rating = rating;
                    existingReview.Comment = string.IsNullOrWhiteSpace(comment) ? null : comment;
                    existingReview.UpdatedAt = DateTime.Now;
                    ctx.Reviews.Update(existingReview);
                    await ctx.SaveChangesAsync();
                    await ShowMessage("Успех", "Отзыв обновлён", MsBox.Avalonia.Enums.Icon.Success);
                    CommentBox.Text = "";
                    RatingComboBox.SelectedIndex = 0;
                    LoadReviews();
                }
                return;
            }

            var review = new Review
            {
                ProductId = _product.Id,
                CustomerId = _currentUser.Id,
                Rating = rating,
                Comment = string.IsNullOrWhiteSpace(comment) ? null : comment,
                CreatedAt = DateTime.Now
            };
            ctx.Reviews.Add(review);
            await ctx.SaveChangesAsync();

            CommentBox.Text = "";
            RatingComboBox.SelectedIndex = 0;
            await ShowMessage("Успех", "Отзыв добавлен", MsBox.Avalonia.Enums.Icon.Success);
            LoadReviews();
        }

        private async void EditReview_Click(object? sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var review = btn?.Tag as ReviewDisplay;
            if (review == null || !review.IsOwner) return;

            var editWindow = new EditReviewWindow(review);
            await editWindow.ShowDialog(this);
            LoadReviews();
        }

        private async void DeleteReview_Click(object? sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var review = btn?.Tag as ReviewDisplay;
            if (review == null || !review.CanDelete) return;

            var confirm = await MessageBoxManager.GetMessageBoxStandard("Удаление",
                "Удалить отзыв?", ButtonEnum.YesNo, MsBox.Avalonia.Enums.Icon.Warning).ShowAsync();
            if (confirm == ButtonResult.Yes)
            {
                using var ctx = new DemoContext();
                var dbReview = await ctx.Reviews.FindAsync(review.Id);
                if (dbReview != null)
                {
                    ctx.Reviews.Remove(dbReview);
                    await ctx.SaveChangesAsync();
                    await ShowMessage("Удалено", "Отзыв удалён", MsBox.Avalonia.Enums.Icon.Info);
                    LoadReviews();
                }
            }
        }

        private async Task ShowMessage(string title, string message, MsBox.Avalonia.Enums.Icon icon)
        {
            var box = MessageBoxManager.GetMessageBoxStandard(title, message, ButtonEnum.Ok, icon);
            await box.ShowAsync();
        }
    }
}