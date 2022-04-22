// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Linq;
using DevExpress.Mvvm;
using Gamma.Common;
using Gamma.Entities;
using Gamma.Interfaces;
using Gamma.Models;
using System.Data.Entity.Validation;
using System.Windows;

namespace Gamma.ViewModels
{
    public class DocCloseShiftConvertingGridViewModel: SaveImplementedViewModel, ICheckedAccess, IBarImplemented, IFillClearGrid
    {
        /// <summary>
        /// Создает новый экземпляр модели для грида закрытия смены конвертингов
        /// </summary>
        public DocCloseShiftConvertingGridViewModel()
        {
            Bars.Add(ReportManager.GetReportBar("DocCloseShiftConverting", VMID));
            AddAuxiliaryMaterialCommand = new DelegateCommand(AddAuxiliaryMaterial, () => !IsReadOnly);
            DeleteAuxiliaryMaterialCommand = new DelegateCommand(DeleteAuxiliaryMaterial, () => !IsReadOnly);
            ShowProductCommand = new DelegateCommand(ShowProduct, SelectedProduct != null);
            MaterialCommand = new DelegateCommand(ShowMaterialTab, SelectedProduct != null);
            WithdrawalMaterialCommand = new DelegateCommand(ShowWithdrawalMaterialTab, SelectedProduct != null);
            
        }

        public DocCloseShiftConvertingGridViewModel(DateTime closeDate, int? placeID = null, int? shiftID = null) : this()
        {
            PlaceID = placeID ?? WorkSession.PlaceID;
            ShiftID = shiftID ?? WorkSession.ShiftID;
            CloseDate = closeDate;

            IsWithdrawalMaterial = WorkSession.Places.Where(x => x.PlaceID == PlaceID).Select(x => x.PlaceWithdrawalMaterialTypeID != 0).First();
            WithdrawalMaterialsGrid = new DocCloseShiftWithdrawalMaterialViewModel(PlaceID, ShiftID, CloseDate);
            DocCloseShiftProductsGrid = new DocCloseShiftProductViewModel(PlaceID, ShiftID, CloseDate);
            DowntimesGrid = new DocCloseShiftDowntimesViewModel(PlaceID, ShiftID, CloseDate);
            var place = WorkSession.Places.Where(x => x.PlaceID == PlaceID).FirstOrDefault();
            IsEnabledSamples = place?.IsEnabledSamplesInDocCloseShift ?? true;
            IsEnabledAuxiliaryMaterials = place?.IsEnabledAuxiliaryMaterialsInDocCloseShift ?? false;
            IsEnabledDowntimes = place?.IsEnabledDowntimes ?? false;
        }
        public DocCloseShiftConvertingGridViewModel(Guid docId, DocCloseShiftUnwinderRemainderViewModel _spoolUnwinderRemainders) : this()
        {
            using (var gammaBase = DB.GammaDb)
            {
                var doc = gammaBase.Docs.Include(d => d.DocCloseShiftDocs).First(d => d.DocID == docId);
                DocCloseDocIds = doc.DocCloseShiftDocs.Select(dc => dc.DocID).ToList();
                PlaceID = (int)doc.PlaceID;
                ShiftID = doc.ShiftID ?? 0;
                CloseDate = doc.Date;
                IsConfirmed = doc.IsConfirmed;
                DocId = docId;
                spoolUnwinderRemainders = _spoolUnwinderRemainders;
                var place = WorkSession.Places.Where(x => x.PlaceID == PlaceID).FirstOrDefault();
                IsWithdrawalMaterial = (place?.PlaceWithdrawalMaterialTypeID != 0) ;
                WithdrawalMaterialsGrid = new DocCloseShiftWithdrawalMaterialViewModel(PlaceID, ShiftID, CloseDate);
                DocCloseShiftProductsGrid = new DocCloseShiftProductViewModel(docId,IsConfirmed);
                DowntimesGrid = new DocCloseShiftDowntimesViewModel(PlaceID, ShiftID, CloseDate, docId, IsConfirmed);
                IsEnabledSamples = place?.IsEnabledSamplesInDocCloseShift ?? true;
                IsEnabledAuxiliaryMaterials = place?.IsEnabledAuxiliaryMaterialsInDocCloseShift ?? false;
                IsEnabledDowntimes = place?.IsEnabledDowntimes ?? false;
                Pallets = new ObservableCollection<Pallet>(gammaBase.GetDocCloseShiftConvertingPallets(docId).Select(d => new Pallet()
                {
                    DocID = d.DocID,
                    ProductID = d.ProductID,
                    NomenclatureName = d.NomenclatureName,
                    Quantity = d.Quantity ?? 0,
                    NomenclatureID = d.NomenclatureID,
                    CharacteristicID = d.CharacteristicID,
                    Number = d.Number
                }));
                IsWithdrawalMaterial = WorkSession.Places.Where(x => x.PlaceID == PlaceID).Select(x => x.PlaceWithdrawalMaterialTypeID != 0).First();
                var pallets = new List<DocCloseShiftWithdrawalMaterial.Product>(Pallets.Select(x => new DocCloseShiftWithdrawalMaterial.Product()
                {
                    ProductID = x.ProductID,
                    CharacteristicID = x.CharacteristicID
                })).ToList();
                WithdrawalMaterialsGrid = new DocCloseShiftWithdrawalMaterialViewModel(PlaceID, ShiftID, CloseDate, DocId, pallets, IsConfirmed, productionProductCharacteristicIDs);

                // Получение количества образцов
                var samples = new ObservableCollection<Sample>(gammaBase.DocCloseShiftSamples.Where(ds => ds.DocID == docId)
                    .Select(ds => new Sample
                    {
                        NomenclatureID = ds.C1CNomenclatureID,
                        CharacteristicID = ds.C1CCharacteristicID,
                        Quantity = ds.Quantity,
                        NomenclatureName = ds.C1CNomenclature.Name + " " + ds.C1CCharacteristics.Name,
                        MeasureUnitId = ds.C1CMeasureUnitID,
                        ProductionTaskID = ds.ProductionTaskID
                    }));
                foreach (var sample in samples)
                {
                    sample.MeasureUnits = GetSampleMeasureUnits(sample.NomenclatureID, sample.CharacteristicID);
                    if (sample.MeasureUnitId == null) sample.MeasureUnitId = sample.MeasureUnits.FirstOrDefault().Key;
                }
                Samples = samples;
                // Получение переходных остатков
                NomenclatureRests = new ObservableCollection<Sample>(gammaBase.DocCloseShiftNomenclatureRests.Where(dr => dr.DocID == docId)
                    .Select(dr => new Sample
                    {
                        NomenclatureID = dr.C1CNomenclatureID,
                        CharacteristicID = dr.C1CCharacteristicID,
                        Quantity = dr.Quantity / dr.C1CCharacteristics.C1CMeasureUnitsPackage.Coefficient ?? 1,
                        NomenclatureName = dr.C1CNomenclature.Name + " " + dr.C1CCharacteristics.Name
                    }));
                // Получение отходов
                Wastes = new ObservableCollection<DocCloseShiftWaste>(gammaBase.DocCloseShiftWastes.Where(dw => dw.DocID == docId)
                    .Select(dw => new DocCloseShiftWaste
                    {
                        NomenclatureID = dw.C1CNomenclatureID,
                        CharacteristicID = dw.C1CCharacteristicID,
                        Quantity = dw.C1CMeasureUnits.Name == "т" ? Math.Round(dw.Quantity * 1000,0) : dw.Quantity,
                        NomenclatureName = dw.C1CNomenclature.Name + " " + dw.C1CCharacteristics.Name ?? "",
                        MeasureUnitId = dw.C1CMeasureUnitID,
                        MeasureUnit = dw.C1CMeasureUnits.Name == "т" ? "кг  " : dw.C1CMeasureUnits.Name,
                        ProductNomenclatureID = dw.ProductNomenclatureID,
                        ProductCharacteristicID = dw.ProductCharacteristicID,
                        ProductName = dw.C1CNomenclature1.Name + " " + dw.C1CCharacteristics1.Name ?? "",
                    }));
                foreach (var waste in Wastes)
                {
                    waste.MeasureUnits = GetWasteMeasureUnits(waste.NomenclatureID, waste.MeasureUnitId);
                }
                // Получение вспомогательных материалов
                var auxiliaryMaterials = new ObservableCollection<Sample>(gammaBase.DocCloseShiftAuxiliaryMaterials.Where(ds => ds.DocID == docId)
                    .Select(ds => new Sample
                    {
                        NomenclatureID = ds.C1CNomenclatureID,
                        CharacteristicID = ds.C1CCharacteristicID,
                        Quantity = ds.Quantity,
                        NomenclatureName = ds.C1CNomenclature.Name + " " + ds.C1CCharacteristics.Name,
                        MeasureUnitId = ds.C1CMeasureUnitID,
                        ProductionTaskID = ds.ProductionTaskID
                    }));
                foreach (var auxiliaryMaterial in auxiliaryMaterials)
                {
                    auxiliaryMaterial.MeasureUnits = GetSampleMeasureUnits(auxiliaryMaterial.NomenclatureID, auxiliaryMaterial.CharacteristicID);
                    if (auxiliaryMaterial.MeasureUnitId == null) auxiliaryMaterial.MeasureUnitId = auxiliaryMaterial.MeasureUnits.FirstOrDefault().Key;
                }
                AuxiliaryMaterials = auxiliaryMaterials;

            }
        }

