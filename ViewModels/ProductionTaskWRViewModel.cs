using Gamma.Interfaces;
using System;
using Gamma.Models;
using System.Linq;
using Gamma.Attributes;
using System.ComponentModel.DataAnnotations;
using System.Windows;

namespace Gamma.ViewModels
{
    /// <summary>
    /// Свойства задания для упаковщика
    /// </summary>
    public class ProductionTaskWrViewModel : SaveImplementedViewModel, ICheckedAccess
    {
        public ProductionTaskWrViewModel()
        {
        }
        public ProductionTaskWrViewModel(Guid productionTaskBatchID, GammaEntities gammaBase = null) : base(gammaBase)
        {
            var productionTaskWr = GammaBase.GetProductionTaskBatchWRProperties(productionTaskBatchID).FirstOrDefault();
            if (productionTaskWr != null)
            {
                NumFilmLayers = productionTaskWr.NumFilmLayers ?? 0;
                IsEndProtected = productionTaskWr.IsEndProtected ?? false;
                IsWithCarton = productionTaskWr.IsWithCarton ?? false;
                GroupPackConfig = productionTaskWr.GroupPackConfig;
                IsConfirmed = productionTaskWr.IsActual;
            }
        }
        private bool IsConfirmed { get; set; }
        private bool _isEndProtected;
        [UIAuth(UIAuthLevel.ReadOnly)]
        public bool IsEndProtected
        {
            get
            {
                return _isEndProtected;
            }
            set
            {
                _isEndProtected = value;
                RaisePropertyChanged("IsEndProtected");
            }
        }
        private bool _isWithCarton;
        [UIAuth(UIAuthLevel.ReadOnly)]
        public bool IsWithCarton
        {
            get
            {
                return _isWithCarton;
            }
            set
            {
                _isWithCarton = value;
                RaisePropertyChanged("IsWithCarton");
            }
        }
        private byte _numFilmLayers;
        [UIAuth(UIAuthLevel.ReadOnly)]
        [Range(0,300,ErrorMessage="Количество слоев за пределами диапозона")]
        public byte NumFilmLayers
        {
            get
            {
                return _numFilmLayers;
            }
            set
            {
                _numFilmLayers = value;
                RaisePropertyChanged("NumFilmLayers");
            }
        }
        private string _groupPackConfig;
        [UIAuth(UIAuthLevel.ReadOnly)]
        public string GroupPackConfig
        {
            get
            {
                return _groupPackConfig;
            }
            set
            {
                _groupPackConfig = value;
                RaisePropertyChanged("GroupPackConfig");
            }
        }
        public override void SaveToModel(Guid itemID, GammaEntities gammaBase = null) // Сохранение по ProductionTaskID
        {
            gammaBase = gammaBase ?? DB.GammaDb;
            base.SaveToModel(itemID, gammaBase);
            var productionTask = gammaBase.ProductionTasks.Include("ProductionTaskSGB").FirstOrDefault(p => p.ProductionTaskID == itemID);
            if (productionTask == null)
            {
                MessageBox.Show("Что-то пошло не так при сохранении.", "Ошибка сохранения", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (productionTask.ProductionTaskWR == null)
            {
                productionTask.ProductionTaskWR = new ProductionTaskWR();
            }
            productionTask.ProductionTaskWR.IsEndProtected = IsEndProtected;
            productionTask.ProductionTaskWR.IsWithCarton = IsWithCarton;
            productionTask.ProductionTaskWR.NumFilmLayers = NumFilmLayers;
            productionTask.ProductionTaskWR.GroupPackConfig = GroupPackConfig;
            gammaBase.SaveChanges();
        }

        public bool IsReadOnly => IsConfirmed || !DB.HaveWriteAccess("ProductionTaskWR");
    }
}