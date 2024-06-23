using System.Globalization;
using Microsoft.Maui.Controls;

namespace DiziFilmTanitim.MAUI.Converters
{
    public class InverseBoolConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return !boolValue;
            }
            return true; // Varsayılan olarak, eğer değer bool değilse veya null ise true döndür (gizleme eğilimi)
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return !boolValue;
            }
            return false;
        }
    }
}