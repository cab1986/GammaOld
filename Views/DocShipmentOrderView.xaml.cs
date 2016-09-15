using System;
using System.Windows;
using Gamma.ViewModels;

namespace Gamma.Views
{
    /// <summary>
    /// Логика взаимодействия для DocShipmentOrderView.xaml
    /// </summary>
    public partial class DocShipmentOrderView : Window
    {
        public DocShipmentOrderView(Guid docShipmentOrderId)
        {
            DataContext = new DocShipmentOrderViewModel(docShipmentOrderId);
            InitializeComponent();
        }
    }
}
