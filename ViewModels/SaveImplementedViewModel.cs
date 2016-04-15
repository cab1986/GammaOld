using DevExpress.Mvvm;
using Gamma.Common;
using System;

namespace Gamma.ViewModels
{
    /// <summary>
    /// Класс, реализующий сохранение в БД.
    /// </summary>
    public class SaveImplementedViewModel : ValidationViewModelBase
    {
        /// <summary>
        /// Инициализация SaveImplementedViewModel
        /// </summary>
        public SaveImplementedViewModel()
        {
            SaveAndCloseCommand = new DelegateCommand(SaveToModelAndClose,CanSaveExecute);
        }

        public DelegateCommand SaveAndCloseCommand { get; private set; }

        public virtual void SaveToModel()
        {

        }
        /// <summary>
        /// Сохранение в БД
        /// </summary>
        /// <param name="itemID"></param>
        public virtual void SaveToModel(Guid itemID)
        {
            UiServices.SetBusyState();
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