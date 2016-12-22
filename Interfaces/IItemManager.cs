// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
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
