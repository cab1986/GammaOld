// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using DevExpress.Mvvm;
using Gamma.Common;
using System;
using Gamma.Entities;

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

        public virtual bool SaveToModel()
        {
            UIServices.SetBusyState();
            return true;
        }
        /// <summary>
        /// Сохранение в БД
        /// </summary>
        /// <param name="itemID"></param>
        public virtual bool SaveToModel(Guid itemID)
        {
            UIServices.SetBusyState();
            return true;
        }

        protected virtual void SaveToModelAndClose()
        {
            UIServices.SetBusyState();
            if (SaveToModel())
                CloseWindow();
        }

        public override void Dispose()
        {
            base.Dispose();
            SaveAndCloseCommand = null;
        }

        public virtual bool CanSaveExecute()
        {
            return IsValid;
        }
    }
}