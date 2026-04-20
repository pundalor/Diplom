using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using DiplomchikFlowers.Model;
using Microsoft.EntityFrameworkCore;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DiplomchikFlowers
{
    public partial class ProductEditWindow : Window
    {
        private Product? _product;
        private Customer _currentUser;
        private bool _isNew;
        private string? _selectedImagePath;
        private string? _oldImageFileName;
        private Dictionary<string, int> _categoriesDict;

        public ProductEditWindow(Product? product, Customer currentUser)
        {
            InitializeComponent();
            _product = product;
            _currentUser = currentUser;
            _isNew = product == null;

            Title = _isNew ? "Добавление товара" : "Редактирование товара";

            LoadCategories();

            if (!_isNew && _product != null)
            {
                NameBox.Text = _product.Name;
                DescriptionBox.Text = _product.Description;
                PriceBox.Text = _product.Price.ToString("F2", CultureInfo.InvariantCulture);
                StockBox.Text = _product.StockQuantity.ToString();
                _oldImageFileName = _product.ImageUrl;
                DiscountPercentBox.Text = _product.DiscountPercent.ToString();
                if (_product.DiscountStartDate.HasValue)
                    DiscountStartDatePicker.SelectedDate = new DateTimeOffset(_product.DiscountStartDate.Value);
                if (_product.DiscountEndDate.HasValue)
                    DiscountEndDatePicker.SelectedDate = new DateTimeOffset(_product.DiscountEndDate.Value);

                LoadCurrentImage();

                if (_product.CategoryId > 0 && _categoriesDict != null)
                {
                    var catName = _categoriesDict.FirstOrDefault(x => x.Value == _product.CategoryId).Key;
                    if (catName != null)
                        CategoryCombo.SelectedItem = catName;
                }
            }
        }

        private void LoadCurrentImage()
        {
            try
            {
                if (!string.IsNullOrEmpty(_product?.ImageUrl))
                {
                    string absolutePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", _product.ImageUrl);
                    if (File.Exists(absolutePath))
                    {
                        ImagePreview.Source = new Bitmap(absolutePath);
                        ImagePathBox.Text = _product.ImageUrl;
                    }
                    else
                    {
                        ImagePathBox.Text = "Файл не найден";
                        ImagePreview.Source = null;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки изображения: {ex.Message}");
                ImagePreview.Source = null;
                ImagePathBox.Text = "Ошибка загрузки";
            }
        }

        private void LoadCategories()
        {
            using var ctx = new DemoContext();
            var categories = ctx.Categories.ToList();
            _categoriesDict = categories.ToDictionary(c => c.Name, c => c.Id);
            CategoryCombo.ItemsSource = _categoriesDict.Keys.ToList();
        }

        private async void SelectImage_Click(object? sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Title = "Выберите изображение",
                AllowMultiple = false,
                Filters = new List<FileDialogFilter>
                {
                    new FileDialogFilter { Name = "Изображения", Extensions = { "jpg", "jpeg", "png", "bmp", "gif" } }
                }
            };
            var result = await dialog.ShowAsync(this);
            if (result != null && result.Length > 0)
            {
                _selectedImagePath = result[0];
                ImagePathBox.Text = Path.GetFileName(_selectedImagePath);
                try
                {
                    ImagePreview.Source = new Bitmap(_selectedImagePath);
                }
                catch (Exception ex)
                {
                    await ShowError($"Не удалось загрузить изображение: {ex.Message}");
                }
            }
        }

        private bool TryParseDecimal(string input, out decimal result)
        {
            result = 0;
            if (string.IsNullOrWhiteSpace(input)) return false;
            input = input.Replace(',', '.');
            return decimal.TryParse(input, NumberStyles.Number, CultureInfo.InvariantCulture, out result);
        }

        private bool TryParseInt(string input, out int result)
        {
            result = 0;
            if (string.IsNullOrWhiteSpace(input)) return false;
            return int.TryParse(input, NumberStyles.Integer, CultureInfo.InvariantCulture, out result);
        }

        private async void Save_Click(object? sender, RoutedEventArgs e)
        {
            ErrorTextBlock.IsVisible = false;
            ErrorTextBlock.Text = "";

            string name = NameBox.Text?.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                await ShowError("Название товара обязательно.");
                return;
            }
            if (name.Length > 100)
            {
                await ShowError("Название не должно превышать 100 символов.");
                return;
            }

            if (!TryParseDecimal(PriceBox.Text, out decimal price) || price <= 0)
            {
                await ShowError("Введите корректную цену больше нуля.");
                return;
            }
            if (price > 1_000_000)
            {
                await ShowError("Цена не может превышать 1 000 000.");
                return;
            }

            if (!TryParseInt(StockBox.Text, out int stock) || stock < 0)
            {
                await ShowError("Введите корректное целое количество (без десятичных).");
                return;
            }

            string selectedCategoryName = CategoryCombo.SelectedItem as string;
            if (string.IsNullOrWhiteSpace(selectedCategoryName) || !_categoriesDict.ContainsKey(selectedCategoryName))
            {
                await ShowError("Выберите категорию.");
                return;
            }
            int categoryId = _categoriesDict[selectedCategoryName];

            string description = DescriptionBox.Text?.Trim() ?? "";
            if (description.Length > 500)
            {
                await ShowError("Описание не должно превышать 500 символов.");
                return;
            }

            if (!TryParseInt(DiscountPercentBox.Text, out int discountPercent) || discountPercent < 0 || discountPercent > 100)
            {
                await ShowError("Процент скидки должен быть целым числом от 0 до 100.");
                return;
            }

            DateTime? discountStart = DiscountStartDatePicker.SelectedDate?.DateTime;
            DateTime? discountEnd = DiscountEndDatePicker.SelectedDate?.DateTime;

            if (discountPercent > 0)
            {
                if (!discountStart.HasValue || !discountEnd.HasValue)
                {
                    await ShowError("Для скидки укажите даты начала и окончания.");
                    return;
                }
                if (discountEnd.Value <= discountStart.Value)
                {
                    await ShowError("Дата окончания скидки должна быть позже даты начала.");
                    return;
                }
            }

            using var ctx = new DemoContext();
            bool nameExists = await ctx.Products.AnyAsync(p => p.Name == name && (_isNew || p.Id != _product!.Id));
            if (nameExists)
            {
                await ShowError("Товар с таким названием уже существует.");
                return;
            }

            string? finalImageFileName = _oldImageFileName;
            if (_selectedImagePath != null)
            {
                var fileInfo = new FileInfo(_selectedImagePath);
                if (fileInfo.Length > 5 * 1024 * 1024)
                {
                    await ShowError("Размер изображения не должен превышать 5 МБ.");
                    return;
                }

                string imagesDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");
                if (!Directory.Exists(imagesDir))
                    Directory.CreateDirectory(imagesDir);

                string ext = Path.GetExtension(_selectedImagePath);
                string newFileName = $"{Guid.NewGuid():N}{ext}";
                string destPath = Path.Combine(imagesDir, newFileName);

                File.Copy(_selectedImagePath, destPath, overwrite: true);
                finalImageFileName = newFileName;

                if (!_isNew && !string.IsNullOrEmpty(_oldImageFileName))
                {
                    string oldAbsolutePath = Path.Combine(imagesDir, _oldImageFileName);
                    if (File.Exists(oldAbsolutePath))
                        File.Delete(oldAbsolutePath);
                }
            }

            if (_isNew)
            {
                _product = new Product();
                ctx.Products.Add(_product);
            }
            else
            {
                _product = await ctx.Products.FindAsync(_product!.Id);
                if (_product == null)
                {
                    await ShowError("Товар не найден в базе данных.");
                    return;
                }
            }

            _product.Name = name;
            _product.Description = description;
            _product.Price = price;
            _product.StockQuantity = stock;
            _product.CategoryId = categoryId;
            _product.DiscountPercent = discountPercent;
            _product.DiscountStartDate = discountStart;
            _product.DiscountEndDate = discountEnd;
            _product.UpdatedAt = DateTime.Now;
            _product.ImageUrl = finalImageFileName;

            try
            {
                await ctx.SaveChangesAsync();
                await ShowMessage("Успешно", "Товар сохранён", MsBox.Avalonia.Enums.Icon.Success);
                Close();
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException?.Message.Contains("duplicate key") == true ||
                    ex.InnerException?.Message.Contains("23505") == true)
                {
                    await ShowError("Ошибка базы данных: конфликт идентификаторов. Попробуйте перезапустить приложение.");
                }
                else
                {
                    await ShowError($"Ошибка сохранения: {ex.Message}");
                }
            }
        }

        private void Cancel_Click(object? sender, RoutedEventArgs e) => Close();

        private async Task ShowError(string message)
        {
            ErrorTextBlock.Text = message;
            ErrorTextBlock.IsVisible = true;
            var box = MessageBoxManager.GetMessageBoxStandard("Ошибка", message, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowAsync();
        }

        private async Task ShowMessage(string title, string message, Icon icon)
        {
            var box = MessageBoxManager.GetMessageBoxStandard(title, message, ButtonEnum.Ok, icon);
            await box.ShowAsync();
        }
    }
}