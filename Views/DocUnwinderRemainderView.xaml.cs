using System;
using Gamma.ViewModels;

namespace Gamma.Views
{
    /// <summary>
    /// Логика взаимодействия для DocUnwinderRemainderView.xaml
    /// </summary>
    public partial class DocUnwinderRemainderView
    {
        public DocUnwinderRemainderView(OpenDocUnwinderRemainderMessage msg)
        {
            DataContext = new DocUnwinderRemainderViewModel(msg);
            InitializeComponent();
        }
    }
}
