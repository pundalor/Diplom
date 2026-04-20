using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace DiplomchikFlowers
{
    public class DateOnlyToStringConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is DateOnly date)
                return date.ToString("dd.MM.yyyy");
            if (value is DateTime dt)
                return dt.ToString("dd.MM.yyyy");
            return "";
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}