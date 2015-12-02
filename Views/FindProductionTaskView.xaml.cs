using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Gamma.ViewModels;

namespace Gamma.Views
{
    /// <summary>
    /// Interaction logic for FindProductionTaskView.xaml
    /// </summary>
    public partial class FindProductionTaskView : Window
    {
        public FindProductionTaskView(PlaceGroups placeGroup)
        {
            this.DataContext = new FindProductionTaskViewModel(placeGroup);
            InitializeComponent();
        }
    }
}
