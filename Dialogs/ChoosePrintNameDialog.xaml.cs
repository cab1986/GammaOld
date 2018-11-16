// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System;
using System.Linq;
using System.Windows;
using Gamma.Entities;
using System.Data.Entity.SqlServer;
using System.Collections.Generic;

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
            GammaBase = gammaBase ?? DB.GammaDb;
            List<string> printNames;
            if (WorkSession.IsShipmentWarehouse || WorkSession.IsTransitWarehouse)
            {
                printNames = (from d in GammaBase.Persons select d.Name).Distinct().ToList();
                EdtPrintName.IsTextEditable = false;
            }
            else
            {
                printNames = (from d in GammaBase.Docs where d.UserID == WorkSession.UserID && d.Date >= SqlFunctions.DateAdd("d", -45, SqlFunctions.GetDate()) select d.PrintName).Distinct().ToList();
                EdtPrintName.IsTextEditable = true;
            }
            EdtPrintName.ItemsSource = printNames;
        }

        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            PrintName = EdtPrintName.Text.Trim();
            if (WorkSession.IsShipmentWarehouse || WorkSession.IsTransitWarehouse)
            {
                PersonID = (from d in GammaBase.Persons where d.Name == PrintName select d.PersonID).FirstOrDefault();
            }
            DialogResult = true;
        }

        public string PrintName { get; private set; }
        public Guid PersonID { get; private set; }
        private GammaEntities GammaBase { get; set; }

        private void EdtPrintName_EditValueChanged(object sender, DevExpress.Xpf.Editors.EditValueChangedEventArgs e)
        {
            btnOK.IsEnabled = !string.IsNullOrEmpty(EdtPrintName.Text);
        }

    }
}
