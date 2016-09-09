using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Linq;
using DevExpress.Mvvm;
using Gamma.Common;
using Gamma.Interfaces;
using Gamma.Models;

namespace Gamma.ViewModels
{
    public class DocCloseShiftConvertingGridViewModel: SaveImplementedViewModel, ICheckedAccess, IBarImplemented, IFillClearGrid
    {
        /// <summary>
        /// Создает новый экземпляр модели для грида закрытия смены конвертингов
        /// </summary>
        public DocCloseShiftConvertingGridViewModel(GammaEntities gammaBase = null): base(gammaBase)
        {
            Bars.Add(ReportManager.GetReportBar("DocCloseShiftConverting", VMID));
            PlaceID = WorkSession.PlaceID;
            ShiftID = WorkSession.ShiftID;
            CloseDate = DB.CurrentDateTime;
            AddWithdrawalMaterialCommand = new DelegateCommand(AddWithdrawalMaterial, () => !IsReadOnly);
            DeleteWithdrawalMaterialCommand = new DelegateCommand(DeleteWithdrawalMaterial, () => !IsReadOnly);
        }

        public DocCloseShiftConvertingGridViewModel(Guid docId, GammaEntities gammaBase = null) : this(gammaBase)
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
                MeasureUnit = d.C1CNomenclature.C1CMeasureUnits.FirstOrDefault(mu => mu.C1CMeasureUnitQualifierID == d.C1CNomenclature.C1CBaseMeasureUnitQualifier).Name,
                MeasureUnitID = d.C1CNomenclature.C1CMeasureUnits.FirstOrDefault(mu => mu.C1CMeasureUnitQualifierID == d.C1CNomenclature.C1CBaseMeasureUnitQualifier).C1CMeasureUnitID,
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
            Samples = new ObservableCollection<Sample>(GammaBase.DocCloseShiftSamples.Where(ds => ds.DocID == docId)
                .Select(ds => new Sample
                {
                    NomenclatureID = ds.C1CNomenclatureID,
                    CharacteristicID = ds.C1CCharacteristicID,
                    Quantity = ds.Quantity/ds.C1CCharacteristics.C1CMeasureUnitsPackage.Coefficient??1,
                    NomenclatureName = ds.C1CNomenclature.Name + " " + ds.C1CCharacteristics.Name
                }));
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
                    d.Date >= SqlFunctions.DateAdd("hh", -1, SqlFunctions.DateAdd("hh", -1, DB.GetShiftBeginTime(CloseDate))) &&
                    d.Date <= SqlFunctions.DateAdd("hh", 1, SqlFunctions.DateAdd("hh", -1, DB.GetShiftEndTime(CloseDate))) &&
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
            Samples = new ObservableCollection<Sample>(Pallets
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
            IsChanged = true;
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
            MessageManager.FindNomenclature(MaterialTypes.MaterialsSGI);
        }

        private void SetMaterialNomenclature(Nomenclature1CMessage msg)
        {
            Messenger.Default.Unregister<Nomenclature1CMessage>(this);
            var nomenclatureInfo =
                GammaBase.C1CNomenclature.Include(n => n.C1CMeasureUnits)
                    .First(n => n.C1CNomenclatureID == msg.Nomenclature1CID);
            WithdrawalMaterials.Add(new WithdrawalMaterial
            {
                NomenclatureID = nomenclatureInfo.C1CNomenclatureID,
                QuantityIsReadOnly = false,
                Quantity = 0,
                MeasureUnitID = nomenclatureInfo.C1CMeasureUnits.FirstOrDefault(mu => mu.C1CMeasureUnitQualifierID == nomenclatureInfo.C1CBaseMeasureUnitQualifier)?.C1CMeasureUnitID,
                MeasureUnit = nomenclatureInfo.C1CMeasureUnits.FirstOrDefault(mu => mu.C1CMeasureUnitQualifierID == nomenclatureInfo.C1CBaseMeasureUnitQualifier)?.Name,
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

        private ObservableCollection<Pallet> _pallets = new ObservableCollection<Pallet>();
        
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
        public override bool SaveToModel(Guid docId, GammaEntities gammaBase = null)
        {
            if (IsReadOnly) return true;
            var docCloseShift = GammaBase.Docs.Include(d => d.DocCloseShiftDocs).Include(d => d.DocCloseShiftWithdrawals)
                .Include(d => d.DocCloseShiftSamples)
                .First(d => d.DocID == docId);
            if (docCloseShift.DocCloseShiftDocs == null) docCloseShift.DocCloseShiftDocs = new List<Docs>();
            Guid docWithdrawalId;
            if (docCloseShift.DocCloseShiftWithdrawals == null)
                docCloseShift.DocCloseShiftWithdrawals = new List<DocWithdrawal>();
            if (docCloseShift.DocCloseShiftSamples == null)
                docCloseShift.DocCloseShiftSamples = new List<DocCloseShiftSamples>();
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
                    C1CCharacteristicID = sample.CharacteristicID,
                    DocCloseShiftSampleID = SqlGuidUtil.NewSequentialid(),
                    Quantity = sample.Quantity*GammaBase.C1CCharacteristics
                        .FirstOrDefault(c => c.C1CCharacteristicID == sample.CharacteristicID)?.C1CMeasureUnitsPackage.Coefficient??1
                });
            }
            docCloseShift.DocCloseShiftDocs.Clear();
            foreach (var doc in DocCloseShiftDocs)
            {
                docCloseShift.DocCloseShiftDocs.Add(doc);
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

        public class Sample
        {
            public Guid NomenclatureID { get; set; }
            public Guid CharacteristicID { get; set; }
            public string NomenclatureName { get; set; }
            public decimal Quantity { get; set; }

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
