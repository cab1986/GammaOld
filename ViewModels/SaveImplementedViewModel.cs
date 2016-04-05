using DevExpress.Mvvm;
using Gamma.Common;
using System;

namespace Gamma.ViewModels
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class SaveImplementedViewModel : ValidationViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the DataBaseEditViewModel class.
        /// </summary>
        public SaveImplementedViewModel()
        {
            SaveAndCloseCommand = new DelegateCommand(SaveToModelAndClose,CanSaveExecute);
        }

        public DelegateCommand SaveAndCloseCommand { get; private set; }

        public virtual void SaveToModel()
        {

        }

        public virtual void SaveToModel(Guid itemId)
        {
            UIServices.SetBusyState();
        }

        protected void SaveToModelAndClose()
        {
            SaveToModel();
            CloseWindow();
        }

        public virtual bool CanSaveExecute()
        {
            return IsValid;
        }
    }
}