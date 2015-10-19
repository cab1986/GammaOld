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
        }
        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
        private void RadioCompletly_Checked(object sender, RoutedEventArgs e)
        {
            ChangeState = SpoolChangeState.FullyConverted;
        }
        private void RadioBroke_Checked(object sender, RoutedEventArgs e)
        {
            ChangeState = SpoolChangeState.WithBroke;
            Weight = edtBrokeWeight.Text;
            Reason = edtBrokeReason.Text;
        }

        private void RadioReminder_Checked(object sender, RoutedEventArgs e)
        {
            ChangeState = SpoolChangeState.WithRemainder;
            Weight = edtRemainderWeight.Text;
        }
        public string Weight { get; set; }
        public string Reason { get; set; }
        public SpoolChangeState ChangeState { get; set; }
    }
}
