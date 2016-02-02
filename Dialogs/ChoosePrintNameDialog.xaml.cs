using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Gamma.Dialogs
{
    /// <summary>
    /// Interaction logic for ChoosePrintNameDialog.xaml
    /// </summary>
    public partial class ChoosePrintNameDialog : Window
    {
        public ChoosePrintNameDialog()
        {
            InitializeComponent();
            var printNames = (from d in DB.GammaBase.Docs where d.UserID == WorkSession.UserID select d.PrintName).ToList();
            edtPrintName.ItemsSource = printNames;
        }
        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            PrintName = edtPrintName.Text;
            DialogResult = true;
        }
        public string PrintName { get; set; }
    }
}
