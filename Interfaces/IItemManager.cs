using DevExpress.Mvvm;

namespace Gamma.Interfaces
{
    interface IItemManager
    {
        DelegateCommand NewItemCommand { get; }
        DelegateCommand<object> EditItemCommand { get; }
        DelegateCommand DeleteItemCommand { get; }
        DelegateCommand RefreshCommand { get; }
    }
}
