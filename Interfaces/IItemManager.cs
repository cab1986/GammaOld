using DevExpress.Mvvm;

namespace Gamma.Interfaces
{
    interface IItemManager
    {
        DelegateCommand NewItemCommand { get; set; }
        DelegateCommand EditItemCommand { get; set; }
        DelegateCommand DeleteItemCommand { get; set; }
        DelegateCommand RefreshCommand { get; set; }
    }
}
