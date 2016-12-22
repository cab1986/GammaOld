// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
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
