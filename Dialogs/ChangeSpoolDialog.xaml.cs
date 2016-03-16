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
            lkpBrokeReason.ItemsSource = RejectionReasons;
        }
        public ChangeSpoolDialog(Guid productSpoolID) : this()
        {
            edtBrokeWeight.Value = DB.GammaBase.ProductSpools.Where(p => p.ProductID == productSpoolID).Select(p => p.Weight).FirstOrDefault();
        }
        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            switch (ChangeState)
            {
                case SpoolChangeState.WithBroke:
                    Weight = Convert.ToInt32(edtBrokeWeight.EditValue);
                    RejectionReasonID = (Guid)lkpBrokeReason.EditValue;
                    break;
                case SpoolChangeState.WithRemainder:
                    Weight = Convert.ToInt32(edtRemainderWeight.Text);
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
