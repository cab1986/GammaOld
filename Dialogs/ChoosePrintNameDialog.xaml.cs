// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System;
using System.Linq;
using System.Windows;
using Gamma.Entities;
using System.Data.Entity.SqlServer;

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
            var printNames = (from d in gammaBase.Docs where d.UserID == WorkSession.UserID && d.Date >= SqlFunctions.DateAdd("d", -45, SqlFunctions.GetDate()) select d.PrintName).Distinct().ToList();
            EdtPrintName.ItemsSource = printNames;
        }

        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            PrintName = EdtPrintName.Text.Trim();
            DialogResult = true;
        }

        public string PrintName { get; private set; }

        private void EdtPrintName_EditValueChanged(object sender, DevExpress.Xpf.Editors.EditValueChangedEventArgs e)
        {
            btnOK.IsEnabled = !string.IsNullOrEmpty(EdtPrintName.Text);
        }

    }
}
