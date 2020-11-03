using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows.Markup;
using System.Windows.Data;
using System.Globalization;


namespace Gamma.Converters
{
    class MarkupMultivalueConverter : MarkupExtension, IMultiValueConverter
        {
            public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
            {
                return values.ToList();
            }
            public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
            public override object ProvideValue(IServiceProvider serviceProvider)
            {
                return this;
            }
    }
}
