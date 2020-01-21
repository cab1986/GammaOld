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
            /*AddWithdrawalMaterialCommand = new DelegateCommand(AddWithdrawalMaterial, () => !IsReadOnly);
            DeleteWithdrawalMaterialCommand = new DelegateCommand(DeleteWithdrawalMaterial, () => !IsReadOnly);
            */
            ShowProductCommand = new DelegateCommand(ShowProduct, SelectedProduct != null);
            PlaceID = WorkSession.PlaceID;
            ShiftID = WorkSession.ShiftID;
            CloseDate = DB.CurrentDateTime;
            IsWithdrawalMaterial = GammaBase.Places.Where(x => x.PlaceID == PlaceID).Select(x => x.PlaceWithdrawalMaterialTypeID != 0).First();
            WithdrawalMaterialsGrid = new DocCloseShiftWithdrawalMaterialViewModel(PlaceID, ShiftID, CloseDate);
            IsEnabledSamples = GammaBase.Places.Where(x => x.PlaceID == PlaceID).Select(x => x.IsEnabledSamplesInDocCloseShift).First() ?? true;
        }

        public DocCloseShiftConvertingGridViewModel(Guid docId) : this()
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
                IsWithdrawalMaterial = GammaBase.Places.Where(x => x.PlaceID == PlaceID).Select(x => x.PlaceWithdrawalMaterialTypeID != 0).First();
                WithdrawalMaterialsGrid = new DocCloseShiftWithdrawalMaterialViewModel(PlaceID, ShiftID, CloseDate);
                IsEnabledSamples = GammaBase.Places.Where(x => x.PlaceID == PlaceID).Select(x => x.IsEnabledSamplesInDocCloseShift).First() ?? true;
                Pallets = new ObservableCollection<Pallet>(gammaBase.GetDocCloseShiftConvertingPallets(docId).Select(d => new Pallet()
                {
                    DocId = d.DocID,
                    ProductId = d.ProductID,
                    NomenclatureName = d.NomenclatureName,
                    Quantity = d.Quantity ?? 0,
                    NomenclatureID = d.NomenclatureID,
                    CharacteristicID = d.CharacteristicID,
                    Number = d.Number
                }));
                //Получение списка списанных материалов
                //WithdrawalMaterials = new ItemsChangeObservableCollection<WithdrawalMaterial>(gammaBase.DocWithdrawalMaterials
                //    .Where(dm => dm.DocWithdrawal.DocCloseShift.FirstOrDefault().DocID == docId)
                //    .Select(d => new WithdrawalMaterial
                //    {
                //        CharacteristicID = d.C1CCharacteristicID,
                //        NomenclatureID = d.C1CNomenclatureID,
                //        NomenclatureName = d.C1CNomenclature.Name,
                //        DocWithdrawalMaterialID = d.DocWithdrawalMaterialID,
                //        Quantity = d.Quantity,
                //        BaseQuantity = d.Quantity,
                //        MeasureUnit = d.C1CNomenclature.C1CMeasureUnitStorage.Name,
                //        MeasureUnitID = d.C1CNomenclature.C1CMeasureUnitStorage.C1CMeasureUnitID,
                //    /*                AvailableNomenclatures = new List<WithdrawalMaterial.NomenclatureAnalog>()
                //                    {
                //                        new WithdrawalMaterial.NomenclatureAnalog()
                //                        {
                //                            NomenclatureID = d.C1CNomenclatureID,
                //                            IsMarked = d.C1CNomenclature.IsArchive ?? false
                //                        }
                //                    },
                //    */
                //        QuantityIsReadOnly = !d.WithdrawByFact ?? true
                //    }));
                IsWithdrawalMaterial = GammaBase.Places.Where(x => x.PlaceID == PlaceID).Select(x => x.PlaceWithdrawalMaterialTypeID != 0).First();
                var pallets = new List<DocCloseShiftWithdrawalMaterial.Product>(Pallets.Select(x => new DocCloseShiftWithdrawalMaterial.Product()
                {
                    ProductID = x.ProductId,
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
                        MeasureUnitId = ds.C1CMeasureUnitID
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
                Wastes = new ObservableCollection<Sample>(gammaBase.DocCloseShiftWastes.Where(dw => dw.DocID == docId)
                    .Select(dw => new Sample
                    {
                        NomenclatureID = dw.C1CNomenclatureID,
                        CharacteristicID = dw.C1CCharacteristicID,
                        Quantity = dw.Quantity,
                        NomenclatureName = dw.C1CNomenclature.Name + " " + dw.C1CCharacteristics.Name ?? "",
                        MeasureUnitId = dw.C1CMeasureUnitID,
                        MeasureUnit = dw.C1CMeasureUnits.Name
                    }));
                foreach (var waste in Wastes)
                {
                    waste.MeasureUnits = GetWasteMeasureUnits(waste.NomenclatureID);
                }
                //Получение остатков
                BeginProducts = new ItemsChangeObservableCollection<DocCloseShiftRemainder>(gammaBase.DocCloseShiftRemainders
                   //.Include(dr => dr.DocCloseShifts)
                   .Where(d => d.DocID == docId && (d.RemainderTypes.RemainderTypeID == 1 || d.RemainderTypes.RemainderTypeID == 3))
                   .Select(d => new DocCloseShiftRemainder()
                   {
                       ProductID = (Guid)d.ProductID,
                       StateID = d.StateID,
                       Quantity = d.Quantity,
                       RemainderTypeID = d.RemainderTypeID
                   }));
                EndProducts = new ItemsChangeObservableCollection<DocCloseShiftRemainder>(gammaBase.DocCloseShiftRemainders
                    //.Include(dr => dr.DocCloseShifts)
                    .Where(d => d.DocID == docId && d.RemainderTypes.RemainderTypeID == 2)
                    .Select(d => new DocCloseShiftRemainder()
                    {
                        ProductID = (Guid)d.ProductID,
                        StateID = d.StateID,
                        Quantity = d.Quantity,
                        RemainderTypeID = d.RemainderTypeID
                    }));
            }
        }

        public bool IsChanged { get; private set; }

        private bool IsConfirmed { get; set; }
        private List<Guid> DocCloseDocIds { get; set; }
        public bool IsReadOnly => !DB.HaveWriteAccess("DocCloseShiftWithdrawals") || !DB.HaveWriteAccess("DocCloseShiftSamples");// || IsConfirmed;
        public ObservableCollection<BarViewModel> Bars { get; set; } = new ObservableCollection<BarViewModel>();
        public Guid? VMID { get; } = Guid.NewGuid();

        public Pallet SelectedProduct { get; set; }
        public DelegateCommand ShowProductCommand { get; private set; }

        private void ShowProduct()
        {
            MessageManager.OpenDocProduct(DocProductKinds.DocProductPallet, SelectedProduct.ProductId);
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

        public void FillGrid()
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
                    DocId = p.DocID,
                    ProductId = p.ProductID,
                    NomenclatureName = p.NomenclatureName,
                    Number = p.Number
                }));
                if (IsEnabledSamples)
                {
                    var samples = new ObservableCollection<Sample>(Pallets
                        .Select(p => new Sample
                        {
                            NomenclatureID = p.NomenclatureID,
                            CharacteristicID = p.CharacteristicID,
                            Quantity = 0,
                            NomenclatureName = p.NomenclatureName,
                        }).Distinct());
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
                    ProductID = x.ProductId,
                    CharacteristicID = x.CharacteristicID
                })).ToList();
                WithdrawalMaterialsGrid.FillWithdrawalMaterials(productionProductCharacteristicIDs, pallets);
                //WithdrawalMaterials =
                //    new ItemsChangeObservableCollection<WithdrawalMaterial>(
                //        gammaBase.FillDocCloseShiftConvertingMaterials(PlaceID, ShiftID, CloseDate)
                //            .Select(m => new WithdrawalMaterial()
                //            {
                //                NomenclatureID = (Guid)m.NomenclatureID,
                //                CharacteristicID = m.CharacteristicID,
                //                QuantityIsReadOnly = !m.WithdrawByFact ?? true,
                //                Quantity = m.Quantity ?? 0,
                //                BaseQuantity = m.Quantity ?? 0,
                //                DocWithdrawalMaterialID = SqlGuidUtil.NewSequentialid(),
                //            /*                          AvailableNomenclatures = new List<WithdrawalMaterial.NomenclatureAnalog>
                //                                      {
                //                                          new WithdrawalMaterial.NomenclatureAnalog()
                //                                          {
                //                                              NomenclatureID = (Guid) m.NomenclatureID
                //                                          }
                //                                      },*/
                //                MeasureUnit = m.MeasureUnit,
                //                MeasureUnitID = m.MeasureUnitID
                //            }));
                var wastes = new ItemsChangeObservableCollection<Sample>(
                    gammaBase.FillDocCloseShiftConvertingWastes(PlaceID, ShiftID, CloseDate)
                    .Select(w => new Sample
                    {
                        NomenclatureID = (Guid)w.NomenclatureID,
                        CharacteristicID = w.CharacteristicID,
                        Quantity = w.Quantity ?? 0,
                        NomenclatureName = w.NomenclatureName,
                        MeasureUnitId = w.MeasureUnitID,
                        MeasureUnit = w.MeasureUnit
                    }));
                if (Wastes == null)
                    Wastes = new ObservableCollection<Sample>();
                foreach (var waste in wastes)
                {
                    waste.MeasureUnits = GetWasteMeasureUnits(waste.NomenclatureID);
                    waste.MeasureUnitId = waste.MeasureUnits.FirstOrDefault().Key;
                    if (!Wastes.Any(s => s.NomenclatureID == waste.NomenclatureID && (s.CharacteristicID == waste.CharacteristicID || (s.CharacteristicID == null && waste.CharacteristicID == null))))
                    {
                        Wastes.Add(waste);
                    }
                }
                //Wastes = wastes;

                var PreviousDocCloseShift = gammaBase.Docs
                    .Where(d => d.DocTypeID == 3 && d.PlaceID == PlaceID && d.Date < CloseDate)
                    .OrderByDescending(d => d.Date)
                    .FirstOrDefault();
                BeginProducts = new ObservableCollection<DocCloseShiftRemainder>(gammaBase.DocCloseShiftRemainders
                    .Where(d => (d.RemainderTypeID == 2 || (d.RemainderTypeID ?? 0) == 0) && d.DocCloseShifts.DocID == PreviousDocCloseShift.DocID)
                    .Select(d => new DocCloseShiftRemainder
                    {
                        ProductID = (Guid)d.ProductID,
                        StateID = d.StateID,
                        Quantity = d.Quantity,
                        RemainderTypeID = d.RemainderTypeID == 0 || d.RemainderTypeID == null ? 3 : 1
                    }));

                FillEndProducts();

                IsChanged = true;
            }
        }

        private Dictionary<Guid, string> GetWasteMeasureUnits(Guid nomenclatureId)
        {
            Dictionary<Guid, string> dict;
            using (var gammaBase = DB.GammaDb)
            {
                dict = gammaBase.C1CMeasureUnits.Where(mu => mu.C1CNomenclatureID == nomenclatureId).OrderBy(mu => mu.Coefficient).ToDictionary(x => x.C1CMeasureUnitID, v => v.Name);                
            }
            return dict.OrderBy(x => x.Value).ToDictionary(x => x.Key, x=> x.Value);
        }

        private Dictionary<Guid, string> GetSampleMeasureUnits(Guid nomenclatureId, Guid? characteristicId)
        {
            var dict = new Dictionary<Guid, string>();
            using (var gammaBase = DB.GammaDb)
            {
                var unit =
                    gammaBase.C1CCharacteristics.FirstOrDefault(c => c.C1CCharacteristicID == characteristicId)?
                        .C1CMeasureUnitsPackage;
                if (unit != null && !dict.ContainsKey(unit.C1CMeasureUnitID))
                    dict.Add(unit.C1CMeasureUnitID, unit.Name);
                unit =
                    gammaBase.C1CNomenclature.FirstOrDefault(n => n.C1CNomenclatureID == nomenclatureId)?
                        .C1CMeasureUnitStorage;
                if (unit != null && !dict.ContainsKey(unit.C1CMeasureUnitID))
                    dict.Add(unit.C1CMeasureUnitID, unit.Name);
                unit =
                    gammaBase.C1CNomenclature.FirstOrDefault(n => n.C1CNomenclatureID == nomenclatureId)?
                        .C1CMeasureUnitSets;
                if (unit != null && !dict.ContainsKey(unit.C1CMeasureUnitID))
                    dict.Add(unit.C1CMeasureUnitID, unit.Name);
            }
            return dict;
        }

        private void FillEndProducts()
        {
            using (var gammaBase = DB.GammaDb)
            {
                EndProducts?.Clear();

                EndProducts = new ItemsChangeObservableCollection<DocCloseShiftRemainder>(gammaBase.Rests
                    .Where(d => d.PlaceID == PlaceID).Join(gammaBase.vProductsInfo, d => d.ProductID, p => p.ProductID
                    , (d, p) => new DocCloseShiftRemainder
                    {
                        ProductID = (Guid)d.ProductID,
                        StateID = (p.StateID == null ? 0 : p.StateID),
                        Quantity = p.Quantity ?? 0 ,
                        RemainderTypeID = 2,
                        ProductKindID = p.ProductKindID
                    }));

                foreach (var endProduct in EndProducts)
                {
                    endProduct.Quantity = endProduct.ProductKindID == 0 ? endProduct.Quantity * 1000 :
                            endProduct.ProductKindID == 2 ? endProduct.Quantity * 1000 :
                            endProduct.Quantity;
                }
                //убираем из остатков переходящую следующей смене палету в процессе производства
                var doc = gammaBase.Docs.Where(d => d.DocTypeID == 3 && d.PlaceID == PlaceID && d.ShiftID == ShiftID && d.Date == CloseDate).FirstOrDefault();
                if (doc != null)
                {
                    var endProductsProductIDs = EndProducts.Select(d => (Guid?)d.ProductID).ToList();
                    var ProductRemainders = gammaBase.DocCloseShiftRemainders.Where(r => (r.RemainderTypeID ?? 0) == 0 && r.DocID == doc.DocID && endProductsProductIDs.Contains(r.ProductID));
                    foreach (var Product in ProductRemainders)
                    {
                        var removeProduct = EndProducts.FirstOrDefault(d => d.ProductID == Product.ProductID);
                        if (removeProduct != null)
                            EndProducts.Remove(removeProduct);
                    }
                }
            }
        }

        private Guid DocId { get; set; }
        private int PlaceID { get; set; }
        private int ShiftID { get; set; }
        private DateTime CloseDate { get; set; }
        
        public bool IsWithdrawalMaterial { get; set; }
        public bool IsEnabledSamples { get; set; }

        private List<Guid> productionProductCharacteristicIDs { get; set; }

        public void ClearGrid()
        {
            Pallets?.Clear();
            DocCloseDocIds?.Clear();

            //WithdrawalMaterials?.Clear();
            //WithdrawalMaterialsGrid?.Clear();

            Samples?.Clear();
            IsChanged = true;
            Wastes?.Clear();
            NomenclatureRests?.Clear();
            WithdrawalMaterialsGrid.Clear();
            //            gammaBase.SaveChanges();           
        }

        /*private ItemsChangeObservableCollection<WithdrawalMaterial> _withdrawalMaterials = new ItemsChangeObservableCollection<WithdrawalMaterial>();

        public ItemsChangeObservableCollection<WithdrawalMaterial> WithdrawalMaterials
        {
            get { return _withdrawalMaterials; }
            set
            {
                _withdrawalMaterials = value;
                RaisePropertiesChanged("WithdrawalMaterials");
            }
        }*/

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

        public ObservableCollection<Sample> Wastes
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


        private ObservableCollection<Pallet> _pallets = new ObservableCollection<Pallet>();
        private ObservableCollection<Sample> _wastes;

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

        private ObservableCollection<DocCloseShiftRemainder> _beginProducts = new ObservableCollection<DocCloseShiftRemainder>();
        public ObservableCollection<DocCloseShiftRemainder> BeginProducts
        {
            get { return _beginProducts; }
            set
            {
                _beginProducts = value;
                RaisePropertiesChanged("BeginProducts");
            }
        }

        private ObservableCollection<DocCloseShiftRemainder> _endProducts = new ObservableCollection<DocCloseShiftRemainder>();
        public ObservableCollection<DocCloseShiftRemainder> EndProducts
        {
            get { return _endProducts; }
            set
            {
                _endProducts = value;
                RaisePropertiesChanged("EndProducts");
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
                var docCloseShift = gammaBase.Docs.Include(d => d.DocCloseShiftDocs).Include(d => d.DocCloseShiftWithdrawals)
                .Include(d => d.DocCloseShiftSamples).Include(d => d.DocCloseShiftWastes).Include(d => d.DocCloseShiftNomenclatureRests)
                .First(d => d.DocID == docId);
                if (docCloseShift.DocCloseShiftDocs == null) docCloseShift.DocCloseShiftDocs = new List<Docs>();
                //  Guid docWithdrawalId;
                //if (docCloseShift.DocCloseShiftWithdrawals == null)
                //    docCloseShift.DocCloseShiftWithdrawals = new List<DocWithdrawal>();
                if (docCloseShift.DocCloseShiftSamples == null)
                    docCloseShift.DocCloseShiftSamples = new List<DocCloseShiftSamples>();
                if (docCloseShift.DocCloseShiftWastes == null)
                    docCloseShift.DocCloseShiftWastes = new List<DocCloseShiftWastes>();
                if (docCloseShift.DocCloseShiftRemainders == null)
                    docCloseShift.DocCloseShiftRemainders = new List<DocCloseShiftRemainders>();
                if (docCloseShift.DocCloseShiftNomenclatureRests == null)
                    docCloseShift.DocCloseShiftNomenclatureRests = new List<DocCloseShiftNomenclatureRests>();
                /*if (docCloseShift.DocCloseShiftWithdrawals.Count == 0)
                    {
                        docWithdrawalId = SqlGuidUtil.NewSequentialid();
                        docCloseShift.DocCloseShiftWithdrawals.Add
                        (
                            new DocWithdrawal
                            {
                                DocID = docWithdrawalId,
                                DocWithdrawalMaterials = new List<DocWithdrawalMaterials>(),
                                Docs = new Docs()
                                {
                                    DocID = docWithdrawalId,
                                    Date = docCloseShift.Date,
                                    DocTypeID = (byte)DocTypes.DocWithdrawal,
                                    IsConfirmed = docCloseShift.IsConfirmed,
                                    PlaceID = WorkSession.PlaceID,
                                    ShiftID = WorkSession.ShiftID,
                                    UserID = WorkSession.UserID,
                                    PrintName = WorkSession.PrintName
                                }
                            }
                        );
                    }*/
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
                            C1CMeasureUnitID = sample.MeasureUnitId
                        });
                    }

                gammaBase.DocCloseShiftRemainders.RemoveRange(docCloseShift.DocCloseShiftRemainders.Where(p => p.RemainderTypeID != null));//(p => (p.RemainderTypeID ?? 0) != 0));
                if (BeginProducts != null)
                    foreach (var Product in BeginProducts)
                    {
                        docCloseShift.DocCloseShiftRemainders.Add(new DocCloseShiftRemainders
                        {
                            DocID = docId,
                            ProductID = Product.ProductID,
                            DocCloseShiftRemainderID = SqlGuidUtil.NewSequentialid(),
                            RemainderTypeID = Product.RemainderTypeID,
                            Quantity = Product.Quantity,
                            StateID = Product.StateID
                        });
                    }
                if (EndProducts != null)
                    foreach (var Product in EndProducts)
                    {
                        if (docCloseShift.DocCloseShiftRemainders.Where(p => (p.RemainderTypeID ?? 0) == 0 && p.ProductID == Product.ProductID).Count() == 0)
                            docCloseShift.DocCloseShiftRemainders.Add(new DocCloseShiftRemainders
                            {
                                DocID = docId,
                                ProductID = Product.ProductID,
                                DocCloseShiftRemainderID = SqlGuidUtil.NewSequentialid(),
                                RemainderTypeID = Product.RemainderTypeID,
                                Quantity = Product.Quantity,
                                StateID = Product.StateID
                            });
                    }

                gammaBase.DocCloseShiftNomenclatureRests.RemoveRange(docCloseShift.DocCloseShiftNomenclatureRests);
                if (NomenclatureRests != null)
                    foreach (var rest in NomenclatureRests)
                    {
                        docCloseShift.DocCloseShiftNomenclatureRests.Add(new DocCloseShiftNomenclatureRests
                        {
                            DocID = docId,
                            C1CNomenclatureID = rest.NomenclatureID,
                            C1CCharacteristicID = (Guid)rest.CharacteristicID,
                            DocCloseShiftNomenclatureRestID = SqlGuidUtil.NewSequentialid(),
                            Quantity = rest.Quantity * gammaBase.C1CCharacteristics
                                .FirstOrDefault(c => c.C1CCharacteristicID == rest.CharacteristicID)?.C1CMeasureUnitsPackage.Coefficient ?? 1
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
                            Quantity = waste.Quantity
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
                            IsMovementIn = true
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
                            IsMovementIn = false
                        });
                    }

                gammaBase.DocCloseShiftUtilizationProducts.RemoveRange(docCloseShift.DocCloseShiftUtilizationProducts);
                if (WithdrawalMaterialsGrid?.DocCloseShiftWithdrawalMaterials?.UtilizationSpools != null)
                    foreach (var utilizationSpool in WithdrawalMaterialsGrid?.DocCloseShiftWithdrawalMaterials?.UtilizationSpools)
                    {
                        docCloseShift.DocCloseShiftUtilizationProducts.Add(new DocCloseShiftUtilizationProducts
                        {
                            DocID = docId,
                            DocCloseShiftUtilizationProductID = SqlGuidUtil.NewSequentialid(),
                            ProductID = utilizationSpool.ProductID,
                            Quantity = utilizationSpool.Weight
                        });
                    }

                if (IsChanged)
                {
                    docCloseShift.DocCloseShiftDocs.Clear();
                    /*               var docCloseShiftDocs = GammaBase.Docs.
                                       Where(d => d.PlaceID == PlaceID && d.ShiftID == ShiftID && d.IsConfirmed &&
                                           d.Date >= SqlFunctions.DateAdd("hh", -1, DB.GetShiftBeginTime((DateTime)SqlFunctions.DateAdd("hh", -1, CloseDate))) &&
                                           d.Date <= SqlFunctions.DateAdd("hh", 1, DB.GetShiftEndTime((DateTime)SqlFunctions.DateAdd("hh", -1, CloseDate))) &&
                                           (d.DocTypeID == (int)DocTypes.DocProduction || d.DocTypeID == (int)DocTypes.DocWithdrawal));*/
                    foreach (var id in DocCloseDocIds)
                    {
                        docCloseShift.DocCloseShiftDocs.Add(gammaBase.Docs.First(d => d.DocID == id));
                    }
                }

                /*
                gammaBase.DocWithdrawalMaterials.RemoveRange(
                    docCloseShift.DocCloseShiftWithdrawals.FirstOrDefault()?.DocWithdrawalMaterials);
                docWithdrawalId = docCloseShift.DocCloseShiftWithdrawals.First().DocID;
                foreach (var material in WithdrawalMaterials)
                {
                    docCloseShift.DocCloseShiftWithdrawals.FirstOrDefault()?.DocWithdrawalMaterials.Add(new DocWithdrawalMaterials()
                    {
                        DocID = docWithdrawalId,
                        DocWithdrawalMaterialID = SqlGuidUtil.NewSequentialid(),
                        C1CCharacteristicID = material.CharacteristicID,
                        C1CNomenclatureID = material.NomenclatureID,
                        Quantity = material.Quantity,
                        WithdrawByFact = !material.QuantityIsReadOnly
                    });
                }*/
                WithdrawalMaterialsGrid?.SaveToModel(docId);

                gammaBase.SaveChanges();

            }
            return true;
        }
    }
}
