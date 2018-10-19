using System;
using Gamma.ViewModels;
using System.Windows.Data;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;

namespace Gamma.Views
{
    /// <summary>
    /// Логика взаимодействия для LogEventView.xaml
    /// </summary>
    public partial class LogEventView : MvvmWindow
    {
        public LogEventView(Guid eventID, Guid? parentEventID)
        {
            DataContext = new LogEventViewModel(eventID, parentEventID);
            InitializeComponent();
            treeListView1.ExpandAllNodes();
        }
    }
    public class DepartmentToConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            List<short> list = value as List<short>;
            if (list == null)
                return null;
            return new List<object>(list.Cast<object>());
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            List<object> list = value as List<object>;
            if (list == null)
                return null;
            return new List<short>(list.Cast<short>());
        }
    }
}
