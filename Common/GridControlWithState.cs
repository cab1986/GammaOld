// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System.Windows;
using DevExpress.Xpf.Grid;

namespace Gamma.Common
{
    public class GridControlWithState : GridControl
    {
        public GridControlWithState() : base()
        {
            
        }

        protected override void OnLoaded(object sender, RoutedEventArgs e)
        {
            base.OnLoaded(sender, e);
            var tableView = View as TableView;
            tableView?.BestFitColumns();          
        }

    }
}