        public bool IsChanged { get; private set; }

        private bool _isConfirmed { get; set; }
        private bool IsConfirmed
        {
            get { return _isConfirmed; }
            set
            {
                _isConfirmed = value;
                WithdrawalMaterialsGrid?.UpdateIsConfirmed(value);
                DowntimesGrid?.UpdateIsConfirmed(value);
            }
        }

        public void UpdateIsConfirmed(bool isConfirmed)
        {
            IsConfirmed = isConfirmed;
        }

        private List<Guid> DocCloseDocIds { get; set; }
        public bool IsReadOnly => !DB.HaveWriteAccess("DocCloseShiftWithdrawals") || !DB.HaveWriteAccess("DocCloseShiftSamples");// || IsConfirmed;
        public ObservableCollection<BarViewModel> Bars { get; set; } = new ObservableCollection<BarViewModel>();
        public Guid? VMID { get; } = Guid.NewGuid();

        public Pallet SelectedProduct { get; set; }
        public Sample SelectedAuxiliaryMaterial { get; set; }
        public DelegateCommand ShowProductCommand { get; private set; }
        public DelegateCommand MaterialCommand { get; private set; }
        public DelegateCommand WithdrawalMaterialCommand { get; private set; }
        public DelegateCommand AddAuxiliaryMaterialCommand { get; private set; }
        public DelegateCommand DeleteAuxiliaryMaterialCommand { get; private set; }

        private void ShowProduct()
        {
            MessageManager.OpenDocProduct(DocProductKinds.DocProductPallet, SelectedProduct.ProductID);
        }

