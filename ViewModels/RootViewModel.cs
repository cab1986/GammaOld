// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using DevExpress.Mvvm;
using Gamma.Entities;


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
            if (GammaSettings.IsConnectionStringSetted)
                GammaBase = DB.GammaDbWithNoCheckConnection;
            CloseCommand = new DelegateCommand(CloseWindow);
        }

        /*
       protected RootViewModel(GammaEntities gammaBase = null): this()
        {
            GammaBase = gammaBase ?? DB.GammaDb;
        }
        */
        protected GammaEntities GammaBase { get; set; }

        protected void CloseWindow()
        {
//            DB.RollBack();
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

        private void Cleanup()
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