using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using System;
using System.Globalization;
using System.IO;

namespace DiplomchikFlowers
{
    public class ImagePathConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string fileName && !string.IsNullOrWhiteSpace(fileName))
            {
                string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", fileName);
                if (File.Exists(fullPath))
                {
                    try
                    {
                        return new Bitmap(fullPath);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Ошибка загрузки изображения {fullPath}: {ex.Message}");
                        return null;
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Изображение не найдено: {fullPath}");
                    return null;
                }
            }
            return null;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}