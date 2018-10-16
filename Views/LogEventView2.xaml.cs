using System;
using Gamma.ViewModels;

namespace Gamma.Views
{
    /// <summary>
    /// Логика взаимодействия для LogEventView.xaml
    /// </summary>
    public partial class LogEventView : MvvmWindow
    {
        public LogEventView()
        {
            InitializeComponent();
        }

        public LogEventView(Guid docId)
        {
            DataContext = new LogEventViewModel(docId);
            InitializeComponent();
        }
    }
}
