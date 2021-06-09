using System;
using Gamma.ViewModels;

namespace Gamma.Views
{
    /// <summary>
    /// Логика взаимодействия для DocRepackOrderView.xaml
    /// </summary>
    public partial class DocRepackView
    {
        private DocRepackView()
        {
            InitializeComponent();
        }

        public DocRepackView(Guid? docId)
        {
            DataContext = new DocRepackViewModel(docId);
            InitializeComponent();
        }
    }
}
