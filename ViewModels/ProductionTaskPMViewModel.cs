using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using Gamma.Models;

namespace Gamma.ViewModels
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class ProductionTaskPMViewModel : DBEditItemWithNomenclatureViewModel
    {
        /// <summary>
        /// Initializes a new instance of the ProductionTaskPMViewModel class.
        /// </summary>
        /// 
        public ProductionTaskPMViewModel()
        {
            Messenger.Default.Register<Nomenclature1CMessage>(this, NomenclatureChanged);
        }

        public ProductionTaskPMViewModel(Guid? ProductionTaskID) : this()
        {
            var productionTaskPM = (from ptpm in DB.GammaBase.ProductionTaskPM
                                    where ptpm.ProductionTaskID == ProductionTaskID
                                    select ptpm).FirstOrDefault();
            var productionTaskConfig = (from ptconf in DB.GammaBase.ProductionTaskConfig where ptconf.ProductionTaskID == ProductionTaskID
                                            select ptconf).FirstOrDefault();
            this.ProductionTaskID = ProductionTaskID;
            NomenclatureID = productionTaskConfig.C1CNomenclatureID;
            Characteristics = DB.GetCharacteristics(NomenclatureID);
            SelectedCharacteristic = Characteristics.Where(ch => ch.CharacteristicID == productionTaskConfig.C1CCharacteristicID).FirstOrDefault();
            TaskQuantity = productionTaskConfig.TaskQuantity;
        }
        [Range(1,1000000,ErrorMessage="Значение должно быть больше 0")]
        public int TaskQuantity { get; set; }
        public Guid? ProductionTaskID { get; set; }

        private Characteristic _selectedCharacteristic;
        [Required(ErrorMessage="Необходимо выбрать характеристику")]
        public Characteristic SelectedCharacteristic
        {
            get { return _selectedCharacteristic; }
            set 
            {
                _selectedCharacteristic = value;
                RaisePropertyChanged("SelectedCharacteristic");
            }
        }

        public override void SaveToModel(Guid itemID)
        {
            base.SaveToModel(itemID);
            var ProductionTaskConfig = DB.GammaBase.ProductionTaskConfig.Where(ptconf => ptconf.ProductionTaskID == itemID).FirstOrDefault();
            if (ProductionTaskConfig == null)
            {
                ProductionTaskConfig = new ProductionTaskConfig();
                ProductionTaskConfig.ProductionTaskID = itemID;
                DB.GammaBase.ProductionTaskConfig.Add(ProductionTaskConfig);
            }
            ProductionTaskConfig.C1CNomenclatureID = NomenclatureID;
            ProductionTaskConfig.C1CCharacteristicID = SelectedCharacteristic.CharacteristicID;
            ProductionTaskConfig.TaskQuantity = TaskQuantity;
            ProductionTaskConfig.DocProductsQuantity = 1;
            DB.GammaBase.SaveChanges();
        }

    }
}