        private void ShowMaterialTab()
        {
            if (WithdrawalMaterialsGrid != null)
                WithdrawalMaterialsGrid.SelectedMaterialTabIndex = 0;
        }

        private void ShowWithdrawalMaterialTab()
        {
            if (WithdrawalMaterialsGrid != null)
                WithdrawalMaterialsGrid.SelectedMaterialTabIndex = 1;
        }

        private void DeleteAuxiliaryMaterial()
        {
            if (SelectedAuxiliaryMaterial == null) return;
            //AuxiliaryMaterials.RemoveRange(AuxiliaryMaterials.Where(d => d.NomenclatureID == SelectedAuxiliaryMaterial.NomenclatureID && (d.CharacteristicID == SelectedAuxiliaryMaterial.CharacteristicID || (d.CharacteristicID == null && SelectedAuxiliaryMaterial.CharacteristicID == null))));

            //var removeItems = AuxiliaryMaterials.Where(d => d.NomenclatureID == SelectedAuxiliaryMaterial.NomenclatureID && (d.CharacteristicID == SelectedAuxiliaryMaterial.CharacteristicID || (d.CharacteristicID == null && SelectedAuxiliaryMaterial.CharacteristicID == null))).ToArray();
            //foreach (var item in removeItems)
            //    AuxiliaryMaterials.Remove(item);
            AuxiliaryMaterials.Remove(SelectedAuxiliaryMaterial);

        }

        private void AddAuxiliaryMaterial()
        {
            Messenger.Default.Register<Nomenclature1CMessage>(this, AuxiliaryMaterialNomenclature);
            MessageManager.FindNomenclature(MaterialType.MaterialsSGI);
        }

        private void AuxiliaryMaterialNomenclature(Nomenclature1CMessage msg)
        {
            Messenger.Default.Unregister<Nomenclature1CMessage>(this);
            using (var gammaBase = DB.GammaDb)
            {
                var nomenclatureInfo =
                gammaBase.C1CNomenclature.Include(n => n.C1CMeasureUnitStorage)
                    .First(n => n.C1CNomenclatureID == msg.Nomenclature1CID);
                AuxiliaryMaterials.Add(new Sample()
                {
                    NomenclatureID = nomenclatureInfo.C1CNomenclatureID,
                    CharacteristicID = nomenclatureInfo.C1CCharacteristics.FirstOrDefault()?.C1CCharacteristicID,
                    Quantity = 0,
                    NomenclatureName = nomenclatureInfo.Name,
                    ProductionTaskID = null
                });
            }
        }

        private DocCloseShiftWithdrawalMaterialViewModel _withdrawalMaterialsGrid;
        public DocCloseShiftWithdrawalMaterialViewModel WithdrawalMaterialsGrid
        {
            get
            {
                return _withdrawalMaterialsGrid;
            }
            set
            {
                _withdrawalMaterialsGrid = value;
                //Bars = (_currentViewModelGrid as IBarImplemented).Bars;
            }
        }

        private DocCloseShiftProductViewModel _docCloseShiftProductsGrid;
        public DocCloseShiftProductViewModel DocCloseShiftProductsGrid
        {
            get
            {
                return _docCloseShiftProductsGrid;
            }
            set
            {
                _docCloseShiftProductsGrid = value;
                //Bars = (_currentViewModelGrid as IBarImplemented).Bars;
            }
        }

        private DocCloseShiftDowntimesViewModel _downtimesGrid;
        public DocCloseShiftDowntimesViewModel DowntimesGrid
        {
            get
            {
                return _downtimesGrid;
            }
            set
            {
                _downtimesGrid = value;
                //Bars = (_currentViewModelGrid as IBarImplemented).Bars;
            }
        }

        private int _selectedTabIndex;
        public int SelectedTabIndex
        {
            get
            {
                return _selectedTabIndex;
            }
            set
            {
                _selectedTabIndex = value;
                if (DocCloseShiftProductsGrid != null)
                    DocCloseShiftProductsGrid.SelectedTabIndex = 2;//tab = Изготовлено
                RaisePropertiesChanged("SelectedTabIndex");
            }
        }

        public int SelectedMaterialTabIndex
        {
            get
            {
                return WithdrawalMaterialsGrid.SelectedMaterialTabIndex;
            }
            set
            {
                SelectedTabIndex = 0;
                if (WithdrawalMaterialsGrid != null)
                    WithdrawalMaterialsGrid.SelectedMaterialTabIndex = value;
            }
        }

        public void FillGrid()
        {
            FillGridWithParameter(true);
        }

        public void FillGridWithNoFillEnd()
        {
            FillGridWithParameter(false);
        }

