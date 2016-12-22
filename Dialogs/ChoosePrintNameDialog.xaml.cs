// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System.Linq;
using System.Windows;
using Gamma.Entities;

namespace Gamma.Dialogs
{
    /// <summary>
    /// Interaction logic for ChoosePrintNameDialog.xaml
    /// </summary>
    public partial class ChoosePrintNameDialog : Window
    {
        public ChoosePrintNameDialog(GammaEntities gammaBase = null)
        {
            InitializeComponent();
            gammaBase = gammaBase ?? DB.GammaDb;
            var printNames = (from d in gammaBase.Docs where d.UserID == WorkSession.UserID select d.PrintName).Distinct().ToList();
            EdtPrintName.ItemsSource = printNames;
        }
        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            PrintName = EdtPrintName.Text;
            DialogResult = true;
        }
        public string PrintName { get; private set; }
    }
}
