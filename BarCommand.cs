using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace Gamma
{
    public class BarViewModel {
        public BarViewModel()
        {
            Name = "";
            Commands = new ObservableCollection<BarCommand<PrintReportMessage>>();
        }
        public string Name { get; set; }
        public  ObservableCollection<BarCommand<PrintReportMessage>> Commands { get; set; }
        }
    public class BarCommand<T> : RelayCommand<T>
    {
        public BarCommand(Action<T> execute) : base(execute) {}
        public  string Caption { get; set; }
        public  ImageSource LargeGlyph { get; set; }
        public  ImageSource SmallGlyph { get; set; }
        public PrintReportMessage CommandParameter { get; set; }
        
    }

}
