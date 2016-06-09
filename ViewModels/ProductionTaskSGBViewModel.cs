using Gamma.Interfaces;
using System.Linq;
using Gamma.Models;
using System;
using Gamma.Attributes;
using System.ComponentModel.DataAnnotations;
using System.Windows;
using System.Data.Entity;

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
        public ProductionTaskSGBViewModel(Guid productionTaskID)
        {
            var productionTaskSGB = DB.GammaDb.ProductionTaskSGB.
                Include(pt => pt.ProductionTasks.ProductionTaskBatches).FirstOrDefault(pt => pt.ProductionTaskID == productionTaskID);
            if (productionTaskSGB != null)
            {
                IsConfirmed = productionTaskSGB.ProductionTasks.ProductionTaskBatches.FirstOrDefault() != null 
                    && productionTaskSGB.ProductionTasks.ProductionTaskBatches.First().ProductionTaskStateID != (byte)ProductionTaskStates.NeedsDecision;
                Crepe = productionTaskSGB.Crepe ?? 0;
                Diameter = productionTaskSGB.Diameter ?? 0;
                DiameterMinus = productionTaskSGB.DiameterMinus ?? 0;
                DiameterPlus = productionTaskSGB.DiameterPlus ?? 0;
                QualitySpecification = productionTaskSGB.QualitySpecification;
                TechSpecification = productionTaskSGB.TechSpecification;
            }
        }
        private int _crepe;
        [UIAuth(UIAuthLevel.ReadOnly)]
        [Range(0,60,ErrorMessage=@"Креп за пределами допустимого диапозона")]
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
        [Range(0, 4000, ErrorMessage = @"Диаметр за пределами допустимого диапозона")]
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

        [UIAuth(UIAuthLevel.ReadOnly)]
        public string TechSpecification { get; set; }
        [UIAuth(UIAuthLevel.ReadOnly)]
        public string QualitySpecification { get; set; }

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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="itemID">ProductionTaskID</param>
        /// <param name="gammaBase">Контекст БД</param>
        public override void SaveToModel(Guid itemID, GammaEntities gammaBase = null)
        {
            gammaBase = gammaBase ?? DB.GammaDb;
            base.SaveToModel(itemID, gammaBase);
            var productionTask = gammaBase.ProductionTasks.Include("ProductionTaskSGB").FirstOrDefault(p => p.ProductionTaskID == itemID);
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
            productionTask.ProductionTaskSGB.TechSpecification = TechSpecification;
            productionTask.ProductionTaskSGB.QualitySpecification = QualitySpecification;
            gammaBase.SaveChanges();
        }

        public bool IsReadOnly => IsConfirmed || !DB.HaveWriteAccess("ProductionTaskSGB");
    }
}