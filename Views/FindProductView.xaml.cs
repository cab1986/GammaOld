using System.Windows;
using Gamma.ViewModels;
using GalaSoft.MvvmLight.Messaging;

namespace Gamma.Views

{
    /// <summary>
    /// Description for FindProductView.
    /// </summary>
    public partial class FindProductView : MVVMWindow
    {
        /// <summary>
        /// Initializes a new instance of the FindProductView class.
        /// </summary>
        public FindProductView(FindProductMessage msg)
        {
            this.DataContext = new FindProductViewModel(msg);
            InitializeComponent();
            Messenger.Default.Register<OpenNomenclatureMessage>(this,OpenNomenclature);
            Messenger.Default.Register<OpenDocProductMessage>(this, OpenDocProduct);
        }
        private void OpenNomenclature(OpenNomenclatureMessage msg)
        {
            new NomenclatureView().Show();
        }
        private void OpenDocProduct(OpenDocProductMessage msg)
        {
            new DocProductView(msg).Show();
        }
    }
}