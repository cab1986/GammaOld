using System.Windows;
using Gamma.ViewModels;

namespace Gamma.Views
{
    /// <summary>
    /// Interaction logic for ComPortSettingsView.xaml
    /// </summary>
    public partial class ComPortSettingsView : Window
    {
        public ComPortSettingsView()
        {
            this.DataContext = new ComPortSettingsViewModel();
            InitializeComponent();
        }
    }
}
