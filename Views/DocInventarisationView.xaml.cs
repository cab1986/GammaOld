using System;
using Gamma.ViewModels;

namespace Gamma.Views
{
    /// <summary>
    /// Логика взаимодействия для DocInventarisationView.xaml
    /// </summary>
    public partial class DocInventarisationView : MvvmWindow
    {
        public DocInventarisationView()
        {
            InitializeComponent();
        }

        public DocInventarisationView(Guid docId)
        {
            DataContext = new DocInventarisationViewModel(docId);
            InitializeComponent();
        }
    }
}
