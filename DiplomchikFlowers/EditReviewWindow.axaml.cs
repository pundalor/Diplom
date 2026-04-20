using Avalonia.Controls;
using Avalonia.Interactivity;
using DiplomchikFlowers.Model;
using Microsoft.EntityFrameworkCore;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System;
using System.Threading.Tasks;
using static DiplomchikFlowers.ProductReviewsWindow;

namespace DiplomchikFlowers
{
    public partial class EditReviewWindow : Window
    {
        private ReviewDisplay _review;

        public EditReviewWindow()
        {
            InitializeComponent();
        }

        public EditReviewWindow(ReviewDisplay review)
        {
            InitializeComponent();
            _review = review;
            foreach (ComboBoxItem item in RatingComboBox.Items)
            {
                if (int.Parse(item.Content?.ToString() ?? "0") == review.Rating)
                {
                    RatingComboBox.SelectedItem = item;
                    break;
                }
            }
            CommentBox.Text = review.Comment;
        }

        private async void Save_Click(object? sender, RoutedEventArgs e)
        {
            var selectedRating = RatingComboBox.SelectedItem as ComboBoxItem;
            int rating = int.Parse(selectedRating?.Content?.ToString() ?? "5");
            string comment = CommentBox.Text?.Trim();

            using var ctx = new DemoContext();
            var dbReview = await ctx.Reviews.FindAsync(_review.Id);
            if (dbReview != null)
            {
                dbReview.Rating = rating;
                dbReview.Comment = string.IsNullOrWhiteSpace(comment) ? null : comment;
                dbReview.UpdatedAt = DateTime.Now;
                await ctx.SaveChangesAsync();
            }

            Close();
        }

        private void Cancel_Click(object? sender, RoutedEventArgs e) => Close();
    }
}