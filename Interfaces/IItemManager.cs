using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gamma.Interfaces
{
    interface IItemManager
    {
        RelayCommand NewItemCommand { get; set; }
        RelayCommand EditItemCommand { get; set; }
        RelayCommand DeleteItemCommand { get; set; }
        RelayCommand RefreshCommand { get; set; }
    }
}
