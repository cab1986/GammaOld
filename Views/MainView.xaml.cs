using System.Windows;
using DevExpress.Mvvm;
using Gamma.Entities;
using Gamma.Models;
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
        public MainView(GammaEntities gammaBase = null)
        {
            DataContext = new MainViewModel(gammaBase);
            InitializeComponent();
            Closed += MainView_Closed;
        }
        void MainView_Closed(object sender, System.EventArgs e)
        {
            Messenger.Default.Unregister(this);
        }          
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (MessageBox.Show("Хотите закончить работу с программой?", "Завершение работы", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                e.Cancel = true;
 //           DXSerializer.Serialize(this, "testLayout.xml", "Gamma", null);
            base.OnClosing(e);
        }
        protected override void OnClosed(System.EventArgs e)
        {
            base.OnClosed(e);
            Application.Current.Shutdown();
        }
    }
}