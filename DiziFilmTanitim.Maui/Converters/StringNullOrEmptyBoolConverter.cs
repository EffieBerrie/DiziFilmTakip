using System.Globalization;
using Microsoft.Maui.Controls;

namespace DiziFilmTanitim.MAUI.Converters
{
    public class StringNullOrEmptyBoolConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            bool isInverted = false;
            if (parameter != null)
            {
                if (parameter is bool bParam)
                {
                    isInverted = bParam;
                }
                else if (parameter is string sParam && bool.TryParse(sParam, out bool bResult))
                {
                    isInverted = bResult;
                }
            }

            bool isNullOrEmpty = string.IsNullOrEmpty(value as string);

            return isInverted ? !isNullOrEmpty : isNullOrEmpty;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}