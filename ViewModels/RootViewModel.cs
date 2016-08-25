using System;
using DevExpress.Mvvm;
using Gamma.Models;


namespace Gamma.ViewModels
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// </summary>
    public abstract class RootViewModel : ViewModelBase, IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the RootViewModel class.
        /// </summary>
        protected RootViewModel()
        {
            CloseCommand = new DelegateCommand(CloseWindow);
        }

        protected RootViewModel(GammaEntities gammaBase = null): this()
        {
            GammaBase = gammaBase ?? DB.GammaDb;
        }
        protected GammaEntities GammaBase { get; set; }
        protected void CloseWindow()
        {
            DB.RollBack();
            CloseSignal = true;
            Cleanup();
        }

        private bool _closeSignal;

        public bool CloseSignal
        {
            get { return _closeSignal; }
            protected set
            { 
                _closeSignal = value;
                RaisePropertyChanged("CloseSignal");
            }
        }

        public DelegateCommand CloseCommand { get; private set; }

        protected void Cleanup()
        {
            Messenger.Default.Unregister(this);
//            GammaBase?.Dispose();
            CloseCommand = null;
        }

/*
        public RootViewModel Clone()
        {
            return (RootViewModel)MemberwiseClone();
        }
*/

        public virtual void Dispose()
        {
            Cleanup();
        }
    }
}