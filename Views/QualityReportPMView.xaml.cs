using System;
using System.Windows;
using Gamma.ViewModels;
using Microsoft.Win32;

namespace Gamma.Views
{
    /// <summary>
    /// Логика взаимодействия для QualityReportPMView.xaml
    /// </summary>
    public partial class QualityReportPMView
    {
        public QualityReportPMView()
        {
            DataContext = new QualityReportPMViewModel();
            InitializeComponent();
        }

        private void ExportToXLS_ItemClick(object sender, DevExpress.Xpf.Bars.ItemClickEventArgs e)
        {
            var dialog = new SaveFileDialog
            {
                DefaultExt = ".xls",
                Filter = "Файлы excel (.xls)|*.xls" // Filter files by extension
        };
            var result = dialog.ShowDialog();
            var filePath = "";
            if (result == true)
            {
                filePath = dialog.FileName;
            }
            if (string.IsNullOrEmpty(filePath)) return;
            try
            {
                View.ExportToXls(filePath);
            }
            catch (Exception)
            {
                MessageBox.Show(@"Не удалось сохранить");
            }
            
        }
    }
}
