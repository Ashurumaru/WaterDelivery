using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace WaterDelivery.Utils
{
    /// <summary>
    /// Конвертер для отображения префикса бонусных баллов
    /// </summary>
    public class LoyaltyPointPrefixConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int movementTypeId)
            {
                switch (movementTypeId)
                {
                    case 1: // Начисление
                        return "+";
                    case 2: // Списание
                        return "-";
                    default:
                        return "";
                }
            }

            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
