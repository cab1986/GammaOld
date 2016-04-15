using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Gamma.Dialogs
{
    /// <summary>
    /// Interaction logic for ChangeSpoolDialog.xaml
    /// </summary>
    public partial class ChangeSpoolDialog : Window
    {
        public ChangeSpoolDialog()
        {
            InitializeComponent();
            RadioCompletly.IsChecked = true;
            RejectionReasons = (from r in DB.GammaBase.GetSpoolRejectionReasons()
                 select new RejectionReason
                 {
                     RejectionReasonID = (Guid)r.RejectionReasonID,
                     Description = r.Description,
                     FullDescription = r.FullDescription
                 }).ToList();
            LkpBrokeReason.ItemsSource = RejectionReasons;
        }
        public ChangeSpoolDialog(Guid productSpoolid) : this()
        {
            EdtBrokeWeight.Value = DB.GammaBase.ProductSpools.Where(p => p.ProductID == productSpoolid).Select(p => p.Weight).FirstOrDefault();
        }
        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            switch (ChangeState)
            {
                case SpoolChangeState.WithBroke:
                    Weight = Convert.ToInt32(EdtBrokeWeight.EditValue);
                    RejectionReasonID = (Guid)LkpBrokeReason.EditValue;
                    break;
                case SpoolChangeState.WithRemainder:
                    Weight = Convert.ToInt32(EdtRemainderWeight.Text);
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
        public int Weight { get; set; }
        public Guid? RejectionReasonID { get; set; }
        private List<RejectionReason> RejectionReasons { get; set; }
        public SpoolChangeState ChangeState { get; set; }
    }
}
