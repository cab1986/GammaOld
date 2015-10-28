using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using System.Windows;

namespace Gamma.ViewModels
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public abstract class RootViewModel : ViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the RootViewModel class.
        /// </summary>
        public RootViewModel()
        {
            CloseCommand = new RelayCommand(CloseWindow);
        }
        protected void CloseWindow()
        {
            DB.RollBack();
            CloseSignal = true;
            this.Cleanup();
        }

        private bool closeSignal = false;

        public bool CloseSignal
        {
            get { return closeSignal; }
            private set { 
                closeSignal = value;
                RaisePropertyChanged("CloseSignal");
                }
        }

        public RelayCommand CloseCommand { get; private set; }

        //protected bool IsScannerUsed = false;
    }
}