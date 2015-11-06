using GalaSoft.MvvmLight.Messaging;
using System.Windows;
using Gamma.Views;
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
            InitializeComponent();
            Messenger.Default.Register<OpenProductionTaskMessage>(this, OpenProductionTask);
            Messenger.Default.Register<OpenReportListMessage>(this, OpenReportList);
            Messenger.Default.Register<FindProductMessage>(this, OpenFindProduct);
            Messenger.Default.Register<OpenManageUsersMessage>(this, OpenManageUsers);
            this.Closed += MainView_Closed;
        }

        void MainView_Closed(object sender, System.EventArgs e)
        {
            Messenger.Default.Unregister(this);
        }

        private void OpenManageUsers(OpenManageUsersMessage obj)
        {
            var View = new ManageUsersView();
            View.Show();
        }
        private void OpenProductionTask(OpenProductionTaskMessage msg)
        {
            var View = new ProductionTaskView(msg);
            View.Show();
        }
        private void OpenReportList(OpenReportListMessage msg)
        {
            var View = new ReportListView();
            View.Show();
        }
        private void OpenFindProduct(FindProductMessage msg)
        {
            var View = new FindProductView(msg);
            View.Show();
        }
        
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (MessageBox.Show("Хотите закончить работу с программой?", "Завершение работы", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                e.Cancel = true;
                base.OnClosing(e);
        }
        protected override void OnClosed(System.EventArgs e)
        {
            base.OnClosed(e);
            Application.Current.Shutdown();
        }
    }
}