        public void FillGridWithParameter(bool IsFillEnd = true)
        {
            UIServices.SetBusyState();
            //при заполнении рапорта очищаем только паллеты и документы
            //ClearGrid();
            Pallets?.Clear();
            DocCloseDocIds?.Clear();

            using (var gammaBase = DB.GammaDb)
            {
                DocCloseDocIds = gammaBase.Docs.
                Where(d => d.PlaceID == PlaceID && d.ShiftID == ShiftID && d.IsConfirmed &&
                    d.Date >= SqlFunctions.DateAdd("hh", -1, DB.GetShiftBeginTime((DateTime)SqlFunctions.DateAdd("hh", -1, CloseDate))) &&
                    d.Date <= SqlFunctions.DateAdd("hh", 1, DB.GetShiftEndTime((DateTime)SqlFunctions.DateAdd("hh", -1, CloseDate))) &&
                    (d.DocTypeID == (int)DocTypes.DocProduction || d.DocTypeID == (int)DocTypes.DocWithdrawal)).Select(d => d.DocID).ToList();
                Pallets = new ObservableCollection<Pallet>(gammaBase.FillDocCloseShiftConvertingPallets(PlaceID, ShiftID, CloseDate).Select(p => new Pallet()
                {
                    NomenclatureID = p.NomenclatureID,
                    CharacteristicID = p.CharacteristicID,
                    Quantity = p.Quantity ?? 0,
                    DocID = p.DocID,
                    ProductID = p.ProductID,
                    NomenclatureName = p.NomenclatureName,
                    Number = p.Number
                }));
                if (IsEnabledSamples)
                {
                    var samples = new ObservableCollection<Sample>(gammaBase.FillDocCloseShiftConvertingSamples(PlaceID, ShiftID, CloseDate)
                        .Select(p => new Sample
                        {
                            NomenclatureID = p.NomenclatureID,
                            CharacteristicID = p.CharacteristicID,
                            Quantity = p.Quantity,
                            NomenclatureName = p.NomenclatureName,
                            ProductionTaskID = p.ProductionTaskID
                        }));
                    //Samples.Clear();
                    foreach (var sample in samples)
                    {
                        sample.MeasureUnits = GetSampleMeasureUnits(sample.NomenclatureID, sample.CharacteristicID);
                        sample.MeasureUnitId = sample.MeasureUnits.FirstOrDefault().Key;
                        if (!Samples.Any(s => s.NomenclatureID == sample.NomenclatureID && (s.CharacteristicID == sample.CharacteristicID || (s.CharacteristicID == null && sample.CharacteristicID == null))))
                        {
                            Samples.Add(sample);
                        }
                    }
                    //Samples = samples;
                }
                var nomenclatureRests = new ObservableCollection<Sample>(Pallets
                    .Select(p => new Sample
                    {
                        NomenclatureID = p.NomenclatureID,
                        CharacteristicID = p.CharacteristicID,
                        Quantity = 0,
                        NomenclatureName = p.NomenclatureName,
                    }).Distinct());
                if (NomenclatureRests == null)
                    NomenclatureRests = new ObservableCollection<Sample>();
                foreach (var rest in nomenclatureRests)
                {
                    if (!NomenclatureRests.Any(s => s.NomenclatureID == rest.NomenclatureID && (s.CharacteristicID == rest.CharacteristicID || (s.CharacteristicID == null && rest.CharacteristicID == null))))
                    {
                        NomenclatureRests.Add(rest);
                    }
                }

                var pallets = new List<DocCloseShiftWithdrawalMaterial.Product>(Pallets.Select(x => new DocCloseShiftWithdrawalMaterial.Product()
                {
                    ProductID = x.ProductID,
                    CharacteristicID = x.CharacteristicID
                })).ToList();
                WithdrawalMaterialsGrid.FillWithdrawalMaterials(productionProductCharacteristicIDs, pallets, spoolUnwinderRemainders, IsFillEnd);
                DocCloseShiftProductsGrid.FillGrid(IsFillEnd);
                var wastes = new ItemsChangeObservableCollection<DocCloseShiftWaste>(
                    gammaBase.FillDocCloseShiftConvertingWastesProduct(PlaceID, ShiftID, CloseDate)
                    .Select(w => new DocCloseShiftWaste
                    {
                        NomenclatureID = (Guid)w.NomenclatureID,
                        CharacteristicID = w.CharacteristicID,
                        Quantity = w.Quantity ?? 0,
                        NomenclatureName = w.NomenclatureName,
                        MeasureUnitId = w.MeasureUnitID,
                        MeasureUnit = w.MeasureUnit,
                        ProductNomenclatureID = w.ProductNomenclatureID,
                        ProductCharacteristicID = w.ProductCharacteristicID,
                        ProductName = w.ProductName
                    }));
                if (Wastes == null)
                    Wastes = new ObservableCollection<DocCloseShiftWaste>();
                foreach (var waste in wastes)
                {
                    waste.MeasureUnits = GetWasteMeasureUnits(waste.NomenclatureID, waste.MeasureUnitId);
                    waste.MeasureUnitId = waste.MeasureUnits.FirstOrDefault().Key;
                    if (!Wastes.Any(s => s.NomenclatureID == waste.NomenclatureID && (s.CharacteristicID == waste.CharacteristicID || (s.CharacteristicID == null && waste.CharacteristicID == null))
                        && ((s.ProductNomenclatureID == waste.ProductNomenclatureID || (s.ProductNomenclatureID == null && waste.ProductNomenclatureID == null)) && (s.ProductCharacteristicID == waste.ProductCharacteristicID || (s.ProductCharacteristicID == null && waste.ProductCharacteristicID == null)))))
                    {
                        Wastes.Add(waste);
                    }
                }
                //Wastes = wastes;

                if (IsEnabledAuxiliaryMaterials)
                {
                    var auxiliaryMaterials = new ObservableCollection<Sample>(gammaBase.FillDocCloseShiftConvertingAuxiliaryMaterials(PlaceID, ShiftID, CloseDate)
                        .Select(p => new Sample
                        {
                            NomenclatureID = p.NomenclatureID,
                            CharacteristicID = p.CharacteristicID,
                            Quantity = p.Quantity ?? 0,
                            NomenclatureName = p.NomenclatureName,
                            ProductionTaskID = p.ProductionTaskID
                        }));
                    //AuxiliaryMaterials.Clear();
                    foreach (var auxiliaryMaterial in auxiliaryMaterials)
                    {
                        auxiliaryMaterial.MeasureUnits = GetSampleMeasureUnits(auxiliaryMaterial.NomenclatureID, auxiliaryMaterial.CharacteristicID);
                        auxiliaryMaterial.MeasureUnitId = auxiliaryMaterial.MeasureUnits.FirstOrDefault().Key;
                        if (!AuxiliaryMaterials.Any(s => s.NomenclatureID == auxiliaryMaterial.NomenclatureID && (s.CharacteristicID == auxiliaryMaterial.CharacteristicID || (s.CharacteristicID == null && auxiliaryMaterial.CharacteristicID == null))))
                        {
                            AuxiliaryMaterials.Add(auxiliaryMaterial);
                        }
                    }
                    //AuxiliaryMaterials = auxiliaryMaterials;
                }
                DowntimesGrid?.FillGrid();
                IsChanged = true;
            }
        }

