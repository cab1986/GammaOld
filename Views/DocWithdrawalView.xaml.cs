using System;
using Gamma.ViewModels;

namespace Gamma.Views
{
    /// <summary>
    /// Логика взаимодействия для DocWithdrawalView.xaml
    /// </summary>
    public partial class DocWithdrawalView
    {
        public DocWithdrawalView(Guid docId)
        {
            DataContext = new DocWithdrawalViewModel(docId);
            InitializeComponent();
        }
    }
}
