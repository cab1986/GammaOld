using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace Gamma.Interfaces
{
    interface IBarImplemented
    {
        ObservableCollection<BarViewModel> Bars { get; set; }
        Guid? VMID { get; }
    }
}
