// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Linq;
using DevExpress.Mvvm;
using Gamma.Entities;
using Gamma.Common;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Collections.ObjectModel;
using Gamma.ViewModels;
using System.Data.Entity.SqlServer;

namespace Gamma.Models
{
    public class DocCloseShiftWithdrawalMaterial : ViewModelBase
    {
        public DocCloseShiftWithdrawalMaterial(int placeID, int shiftID, DateTime closeDate)
        {
            GammaBase = DB.GammaDb;
            PlaceID = placeID;
            ShiftID = shiftID;
            CloseDate = closeDate;
            Messenger.Default.Register<RecalcQuantityEndFromUnwinderReaminderMessage>(this, RecalcQuantityEndFromUnwinderReaminder);
        }

        private int PlaceID;
        private int ShiftID;
        DateTime CloseDate;

        private GammaEntities GammaBase { get; }

        public class Product
        {
            public Guid ProductID;
            public Guid CharacteristicID;
        }

        private ItemsChangeObservableCollection<WithdrawalMaterial> _withdrawalMaterials = new ItemsChangeObservableCollection<WithdrawalMaterial>();

        public ItemsChangeObservableCollection<WithdrawalMaterial> WithdrawalMaterials
        {
            get { return _withdrawalMaterials; }
            set
            {
                _withdrawalMaterials = value;
                RaisePropertiesChanged("DocCloseShiftWithdrawalMaterials.WithdrawalMaterials");
            }
        }

        private ItemsChangeObservableCollection<DocCloseShiftMaterial> _docCloseShiftMaterials = new ItemsChangeObservableCollection<DocCloseShiftMaterial>();

        public ItemsChangeObservableCollection<DocCloseShiftMaterial> DocCloseShiftMaterials
        {
            get { return _docCloseShiftMaterials; }
            set
            {
                _docCloseShiftMaterials = value;
                RaisePropertiesChanged("DocCloseShiftWithdrawalMaterials.DocCloseShiftMaterials");
            }
        }

        private ItemsChangeObservableCollection<MovementProduct> _inProducts = new ItemsChangeObservableCollection<MovementProduct>();
        public ItemsChangeObservableCollection<MovementProduct> InProducts
        {
            get { return _inProducts; }
            private set
            {
                _inProducts = value;
                RaisePropertiesChanged("InProducts");
            }
        }

