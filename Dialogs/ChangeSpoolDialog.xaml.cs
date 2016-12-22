// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Gamma.Entities;
using Gamma.Models;

namespace Gamma.Dialogs
{
    /// <summary>
    /// Interaction logic for ChangeSpoolDialog.xaml
    /// </summary>
    public partial class ChangeSpoolDialog : Window
    {
        public ChangeSpoolDialog(GammaEntities gammaBase = null)
        {
            GammaBase = gammaBase ?? DB.GammaDb;
            InitializeComponent();
            RadioCompletly.IsChecked = true;
            RejectionReasons = (from r in GammaBase.GetSpoolRejectionReasons()
                 select new RejectionReason
                 {
                     RejectionReasonID = (Guid)r.RejectionReasonID,
                     Description = r.Description,
                     FullDescription = r.FullDescription
                 }).ToList();
            LkpBrokeReason.ItemsSource = RejectionReasons;
        }
        private GammaEntities GammaBase { get; set; }

        public ChangeSpoolDialog(Guid productSpoolid) : this()
        {
            var maxValue = GammaBase.ProductSpools.Where(p => p.ProductID == productSpoolid).Select(p => p.DecimalWeight).FirstOrDefault();
            if (maxValue < 100) maxValue = maxValue*1000;
            EdtBrokeWeight.Value = maxValue;
            EdtBrokeWeight.MaxValue = maxValue;
            EdtRemainderWeight.MaxValue = maxValue;
        }
        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            switch (ChangeState)
            {
                case SpoolChangeState.WithBroke:
                    Weight = Convert.ToDecimal(EdtBrokeWeight.EditValue);
                    RejectionReasonID = (Guid)LkpBrokeReason.EditValue;
                    break;
                case SpoolChangeState.WithRemainder:
                    Weight = Convert.ToDecimal(EdtRemainderWeight.Text);
                    break;
                default:
                    Weight = 0;
                    RejectionReasonID = null;
                    break;
            }
            DialogResult = true;
        }
        private void RadioCompletly_Checked(object sender, RoutedEventArgs e)
        {
            ChangeState = SpoolChangeState.FullyConverted;
        }
        private void RadioBroke_Checked(object sender, RoutedEventArgs e)
        {
            ChangeState = SpoolChangeState.WithBroke;
//            Weight = edtBrokeWeight.Text;
//            RejectionReasonID = (Guid)lkpBrokeReason.EditValue;
        }

        private void RadioReminder_Checked(object sender, RoutedEventArgs e)
        {
            ChangeState = SpoolChangeState.WithRemainder;
 //           Weight = edtRemainderWeight.Text;
        }
        public decimal Weight { get; set; }
        public Guid? RejectionReasonID { get; set; }
        private List<RejectionReason> RejectionReasons { get; set; }
        public SpoolChangeState ChangeState { get; set; }
    }
}
