using Avalonia.Data.Converters;
using Avalonia.Media;
using DiplomchikFlowers.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiplomchikFlowers
{
    public class SelectedBackgroundConverter : IValueConverter
    {
        public static SelectedBackgroundConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool isSelected && isSelected)
                return new SolidColorBrush(Color.Parse("#FFF0E6F5")); // выделение
            return new SolidColorBrush(Color.Parse("#FFFFFFFF")); // обычный фон
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
