using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Garage.Helpers
{
    /// <summary>
    /// Retourne Visible uniquement pour les entretiens encore en cours et dont la date n'est pas aujourd'hui.
    /// Permet de masquer l'action "Marquer comme fait aujourd'hui" une fois l'entretien terminé.
    /// </summary>
    public class MaintenanceStatusToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is MaintenanceRecord m)
            {
                // Si le statut est "Terminé" on masque le bouton "Terminer"
                if (string.Equals(m.Statut, "Terminé", StringComparison.OrdinalIgnoreCase))
                    return Visibility.Collapsed;

                // Sinon ("En Cours"), l'action reste visible
                return Visibility.Visible;
            }

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
