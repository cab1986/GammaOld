using System;
using Gamma.ViewModels;

namespace Gamma.Views
{
    /// <summary>
    /// Логика взаимодействия для DocMovementOrderView.xaml
    /// </summary>
    public partial class DocMovementView
    {
        private DocMovementView()
        {
            InitializeComponent();
        }

        public DocMovementView(Guid docId)
        {
            DataContext = new DocMovementViewModel(docId);
            InitializeComponent();
        }
    }
}
