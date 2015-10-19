using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Gamma.Dialogs
{
    /// <summary>
    /// Interaction logic for FilterDateReportDialog.xaml
    /// </summary>
    public partial class FilterDateReportDialog : Window
    {
        public FilterDateReportDialog()
        {
            InitializeComponent();
        }
        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        public DateTime DateBegin { get { return edtDateBegin.DateTime; } }
        public DateTime DateEnd { get { return edtDateEnd.DateTime; } }
    }
}
