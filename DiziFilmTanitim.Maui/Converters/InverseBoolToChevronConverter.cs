using System.Globalization;
using Microsoft.Maui.Controls;

namespace DiziFilmTanitim.MAUI.Converters
{
    public class InverseBoolToChevronConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool isExpanded)
            {
                return isExpanded ? "▼" : "▶"; // True ise aşağı, false ise sağa bakan ok
            }
            return "▶"; // Varsayılan veya geçersiz değer için
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}