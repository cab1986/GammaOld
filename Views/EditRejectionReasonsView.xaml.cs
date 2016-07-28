using System;
using System.Collections.Generic;
using System.Windows;
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

        public EditRejectionReasonsView(List<DocBrokeProductRejectionReasons> rejectionReasons, Guid docId,
            Guid productId)
        {
            DataContext = new EditRejectionReasonsViewModel(rejectionReasons, docId, productId);
            InitializeComponent();
        }
    }
}
