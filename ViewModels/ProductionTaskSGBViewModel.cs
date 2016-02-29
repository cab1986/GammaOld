using GalaSoft.MvvmLight;
using Gamma.Interfaces;
using System.Linq;
using Gamma.Models;
using System;
using Gamma.Attributes;
using System.ComponentModel.DataAnnotations;
using System.Windows;

namespace Gamma.ViewModels
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class ProductionTaskSGBViewModel : SaveImplementedViewModel, ICheckedAccess
    {
        /// <summary>
        /// Initializes a new instance of the ProductionTaskSGBViewModel class.
        /// </summary>
        public ProductionTaskSGBViewModel()
        {
        }
        public ProductionTaskSGBViewModel(Guid productionTaskBatchID)
        {
            var productionTaskSGB = DB.GammaBase.GetProductionTaskBatchSGBProperties(productionTaskBatchID).FirstOrDefault();
            if (productionTaskSGB != null)
            {
                IsConfirmed = productionTaskSGB.IsActual;
                Crepe = productionTaskSGB.Crepe ?? 0;
                Diameter = productionTaskSGB.Diameter ?? 0;
                DiameterMinus = productionTaskSGB.DiameterMinus ?? 0;
                DiameterPlus = productionTaskSGB.DiameterPlus ?? 0;
            }
        }
        private int _crepe;
        [UIAuth(UIAuthLevel.ReadOnly)]
        [Range(0,60,ErrorMessage="Креп за пределами допустимого диапозона")]
        public int Crepe
        {
            get
            {
                return _crepe;
            }
            set
            {
                _crepe = value;
                RaisePropertyChanged("Crepe");
            }
        }
        private int _diameter;
        [UIAuth(UIAuthLevel.ReadOnly)]
        [Range(0, 4000, ErrorMessage = "Диаметр за пределами допустимого диапозона")]
        public int Diameter
        {
            get
            {
                return _diameter;
            }
            set
            {
                _diameter = value;
                RaisePropertyChanged("Diameter");
            }
        }
        private int _diameterPlus;
        [UIAuth(UIAuthLevel.ReadOnly)]
        public int DiameterPlus
        {
            get
            {
                return _diameterPlus;
            }
            set
            {
                _diameterPlus = value;
                RaisePropertyChanged("DiameterPlus");
            }
        }
        private int _diameterMinus;
        [UIAuth(UIAuthLevel.ReadOnly)]
        public int DiameterMinus
        {
            get
            {
                return _diameterMinus;
            }
            set
            {
                _diameterMinus = value;
                RaisePropertyChanged("DiameterMinus");
            }
        }
        private bool IsConfirmed { get; set; }
        public override void SaveToModel(Guid itemID) // Сохранение по ProductionTaskID
        {
            base.SaveToModel(itemID);
            var productionTask = DB.GammaBase.ProductionTasks.Include("ProductionTaskSGB").Where(p => p.ProductionTaskID == itemID).FirstOrDefault();
            if (productionTask == null)
            {
                MessageBox.Show("Что-то пошло не так при сохранении.", "Ошибка сохранения", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (productionTask.ProductionTaskSGB == null)
            {
                productionTask.ProductionTaskSGB = new ProductionTaskSGB();
            }
            productionTask.ProductionTaskSGB.Diameter = Diameter;
            productionTask.ProductionTaskSGB.DiameterMinus = DiameterMinus;
            productionTask.ProductionTaskSGB.DiameterPlus = DiameterPlus;
            productionTask.ProductionTaskSGB.Crepe = Crepe;
            DB.GammaBase.SaveChanges();
        }

        public bool IsReadOnly
        {
            get 
            {
                return IsConfirmed || !DB.HaveWriteAccess("ProductionTaskSGB");
            }
        }
    }
}