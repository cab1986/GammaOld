using DevExpress.Mvvm;


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
            CloseCommand = new DelegateCommand(CloseWindow);
        }
        
        protected void CloseWindow()
        {
            DB.RollBack();
            CloseSignal = true;
            this.Cleanup();
        }

        private bool _closeSignal = false;

        public bool CloseSignal
        {
            get { return _closeSignal; }
            private set
            { 
                _closeSignal = value;
                RaisePropertyChanged("CloseSignal");
            }
        }

        public DelegateCommand CloseCommand { get; private set; }

        private void Cleanup()
        {
            Messenger.Default.Unregister(this);
        }
    }
}