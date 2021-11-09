// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Collections.ObjectModel;
using System.Windows.Media;
using DevExpress.Mvvm;

namespace Gamma
{
    public class BarViewModel {
        public BarViewModel()
        {
            Name = "";
            Commands = new ObservableCollection<BarCommand<object>>();
        }
        public string Name { get; set; }
        public  ObservableCollection<BarCommand<object>> Commands { get; set; }
        }
    public class BarCommand<T> : DelegateCommand<T>
    {
        public BarCommand(Action<T> execute) : base(execute) {}
        public  string Caption { get; set; }
        public  ImageSource LargeGlyph { get; set; }
        public  ImageSource SmallGlyph { get; set; }
        public BarCommandParameter CommandParameter { get; set; }
        
    }

    public class BarCommandParameter
    { }

    public class AddDowntimeParameter : BarCommandParameter
    {
        public Guid DowntimeTypeID { get; set; }
        public Guid? DowntimeTypeDetailID { get; set; }
        public Guid? EquipmentNodeID { get; set; }
        public Guid? EquipmentNodeDetailID { get; set; }
        public int? Duration { get; set; }
        public string Comment { get; set; }
    }
}
