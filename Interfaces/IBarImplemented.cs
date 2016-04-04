using System;
using System.Collections.ObjectModel;

namespace Gamma.Interfaces
{
    interface IBarImplemented
    {
        ObservableCollection<BarViewModel> Bars { get; set; }
        Guid? VMID { get; }
    }
}
