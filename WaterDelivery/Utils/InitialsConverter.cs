using SchoolApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaterDelivery.Utils
{
    public class InitialsConverter : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is string firstName && !string.IsNullOrEmpty(firstName) &&
                CurrentUser.LastName != null && !string.IsNullOrEmpty(CurrentUser.LastName))
            {
                return $"{firstName[0]}{CurrentUser.LastName[0]}";
            }
            else if (CurrentUser.UserName != null && CurrentUser.UserName.Length > 0)
            {
                return CurrentUser.UserName[0].ToString().ToUpper();
            }

            return "?";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
