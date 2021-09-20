using System.Windows;
using Gamma.ViewModels;

namespace Gamma.Views
{
    /// <summary>
    /// Логика взаимодействия для DowntimeTemplatesView.xaml
    /// </summary>
    public partial class DowntimeTemplatesView : Window
    {
        public DowntimeTemplatesView()
        {
            DataContext = new DowntimeTemplatesViewModel();
            InitializeComponent();
        }
    }
}
