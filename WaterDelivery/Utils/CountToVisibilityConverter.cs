using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows;

namespace WaterDelivery.Utils
{
    /// <summary>
    /// Конвертер для скрытия/отображения элементов на основе количества элементов
    /// </summary>
    public class CountToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return Visibility.Visible;

            int count = 0;
            if (value is int)
            {
                count = (int)value;
            }

            int compareValue = 0;
            if (parameter != null)
            {
                int.TryParse(parameter.ToString(), out compareValue);
            }

            // Если количество элементов равно заданному значению, показываем сообщение
            return count == compareValue ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
