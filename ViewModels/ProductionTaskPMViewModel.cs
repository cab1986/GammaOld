using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using Gamma.Models;
using Gamma.Common;
using Gamma.Interfaces;
using System.Windows;
using DevExpress.Mvvm;
using Gamma.Attributes;

namespace Gamma.ViewModels
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class ProductionTaskPMViewModel : DBEditItemWithNomenclatureViewModel, IProductionTask, ICheckedAccess
    {
        /// <summary>
        /// Initializes a new instance of the ProductionTaskPMViewModel class.
        /// </summary>
        /// 
        public ProductionTaskPMViewModel()
        {
            Places = new ObservableCollection<Place>(DB.GammaBase.Places.Where(p => p.PlaceGroupID == (short)PlaceGroups.PM).
                Select(p => new Place()
                {
                    PlaceID = p.PlaceID,
                    PlaceName = p.Name
                }));
            if (Places.Count > 0)
                PlaceID = Places[0].PlaceID;
        }

        public ProductionTaskPMViewModel(Guid productionTaskBatchID, bool isForRW) : this()
        {
            ProductionTaskBatchID = productionTaskBatchID;
            IsForRW = isForRW;
            var productionTask = DB.GammaBase.GetProductionTaskByBatchID(productionTaskBatchID, (short)PlaceGroups.PM).FirstOrDefault();
            if (productionTask != null)
            {
                DateBegin = productionTask.DateBegin;
                DateEnd = productionTask.DateEnd;
                TaskQuantity = productionTask.Quantity;
                NomenclatureID = productionTask.C1CNomenclatureID;
                CharacteristicID = (Guid)productionTask.C1CCharacteristicID;
                IsConfirmed = productionTask.IsActual;
                ProductionTaskSGBViewModel = new ProductionTaskSGBViewModel(productionTask.ProductionTaskID);
                PlaceID = productionTask.PlaceID ?? PlaceID;
            }
            else
            {
                ProductionTaskSGBViewModel = new ProductionTaskSGBViewModel();
            }
            if (isForRW)
            {
                var productionTaskRW = DB.GammaBase.GetProductionTaskByBatchID(productionTaskBatchID, (short)PlaceGroups.RW).FirstOrDefault();
                if (productionTaskRW != null)
                {
                    SpecificationNomenclature = new ObservableCollection<Nomenclature1C>(
                        DB.GammaBase.GetInputNomenclature(productionTaskRW.C1CNomenclatureID, (byte)PlaceGroups.PM).Select(n => new Nomenclature1C()
                        {
                            Name = n.Name,
                            NomenclatureID = (Guid)n.C1CNomenclatureID
                        }));
                }
            }
        }
        protected override bool CanChooseNomenclature()
        {
            return base.CanChooseNomenclature() && !IsReadOnly;
        }
        private Guid? _characteristicID;
        
        [Required(ErrorMessage="Необходимо выбрать характеристику")]
        [UIAuth(UIAuthLevel.ReadOnly)]
        public Guid? CharacteristicID
        {
            get
            {
                return _characteristicID;
            }
            set
            {
            	_characteristicID = value;
                RaisePropertyChanged("CharacteristicID");
            }
        }
        private DateTime? _dateBegin;
        [Required(ErrorMessage="Необходимо указать дату начала")]
        [UIAuth(UIAuthLevel.ReadOnly)]
        public DateTime? DateBegin
        {
            get
            {
                return _dateBegin;
            }
            set
            {
            	_dateBegin = value;
                RaisePropertyChanged("DateBegin");
            }
        }
        private DateTime? _dateEnd;
        [Required(ErrorMessage = "Необходимо указать дату окончания")]
        [UIAuth(UIAuthLevel.ReadOnly)]
        public DateTime? DateEnd
        {
            get
            {
                return _dateEnd;
            }
            set
            {
            	_dateEnd = value;
                RaisePropertyChanged("DateEnd");
            }
        }
        private bool IsConfirmed { get; set; }
        private void NomenclatureRWChanged(ProductionTaskRWMessage msg)
        {
            if (msg.ProductionTaskBatchID == ProductionTaskBatchID)
            {
                SpecificationNomenclature = new ObservableCollection<Nomenclature1C>(
                    DB.GammaBase.GetInputNomenclature(msg.NomenclatureID, (byte)PlaceGroups.PM).Select(n => new Nomenclature1C()
                    {
                        Name = n.Name,
                        NomenclatureID = (Guid)n.C1CNomenclatureID
                    }));
                if (SpecificationNomenclature.Count == 1)
                {
                    NomenclatureID = SpecificationNomenclature[0].NomenclatureID;
                }
                else
                    NomenclatureID = null;
            }
        }

        public override ObservableCollection<Characteristic> Characteristics
        {
            get { return base.Characteristics; }
            set
            {
                var tempChars = new ObservableCollection<Characteristic>();
                foreach (var characteristic in value)
                {
                    if (DB.GammaBase.GetCharSpoolLayerNumber(characteristic.CharacteristicID).FirstOrDefault() > 1) continue;
                    tempChars.Add(characteristic);
                }
                base.Characteristics = tempChars;
                RaisePropertiesChanged("Characteristics");
            }
        }

        public Guid ProductionTaskID { get; set; }
        public ProductionTaskSGBViewModel ProductionTaskSGBViewModel { get; private set; }

        private ObservableCollection<Nomenclature1C> _specificationNomenclature;
        public ObservableCollection<Nomenclature1C> SpecificationNomenclature
        {
            get
            {
                return _specificationNomenclature;
            }
            set
            {
            	_specificationNomenclature = value;
                RaisePropertyChanged("SpecificationNomenclature");
            }
        }
        
        private bool _isForRW;
        public bool IsForRW
        {
            get
            {
                return _isForRW;
            }
            set
            {
                if (_isForRW == value) return;
            	_isForRW = value;
                if (IsForRW)
                {
                    Messenger.Default.Register<ProductionTaskRWMessage>(this, NomenclatureRWChanged);
                }
                else
                {
                    Messenger.Default.Unregister<ProductionTaskRWMessage>(this);
                }
                RaisePropertyChanged("IsForRW");
            }
        }
        private int _placeID;
        [UIAuth(UIAuthLevel.ReadOnly)]
        public int PlaceID
        {
            get
            {
                return _placeID;
            }
            set
            {
            	_placeID = value;
                RaisePropertyChanged("Places");
            }
        }
        private ObservableCollection<Place> _places;
        public ObservableCollection<Place> Places
        {
            get
            {
                return _places;
            }
            set
            {
            	_places = value;
                RaisePropertyChanged("Places");
            }
        }
        private Guid ProductionTaskBatchID { get; set; }
/*        private Characteristic _selectedCharacteristic;
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
 * */
        public override void SaveToModel(Guid itemId) // itemID = ProductionTaskBatchID
        {
            base.SaveToModel(itemId);
            var productionTaskBatch = DB.GammaBase.ProductionTaskBatches.Where(p => p.ProductionTaskBatchID == itemId).FirstOrDefault();
            ProductionTasks productionTask;
            if (productionTaskBatch == null)
            {
                MessageBox.Show("Что-то пошло не так при сохранении.", "Ошибка сохранения", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            var productionTaskTemp = DB.GammaBase.GetProductionTaskByBatchID(itemId, (short)PlaceGroups.PM).FirstOrDefault();
            if (productionTaskTemp == null)
            {
                ProductionTaskID = SQLGuidUtil.NewSequentialId();
                productionTask = new ProductionTasks()
                {
                    ProductionTaskID = ProductionTaskID,
                    PlaceGroupID = (short)PlaceGroups.PM
                };
                productionTaskBatch.ProductionTasks.Add(productionTask);
            }
            else
            {
                ProductionTaskID = productionTaskTemp.ProductionTaskID;
                productionTask = DB.GammaBase.ProductionTasks.Find(ProductionTaskID);
            }
            productionTask.PlaceID = PlaceID;
            productionTask.C1CNomenclatureID = (Guid)NomenclatureID;
            productionTask.C1CCharacteristicID = CharacteristicID;
            productionTask.Quantity = TaskQuantity;
            productionTask.DateBegin = DateBegin;
            productionTask.DateEnd = DateEnd;
            DB.GammaBase.SaveChanges();
            ProductionTaskSGBViewModel.SaveToModel(productionTask.ProductionTaskID);
        }
        [Range(1, 1000000, ErrorMessage = "Значение должно быть больше 0")]
        [UIAuth(UIAuthLevel.ReadOnly)]
        public decimal TaskQuantity { get; set; }
        public bool IsReadOnly
        {
            get 
            {
                return IsConfirmed || !DB.HaveWriteAccess("ProductionTasks");
            }
        }
    }
}