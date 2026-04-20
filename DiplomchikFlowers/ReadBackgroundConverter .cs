using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiplomchikFlowers
{
    public class ReadBackgroundConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool isRead && isRead)
                return new SolidColorBrush(Color.Parse("#FFF9F9F9"));  
            else
                return new SolidColorBrush(Color.Parse("#FFF0E6F5"));   // непрочитанное – лёгкий акцент
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
