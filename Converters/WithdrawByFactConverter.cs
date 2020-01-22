using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace Gamma.Converters
{
    class WithdrawByFactConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value)
                return "По факту";
            else
                return "По нормативу";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((string)value == "По факту")
                return true;
            else
                return false;
            //throw new NotImplementedException()
        }
    }
}
