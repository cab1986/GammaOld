using GalaSoft.MvvmLight.Command;

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
