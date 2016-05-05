using DevExpress.Mvvm;
using Gamma.Common;
using System;
using Gamma.Models;

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

        protected SaveImplementedViewModel(GammaEntities gammaBase = null) : this()
        {
            GammaBase = gammaBase ?? DB.GammaDb;
        }

        public DelegateCommand SaveAndCloseCommand { get; private set; }

        protected virtual void SaveToModel(GammaEntities gammaBase = null)
        {
        }
        /// <summary>
        /// Сохранение в БД
        /// </summary>
        /// <param name="itemID"></param>
        /// <param name="gammaBase">Контекст БД</param>
        public virtual void SaveToModel(Guid itemID, GammaEntities gammaBase = null)
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