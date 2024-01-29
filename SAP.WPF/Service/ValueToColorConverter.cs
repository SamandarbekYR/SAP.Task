using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace SAP.WPF.Service
{
    public class ValueToColorConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            double minValue = (double)values[0];
            double[] dataValues = ((IEnumerable<double>)values[1]).ToArray();
            double maxValue = dataValues.Max();

            // Assuming here you want green for minimum and maximum values
            if (dataValues.Length > 0)
            {
                if (dataValues.All(value => value == minValue))
                    return Brushes.Green;

                if (dataValues.All(value => value == maxValue))
                    return Brushes.Green;
            }

            // Default color
            return Brushes.PaleVioletRed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
