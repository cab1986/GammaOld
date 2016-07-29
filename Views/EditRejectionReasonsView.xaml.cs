using System;
using Gamma.Common;
using Gamma.Models;
using Gamma.ViewModels;

namespace Gamma.Views
{
    /// <summary>
    /// Логика взаимодействия для EditRejectionReasonsView.xaml
    /// </summary>
    public partial class EditRejectionReasonsView
    {
        private EditRejectionReasonsView()
        {
            InitializeComponent();
        }

        public EditRejectionReasonsView(ItemsChangeObservableCollection<RejectionReason> rejectionReasons)
        {
            DataContext = new EditRejectionReasonsViewModel(rejectionReasons);
            InitializeComponent();
        }
    }
}
