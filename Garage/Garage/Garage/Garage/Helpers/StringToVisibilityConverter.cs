using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Garage.Helpers
{
    /// <summary>
    /// Retourne Visible si la chaîne n'est ni null ni vide, Collapsed sinon.
    /// </summary>
    public class StringToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.IsNullOrWhiteSpace(value as string)
                ? Visibility.Collapsed
                : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