        private Dictionary<Guid, string> GetWasteMeasureUnits(Guid nomenclatureId, Guid? measureUnitId)
        {
            Dictionary<Guid, string> dict;
            using (var gammaBase = DB.GammaDb)
            {
                //dict = gammaBase.C1CMeasureUnits.Where(mu => mu.C1CNomenclatureID == nomenclatureId).OrderBy(mu => mu.Coefficient).ToDictionary(x => x.C1CMeasureUnitID, v => v.Name);                
                var NomenclatureMeasureUnitID = gammaBase.C1CNomenclature.FirstOrDefault(n => n.C1CNomenclatureID == nomenclatureId).C1CMeaureUnitStorage;

                if (measureUnitId != null)
                {
                    dict = gammaBase.C1CMeasureUnits.Where(mu => mu.C1CMeasureUnitID == measureUnitId | mu.C1CMeasureUnitID == NomenclatureMeasureUnitID).OrderBy(mu => mu.Coefficient).ToDictionary(x => x.C1CMeasureUnitID, v => v.Name);
                }
                else
                {
                    dict = gammaBase.C1CMeasureUnits.Where(mu => mu.C1CMeasureUnitID == NomenclatureMeasureUnitID).OrderBy(mu => mu.Coefficient).ToDictionary(x => x.C1CMeasureUnitID, v => v.Name);
                }
                var mesList = dict.Where(d => d.Value == "т").Select(d => d.Key).ToList();
                foreach (var mes in mesList)
                {
                    dict[mes] = "кг  ";
                }
            }
            return dict.OrderBy(x => x.Value).ToDictionary(x => x.Key, x=> x.Value);
        }

        private Dictionary<Guid, string> GetSampleMeasureUnits(Guid nomenclatureId, Guid? characteristicId)
        {
            var dict = new Dictionary<Guid, string>();
            using (var gammaBase = DB.GammaDb)
            {
                var unit =
                //    gammaBase.C1CCharacteristics.FirstOrDefault(c => c.C1CCharacteristicID == characteristicId)?
                //        .C1CMeasureUnitsPackage;
                //if (unit != null && !dict.ContainsKey(unit.C1CMeasureUnitID))
                //    dict.Add(unit.C1CMeasureUnitID, unit.Name);
                //unit = //оставляем только единицу хранения остатков
                    gammaBase.C1CNomenclature.FirstOrDefault(n => n.C1CNomenclatureID == nomenclatureId)?
                        .C1CMeasureUnitStorage;
                if (unit != null && !dict.ContainsKey(unit.C1CMeasureUnitID))
                    dict.Add(unit.C1CMeasureUnitID, unit.Name);
                //unit =
                //    gammaBase.C1CNomenclature.FirstOrDefault(n => n.C1CNomenclatureID == nomenclatureId)?
                //        .C1CMeasureUnitSets;
                //if (unit != null && !dict.ContainsKey(unit.C1CMeasureUnitID))
                //    dict.Add(unit.C1CMeasureUnitID, unit.Name);
            }
            return dict;
        }



        private Guid DocId { get; set; }
        private int PlaceID { get; set; }
        private int ShiftID { get; set; }
        private DateTime CloseDate { get; set; }
        
        public bool IsWithdrawalMaterial { get; set; }
        public bool IsEnabledSamples { get; set; }
        public bool IsEnabledAuxiliaryMaterials { get; set; }
        public bool IsEnabledDowntimes { get; set; }

        private List<Guid> productionProductCharacteristicIDs { get; set; }
        private DocCloseShiftUnwinderRemainderViewModel spoolUnwinderRemainders { get; set; }
        public void ClearGrid()
        {
            Pallets?.Clear();
            DocCloseDocIds?.Clear();

            //WithdrawalMaterials?.Clear();
            //WithdrawalMaterialsGrid?.Clear();

            //Samples?.Clear();
            var sampleItems = Samples?.Where(d => !(d.Quantity > 0)).ToArray();
            foreach (var item in sampleItems)
            {
                Samples?.Remove(item);
            }
            //AuxiliaryMaterials?.Clear();
            var auxItems = AuxiliaryMaterials?.Where(d => !(d.Quantity > 0 )).ToArray();
            foreach (var item in auxItems)
            {
                AuxiliaryMaterials?.Remove(item);
            }
            IsChanged = true;
            //Wastes?.Clear();
            var wasteItems = Wastes?.Where(d => !(d.Quantity > 0)).ToArray();
            foreach (var item in wasteItems)
            {
                Wastes?.Remove(item);
            }
            NomenclatureRests?.Clear();
            WithdrawalMaterialsGrid.Clear();
            DocCloseShiftProductsGrid.Clear();
            DowntimesGrid?.Clear();
        }


        private ObservableCollection<Sample> _samples = new ObservableCollection<Sample>();

        public ObservableCollection<Sample> Samples
        {
            get { return _samples; }
            set
            {
                _samples = value;
                RaisePropertiesChanged("Samples");
            }
        }

