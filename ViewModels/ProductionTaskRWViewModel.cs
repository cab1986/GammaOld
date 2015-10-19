using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using System;
using Gamma.Models;
using System.Linq;
using System.Collections.ObjectModel;

namespace Gamma.ViewModels
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class ProductionTaskRWViewModel : DBEditItemWithNomenclatureViewModel
    {
        /// <summary>
        /// Initializes a new instance of the ProductionTaskRWViewModel class.
        /// </summary>
        public ProductionTaskRWViewModel()
        {
            Messenger.Default.Register<Nomenclature1CMessage>(this, NomenclatureChanged);
            ConfigurationElements = new ObservableCollection<ConfigurationElement>();
        }
        public ProductionTaskRWViewModel(Guid? ProductionTaskID) : this()
        {
            ProductionTaskRW = DB.GammaBase.ProductionTaskRW.Where(ptrw => ptrw.ProductionTaskID == ProductionTaskID).FirstOrDefault();
            var ProductionTaskConfig = DB.GammaBase.ProductionTaskConfig.Where(ptc => ptc.ProductionTaskID == ProductionTaskID).Select(ptc => ptc);
            NomenclatureID = ProductionTaskConfig.Select(ptc => ptc.C1CNomenclatureID).First();
            ConfigurationElements = new ObservableCollection<ConfigurationElement>
            (from ptc in ProductionTaskConfig
             select
                 new ConfigurationElement
                 {
                     CharacteristicID = ptc.C1CCharacteristicID,
                     DocProductQuantity = ptc.DocProductsQuantity,
                     ProductionTaskConfigID = ptc.ProductionTaskConfigID
                 });
        }
        private ProductionTaskRW ProductionTaskRW;
        public override void SaveToModel(Guid ItemID)
        {
            base.SaveToModel(ItemID);
            if (ProductionTaskRW == null)
            {
                ProductionTaskRW = new ProductionTaskRW { ProductionTaskID = ItemID };
                DB.GammaBase.ProductionTaskRW.Add(ProductionTaskRW);
                AddToProductionTaskConfig(ItemID);
            }
            else
            {
                DB.GammaBase.ProductionTaskConfig.RemoveRange(DB.GammaBase.ProductionTaskConfig.Where(ptc => ptc.ProductionTaskID == ItemID));
                DB.GammaBase.SaveChanges();
                AddToProductionTaskConfig(ItemID);
            }
            DB.GammaBase.SaveChanges();
        }

        private void AddToProductionTaskConfig(Guid ItemID)
        {
            var ConfigElements = new ObservableCollection<ProductionTaskConfig>();
            foreach (var element in ConfigurationElements)
            {                
                ConfigElements.Add(new ProductionTaskConfig
                {
                    ProductionTaskConfigID = Guid.NewGuid(),
                    C1CCharacteristicID = element.CharacteristicID,
                    C1CNomenclatureID = NomenclatureID,
                    DocProductsQuantity = element.DocProductQuantity,
                    ProductionTaskID = ItemID,
                    TaskQuantity = TaskQuantity
                }
                );
            }
            DB.GammaBase.ProductionTaskConfig.AddRange(ConfigElements);
        }
        private ObservableCollection<ConfigurationElement> _configurationElements;
        public ObservableCollection<ConfigurationElement> ConfigurationElements
        {
            get { return _configurationElements; }
            set
            {
                _configurationElements = value;
                RaisePropertyChanged("ConfigurationElements");
            }
        }
        private int _taskQuantity;
        public int TaskQuantity
        {
            get { return _taskQuantity; }
            set 
            {
                _taskQuantity = value;
                RaisePropertyChanged("TaskQuantity");
            }
        }
    }
    public class ConfigurationElement
    {
        public Guid ProductionTaskConfigID { get; set; }
        public Guid CharacteristicID { get; set; }
        public byte? DocProductQuantity { get; set; }
    }
}