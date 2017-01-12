// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
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

        public DateTime DateBegin { get { return EdtDateBegin.DateTime; } }
        public DateTime DateEnd { get { return EdtDateEnd.DateTime; } }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var curDate = DateTime.Today;
            EdtDateBegin.DateTime = new DateTime(curDate.Year, curDate.Month, 1).AddMonths(-1);
            EdtDateEnd.DateTime = new DateTime(curDate.Year, curDate.Month, 1).AddSeconds(-1);
        }
    }
}
