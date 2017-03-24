﻿// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
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
            PlaceID = WorkSession.PlaceID;
            ShiftID = WorkSession.ShiftID;
            CloseDate = DB.CurrentDateTime;
            AddWithdrawalMaterialCommand = new DelegateCommand(AddWithdrawalMaterial, () => !IsReadOnly);
            DeleteWithdrawalMaterialCommand = new DelegateCommand(DeleteWithdrawalMaterial, () => !IsReadOnly);
        }

        public DocCloseShiftConvertingGridViewModel(Guid docId) : this()
        {
            Pallets = new ObservableCollection<Pallet>(GammaBase.GetDocCloseShiftConvertingPallets(docId).Select(d => new Pallet()
            {
                DocId = d.DocID,
                ProductId = d.ProductID,
                NomenclatureName = d.NomenclatureName,
                Quantity = d.Quantity??0,
                NomenclatureID = d.NomenclatureID,
                CharacteristicID = d.CharacteristicID,
                Number = d.Number
            }));
            var doc = GammaBase.Docs.Include(d => d.DocCloseShiftDocs).First(d => d.DocID == docId);
            DocCloseShiftDocs = new ObservableCollection<Docs>(doc.DocCloseShiftDocs);
            PlaceID = (int)doc.PlaceID;
            ShiftID = doc.ShiftID??0;
            CloseDate = doc.Date;
            IsConfirmed = doc.IsConfirmed;
            //Получение списка списанных материалов
            WithdrawalMaterials = new ItemsChangeObservableCollection<WithdrawalMaterial>(GammaBase.DocWithdrawalMaterials
                .Where(dm => dm.DocWithdrawal.DocCloseShift.FirstOrDefault().DocID == docId)
                .Select(d => new WithdrawalMaterial
            {
                CharacteristicID = d.C1CCharacteristicID,
                NomenclatureID = d.C1CNomenclatureID,
                NomenclatureName = d.C1CNomenclature.Name,
                DocWithdrawalMaterialID = d.DocWithdrawalMaterialID,
                Quantity = d.Quantity,
                BaseQuantity = d.Quantity,
                MeasureUnit = d.C1CNomenclature.C1CMeasureUnitStorage.Name,
                MeasureUnitID = d.C1CNomenclature.C1CMeasureUnitStorage.C1CMeasureUnitID,
                    /*                AvailableNomenclatures = new List<WithdrawalMaterial.NomenclatureAnalog>()
                                    {
                                        new WithdrawalMaterial.NomenclatureAnalog()
                                        {
                                            NomenclatureID = d.C1CNomenclatureID,
                                            IsMarked = d.C1CNomenclature.IsArchive ?? false
                                        }
                                    },
                    */
                    QuantityIsReadOnly = !d.WithdrawByFact??true
            }));
            // Получение количества образцов
            var samples = new ObservableCollection<Sample>(GammaBase.DocCloseShiftSamples.Where(ds => ds.DocID == docId)
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
            NomenclatureRests = new ObservableCollection<Sample>(GammaBase.DocCloseShiftNomenclatureRests.Where(dr => dr.DocID == docId)
                .Select(dr => new Sample
                {
                    NomenclatureID = dr.C1CNomenclatureID,
                    CharacteristicID = dr.C1CCharacteristicID,
                    Quantity = dr.Quantity / dr.C1CCharacteristics.C1CMeasureUnitsPackage.Coefficient ?? 1,
                    NomenclatureName = dr.C1CNomenclature.Name + " " + dr.C1CCharacteristics.Name
                }));
            // Получение отходов
            Wastes = new ObservableCollection<Sample>(GammaBase.DocCloseShiftWastes.Where(dw => dw.DocID == docId)
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
        }

        public bool IsChanged { get; private set; }

        private bool IsConfirmed { get; set; }
        private ObservableCollection<Docs> DocCloseShiftDocs { get; set; }
        public bool IsReadOnly => !DB.HaveWriteAccess("DocCloseShiftWithdrawals") || !DB.HaveWriteAccess("DocCloseShiftSamples") || IsConfirmed;
        public ObservableCollection<BarViewModel> Bars { get; set; } = new ObservableCollection<BarViewModel>();
        public Guid? VMID { get; } = Guid.NewGuid();

        public void FillGrid()
        {
            UIServices.SetBusyState();
            ClearGrid();
            DocCloseShiftDocs = new ObservableCollection<Docs>(GammaBase.Docs.
                Where(d => d.PlaceID == PlaceID && d.ShiftID == ShiftID && d.IsConfirmed &&
                    d.Date >= SqlFunctions.DateAdd("hh", -1, DB.GetShiftBeginTime((DateTime)SqlFunctions.DateAdd("hh", -1, CloseDate))) &&
                    d.Date <= SqlFunctions.DateAdd("hh", 1, DB.GetShiftEndTime((DateTime)SqlFunctions.DateAdd("hh",-1,CloseDate))) &&
                    (d.DocTypeID == (int)DocTypes.DocProduction || d.DocTypeID == (int)DocTypes.DocWithdrawal)));
            Pallets = new ObservableCollection<Pallet>(GammaBase.FillDocCloseShiftConvertingPallets(PlaceID, ShiftID, CloseDate).Select(p => new Pallet()
            {
                NomenclatureID = p.NomenclatureID,
                CharacteristicID = p.CharacteristicID,
                Quantity = p.Quantity ?? 0,
                DocId = p.DocID,
                ProductId = p.ProductID,
                NomenclatureName = p.NomenclatureName,
                Number = p.Number
            }));
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
            }
            Samples = samples;
            NomenclatureRests = new ObservableCollection<Sample>(Pallets
                .Select(p => new Sample
                {
                    NomenclatureID = p.NomenclatureID,
                    CharacteristicID = p.CharacteristicID,
                    Quantity = 0,
                    NomenclatureName = p.NomenclatureName,
                }).Distinct());
            WithdrawalMaterials =
                new ItemsChangeObservableCollection<WithdrawalMaterial>(
                    GammaBase.FillDocCloseShiftConvertingMaterials(PlaceID, ShiftID, CloseDate)
                        .Select(m => new WithdrawalMaterial()
                        {
                            NomenclatureID = (Guid)m.NomenclatureID,
                            CharacteristicID = m.CharacteristicID,
                            QuantityIsReadOnly = !m.WithdrawByFact ?? true,
                            Quantity = m.Quantity ?? 0,
                            BaseQuantity = m.Quantity ?? 0,
                            DocWithdrawalMaterialID = SqlGuidUtil.NewSequentialid(),
  /*                          AvailableNomenclatures = new List<WithdrawalMaterial.NomenclatureAnalog>
                            {
                                new WithdrawalMaterial.NomenclatureAnalog()
                                {
                                    NomenclatureID = (Guid) m.NomenclatureID
                                }
                            },*/
                            MeasureUnit = m.MeasureUnit,
                            MeasureUnitID = m.MeasureUnitID
                        }));
            var wastes = new ItemsChangeObservableCollection<Sample>(
                GammaBase.FillDocCloseShiftConvertingWastes(PlaceID, ShiftID, CloseDate)
                .Select(w => new Sample
                {
                    NomenclatureID = (Guid)w.NomenclatureID,
                    CharacteristicID = w.CharacteristicID,
                    Quantity = w.Quantity??0,
                    NomenclatureName = w.NomenclatureName,
                    MeasureUnitId = w.MeasureUnitID,
                    MeasureUnit = w.MeasureUnit
                }));
            foreach (var waste in wastes)
            {
                waste.MeasureUnits = GetWasteMeasureUnits(waste.NomenclatureID);
                waste.MeasureUnitId = waste.MeasureUnits.FirstOrDefault().Key;
            }
            Wastes = wastes;
            IsChanged = true;
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

        public DelegateCommand AddWithdrawalMaterialCommand { get; private set; }
        public DelegateCommand DeleteWithdrawalMaterialCommand { get; private set; }

        private void DeleteWithdrawalMaterial()
        {
            if (SelectedWithdrawalMaterial == null) return;
            WithdrawalMaterials.Remove(SelectedWithdrawalMaterial);
        }

        public WithdrawalMaterial SelectedWithdrawalMaterial { get; set; }
        
        private void AddWithdrawalMaterial()
        {
            Messenger.Default.Register<Nomenclature1CMessage>(this, SetMaterialNomenclature);
            MessageManager.FindNomenclature(MaterialType.MaterialsSGI);
        }

        private void SetMaterialNomenclature(Nomenclature1CMessage msg)
        {
            Messenger.Default.Unregister<Nomenclature1CMessage>(this);
            var nomenclatureInfo =
                GammaBase.C1CNomenclature.Include(n => n.C1CMeasureUnitStorage)
                    .First(n => n.C1CNomenclatureID == msg.Nomenclature1CID);
            WithdrawalMaterials.Add(new WithdrawalMaterial
            {
                NomenclatureID = nomenclatureInfo.C1CNomenclatureID,
                QuantityIsReadOnly = false,
                Quantity = 0,
                MeasureUnitID = nomenclatureInfo.C1CMeasureUnitStorage.C1CMeasureUnitID,
                MeasureUnit = nomenclatureInfo.C1CMeasureUnitStorage.Name,
                DocWithdrawalMaterialID = SqlGuidUtil.NewSequentialid()
            });
        }

        private int PlaceID { get; set; }
        private int ShiftID { get; set; }
        private DateTime CloseDate { get; set; }
        public void ClearGrid()
        {
            Pallets?.Clear();
            DocCloseShiftDocs?.Clear();
            WithdrawalMaterials?.Clear();
            Samples?.Clear();
                IsChanged = true;
            Wastes?.Clear();
            NomenclatureRests?.Clear();
            GammaBase.SaveChanges();
        }

        private ItemsChangeObservableCollection<WithdrawalMaterial> _withdrawalMaterials = new ItemsChangeObservableCollection<WithdrawalMaterial>();

        public ItemsChangeObservableCollection<WithdrawalMaterial> WithdrawalMaterials
        {
            get { return _withdrawalMaterials; }
            set
            {
                _withdrawalMaterials = value;
                RaisePropertiesChanged("WithdrawalMaterials");
            }
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
                RaisePropertiesChanged("Pallets");
            }
        }
        /// <summary>
        /// Сохранение в БД
        /// </summary>
        /// <param name="docId">ID документа закрытия смены</param>
        /// <param name="gammaBase">Контекст БД(в процедуре использован private GammaBase)</param>
        public override bool SaveToModel(Guid docId)
        {
            if (IsReadOnly) return true;
            var docCloseShift = GammaBase.Docs.Include(d => d.DocCloseShiftDocs).Include(d => d.DocCloseShiftWithdrawals)
                .Include(d => d.DocCloseShiftSamples).Include(d => d.DocCloseShiftWastes).Include(d => d.DocCloseShiftNomenclatureRests)
                .First(d => d.DocID == docId);
            if (docCloseShift.DocCloseShiftDocs == null) docCloseShift.DocCloseShiftDocs = new List<Docs>();
            Guid docWithdrawalId;
            if (docCloseShift.DocCloseShiftWithdrawals == null)
                docCloseShift.DocCloseShiftWithdrawals = new List<DocWithdrawal>();
            if (docCloseShift.DocCloseShiftSamples == null)
                docCloseShift.DocCloseShiftSamples = new List<DocCloseShiftSamples>();
            if (docCloseShift.DocCloseShiftWastes == null)
                docCloseShift.DocCloseShiftWastes = new List<DocCloseShiftWastes>();
            if (docCloseShift.DocCloseShiftNomenclatureRests == null)
                docCloseShift.DocCloseShiftNomenclatureRests = new List<DocCloseShiftNomenclatureRests>();
            if (docCloseShift.DocCloseShiftWithdrawals.Count == 0)
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
            }
            GammaBase.DocCloseShiftSamples.RemoveRange(docCloseShift.DocCloseShiftSamples);
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
            GammaBase.DocCloseShiftNomenclatureRests.RemoveRange(docCloseShift.DocCloseShiftNomenclatureRests);
            foreach (var rest in NomenclatureRests)
            {
                docCloseShift.DocCloseShiftNomenclatureRests.Add(new DocCloseShiftNomenclatureRests
                {
                    DocID = docId,
                    C1CNomenclatureID = rest.NomenclatureID,
                    C1CCharacteristicID = (Guid)rest.CharacteristicID,
                    DocCloseShiftNomenclatureRestID = SqlGuidUtil.NewSequentialid(),
                    Quantity = rest.Quantity * GammaBase.C1CCharacteristics
                        .FirstOrDefault(c => c.C1CCharacteristicID == rest.CharacteristicID)?.C1CMeasureUnitsPackage.Coefficient ?? 1
                });
            }
            GammaBase.DocCloseShiftWastes.RemoveRange(docCloseShift.DocCloseShiftWastes);
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
            if (IsChanged)
            {
                docCloseShift.DocCloseShiftDocs.Clear();
 /*               var docCloseShiftDocs = GammaBase.Docs.
                    Where(d => d.PlaceID == PlaceID && d.ShiftID == ShiftID && d.IsConfirmed &&
                        d.Date >= SqlFunctions.DateAdd("hh", -1, DB.GetShiftBeginTime((DateTime)SqlFunctions.DateAdd("hh", -1, CloseDate))) &&
                        d.Date <= SqlFunctions.DateAdd("hh", 1, DB.GetShiftEndTime((DateTime)SqlFunctions.DateAdd("hh", -1, CloseDate))) &&
                        (d.DocTypeID == (int)DocTypes.DocProduction || d.DocTypeID == (int)DocTypes.DocWithdrawal));*/
                foreach (var doc in DocCloseShiftDocs)
                {
                    docCloseShift.DocCloseShiftDocs.Add(doc);
                }
            }
            GammaBase.DocWithdrawalMaterials.RemoveRange(
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
            }
            GammaBase.SaveChanges();
            return true;
        }

        public class Sample : ViewModelBase
        {
            public Guid NomenclatureID { get; set; }
            public Guid? CharacteristicID { get; set; }
            public string NomenclatureName { get; set; }
            public Guid? MeasureUnitId { get; set; }
            public string MeasureUnit { get; set; }
            public decimal Quantity { get; set; }
            public Dictionary<Guid, string> MeasureUnits { get; set; }

            protected bool Equals(Sample other)
            {
                return NomenclatureID.Equals(other.NomenclatureID) && CharacteristicID.Equals(other.CharacteristicID);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((Sample) obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (NomenclatureID.GetHashCode()*397) ^ CharacteristicID.GetHashCode();
                }
            }
        }


    }
}