        private ItemsChangeObservableCollection<MovementProduct> _outProducts = new ItemsChangeObservableCollection<MovementProduct>();
        public ItemsChangeObservableCollection<MovementProduct> OutProducts
        {
            get { return _outProducts; }
            private set
            {
                _outProducts = value;
                RaisePropertiesChanged("OutProducts");
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

        private ObservableCollection<PaperBase> _utilizationProducts = new ItemsChangeObservableCollection<PaperBase>();
        public ObservableCollection<PaperBase> UtilizationProducts
        {
            get { return _utilizationProducts; }
            private set
            {
                _utilizationProducts = value;
                RaisePropertiesChanged("UtilizationProducts");
            }
        }

        private ObservableCollection<DocMaterialProductionDirectCalculationItem> _materialsDismiss { get; set; } //= new ItemsChangeObservableCollection<DocMaterialProductionDirectCalculationItem>();
        public ObservableCollection<DocMaterialProductionDirectCalculationItem> MaterialsDismiss
        {
            get { return _materialsDismiss; }
            private set
            {
                _materialsDismiss = value;
                RaisePropertiesChanged("MaterialsDismiss");
            }
        }


        private List<Guid> ProductionProductCharacteristicIDs { get; set; }

        public void LoadWithdrawalMaterials(Guid docId, List<Guid> productionProductCharacteristicIDs)
        {
            ProductionProductCharacteristicIDs = productionProductCharacteristicIDs;
            DocCloseShiftMaterials = new ItemsChangeObservableCollection<DocCloseShiftMaterial>(GammaBase.vDocCloseShiftMaterials
                    .Where(dm => dm.DocID == docId)
                    .Select(d => new DocCloseShiftMaterial
                    {
                        CharacteristicID = d.C1CCharacteristicID,
                        NomenclatureID = d.C1CNomenclatureID,
                        NomenclatureName = d.NomenclatureName,
                        QuantityRemainderAtBegin = d.QuantityRemainderAtBegin,
                        QuantityIn = d.QuantityIn,
                        QuantityOut = d.QuantityOut,
                        QuantityUtil = d.QuantityUtil,
                        QuantityExperimental = d.QuantityExperimental,
                        QuantityRePack = d.QuantityRePack,
                        QuantityRemainderAtEnd = d.QuantityRemainderAtEnd,
                        QuantityWithdrawalMaterial = d.QuantityWithdrawalMaterial,
                        MeasureUnit = d.MeasureUnitName,
                        MeasureUnitID = d.C1CMeasureUnitID,
                        WithdrawByFact = d.WithdrawByFact,
                        QuantityIsReadOnly = d.QuantityIsReadOnly ?? false,
                        StandardQuantity = d.StandardQuantity
                        //,QuantityDismiss = d.QuantityDismiss
                    }).OrderBy(d => d.NomenclatureName));
            
            InProducts = new ItemsChangeObservableCollection<MovementProduct>(GammaBase.DocCloseShiftMovementProducts
                .Where(d => d.DocID == docId && (bool)d.IsMovementIn && (bool)d.IsMaterial)
                    .Join(GammaBase.vProductsInfo, d => d.ProductID, p => p.ProductID
                    , (d, p) => new MovementProduct
                    {
                        NomenclatureName = p.NomenclatureName,
                        Number = d.Products.Number,
                        ProductId = d.ProductID,
                        Quantity = d.Quantity ?? 0,
                        ProductKindName = d.Products.ProductKinds.Name,
                        //OrderTypeName = p.OrderTypeName,
                        DocMovementId = d.DocMovementID,
                        OutPlaceName = d.MovementPlaceName,
                        OutPlaceZoneName = d.MovementPlaceZoneName,
                        OutDate = d.DateMovement,
                        NomenclatureId = p.C1CNomenclatureID,
                        CharacteristicId = p.C1CCharacteristicID,
                        NomenclatureKindID = p.NomenclatureKindID,
                        MeasureUnit = p.BaseMeasureUnit,
                        MeasureUnitID = p.BaseMeasureUnitID
                    }));

            OutProducts = new ItemsChangeObservableCollection<MovementProduct>(GammaBase.DocCloseShiftMovementProducts
                .Where(d => d.DocID == docId && !((bool)d.IsMovementIn) && (bool)d.IsMaterial)
                    .Join(GammaBase.vProductsInfo, d => d.ProductID, p => p.ProductID
                    , (d, p) => new MovementProduct
                    {
                        NomenclatureName = p.NomenclatureName,
                        Number = d.Products.Number,
                        ProductId = d.ProductID,
                        Quantity = d.Quantity ?? 0,
                        ProductKindName = d.Products.ProductKinds.Name,
                        //OrderTypeName = p.OrderTypeName,
                        DocMovementId = d.DocMovementID,
                        OutPlaceName = d.MovementPlaceName,
                        OutPlaceZoneName = d.MovementPlaceZoneName,
                        OutDate = d.DateMovement,
                        NomenclatureId = p.C1CNomenclatureID,
                        CharacteristicId = p.C1CCharacteristicID,
                        NomenclatureKindID = p.NomenclatureKindID,
                        MeasureUnit = p.BaseMeasureUnit,
                        MeasureUnitID = p.BaseMeasureUnitID
                    }));

            //Получение остатков
            BeginProducts = new ItemsChangeObservableCollection<DocCloseShiftRemainder>(GammaBase.DocCloseShiftRemainders
               //.Include(dr => dr.DocCloseShifts)
               .Where(d => d.DocID == docId && (d.IsMaterial ?? false) && (d.RemainderTypes.RemainderTypeID == 1 || d.RemainderTypes.RemainderTypeID == 3))
               .Select(d => new DocCloseShiftRemainder()
               {
                   ProductID = (Guid)d.ProductID,
                   StateID = d.StateID,
                   Quantity = d.Quantity,
                   RemainderTypeID = d.RemainderTypeID,
                   ProductKind = d.Products == null ? new ProductKind() : (ProductKind)d.Products.ProductKindID,
                   IsSourceProduct = d.IsSourceProduct
               }));
            EndProducts = new ItemsChangeObservableCollection<DocCloseShiftRemainder>(GammaBase.DocCloseShiftRemainders
                //.Include(dr => dr.DocCloseShifts)
                .Where(d => d.DocID == docId && (d.IsMaterial ?? false) && (d.RemainderTypes.RemainderTypeID == 2 || d.RemainderTypes.RemainderTypeID == 0))
                .Select(d => new DocCloseShiftRemainder()
                {
                    ProductID = (Guid)d.ProductID,
                    StateID = d.StateID,
                    Quantity = d.Quantity,
                    RemainderTypeID = d.RemainderTypeID,
                    ProductKind = d.Products == null ? new ProductKind() : (ProductKind)d.Products.ProductKindID,
                    NomenclatureID = d.Products.ProductSpools.C1CNomenclatureID,
                    CharacteristicID = d.Products.ProductSpools.C1CCharacteristicID,
                    IsSourceProduct = d.IsSourceProduct
                }));

            //Получение списка утилизированных тамбуров
            UtilizationProducts = new ObservableCollection<PaperBase>(GammaBase.DocCloseShiftUtilizationProducts
                .Where(d => d.DocID == docId && (d.IsMaterial ?? true))
                .Join(GammaBase.vProductsInfo, d => d.ProductID, p => p.ProductID
                , (d, p) => new PaperBase()
                {
                    CharacteristicID = (Guid)p.C1CCharacteristicID,
                    NomenclatureID = p.C1CNomenclatureID,
                    Nomenclature = p.NomenclatureName,
                    Number = p.Number,
                    ProductID = d.ProductID,
                    Weight = d.Quantity ?? 0
                }));

            //Получение материалов из документов Расход сырья и материалов
            var docs = GammaBase.Docs.FirstOrDefault(d => d.DocID == docId).DocCloseShiftDocs;//.Select(s => s.DocID).ToList();
            if (docs != null && docs.Any(ds => ds.DocTypeID == (int)DocTypes.DocMaterialProduction))
            {
                var docIDs = docs.Where(ds => ds.DocTypeID == (int)DocTypes.DocMaterialProduction).Select(ds => ds.DocID).ToList();
                MaterialsDismiss = new ItemsChangeObservableCollection<DocMaterialProductionDirectCalculationItem>(GammaBase.vDocMaterialProductionDirectCalculations
                        .Where(dm => docIDs.Contains(dm.DocID))
                        .Join(GammaBase.Docs, d => d.DocID, dm => dm.DocID, (d, dm)
                         => new DocMaterialProductionDirectCalculationItem
                         {
                             CharacteristicID = d.C1CCharacteristicID,
                             NomenclatureID = d.C1CNomenclatureID,
                             NomenclatureName = d.NomenclatureName,
                             QuantityRemainderAtBegin = d.QuantityRemainderAtBegin,
                             QuantityIn = d.QuantityIn,
                             QuantityOut = d.QuantityOut,
                             QuantityUtil = d.QuantityUtil,
                             QuantityExperimental = d.QuantityExperimental,
                             QuantityRemainderAtEnd = d.QuantityRemainderAtEnd,
                             QuantitySend = d.QuantitySend,
                             MeasureUnit = d.MeasureUnitName,
                             MeasureUnitID = d.C1CMeasureUnitID,
                             WithdrawByFact = d.WithdrawByFact,
                             QuantityIsReadOnly = d.QuantityIsReadOnly ?? false,
                             ParentID = d.ParentID,
                             ParentName = d.ParentName,
                             PlaceID = dm.PlaceID ?? 0,
                             DocID = dm.DocID,
                             DocNumberDate = dm.Number + " от " + dm.Date
                         }).OrderBy(d => d.NomenclatureName));
            }
            //Получение списка списанных материалов
            WithdrawalMaterials = new ItemsChangeObservableCollection<WithdrawalMaterial>(GammaBase.DocWithdrawalMaterials
                //.Join(GammaBase.Docs, dm => dm.DocWithdrawal.Docs.DocID, dw => dw.DocCloseShift.DocID, (dm, dw) => new { })
                .Where(dm => dm.DocWithdrawal.DocCloseShift.FirstOrDefault().DocID == docId)
                .Select(d => new WithdrawalMaterial()
                {
                    CharacteristicID = d.C1CCharacteristicID,
                    NomenclatureID = d.C1CNomenclatureID,
                    NomenclatureName = d.C1CNomenclature.Name + d.C1CCharacteristicID == null || d.C1CCharacteristicID == Guid.Empty ? string.Empty : " " + d.C1CCharacteristics.Name,
                    DocWithdrawalMaterialID = d.DocWithdrawalMaterialID,
                    Quantity = d.Quantity,
                    BaseQuantity = d.Quantity,
                    MeasureUnit = d.C1CNomenclature.C1CMeasureUnitStorage.Name,
                    MeasureUnitID = d.C1CNomenclature.C1CMeasureUnitStorage.C1CMeasureUnitID,
                    QuantityIsReadOnly = !d.WithdrawByFact ?? true,
                    ProductionProductCharacteristicID = d.DocWithdrawal.DocProduction.FirstOrDefault().DocProductionProducts.FirstOrDefault().C1CCharacteristicID,
                    WithdrawByFact = d.WithdrawByFact
                }).OrderBy(d => d.NomenclatureName));
            foreach (var item in WithdrawalMaterials)
            {
                item.PlaceID = PlaceID;
                item.SetAvailableProductionProducts = productionProductCharacteristicIDs;

            }
        }

        private void FillEndProducts(bool IsFillEnd = true)
        {
            if (IsFillEnd)
                using (var gammaBase = DB.GammaDb)
                {
                    EndProducts?.Clear();

                    EndProducts = new ItemsChangeObservableCollection<DocCloseShiftRemainder>(gammaBase.Rests
                        .Where(d => d.PlaceID == PlaceID).Join(gammaBase.vProductsInfo.Where(v => ((v.ProductKindID == (byte)ProductKind.ProductSpool || v.ProductKindID == (byte)ProductKind.ProductGroupPack) && (gammaBase.Places.Any(pl => pl.PlaceID == PlaceID && pl.PlaceGroupID == (short)PlaceGroup.Convertings && pl.PlaceID == v.CurrentPlaceID)))), d => d.ProductID, p => p.ProductID
                        , (d, p) => new DocCloseShiftRemainder
                        {
                            ProductID = (Guid)d.ProductID,
                            StateID = (p.StateID == null ? 0 : p.StateID),
                            Quantity = (p.Quantity ?? 0) * (p.ProductKindID == 0 || p.ProductKindID == 2 ? 1000 : 1),
                            RemainderTypeID = 2,
                            ProductKind = (ProductKind)p.ProductKindID,
                            NomenclatureID = p.C1CNomenclatureID,
                            CharacteristicID = p.C1CCharacteristicID
                        }));
                    var docUnwinderRemainders = GammaBase.DocUnwinderRemainders.Where(r => r.DocWithdrawalID != null && r.Docs1.PlaceID == PlaceID && r.Docs1.ShiftID == ShiftID &&
                            r.Docs1.Date >= SqlFunctions.DateAdd("hh", -1, DB.GetShiftBeginTime((DateTime)SqlFunctions.DateAdd("hh", -1, CloseDate))) &&
                            r.Docs1.Date <= SqlFunctions.DateAdd("hh", 1, DB.GetShiftEndTime((DateTime)SqlFunctions.DateAdd("hh", -1, CloseDate)))).Select(r => r.ProductID).ToList();
                    if (docUnwinderRemainders?.Count() > 0)
                    {
                        foreach (var item in EndProducts.Where(p => docUnwinderRemainders.Contains(p.ProductID)))
                        {
                            item.RemainderTypeID = 0;
                            item.IsSourceProduct = true;
                        }
                    }
                    /*
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
                    }*/
                }
        }

        public void FillWithdrawalMaterials(List<Guid> productionProductCharacteristicIDs, DocCloseShiftUnwinderRemainderViewModel spoolUnwinderRemainders, bool IsFillEnd = true)
        {
            ProductionProductCharacteristicIDs = productionProductCharacteristicIDs;
            {
                Clear(IsFillEnd);

                var docCloseShiftMaterials = new ItemsChangeObservableCollection<DocCloseShiftMaterial>(DocCloseShiftMaterials);
                DocCloseShiftMaterials.Clear();
                foreach (var item in docCloseShiftMaterials)
                {
                    DocCloseShiftMaterials.Add(item);
                }

                InProducts = new ItemsChangeObservableCollection<MovementProduct>(GammaBase.FillDocCloseShiftMovementProducts(PlaceID, ShiftID, CloseDate)
                                .Where(d => (bool)d.IsMovementIn && (bool)d.IsMaterial).Select(d => new MovementProduct
                                {
                                    NomenclatureName = d.NomenclatureName,
                                    Number = d.Number,
                                    ProductId = d.ProductID,
                                    Quantity = (d.Quantity ?? 0),
                                    ProductKindName = d.ProductKindName,
                                    OrderTypeName = d.OrderTypeName,
                                    DocMovementId = d.DocMovementID,
                                    OutPlaceName = d.OutPlace,
                                    OutPlaceZoneName = d.OutPlaceZone,
                                    OutDate = d.OutDate,
                                    NomenclatureId = d.NomenclatureID,
                                    CharacteristicId = d.CharacteristicID,
                                    NomenclatureKindID = d.NomenclatureKindID,
                                    MeasureUnit = d.MeasureUnit,
                                    MeasureUnitID = d.MeasureUnitID
                                }));
                foreach (MovementProduct addedItem in InProducts)
                {
                    var item = DocCloseShiftMaterials.FirstOrDefault(d => d.NomenclatureID == addedItem.NomenclatureId && (d.CharacteristicID == addedItem.CharacteristicId || (d.CharacteristicID == null && addedItem.CharacteristicId == null)));
                    if (item == null)
                    {
                        var standardQuantity = DocCloseShiftMaterials.Where(d => d.AvailableNomenclatures.Any(n => n.NomenclatureID == addedItem.NomenclatureId && (n.CharacteristicID == addedItem.CharacteristicId || (n.CharacteristicID == null && addedItem.CharacteristicId == null)))).FirstOrDefault();
                        DocCloseShiftMaterials.Add(new DocCloseShiftMaterial()
                        {
                            NomenclatureID = (Guid)addedItem.NomenclatureId,
                            CharacteristicID = addedItem.CharacteristicId,
                            NomenclatureName = addedItem.NomenclatureName,
                            QuantityIsReadOnly = true,
                            QuantityIn = addedItem.Quantity,
                            DocWithdrawalMaterialID = SqlGuidUtil.NewSequentialid(),
                            MeasureUnit = addedItem.MeasureUnit,
                            MeasureUnitID = addedItem.MeasureUnitID,
                            WithdrawByFact = true,
                            NomenclatureKindID = addedItem.NomenclatureKindID,
                            StandardQuantity = standardQuantity?.StandardQuantity
                        });
                    }
                    else
                        item.QuantityIn = (item.QuantityIn ?? 0) + addedItem.Quantity;
                }

                OutProducts = new ItemsChangeObservableCollection<MovementProduct>(GammaBase.FillDocCloseShiftMovementProducts(PlaceID, ShiftID, CloseDate)
                            .Where(d => (bool)d.IsMovementOut && (bool)d.IsMaterial).Select(d => new MovementProduct
                            {
                                NomenclatureName = d.NomenclatureName,
                                Number = d.Number,
                                ProductId = d.ProductID,
                                Quantity = (d.Quantity ?? 0),
                                ProductKindName = d.ProductKindName,
                                OrderTypeName = d.OrderTypeName,
                                DocMovementId = d.DocMovementID,
                                InPlaceName = d.InPlace,
                                InPlaceZoneName = d.InPlaceZone,
                                InDate = d.InDate,
                                NomenclatureId = d.NomenclatureID,
                                CharacteristicId = d.CharacteristicID,
                                NomenclatureKindID = d.NomenclatureKindID,
                                MeasureUnit = d.MeasureUnit,
                                MeasureUnitID = d.MeasureUnitID
                            }));
                foreach (MovementProduct addedItem in OutProducts)
                {
                    var item = DocCloseShiftMaterials.FirstOrDefault(d => d.NomenclatureID == addedItem.NomenclatureId && (d.CharacteristicID == addedItem.CharacteristicId || (d.CharacteristicID == null && addedItem.CharacteristicId == null)));
                    if (item == null)
                    {
                        var standardQuantity = DocCloseShiftMaterials.Where(d => d.AvailableNomenclatures.Any(n => n.NomenclatureID == addedItem.NomenclatureId && (n.CharacteristicID == addedItem.CharacteristicId || (n.CharacteristicID == null && addedItem.CharacteristicId == null)))).FirstOrDefault();
                        DocCloseShiftMaterials.Add(new DocCloseShiftMaterial()
                        {
                            NomenclatureID = (Guid)addedItem.NomenclatureId,
                            CharacteristicID = addedItem.CharacteristicId,
                            NomenclatureName = addedItem.NomenclatureName,
                            QuantityIsReadOnly = true,
                            QuantityOut = addedItem.Quantity,
                            DocWithdrawalMaterialID = SqlGuidUtil.NewSequentialid(),
                            MeasureUnit = addedItem.MeasureUnit,
                            MeasureUnitID = addedItem.MeasureUnitID,
                            WithdrawByFact = true,
                            NomenclatureKindID = addedItem.NomenclatureKindID,
                            StandardQuantity = standardQuantity?.StandardQuantity
                        });
                    }
                    else
                        item.QuantityOut = (item.QuantityOut ?? 0) + addedItem.Quantity;
                }

                var PreviousDocCloseShift = GammaBase.Docs
                    .Where(d => d.DocTypeID == 3 && d.PlaceID == PlaceID && d.Date < CloseDate)
                    .OrderByDescending(d => d.Date)
                    .Select(d => d.DocID)
                    .FirstOrDefault();
                BeginProducts = new ObservableCollection<DocCloseShiftRemainder>(GammaBase.DocCloseShiftRemainders
                    .Where(d => (d.RemainderTypeID == 2 || (d.RemainderTypeID ?? 0) == 0) && d.DocCloseShifts.DocID == PreviousDocCloseShift && (d.IsMaterial ?? false))
                    .Select(d => new DocCloseShiftRemainder
                    {
                        ProductID = (Guid)d.ProductID,
                        StateID = d.StateID,
                        Quantity = d.Quantity,
                        RemainderTypeID = d.RemainderTypeID == 0 || d.RemainderTypeID == null ? 3 : 1,
                        ProductKind = d.Products == null ? new ProductKind() : (ProductKind)d.Products.ProductKindID
                    }));

                FillEndProducts(IsFillEnd);

                UtilizationProducts = new ObservableCollection<PaperBase>(GammaBase.FillDocCloseShiftUtilizationProducts(PlaceID, ShiftID, CloseDate)
                    .Where(p => (p.IsMaterial ?? true)).Select(p => new PaperBase()
                {
                    CharacteristicID = p.CharacteristicID,
                    NomenclatureID = p.NomenclatureID,
                    Nomenclature = p.NomenclatureName,
                    Number = p.Number,
                    ProductID = p.ProductID,
                    Weight = (p.Quantity ?? 0),// * 1000
                    BaseMeasureUnit = p.BaseMeasureUnit,
                    BaseMeasureUnitID = p.BaseMeasureUnitID ?? Guid.Empty
                }));

                foreach (PaperBase addedItem in UtilizationProducts)
                {
                    var item = DocCloseShiftMaterials.FirstOrDefault(d => d.NomenclatureID == addedItem.NomenclatureID && (d.CharacteristicID == addedItem.CharacteristicID || (d.CharacteristicID == null && addedItem.CharacteristicID == null)));
                    if (item == null)
                    {
                        var standardQuantity = DocCloseShiftMaterials.Where(d => d.AvailableNomenclatures.Any(n => n.NomenclatureID == addedItem.NomenclatureID && (n.CharacteristicID == addedItem.CharacteristicID || (n.CharacteristicID == null && addedItem.CharacteristicID == null)))).FirstOrDefault();
                        DocCloseShiftMaterials.Add(new DocCloseShiftMaterial()
                        {
                            NomenclatureID = (Guid)addedItem.NomenclatureID,
                            CharacteristicID = addedItem.CharacteristicID,
                            NomenclatureName = addedItem.Nomenclature,
                            QuantityIsReadOnly = true,
                            QuantityUtil = addedItem.Weight,
                            DocWithdrawalMaterialID = SqlGuidUtil.NewSequentialid(),
                            MeasureUnit = addedItem.BaseMeasureUnit,
                            MeasureUnitID = addedItem.BaseMeasureUnitID,
                            WithdrawByFact = true,
                            StandardQuantity = standardQuantity?.StandardQuantity
                        });
                    }
                    else
                        item.QuantityUtil = (item.QuantityUtil ?? 0) + addedItem.Weight;
                }

                var WithdrawalMaterialsLoad =
                        new ItemsChangeObservableCollection<WithdrawalMaterialBaseItem>(
                            GammaBase.FillDocCloseShiftMaterials(PlaceID, ShiftID, CloseDate)
                            //.Take(0)    
                            .Select(m => new WithdrawalMaterialBaseItem()
                            {
                                NomenclatureID = (Guid)m.NomenclatureID,
                                CharacteristicID = m.CharacteristicID,
                                NomenclatureName = m.NomenclatureName,
                                QuantityIsReadOnly = false, //!m.WithdrawByFact ?? true,
                                Quantity = m.Quantity ?? 0,
                                BaseQuantity = m.Quantity ?? 0,
                                //DocWithdrawalMaterialID = SqlGuidUtil.NewSequentialid(),
                                MeasureUnit = m.MeasureUnit,
                                MeasureUnitID = m.MeasureUnitID,
                                WithdrawByFact = m.WithdrawByFact ?? true
                            }).OrderBy(m => m.NomenclatureName));


                foreach (WithdrawalMaterialBaseItem addedItem in WithdrawalMaterialsLoad)
                {
                    var materialItem = DocCloseShiftMaterials.FirstOrDefault(d => d.NomenclatureID == addedItem.NomenclatureID && (d.CharacteristicID == addedItem.CharacteristicID || (d.CharacteristicID == null && addedItem.CharacteristicID == null)));
                    if (materialItem == null)
                    {
                        DocCloseShiftMaterials.Add(new DocCloseShiftMaterial()
                        {
                            NomenclatureID = addedItem.NomenclatureID,
                            CharacteristicID = addedItem.CharacteristicID,
                            NomenclatureName = addedItem.NomenclatureName,
                            QuantityIsReadOnly = addedItem.QuantityIsReadOnly,
                            DocWithdrawalMaterialID = SqlGuidUtil.NewSequentialid(),
                            MeasureUnit = addedItem.MeasureUnit,
                            MeasureUnitID = addedItem.MeasureUnitID,
                            WithdrawByFact = addedItem.WithdrawByFact,
                            StandardQuantity = addedItem.Quantity
                        });
                    }
                    else
                    {
                        materialItem.StandardQuantity = addedItem.Quantity;
                    };
                }

                var withdrawalMaterialsRemainderAtBegin =
                        new ItemsChangeObservableCollection<WithdrawalMaterialBaseItem>(
                            GammaBase.FillDocCloseShiftMaterialsAtBegin(PlaceID, ShiftID, CloseDate)
                            //.Take(0)    
                            .Select(m => new WithdrawalMaterialBaseItem()
                            {
                                NomenclatureID = (Guid)m.NomenclatureID,
                                CharacteristicID = m.CharacteristicID,
                                NomenclatureName = m.NomenclatureName,
                                QuantityIsReadOnly =  m.QuantityIsReadOnly ?? false,
                                Quantity = m.Quantity ?? 0,
                                BaseQuantity = m.Quantity ?? 0,
                                //DocWithdrawalMaterialID = SqlGuidUtil.NewSequentialid(),
                                MeasureUnit = m.MeasureUnit,
                                MeasureUnitID = m.MeasureUnitID,
                                WithdrawByFact = m.WithdrawByFact ?? true
                            }).OrderBy(m => m.NomenclatureName));

                foreach (WithdrawalMaterialBaseItem addedItem in withdrawalMaterialsRemainderAtBegin.Where(x => x.Quantity != 0))
                {
                    var item = DocCloseShiftMaterials.FirstOrDefault(d => d.NomenclatureID == addedItem.NomenclatureID && (d.CharacteristicID == addedItem.CharacteristicID || (d.CharacteristicID == null && addedItem.CharacteristicID == null)));
                    if (item == null)
                    {
                        var standardQuantity = DocCloseShiftMaterials.Where(d => d.AvailableNomenclatures.Any(n => n.NomenclatureID == addedItem.NomenclatureID && (n.CharacteristicID == addedItem.CharacteristicID || (n.CharacteristicID == null && addedItem.CharacteristicID == null)))).FirstOrDefault();
                        DocCloseShiftMaterials.Add(new DocCloseShiftMaterial()
                        {
                            NomenclatureID = (Guid)addedItem.NomenclatureID,
                            CharacteristicID = addedItem.CharacteristicID,
                            NomenclatureName = addedItem.NomenclatureName,
                            QuantityIsReadOnly = addedItem.QuantityIsReadOnly,
                            QuantityRemainderAtBegin = addedItem.Quantity,
                            DocWithdrawalMaterialID = SqlGuidUtil.NewSequentialid(),
                            MeasureUnit = addedItem.MeasureUnit,
                            MeasureUnitID = addedItem.MeasureUnitID,
                            WithdrawByFact = addedItem.WithdrawByFact,
                            NomenclatureKindID = addedItem.NomenclatureKindID,
                            StandardQuantity = standardQuantity?.StandardQuantity
                        });
                    }
                    else
                        item.QuantityRemainderAtBegin = addedItem.Quantity;
                }

                if (IsFillEnd)
                {
                    var withdrawalMaterialsRemainderAtEnd =
                            new ItemsChangeObservableCollection<WithdrawalMaterialBaseItem>(
                                GammaBase.FillDocCloseShiftMaterialsAtEnd(PlaceID, ShiftID, CloseDate)
                                //.Take(0)    
                                .Select(m => new WithdrawalMaterialBaseItem()
                                {
                                    NomenclatureID = (Guid)m.NomenclatureID,
                                    CharacteristicID = m.CharacteristicID,
                                    NomenclatureName = m.NomenclatureName,
                                    QuantityIsReadOnly = m.QuantityIsReadOnly ?? false,
                                    Quantity = m.Quantity ?? 0,
                                    BaseQuantity = m.Quantity ?? 0,
                                    //DocWithdrawalMaterialID = SqlGuidUtil.NewSequentialid(),
                                    MeasureUnit = m.MeasureUnit,
                                    MeasureUnitID = m.MeasureUnitID,
                                    WithdrawByFact = m.WithdrawByFact ?? true
                                }).OrderBy(m => m.NomenclatureName));

                    var docUnwinderRemainders = GammaBase.DocUnwinderRemainders.Where(r => r.DocWithdrawalID != null && r.Docs1.PlaceID == PlaceID && r.Docs1.ShiftID == ShiftID &&
                            r.Docs1.Date >= SqlFunctions.DateAdd("hh", -1, DB.GetShiftBeginTime((DateTime)SqlFunctions.DateAdd("hh", -1, CloseDate))) &&
                            r.Docs1.Date <= SqlFunctions.DateAdd("hh", 1, DB.GetShiftEndTime((DateTime)SqlFunctions.DateAdd("hh", -1, CloseDate))));

                    foreach (WithdrawalMaterialBaseItem addedItem in withdrawalMaterialsRemainderAtEnd.Where(x => x.Quantity != 0))
                    {
                        var endProducts = EndProducts.Where(p => p.CharacteristicID == addedItem.CharacteristicID).Select(p => p.ProductID).ToList();
                        //var spoolsUnwinderRemainder = endProducts == null || spoolUnwinderRemainders == null ? null : spoolUnwinderRemainders.SpoolRemainders.Where(s => endProducts.Contains((Guid)s.ProductID));
                        var spoolsUnwinderRemainder = docUnwinderRemainders.Where(r => endProducts.Contains((Guid)r.ProductID));
                        var endProductsQuantity = spoolsUnwinderRemainder == null ? 0 : EndProducts.Where(p => spoolsUnwinderRemainder.Select(s => s.ProductID).ToList().Contains((Guid) p.ProductID)).Sum(p => p.Quantity);
                        var spoolUnwinderRemainderQuantity = spoolsUnwinderRemainder?.Count() == 0 ? 0 : spoolsUnwinderRemainder?.Sum(s => s.Quantity) ?? 0;
                        var item = DocCloseShiftMaterials.FirstOrDefault(d => d.NomenclatureID == addedItem.NomenclatureID && (d.CharacteristicID == addedItem.CharacteristicID || (d.CharacteristicID == null && addedItem.CharacteristicID == null)));
                        if (item == null)
                        {
                            var standardQuantity = DocCloseShiftMaterials.Where(d => d.AvailableNomenclatures.Any(n => n.NomenclatureID == addedItem.NomenclatureID && (n.CharacteristicID == addedItem.CharacteristicID || (n.CharacteristicID == null && addedItem.CharacteristicID == null)))).FirstOrDefault();
                            DocCloseShiftMaterials.Add(new DocCloseShiftMaterial()
                            {
                                NomenclatureID = (Guid)addedItem.NomenclatureID,
                                CharacteristicID = addedItem.CharacteristicID,
                                NomenclatureName = addedItem.NomenclatureName,
                                QuantityIsReadOnly = addedItem.QuantityIsReadOnly,
                                QuantityRemainderAtEnd = addedItem.Quantity - endProductsQuantity + spoolUnwinderRemainderQuantity,
                                DocWithdrawalMaterialID = SqlGuidUtil.NewSequentialid(),
                                MeasureUnit = addedItem.MeasureUnit,
                                MeasureUnitID = addedItem.MeasureUnitID,
                                WithdrawByFact = addedItem.WithdrawByFact,
                                NomenclatureKindID = addedItem.NomenclatureKindID,
                                StandardQuantity = standardQuantity?.StandardQuantity
                            });
                        }
                        else
                        {
                            item.QuantityRemainderAtEnd = addedItem.Quantity - endProductsQuantity + spoolUnwinderRemainderQuantity;
                            item.QuantityIsReadOnly = addedItem.QuantityIsReadOnly;
                        }
                    }
                }

                MaterialsDismiss?.Clear();
                MaterialsDismiss =
                        new ItemsChangeObservableCollection<DocMaterialProductionDirectCalculationItem>(
                            GammaBase.FillDocCloseShiftMaterialsDismiss(PlaceID, ShiftID, CloseDate)
                           //.Take(0)    
                           .Select(m => new DocMaterialProductionDirectCalculationItem()
                           {
                               CharacteristicID = m.C1CCharacteristicID,
                               NomenclatureID = m.C1CNomenclatureID,
                               NomenclatureName = m.NomenclatureName,
                               QuantityRemainderAtBegin = m.QuantityRemainderAtBegin,
                               QuantityIn = m.QuantityIn,
                               QuantityOut = m.QuantityOut,
                               QuantityUtil = m.QuantityUtil,
                               QuantityExperimental = m.QuantityExperimental,
                               QuantityRemainderAtEnd = m.QuantityRemainderAtEnd,
                               QuantitySend = m.QuantitySend,
                               MeasureUnit = m.MeasureUnitName,
                               MeasureUnitID = m.C1CMeasureUnitID,
                               WithdrawByFact = m.WithdrawByFact,
                               QuantityIsReadOnly = m.QuantityIsReadOnly ?? false,
                               ParentID = m.ParentID,
                               ParentName = m.ParentName,
                               PlaceID = m.PlaceID ?? 0,
                               DocID = m.DocID,
                               DocNumberDate = m.DocNumberDate
                           }).OrderBy(m => m.NomenclatureName));

                foreach (DocCloseShiftMaterial addedItem in DocCloseShiftMaterials.Where(x => x.StandardQuantity == null || x.StandardQuantity == 0))
                {

                }
                    foreach (DocCloseShiftMaterial addedItem in DocCloseShiftMaterials.Where(x => x.StandardQuantity == null || x.StandardQuantity == 0 ))
                {
                    var standardQuantity = DocCloseShiftMaterials.Where(d => d.StandardQuantity > 0 && d.AvailableNomenclatures.Any(n => n.NomenclatureID == addedItem.NomenclatureID && (n.CharacteristicID == addedItem.CharacteristicID || (n.CharacteristicID == null && addedItem.CharacteristicID == null)))).FirstOrDefault();
                    if (standardQuantity != null)
                        addedItem.StandardQuantity = standardQuantity.StandardQuantity;
                }

                foreach (DocCloseShiftMaterial addedItem in DocCloseShiftMaterials.Where(x => ((x.WithdrawByFact ?? true) ? x.QuantityWithdrawalMaterial : x.StandardQuantity) != 0))
                {
                    if (WithdrawalMaterials.Count(d => d.NomenclatureID == addedItem.NomenclatureID && (d.CharacteristicID == addedItem.CharacteristicID || (d.CharacteristicID == null && addedItem.CharacteristicID == null))) == 0)
                        WithdrawalMaterials.Add(new WithdrawalMaterial(new ArrayList() { productionProductCharacteristicIDs, PlaceID })
                        {
                            NomenclatureID = addedItem.NomenclatureID,
                            CharacteristicID = addedItem.CharacteristicID,
                            NomenclatureName = addedItem.NomenclatureName,
                            QuantityIsReadOnly = addedItem.QuantityIsReadOnly,
                            Quantity = ((addedItem.WithdrawByFact ?? true) ? addedItem.QuantityWithdrawalMaterial : addedItem.StandardQuantity) ?? 0,
                            BaseQuantity = ((addedItem.WithdrawByFact ?? true) ? addedItem.QuantityWithdrawalMaterial : addedItem.StandardQuantity) ?? 0,
                            DocWithdrawalMaterialID = SqlGuidUtil.NewSequentialid(),
                            MeasureUnit = addedItem.MeasureUnit,
                            MeasureUnitID = addedItem.MeasureUnitID,
                            WithdrawByFact = addedItem.WithdrawByFact
                        });
                }
                
            }
        }

        public void Clear(bool IsFillEnd = true)
        {
            WithdrawalMaterials?.Clear();
            foreach (var item in DocCloseShiftMaterials)
            {
                if (item.QuantityIsReadOnly)
                {
                    item.QuantityIn = null;
                    item.QuantityOut = null;
                    item.QuantityUtil = null;
                    if (IsFillEnd) item.QuantityRemainderAtEnd = null;
                }
                item.StandardQuantity = null;
            }
            var items = DocCloseShiftMaterials?.Where(d => !(d.QuantityIn > 0 || d.QuantityOut > 0 || d.QuantityRePack > 0 || d.QuantityUtil > 0 || d.QuantityExperimental > 0 || d.QuantityRemainderAtEnd > 0)).ToArray();
            foreach (var item in items)
            {
                DocCloseShiftMaterials.Remove(item);
            }
            MaterialsDismiss?.Clear();
            /*if ((WithdrawalMaterialsIn?.Count(d => d.Quantity != 0) == 0 && WithdrawalMaterialsOut?.Count(d => d.Quantity != 0) == 0))
            {
                WithdrawalMaterialsIn?.Clear();
                WithdrawalMaterialsOut?.Clear();
                //MessageBox.Show("Внимание! Во вкладке Принято или Отдано есть строки с кол-вом больше 0. Загрузка материалов не произведена! Обнулите материалы в этих вкладках.", "Ошибка сохранения", MessageBoxButton.OK, MessageBoxImage.Asterisk);
            }
            WithdrawalMaterialsRemainderAtBegin?.Clear();
            if (WithdrawalMaterialsRemainderAtEnd?.Count(d => d.Quantity != 0) == 0)
                WithdrawalMaterialsRemainderAtEnd?.Clear();
                */
        }

        public void RecalcQuantityEndFromUnwinderReaminder (RecalcQuantityEndFromUnwinderReaminderMessage msg)
        {
            var item = DocCloseShiftMaterials.FirstOrDefault(d => d.NomenclatureID == msg.NomenclatureID && (d.CharacteristicID == msg.CharacteristicID || (d.CharacteristicID == null && msg.CharacteristicID == null)));
            if (item == null)
            {
                //Если в таблице материалов нет номенклатуры с раската, то новую номенклатуру не добавляем, так как она там джолжна быть по всем правилам. Может быть в будущем.
                //DocCloseShiftMaterials.Add(new DocCloseShiftMaterial()
                //{
                //    NomenclatureID = (Guid)addedItem.NomenclatureID,
                //    CharacteristicID = addedItem.CharacteristicID,
                //    NomenclatureName = addedItem.NomenclatureName,
                //    QuantityIsReadOnly = addedItem.QuantityIsReadOnly,
                //    QuantityRemainderAtEnd = addedItem.Quantity - endProductQuantity + spoolUnwinderRemainderQuantity,
                //    DocWithdrawalMaterialID = SqlGuidUtil.NewSequentialid(),
                //    MeasureUnit = addedItem.MeasureUnit,
                //    MeasureUnitID = addedItem.MeasureUnitID,
                //    WithdrawByFact = addedItem.WithdrawByFact,
                //    NomenclatureKindID = addedItem.NomenclatureKindID
                //});
            }
            else
            {
                /*var quantityEnd = (msg.Quantity == -1) ?
                         EndProducts.Where(p => p.NomenclatureID == msg.NomenclatureID && p.CharacteristicID == msg.CharacteristicID).Sum(p => p.Quantity) :
                         EndProducts.Where(p => p.ProductID != msg.ProductID && p.NomenclatureID == msg.NomenclatureID && p.CharacteristicID == msg.CharacteristicID).Sum(p => p.Quantity);
            */
                var quantityEndProduct = (msg.Quantity == -1 || msg.Quantity == -2) ? 
                    EndProducts.Where(p => p.ProductID == msg.ProductID).Select(s => s.Quantity).FirstOrDefault()
                    : 0 ;
                item.QuantityRemainderAtEnd = (item.QuantityRemainderAtEnd ?? 0) + (msg.Delta ?? 0) + (msg.Quantity == -1 ? quantityEndProduct : msg.Quantity == -2 ? -quantityEndProduct : 0);

                var n = new DocCloseShiftMaterial{ WithdrawByFact = false };
                DocCloseShiftMaterials.Add(n);
                DocCloseShiftMaterials.Remove(n);
            }
        }

        public void MaterialNomenclatureChanged(C1CNomenclature nomenclatureInfo)//, List<Guid> productionProductCharacteristicIDs)
        {
            var characteristicID = nomenclatureInfo.C1CCharacteristics.Select(x => x.C1CCharacteristicID).FirstOrDefault() == Guid.Empty ? (Guid?)null : nomenclatureInfo.C1CCharacteristics.Select(x => x.C1CCharacteristicID).FirstOrDefault();
            var nomenclatureName = nomenclatureInfo.Name + " " + nomenclatureInfo.C1CCharacteristics.Select(x => x.Name).FirstOrDefault();

            if (DocCloseShiftMaterials.FirstOrDefault(d => d.NomenclatureID == nomenclatureInfo.C1CNomenclatureID) == null)
                DocCloseShiftMaterials.Add(new DocCloseShiftMaterial
                {
                    NomenclatureID = nomenclatureInfo.C1CNomenclatureID,
                    CharacteristicID = characteristicID,
                    NomenclatureName = nomenclatureName,
                    QuantityIsReadOnly = false,
                    MeasureUnitID = nomenclatureInfo.C1CMeasureUnitStorage.C1CMeasureUnitID,
                    MeasureUnit = nomenclatureInfo.C1CMeasureUnitStorage.Name,
                    DocWithdrawalMaterialID = SqlGuidUtil.NewSequentialid(),
                    WithdrawByFact = true
                });

            /* if (WithdrawalMaterials.FirstOrDefault(d => d.NomenclatureID == nomenclatureInfo.C1CNomenclatureID) == null)
                 WithdrawalMaterials.Add(new WithdrawalMaterial(new ArrayList() { ProductionProductCharacteristicIDs, PlaceID })
                 {
                     NomenclatureID = nomenclatureInfo.C1CNomenclatureID,
                     CharacteristicID = characteristicID,
                     NomenclatureName = nomenclatureName,
                     QuantityIsReadOnly = false,
                     Quantity = 0,
                     MeasureUnitID = nomenclatureInfo.C1CMeasureUnitStorage.C1CMeasureUnitID,
                     MeasureUnit = nomenclatureInfo.C1CMeasureUnitStorage.Name,
                     DocWithdrawalMaterialID = SqlGuidUtil.NewSequentialid(),
                     WithdrawByFact = true
                 });*/
        }


        public void MaterialChanged(int selectedMaterialTabIndex, WithdrawalMaterial selectedMaterial)//, List<Guid> productionProductCharacteristicIDs)
        {
            var docCloseShiftMaterial = DocCloseShiftMaterials.Where(d => d.NomenclatureID == selectedMaterial?.NomenclatureID && (d.CharacteristicID == selectedMaterial?.CharacteristicID || (d.CharacteristicID == null && selectedMaterial?.CharacteristicID == null))).Sum(d => ((d.WithdrawByFact ?? true) ? d.QuantityWithdrawalMaterial : d.StandardQuantity)) ?? 0;
            var withdrawalMaterial = WithdrawalMaterials.FirstOrDefault(d => d.NomenclatureID == selectedMaterial?.NomenclatureID && (d.CharacteristicID == selectedMaterial?.CharacteristicID || (d.CharacteristicID == null && selectedMaterial?.CharacteristicID == null)));
            //productionProductCharacteristicIDs = new List<Guid>(Spools
            //        .Select(p => p.CharacteristicID).Distinct().ToList());
            if (selectedMaterialTabIndex != 1)
            {
                if (withdrawalMaterial == null)
                {
                    withdrawalMaterial = new WithdrawalMaterial(new ArrayList() { ProductionProductCharacteristicIDs, PlaceID })
                    {
                        NomenclatureID = selectedMaterial.NomenclatureID,
                        NomenclatureName = selectedMaterial.NomenclatureName,
                        CharacteristicID = selectedMaterial.CharacteristicID,
                        DocWithdrawalMaterialID = SqlGuidUtil.NewSequentialid(),
                        Quantity = docCloseShiftMaterial,
                        BaseQuantity = docCloseShiftMaterial,
                        MeasureUnit = selectedMaterial.MeasureUnit,
                        MeasureUnitID = selectedMaterial.MeasureUnitID,
                        WithdrawByFact = selectedMaterial.WithdrawByFact
                    };
                    WithdrawalMaterials.Add(withdrawalMaterial);
                }
                else
                {
                    withdrawalMaterial.Quantity = docCloseShiftMaterial;
                    withdrawalMaterial.BaseQuantity = docCloseShiftMaterial;

                }
                selectedMaterial = withdrawalMaterial;
                var items = WithdrawalMaterials.ToArray();
                foreach (var item in items)
                {
                    if (DocCloseShiftMaterials.Where(d => d.NomenclatureID == item.NomenclatureID && (d.CharacteristicID == item.CharacteristicID || (d.CharacteristicID == null && item.CharacteristicID == null))).Sum(d => d.QuantityWithdrawalMaterial) == 0)
                        WithdrawalMaterials.Remove(item);
                }
            }
            //else
            //{
            //    selectedMaterial = SelectedWithdrawalMaterial;
            //}
            var material = WithdrawalMaterials.Where(d => d.NomenclatureID == selectedMaterial?.NomenclatureID && (d.CharacteristicID == selectedMaterial?.CharacteristicID || (d.CharacteristicID == null && selectedMaterial?.CharacteristicID == null))).Sum(d => d.Quantity);
            var materialBase = WithdrawalMaterials.Where(d => d.NomenclatureID == selectedMaterial?.NomenclatureID && (d.CharacteristicID == selectedMaterial?.CharacteristicID || (d.CharacteristicID == null && selectedMaterial?.CharacteristicID == null))).Sum(d => d.BaseQuantity);
            if (selectedMaterial.Quantity == docCloseShiftMaterial)
            {
                var items = WithdrawalMaterials.Where(d => d.DocWithdrawalMaterialID != selectedMaterial?.DocWithdrawalMaterialID && d.NomenclatureID == selectedMaterial?.NomenclatureID && (d.CharacteristicID == selectedMaterial?.CharacteristicID || (d.CharacteristicID == null && selectedMaterial?.CharacteristicID == null))).ToArray();
                foreach (var item in items)
                {
                    WithdrawalMaterials.Remove(item);
                }
            }
            else
            {
                if (docCloseShiftMaterial - material != 0)
                    WithdrawalMaterials.Add(new WithdrawalMaterial(new ArrayList() { ProductionProductCharacteristicIDs, PlaceID })
                    {
                        NomenclatureID = selectedMaterial.NomenclatureID,
                        NomenclatureName = selectedMaterial.NomenclatureName,
                        CharacteristicID = selectedMaterial.CharacteristicID,
                        DocWithdrawalMaterialID = SqlGuidUtil.NewSequentialid(),
                        Quantity = docCloseShiftMaterial - material,
                        BaseQuantity = docCloseShiftMaterial - materialBase,
                        MeasureUnit = selectedMaterial.MeasureUnit,
                        MeasureUnitID = selectedMaterial.MeasureUnitID,
                        WithdrawByFact = selectedMaterial.WithdrawByFact
                    });
            }


            //RaisePropertiesChanged("DocCloseShiftWithdrawalMaterials.WithdrawalMaterials");
        }


    }
}