        public ObservableCollection<DocCloseShiftWaste> Wastes
        {
            get { return _wastes; }
            set
            {
                _wastes = value;
                RaisePropertyChanged("Wastes");
            }
        }

        private ObservableCollection<Sample> _nomenclatureRests;

        public ObservableCollection<Sample> NomenclatureRests
        {
            get { return _nomenclatureRests; }
            set
            {
                _nomenclatureRests = value;
                RaisePropertyChanged("NomenclatureRests");
            }
        }

        private ObservableCollection<Sample> _auxiliaryMaterials = new ObservableCollection<Sample>();

        public ObservableCollection<Sample> AuxiliaryMaterials
        {
            get { return _auxiliaryMaterials; }
            set
            {
                _auxiliaryMaterials = value;
                RaisePropertiesChanged("AuxiliaryMaterials");
            }
        }

        private ObservableCollection<Pallet> _pallets = new ObservableCollection<Pallet>();
        private ObservableCollection<DocCloseShiftWaste> _wastes;

        public ObservableCollection<Pallet> Pallets
        {
            get { return _pallets; }
            set
            {
                _pallets = value;
                productionProductCharacteristicIDs = new List<Guid>(_pallets
                    .Select(p => p.CharacteristicID).Distinct().ToList());
                RaisePropertiesChanged("Pallets");
            }
        }

