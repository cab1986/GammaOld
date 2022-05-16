using System.Windows;
using DevExpress.Mvvm;
using Gamma.ViewModels;

namespace Gamma
{
    /// <summary>
    /// Description for MainView.
    /// </summary>
    public partial class MainView : Window
    {
        /// <summary>
        /// Initializes a new instance of the MainView class.
        /// </summary>
        public MainView()
        {
            DataContext = new MainViewModel();
            InitializeComponent();
        }
    }
}