using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Garage.Helpers
{
    /// <summary>
    /// Convertit une liste de valeurs (ex: interventions par mois) en PointCollection
    /// pour dessiner une Polyline dans un Canvas.
    /// 
    /// MultiBinding attendu :
    ///  - values[0] : IEnumerable<int> (ou IEnumerable)
    ///  - values[1] : ActualWidth (double)
    ///  - values[2] : ActualHeight (double)
    /// </summary>
    public class ChartPointsConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 3)
                return new PointCollection();

            var seq = values[0] as IEnumerable;
            if (seq == null)
                return new PointCollection();

            if (values[1] is not double width || values[2] is not double height)
                return new PointCollection();

            if (width <= 0 || height <= 0)
                return new PointCollection();

            var data = new List<int>();
            foreach (var item in seq)
            {
                if (item is int i)
                    data.Add(i);
            }

            if (data.Count == 0)
                return new PointCollection();

            // Un peu de marge interne pour éviter que les points collent aux bords.
            const double paddingX = 4;
            const double paddingY = 4;

            var plotW = Math.Max(0, width - (paddingX * 2));
            var plotH = Math.Max(0, height - (paddingY * 2));

            var max = Math.Max(1, data.Max());

            var points = new PointCollection(data.Count);

            for (int idx = 0; idx < data.Count; idx++)
            {
                var x = (data.Count == 1)
                    ? paddingX + (plotW / 2)
                    : paddingX + (idx / (double)(data.Count - 1)) * plotW;

                // y=0 en haut, y augmente vers le bas -> on inverse.
                var ratio = data[idx] / (double)max;
                var y = paddingY + (1.0 - ratio) * plotH;

                points.Add(new Point(x, y));
            }

            return points;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