        /// <summary>
        /// Сохранение в БД
        /// </summary>
        /// <param name="docId">ID документа закрытия смены</param>
        public override bool SaveToModel(Guid docId)
        {
            if (IsReadOnly) return true;
            using (var gammaBase = DB.GammaDb)
            {
                var docCloseShift = gammaBase.Docs
                .Include(d => d.DocCloseShiftSamples).Include(d => d.DocCloseShiftWastes).Include(d => d.DocCloseShiftNomenclatureRests)
                .Include(d => d.DocCloseShiftAuxiliaryMaterials)
                .First(d => d.DocID == docId);
                if (docCloseShift.DocCloseShiftSamples == null)
                    docCloseShift.DocCloseShiftSamples = new List<DocCloseShiftSamples>();
                if (docCloseShift.DocCloseShiftWastes == null)
                    docCloseShift.DocCloseShiftWastes = new List<DocCloseShiftWastes>();
                if (docCloseShift.DocCloseShiftRemainders == null)
                    docCloseShift.DocCloseShiftRemainders = new List<DocCloseShiftRemainders>();
                if (docCloseShift.DocCloseShiftNomenclatureRests == null)
                    docCloseShift.DocCloseShiftNomenclatureRests = new List<DocCloseShiftNomenclatureRests>();
                if (docCloseShift.DocCloseShiftMovementProducts == null)
                    docCloseShift.DocCloseShiftMovementProducts = new List<DocCloseShiftMovementProducts>();
                if (docCloseShift.DocCloseShiftUtilizationProducts == null)
                    docCloseShift.DocCloseShiftUtilizationProducts = new List<DocCloseShiftUtilizationProducts>();
                if (docCloseShift.DocCloseShiftAuxiliaryMaterials == null)
                    docCloseShift.DocCloseShiftAuxiliaryMaterials = new List<DocCloseShiftAuxiliaryMaterials>();

                gammaBase.DocCloseShiftSamples.RemoveRange(docCloseShift.DocCloseShiftSamples);
                if (Samples != null)
                    foreach (var sample in Samples)
                    {
                        docCloseShift.DocCloseShiftSamples.Add(new DocCloseShiftSamples
                        {
                            DocID = docId,
                            C1CNomenclatureID = sample.NomenclatureID,
                            C1CCharacteristicID = (Guid)sample.CharacteristicID,
                            DocCloseShiftSampleID = SqlGuidUtil.NewSequentialid(),
                            Quantity = sample.Quantity,
                            C1CMeasureUnitID = sample.MeasureUnitId,
                            ProductionTaskID = sample.ProductionTaskID
                        });
                    }

                gammaBase.DocCloseShiftAuxiliaryMaterials.RemoveRange(docCloseShift.DocCloseShiftAuxiliaryMaterials);
                if (AuxiliaryMaterials != null)
                    foreach (var auxiliaryMaterial in AuxiliaryMaterials)
                    {
                        docCloseShift.DocCloseShiftAuxiliaryMaterials.Add(new DocCloseShiftAuxiliaryMaterials
                        {
                            DocID = docId,
                            C1CNomenclatureID = auxiliaryMaterial.NomenclatureID,
                            C1CCharacteristicID = auxiliaryMaterial.CharacteristicID,
                            DocCloseShiftAuxiliaryMaterialID = SqlGuidUtil.NewSequentialid(),
                            Quantity = auxiliaryMaterial.Quantity,
                            C1CMeasureUnitID = auxiliaryMaterial.MeasureUnitId,
                            ProductionTaskID = auxiliaryMaterial.ProductionTaskID
                        });
                    }

                gammaBase.DocCloseShiftRemainders.RemoveRange(docCloseShift.DocCloseShiftRemainders.Where(p => p.RemainderTypeID != null));//(p => (p.RemainderTypeID ?? 0) != 0));
                if (WithdrawalMaterialsGrid?.DocCloseShiftWithdrawalMaterials?.BeginProducts != null)
                    foreach (var Product in WithdrawalMaterialsGrid?.DocCloseShiftWithdrawalMaterials?.BeginProducts)
                    {
                        docCloseShift.DocCloseShiftRemainders.Add(new DocCloseShiftRemainders
                        {
                            DocID = docId,
                            ProductID = Product.ProductID,
                            DocCloseShiftRemainderID = SqlGuidUtil.NewSequentialid(),
                            RemainderTypeID = Product.RemainderTypeID,
                            Quantity = Product.Quantity,
                            StateID = Product.StateID,
                            IsMaterial = true,
                            IsSourceProduct = Product.IsSourceProduct
                        });
                    }
                if (DocCloseShiftProductsGrid?.BeginProducts != null)
                    foreach (var Product in DocCloseShiftProductsGrid?.BeginProducts)
                    {
                        docCloseShift.DocCloseShiftRemainders.Add(new DocCloseShiftRemainders
                        {
                            DocID = docId,
                            ProductID = Product.ProductID,
                            DocCloseShiftRemainderID = SqlGuidUtil.NewSequentialid(),
                            RemainderTypeID = Product.RemainderTypeID,
                            Quantity = Product.Quantity,
                            StateID = Product.StateID,
                            IsMaterial = false
                        });
                    }
                if (WithdrawalMaterialsGrid?.DocCloseShiftWithdrawalMaterials?.EndProducts != null)
                    foreach (var Product in WithdrawalMaterialsGrid?.DocCloseShiftWithdrawalMaterials?.EndProducts)
                    {
                        if (docCloseShift.DocCloseShiftRemainders.Where(p => (p.RemainderTypeID ?? 0) == 0 && p.ProductID == Product.ProductID).Count() == 0)
                            docCloseShift.DocCloseShiftRemainders.Add(new DocCloseShiftRemainders
                            {
                                DocID = docId,
                                ProductID = Product.ProductID,
                                DocCloseShiftRemainderID = SqlGuidUtil.NewSequentialid(),
                                RemainderTypeID = Product.RemainderTypeID,
                                Quantity = Product.Quantity,
                                StateID = Product.StateID,
                                IsMaterial = true,
                                IsSourceProduct = Product.IsSourceProduct
                            });
                    }
                if (DocCloseShiftProductsGrid?.EndProducts != null)
                    foreach (var Product in DocCloseShiftProductsGrid?.EndProducts)
                    {
                        if (docCloseShift.DocCloseShiftRemainders.Where(p => (p.RemainderTypeID ?? 0) == 0 && p.ProductID == Product.ProductID).Count() == 0)
                            docCloseShift.DocCloseShiftRemainders.Add(new DocCloseShiftRemainders
                            {
                                DocID = docId,
                                ProductID = Product.ProductID,
                                DocCloseShiftRemainderID = SqlGuidUtil.NewSequentialid(),
                                RemainderTypeID = Product.RemainderTypeID,
                                Quantity = Product.Quantity,
                                StateID = Product.StateID,
                                IsMaterial = false
                            });
                    }

                gammaBase.DocCloseShiftNomenclatureRests.RemoveRange(docCloseShift.DocCloseShiftNomenclatureRests);
                if (NomenclatureRests != null)
                    foreach (var rest in NomenclatureRests)
                    {
                        decimal coefficient = 1;
                        var charCoefficient =
                        gammaBase.C1CCharacteristics.FirstOrDefault(c => c.C1CCharacteristicID == rest.CharacteristicID)
                            .C1CMeasureUnitsPackage;
                        if (charCoefficient != null && charCoefficient.Coefficient != null)
                            coefficient = (decimal)charCoefficient.Coefficient;
                        else
                        {
                            //Кол во рул в инд упак
                            var nomenklCoefficient1 = gammaBase.C1CCharacteristics.FirstOrDefault(c => c.C1CCharacteristicID == rest.CharacteristicID)
                            .C1CNomenclature.C1CNomenclatureProperties.FirstOrDefault(n => n.C1CPropertyID == new Guid("492288ED-DBB4-11EA-943C-0015B2A9C22A")).C1CPropertyValues.Description;
                            //Кол во упак в гр уп
                            var nomenklCoefficient2 = gammaBase.C1CCharacteristics.FirstOrDefault(c => c.C1CCharacteristicID == rest.CharacteristicID)
                            .C1CNomenclature.C1CNomenclatureProperties.FirstOrDefault(n => n.C1CPropertyID == new Guid("E27C6973-DBB3-11EA-943C-0015B2A9C22A")).C1CPropertyValues.Description;
                            if (nomenklCoefficient1 != null && nomenklCoefficient1 != String.Empty && nomenklCoefficient2 != null && nomenklCoefficient2 != String.Empty)
                            {
                                coefficient = int.Parse(nomenklCoefficient1) * int.Parse(nomenklCoefficient2);
                            }
                        }

                        docCloseShift.DocCloseShiftNomenclatureRests.Add(new DocCloseShiftNomenclatureRests
                        {
                            DocID = docId,
                            C1CNomenclatureID = rest.NomenclatureID,
                            C1CCharacteristicID = (Guid)rest.CharacteristicID,
                            DocCloseShiftNomenclatureRestID = SqlGuidUtil.NewSequentialid(),
                            Quantity = rest.Quantity * coefficient
                        });
                    }
                gammaBase.DocCloseShiftWastes.RemoveRange(docCloseShift.DocCloseShiftWastes);
                if (Wastes != null)
                    foreach (var waste in Wastes)
                    {
                        docCloseShift.DocCloseShiftWastes.Add(new DocCloseShiftWastes
                        {
                            DocID = docId,
                            C1CNomenclatureID = waste.NomenclatureID,
                            C1CCharacteristicID = waste.CharacteristicID,
                            DocCloseWhiftWasteID = SqlGuidUtil.NewSequentialid(),
                            C1CMeasureUnitID = (Guid)waste.MeasureUnitId,
                            Quantity = waste.MeasureUnit == "кг  " ? Math.Round(waste.Quantity / 1000, 3) : waste.Quantity,
                            ProductNomenclatureID = waste.ProductNomenclatureID,
                            ProductCharacteristicID = waste.ProductCharacteristicID
                        });
                    }
                gammaBase.DocCloseShiftMovementProducts.RemoveRange(docCloseShift.DocCloseShiftMovementProducts);
                if (WithdrawalMaterialsGrid?.DocCloseShiftWithdrawalMaterials?.InProducts != null)
                    foreach (var spool in WithdrawalMaterialsGrid?.DocCloseShiftWithdrawalMaterials?.InProducts)
                    {
                        docCloseShift.DocCloseShiftMovementProducts.Add(new DocCloseShiftMovementProducts
                        {
                            DocID = docId,
                            DocCloseShiftMovementProductID = SqlGuidUtil.NewSequentialid(),
                            ProductID = spool.ProductId,
                            DocMovementID = spool.DocMovementId,
                            Quantity = spool.Quantity,
                            MovementPlaceName = spool.OutPlaceName,
                            MovementPlaceZoneName = spool.OutPlaceZoneName,
                            DateMovement = spool.OutDate,
                            IsMovementIn = true,
                            IsMaterial = true
                        });
                    }
                if (DocCloseShiftProductsGrid?.InProducts != null)
                    foreach (var spool in DocCloseShiftProductsGrid?.InProducts)
                    {
                        docCloseShift.DocCloseShiftMovementProducts.Add(new DocCloseShiftMovementProducts
                        {
                            DocID = docId,
                            DocCloseShiftMovementProductID = SqlGuidUtil.NewSequentialid(),
                            ProductID = spool.ProductId,
                            DocMovementID = spool.DocMovementId,
                            Quantity = spool.Quantity,
                            MovementPlaceName = spool.OutPlaceName,
                            MovementPlaceZoneName = spool.OutPlaceZoneName,
                            DateMovement = spool.OutDate,
                            IsMovementIn = true,
                            IsMaterial = false
                        });
                    }
                //gammaBase.DocCloseShiftMovementProducts.RemoveRange(docCloseShift.DocCloseShiftMovementProducts);
                if (WithdrawalMaterialsGrid?.DocCloseShiftWithdrawalMaterials?.OutProducts != null)
                    foreach (var spool in WithdrawalMaterialsGrid?.DocCloseShiftWithdrawalMaterials?.OutProducts)
                    {
                        docCloseShift.DocCloseShiftMovementProducts.Add(new DocCloseShiftMovementProducts
                        {
                            DocID = docId,
                            DocCloseShiftMovementProductID = SqlGuidUtil.NewSequentialid(),
                            ProductID = spool.ProductId,
                            DocMovementID = spool.DocMovementId,
                            Quantity = spool.Quantity,
                            MovementPlaceName = spool.InPlaceName,
                            MovementPlaceZoneName = spool.InPlaceZoneName,
                            DateMovement = spool.InDate,
                            IsMovementIn = false,
                            IsMaterial = true
                        });
                    }
                if (DocCloseShiftProductsGrid?.OutProducts != null)
                    foreach (var spool in DocCloseShiftProductsGrid?.OutProducts)
                    {
                        docCloseShift.DocCloseShiftMovementProducts.Add(new DocCloseShiftMovementProducts
                        {
                            DocID = docId,
                            DocCloseShiftMovementProductID = SqlGuidUtil.NewSequentialid(),
                            ProductID = spool.ProductId,
                            DocMovementID = spool.DocMovementId,
                            Quantity = spool.Quantity,
                            MovementPlaceName = spool.InPlaceName,
                            MovementPlaceZoneName = spool.InPlaceZoneName,
                            DateMovement = spool.InDate,
                            IsMovementIn = false,
                            IsMaterial = false
                        });
                    }

                gammaBase.DocCloseShiftUtilizationProducts.RemoveRange(docCloseShift.DocCloseShiftUtilizationProducts);
                if (WithdrawalMaterialsGrid?.DocCloseShiftWithdrawalMaterials?.UtilizationProducts != null)
                    foreach (var utilizationSpool in WithdrawalMaterialsGrid?.DocCloseShiftWithdrawalMaterials?.UtilizationProducts)
                    {
                        docCloseShift.DocCloseShiftUtilizationProducts.Add(new DocCloseShiftUtilizationProducts
                        {
                            DocID = docId,
                            DocCloseShiftUtilizationProductID = SqlGuidUtil.NewSequentialid(),
                            ProductID = utilizationSpool.ProductID,
                            Quantity = utilizationSpool.Weight,
                            IsMaterial = true
                        });
                    }
                if (DocCloseShiftProductsGrid?.UtilizationProducts != null)
                    foreach (var utilizationSpool in DocCloseShiftProductsGrid?.UtilizationProducts)
                    {
                        docCloseShift.DocCloseShiftUtilizationProducts.Add(new DocCloseShiftUtilizationProducts
                        {
                            DocID = docId,
                            DocCloseShiftUtilizationProductID = SqlGuidUtil.NewSequentialid(),
                            ProductID = utilizationSpool.ProductID,
                            Quantity = utilizationSpool.Quantity,
                            IsMaterial = false
                        });
                    }
                WithdrawalMaterialsGrid?.SaveToModel(docId);
                DocCloseShiftProductsGrid?.SaveToModel(docId);
                DowntimesGrid?.SaveToModel(docId);

                gammaBase.SaveChanges();

            }
            return true;
        }
    }
}
