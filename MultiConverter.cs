using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Data;
using DevExpress.Xpf.Grid;

namespace Gamma
{
    class MultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            GridControl grid = values[0] as GridControl;
            int rowHandle = (int)values[1];

            if (rowHandle == GridControl.InvalidRowHandle)
            {
                return "0 of 0";
            }

            int all = grid.DataController.VisibleListSourceRowCount;
            int rowVisibleIndex = grid.GetRowHandleByListIndex(grid.GetListIndexByRowHandle(rowHandle));

            return string.Format("{0} of {1}", rowVisibleIndex + 1, all);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}