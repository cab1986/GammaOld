// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using Gamma.Models;
using Gamma.Interfaces;
using System.Windows;
using DevExpress.Mvvm;
using Gamma.Attributes;
using Gamma.Entities;
using System.Collections.Generic;

namespace Gamma.ViewModels
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class ProductionTaskPMViewModel : DbEditItemWithNomenclatureViewModel, IProductionTask, ICheckedAccess
    {
        /// <summary>
        /// Initializes a new instance of the ProductionTaskPMViewModel class.
        /// </summary>        
        public ProductionTaskPMViewModel()
        {
            Places = new ObservableCollection<Place>(WorkSession.Places.Where(p => p.PlaceGroupID == (short)PlaceGroup.PM).
                Select(p => new Place()
                {
                    PlaceID = p.PlaceID,
                    PlaceName = p.Name
                }));
            if (Places.Count > 0)
                PlaceID = Places[0].PlaceID;
            PlaceGroupID = (int) PlaceGroup.PM;
        }

        public ProductionTaskPMViewModel(Guid productionTaskBatchID, bool isForRw) : this()
        {
            ProductionTaskBatchID = productionTaskBatchID;
            IsForRw = isForRw;
            if (isForRw)
            {
                var productionTaskRw = DB.GammaDbWithNoCheckConnection.GetProductionTaskByBatchID(productionTaskBatchID, (short)PlaceGroup.Rw).FirstOrDefault();
                if (productionTaskRw != null)
                {
                    var nomenclatures =
                        GammaBase.ProductionTaskRWCutting.Where(
                            pt => pt.ProductionTaskID == productionTaskRw.ProductionTaskID).Select(pt => 
                            new {
                                pt.C1CNomenclatureID,
                                pt.C1CCharacteristicID
                            }).Distinct().ToList();
/*
                    var characteristicID = nomenclatures.FirstOrDefault()?.C1CCharacteristicID;
                    if (characteristicID != null)
                    {
                        var rwCharacteristic = GammaBase.vCharacteristicSGBProperties.FirstOrDefault(p => p.C1CCharacteristicID == characteristicID);                       
                            Color = rwCharacteristic?.Color;
                            Buyer = rwCharacteristic?.Buyer;
                    }
 */                   
                    SpecificationNomenclature = new ObservableCollection<Nomenclature>();
                    var inputNomenclatures = nomenclatures.Select(nomenclature => new ObservableCollection<Nomenclature>(
                        GammaBase.GetSpecificationInputNomenclature(nomenclature.C1CNomenclatureID, nomenclature.C1CCharacteristicID, (byte)PlaceGroup.PM)
                        .Where(n => n.C1CNomenclatureID != null && n.C1CCharacteristicID != null)
                        .Select(n => new Nomenclature()
                        {
                            NomenclatureID = (Guid)n.C1CNomenclatureID,
                            CharacteristicID = (Guid)n.C1CCharacteristicID,
                            NomenclatureName = n.NomenclatureName,
                            CharacteristicName = n.CharacteristicName
                        }))).ToList();
                    foreach (var inputNomenclature in inputNomenclatures)
                        {
                            SpecificationNomenclature = SpecificationNomenclature.Count == 0 ?
                                inputNomenclature : new ObservableCollection<Nomenclature>(SpecificationNomenclature.Intersect(inputNomenclature));
                        }
                    
                    
                }
            }
            var productionTask = DB.GammaDbWithNoCheckConnection.GetProductionTaskByBatchID(productionTaskBatchID, (short)PlaceGroup.PM).FirstOrDefault();
            if (productionTask != null)
            {
                PlaceID = productionTask.PlaceID ?? PlaceID;
                Number = productionTask.Number;
                DateBegin = productionTask.DateBegin;
                DateEnd = productionTask.DateEnd;
                TaskQuantity = productionTask.Quantity;
                NomenclatureID = productionTask.C1CNomenclatureID;
                CharacteristicID = (Guid)productionTask.C1CCharacteristicID;
                IsConfirmed = productionTask.IsActual;
                //SpecificationID = productionTask.C1CSpecificationID;
                ProductionTaskSGBViewModel = new ProductionTaskSGBViewModel(productionTask.ProductionTaskID);
                ProductionTaskSpecificationViewModel = new ProductionTaskSpecificationViewModel(productionTask.C1CSpecificationID, NomenclatureID, CharacteristicID, PlaceID, IsReadOnly);
            }
            else
            {
                ProductionTaskSGBViewModel = new ProductionTaskSGBViewModel();
                ProductionTaskSpecificationViewModel = new ProductionTaskSpecificationViewModel();
            }
        }

        public override bool IsValid => base.IsValid && DateEnd != null && DateBegin != null && DateEnd >= DateBegin ;

        private bool _isEditingQuantity { get; set; }
        public bool IsEditingQuantity
        {
            get
            {
                return _isEditingQuantity;
            }
            set
            {
                _isEditingQuantity = value;
                RaisePropertyChanged("IsEditingQuantity");
            }
        }

        protected override bool CanChooseNomenclature()
        {
            return base.CanChooseNomenclature() && !IsReadOnly;
        }

        public string Number { get; set; }

        private DateTime? _dateBegin;
        [Required(ErrorMessage=@"Необходимо указать дату начала")]
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
        [Required(ErrorMessage = @"Необходимо указать дату окончания")]
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
        [UIAuth(UIAuthLevel.ReadOnly)]
        [Required(ErrorMessage = @"Необходимо выбрать характеристику")]
        public override Guid? CharacteristicID
        {
            get { return base.CharacteristicID; }
            set
            {
                base.CharacteristicID = value;
                ProductionTaskSpecificationViewModel?.SetSpecifications(NomenclatureID, value, PlaceID);
            }
        }
        private bool _isConfirmed { get; set; }
        private bool IsConfirmed
        {
            get { return _isConfirmed; }
            set
            {
                _isConfirmed = value;
                IsReadOnly = value || !DB.HaveWriteAccess("ProductionTasks");
            }
        }

        

        private void ProductionTaskRwChanged(ProductionTaskRwMessage msg)
        {
            if (msg.ProductionTaskBatchID != ProductionTaskBatchID) return;
            if (msg.Nomenclatures != null && msg.Nomenclatures.Count == 0)
            {
                SpecificationNomenclature = null;
            }
            if (msg.Nomenclatures?.Count > 0 && msg.Nomenclatures.All(n => n.CharacteristicID != null))
            {
 //               NomenclatureList = new ObservableCollection<Nomenclature1C>();
                foreach (var nomenclature in msg.Nomenclatures)
                {
//                    Color = "";
                    if (nomenclature != null)
                    {
                        var specificationNomenclature = new ObservableCollection<Nomenclature>(
                            DB.GammaDbWithNoCheckConnection.GetSpecificationInputNomenclature(nomenclature.NomenclatureID, nomenclature.CharacteristicID, (byte)PlaceGroup.PM)
                                .Select(n => new Nomenclature()
                                {
                                    NomenclatureID = (Guid)n.C1CNomenclatureID,
                                    CharacteristicID = (Guid)n.C1CCharacteristicID,
                                    NomenclatureName = n.NomenclatureName,
                                    CharacteristicName = n.CharacteristicName
                                }));
                        SpecificationNomenclature = SpecificationNomenclature == null || SpecificationNomenclature.Count == 0 ? specificationNomenclature 
                            : new ObservableCollection<Nomenclature>(SpecificationNomenclature.Intersect(specificationNomenclature));
                    }
//                    var charProperties = GammaBase.vCharacteristicSGBProperties.FirstOrDefault(
//                        p => p.C1CCharacteristicID == nomenclature.CharacteristicID);
//                    if (charProperties == null) continue;
//                    Color = charProperties.Color;
//                    Buyer = charProperties.Buyer;
                }
                if (SpecificationNomenclature?.Count == 1)
                {
                    NomenclatureID = SpecificationNomenclature[0].NomenclatureID;
                }
                else if ((NomenclatureID != null && (!SpecificationNomenclature?.Select(s => s.NomenclatureID).Contains((Guid)NomenclatureID) ?? false)) 
                    || SpecificationNomenclature?.Count == 0)
                {
                    NomenclatureID = null;
                }                   
            }
            if (msg.DateBegin != null)
            {
                DateBegin = msg.DateBegin;
                DateEnd = msg.DateBegin;
            }
        }

        [UIAuth(UIAuthLevel.ReadOnly)]
        [Required(ErrorMessage = @"Необходимо выбрать номенклатуру")]
        public override Guid? NomenclatureID
        {
            get { return base.NomenclatureID; }
            set
            {
                base.NomenclatureID = value;
                SetCharacteristics();
            }
        }

        private ObservableCollection<Nomenclature1C> _nomenclatureList;

        public ObservableCollection<Nomenclature1C> NomenclatureList
        {
            get
            {
                return _nomenclatureList;
            }
            set
            {
                _nomenclatureList = value;
                if (NomenclatureList == null || NomenclatureList?.Count == 0)
                {
                    NomenclatureID = null;
                }
                else if (NomenclatureID != null &&
                    !NomenclatureList.Select(nl => nl.NomenclatureID).Contains((Guid) NomenclatureID))
                {
                    NomenclatureID = NomenclatureList.Select(nl => nl.NomenclatureID).FirstOrDefault();
                }
                else if (NomenclatureList?.Count == 1)
                {
                    NomenclatureID = NomenclatureList.First().NomenclatureID;
                }
                RaisePropertyChanged("NomenclatureList");
            }
        }

        private void SetCharacteristics()
        {
            if (NomenclatureID == null)
            {
                CharacteristicID = null;
                Characteristics = null;
                return;
            }
            ObservableCollection<Characteristic> characteristics = null;
            if (IsForRw)
            {
                if (SpecificationNomenclature == null || !SpecificationNomenclature.Any()) return;
                characteristics =
                    new ObservableCollection<Characteristic>(
                        SpecificationNomenclature.Where(
                            sn => sn.NomenclatureID == NomenclatureID && sn.CharacteristicID != null)
                            .Select(sn => new Characteristic
                            {
                                CharacteristicID = (Guid)sn.CharacteristicID,
                                CharacteristicName = sn.CharacteristicName
                            }));

            }
            else characteristics = DB.GetCharacteristics(NomenclatureID);
            using (var gammaBase = DB.GammaDbWithNoCheckConnection)
            {
                var charIds = characteristics.Select(c => c.CharacteristicID).ToList();
                Characteristics = new ObservableCollection<Characteristic>(
                    characteristics.Intersect(
                        gammaBase.vCharacteristicSGBProperties.Where(
                            cp => charIds.Contains(cp.C1CCharacteristicID)
                                  && cp.FormatNumeric >= Format && cp.CoreDiameterNumeric >= CoreDiameter && cp.LayerNumberNumeric == 1)
                            .Select(cp => new Characteristic
                            {
                                CharacteristicID = cp.C1CCharacteristicID
                            })));
            }
        }

        

        /// <summary>
        /// Цвет для фильтрации характеристик
        /// </summary>
//        private string Color { get; set; }

        /// <summary>
        /// Контрагент для фильтрации характеристик
        /// </summary>
//        private string Buyer { get; set; }

        /*
        public override ObservableCollection<Characteristic> Characteristics
        {
            get { return base.Characteristics; }
            set
            {
                var tempChars = new ObservableCollection<Characteristic>();
                foreach (var characteristic in from characteristic in value let charProperties = GammaBase.vCharacteristicSGBProperties.Where(
                    p => p.C1CCharacteristicID == characteristic.CharacteristicID)
                    .Select(p => new
                    {
                        p.CoreDiameterNumeric,
                        p.LayerNumberNumeric,
                        p.FormatNumeric,
                        p.Color
                    }).FirstOrDefault() where charProperties != null where !(charProperties.LayerNumberNumeric > 1) && !(charProperties.CoreDiameterNumeric < 280) && !(charProperties.FormatNumeric < 2500) where AvailableColors.Count <= 0 || AvailableColors.Contains(charProperties.Color) select characteristic)
                {
                    tempChars.Add(characteristic);
                }
                base.Characteristics = tempChars;
                
                RaisePropertiesChanged("Characteristics");
            }
        }
        */


        public Guid ProductionTaskID { get; set; }
        public ProductionTaskSGBViewModel ProductionTaskSGBViewModel { get; private set; }
        public ProductionTaskSpecificationViewModel ProductionTaskSpecificationViewModel { get; private set; }

        private ObservableCollection<Nomenclature> _specificationNomenclature;
        public ObservableCollection<Nomenclature> SpecificationNomenclature
        {
            get
            {
                return _specificationNomenclature;
            }
            set
            {
            	_specificationNomenclature = value;
                if (SpecificationNomenclature?.Count > 0)
                {
                    NomenclatureList =
                        new ObservableCollection<Nomenclature1C>(
                            SpecificationNomenclature.Select(sn => new Nomenclature1C
                            {
                                NomenclatureID = (Guid) sn.NomenclatureID,
                                Name = sn.NomenclatureName
                            }).Distinct());
                }
                else NomenclatureList = null;
            }
        }
        
        private bool _isForRw;
        public bool IsForRw
        {
            get
            {
                return _isForRw;
            }
            set
            {
                if (_isForRw == value) return;
            	_isForRw = value;
                if (IsForRw)
                {
                    Messenger.Default.Register<ProductionTaskRwMessage>(this, ProductionTaskRwChanged);
                }
                else
                {
                    Messenger.Default.Unregister<ProductionTaskRwMessage>(this);
                }
                RaisePropertyChanged("IsForRw");
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
                GetLimitParamters(PlaceID);
                SetCharacteristics();
                ProductionTaskSpecificationViewModel?.SetSpecifications(NomenclatureID, CharacteristicID, value);
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

        private void GetLimitParamters(int placeId)
        {
            using (var gammaBase = DB.GammaDbWithNoCheckConnection)
            {
                var placeProperties = gammaBase.vPlacePropertiesValues.FirstOrDefault(pv => pv.PlaceID == placeId);
                if (placeProperties != null)
                {
                    Format = placeProperties.Format;
                    CoreDiameter = placeProperties.CoreDiameter;
                }
                else
                {
                    Format = null;
                    CoreDiameter = null;
                }
            }
        }

        private Guid ProductionTaskBatchID { get; set; }

        private decimal? Format { get; set; }
        private decimal? CoreDiameter { get; set; }
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
        public override bool SaveToModel(Guid itemId) // itemID = ProductionTaskBatchID
        {
            if (!DB.HaveWriteAccess("ProductionTaskSGB")) return true;
            using (var gammaBase = DB.GammaDb)
            {
                var productionTaskBatch =
                    gammaBase.ProductionTaskBatches.FirstOrDefault(p => p.ProductionTaskBatchID == itemId);
                ProductionTasks productionTask;
                if (productionTaskBatch == null)
                {
                    MessageBox.Show("Что-то пошло не так при сохранении.", "Ошибка сохранения", MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return true;
                }
                var productionTaskTemp =
                    gammaBase.GetProductionTaskByBatchID(itemId, (short)PlaceGroup.PM).FirstOrDefault();
                if (productionTaskTemp == null)
                {
                    ProductionTaskID = SqlGuidUtil.NewSequentialid();
                    productionTask = new ProductionTasks()
                    {
                        ProductionTaskID = ProductionTaskID,
                        PlaceGroupID = (short)PlaceGroup.PM
                    };
                    productionTaskBatch.ProductionTasks.Add(productionTask);
                }
                else
                {
                    ProductionTaskID = productionTaskTemp.ProductionTaskID;
                    productionTask = gammaBase.ProductionTasks.Find(ProductionTaskID);
                }
                productionTask.PlaceID = PlaceID;
                productionTask.C1CNomenclatureID = (Guid)NomenclatureID;
                productionTask.C1CCharacteristicID = CharacteristicID;
                productionTask.Quantity = TaskQuantity;
                productionTask.DateBegin = DateBegin;
                productionTask.DateEnd = DateEnd;
                productionTask.C1CSpecificationID = ProductionTaskSpecificationViewModel.SpecificationID;// SelectedSpecification.Key;
                gammaBase.SaveChanges();
                ProductionTaskSGBViewModel.SaveToModel(productionTask.ProductionTaskID);
            }
            return true;
        }
        [Range(1, 1000000, ErrorMessage = @"Значение должно быть больше 0")]
        //[UIAuth(UIAuthLevel.ReadOnly)]
        public decimal TaskQuantity { get; set; }

        private bool _isReadOnly { get; set; }
        public bool IsReadOnly
        {
            get { return _isReadOnly; }
            set
            {
                _isReadOnly = value;
                ProductionTaskSpecificationViewModel?.SetIsReadOnly(value);
            }
        }
    }
}