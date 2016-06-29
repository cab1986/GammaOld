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
        public PrintReportMessage CommandParameter { get; set; }
        
    }

}
