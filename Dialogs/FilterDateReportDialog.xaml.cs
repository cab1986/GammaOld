using System;
using System.Windows;

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
