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
            OnlySaveCommand = new DelegateCommand(OnlySaveToModel, CanSaveExecute);
        }

        public DelegateCommand SaveAndCloseCommand { get; private set; }
        public DelegateCommand OnlySaveCommand { get; private set; }

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
            OnlySaveCommand = null;
        }

        public virtual bool CanSaveExecute()
        {
            return IsValid;
        }

        protected virtual void OnlySaveToModel()
        {
            UIServices.SetBusyState();
            SaveToModel();
        }

        public virtual bool IsChanged { get; private set; }
        public virtual void SetIsChanged(bool value) => IsChanged = value;

        public virtual bool _isReadOnly { get; set; }
        public virtual bool IsReadOnly
        {
            get { return _isReadOnly; }
            set
            {
                _isReadOnly = value;
                //RaisePropertiesChanged("IsReadOnly");
            }
        }
    }
}