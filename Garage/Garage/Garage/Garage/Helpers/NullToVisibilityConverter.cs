using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Garage.Helpers
{
    /// <summary>
    /// Retourne Visible si la valeur est null, Collapsed sinon.
    /// Utile pour afficher un placeholder quand aucun élément n'est sélectionné.
    /// </summary>
    public class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
