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

namespace Gamma.Models
{
    public class DocMaterialProductionDirectCalculationMaterial : ViewModelBase
    {
        public DocMaterialProductionDirectCalculationMaterial(int placeID, int shiftID, DateTime closeDate)
        {
            GammaBase = DB.GammaDb;
            PlaceID = placeID;
            ShiftID = shiftID;
            CloseDate = closeDate;
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

        ////private ItemsChangeObservableCollection<WithdrawalMaterial> _withdrawalMaterials = new ItemsChangeObservableCollection<WithdrawalMaterial>();

        ////public ItemsChangeObservableCollection<WithdrawalMaterial> WithdrawalMaterials
        ////{
        ////    get { return _withdrawalMaterials; }
        ////    set
        ////    {
        ////        _withdrawalMaterials = value;
        ////        RaisePropertiesChanged("DocMaterialProductionDirectCalculationMaterials.WithdrawalMaterials");
        ////    }
        ////}

        private ItemsChangeObservableCollection<DocMaterialProductionDirectCalculationItem> _materials = new ItemsChangeObservableCollection<DocMaterialProductionDirectCalculationItem>();

        public ItemsChangeObservableCollection<DocMaterialProductionDirectCalculationItem> Materials
        {
            get { return _materials; }
            set
            {
                _materials = value;
                RaisePropertiesChanged("DirectCalculationMaterials.Materials");
            }
        }

        //public List<Doc> Docs { get; private set; }

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

        private ItemsChangeObservableCollection<PaperBase> _utilizationSpools = new ItemsChangeObservableCollection<PaperBase>();
        public ItemsChangeObservableCollection<PaperBase> UtilizationSpools
        {
            get { return _utilizationSpools; }
            private set
            {
                _utilizationSpools = value;
                RaisePropertiesChanged("UtilizationSpools");
            }
        }


        private List<Guid> ProductionProductCharacteristicIDs { get; set; }

        public void LoadProductionMaterials(Guid docId, List<Guid> productionProductCharacteristicIDs)
        {
            ProductionProductCharacteristicIDs = productionProductCharacteristicIDs;
            Materials = new ItemsChangeObservableCollection<DocMaterialProductionDirectCalculationItem>(GammaBase.vDocMaterialProductionDirectCalculations
                    .Where(dm => dm.DocID == docId)
                    .Select(d => new DocMaterialProductionDirectCalculationItem
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
                        ParentName = d.ParentName
                    }).OrderBy(d => d.NomenclatureName));

            //var doc = GammaBase.Docs.First(d => d.DocID == docId);
            //Docs = doc.DocMaterialProductDocs.Select(dc => new Doc() { DocID = dc.DocID, Date = dc.Date, DocTypeID = dc.DocTypeID, Number = dc.Number, Person = dc.Persons?.Name, Place = dc.Places?.Name, ShiftID = dc.ShiftID ?? 0, User = dc.Users?.Name, IsConfirmed = dc.IsConfirmed }).ToList();
            //IsVisibleColumnQunatityIn = Docs.Any(d => d.DocTypeID == (int)DocTypes.DocMaterialProduction);

            ////InProducts = new ItemsChangeObservableCollection<MovementProduct>(GammaBase.DocCloseShiftMovementProducts.Where(d => d.DocID == docId && (bool)d.IsMovementIn)
            ////        .Join(GammaBase.vProductsInfo, d => d.ProductID, p => p.ProductID
            ////        , (d, p) => new MovementProduct
            ////        {
            ////            NomenclatureName = p.NomenclatureName,
            ////            Number = d.Products.Number,
            ////            ProductId = d.ProductID,
            ////            Quantity = d.Quantity ?? 0,
            ////            ProductKindName = d.Products.ProductKinds.Name,
            ////            //OrderTypeName = p.OrderTypeName,
            ////            DocMovementId = d.DocMovementID,
            ////            OutPlaceName = d.MovementPlaceName,
            ////            OutPlaceZoneName = d.MovementPlaceZoneName,
            ////            OutDate = d.DateMovement,
            ////            NomenclatureId = p.C1CNomenclatureID,
            ////            CharacteristicId = p.C1CCharacteristicID,
            ////            NomenclatureKindID = p.NomenclatureKindID,
            ////            MeasureUnit = p.BaseMeasureUnit,
            ////            MeasureUnitID = p.BaseMeasureUnitID
            ////        }));

            ////OutProducts = new ItemsChangeObservableCollection<MovementProduct>(GammaBase.DocCloseShiftMovementProducts.Where(d => d.DocID == docId && !((bool)d.IsMovementIn))
            ////        .Join(GammaBase.vProductsInfo, d => d.ProductID, p => p.ProductID
            ////        , (d, p) => new MovementProduct
            ////        {
            ////            NomenclatureName = p.NomenclatureName,
            ////            Number = d.Products.Number,
            ////            ProductId = d.ProductID,
            ////            Quantity = d.Quantity ?? 0,
            ////            ProductKindName = d.Products.ProductKinds.Name,
            ////            //OrderTypeName = p.OrderTypeName,
            ////            DocMovementId = d.DocMovementID,
            ////            OutPlaceName = d.MovementPlaceName,
            ////            OutPlaceZoneName = d.MovementPlaceZoneName,
            ////            OutDate = d.DateMovement,
            ////            NomenclatureId = p.C1CNomenclatureID,
            ////            CharacteristicId = p.C1CCharacteristicID,
            ////            NomenclatureKindID = p.NomenclatureKindID,
            ////            MeasureUnit = p.BaseMeasureUnit,
            ////            MeasureUnitID = p.BaseMeasureUnitID
            ////        }));

            //////Получение списка утилизированных тамбуров
            ////UtilizationSpools = new ItemsChangeObservableCollection<PaperBase>(GammaBase.DocCloseShiftUtilizationProducts.Where(d => d.DocID == docId)
            ////    .Join(GammaBase.vProductsInfo, d => d.ProductID, p => p.ProductID
            ////    , (d, p) => new PaperBase()
            ////    {
            ////        CharacteristicID = (Guid)p.C1CCharacteristicID,
            ////        NomenclatureID = p.C1CNomenclatureID,
            ////        Nomenclature = p.NomenclatureName,
            ////        Number = p.Number,
            ////        ProductID = d.ProductID,
            ////        Weight = d.Quantity ?? 0
            ////    }));

            //Получение списка списанных материалов
            ////WithdrawalMaterials = new ItemsChangeObservableCollection<WithdrawalMaterial>(GammaBase.DocWithdrawalMaterials
            ////    //.Join(GammaBase.Docs, dm => dm.DocWithdrawal.Docs.DocID, dw => dw.DocCloseShift.DocID, (dm, dw) => new { })
            ////    .Where(dm => dm.DocWithdrawal.DocCloseShift.FirstOrDefault().DocID == docId)
            ////    .Select(d => new WithdrawalMaterial()
            ////    {
            ////        CharacteristicID = d.C1CCharacteristicID,
            ////        NomenclatureID = d.C1CNomenclatureID,
            ////        NomenclatureName = d.C1CNomenclature.Name + d.C1CCharacteristicID == null || d.C1CCharacteristicID == Guid.Empty ? string.Empty : " " + d.C1CCharacteristics.Name,
            ////        DocWithdrawalMaterialID = d.DocWithdrawalMaterialID,
            ////        Quantity = d.Quantity,
            ////        BaseQuantity = d.Quantity,
            ////        MeasureUnit = d.C1CNomenclature.C1CMeasureUnitStorage.Name,
            ////        MeasureUnitID = d.C1CNomenclature.C1CMeasureUnitStorage.C1CMeasureUnitID,
            ////        QuantityIsReadOnly = !d.WithdrawByFact ?? true,
            ////        ProductionProductCharacteristicID = d.DocWithdrawal.DocProduction.FirstOrDefault().DocProductionProducts.FirstOrDefault().C1CCharacteristicID,
            ////        WithdrawByFact = d.WithdrawByFact
            ////    }).OrderBy(d => d.NomenclatureName));
            ////foreach (var item in WithdrawalMaterials)
            ////{
            ////    item.PlaceID = PlaceID;
            ////    item.SetAvailableProductionProducts = productionProductCharacteristicIDs;

            ////}
        }


        public void SetProductionProductCharacteristics(List<Guid> productionProductCharacteristicIDs)
        {
            ProductionProductCharacteristicIDs = productionProductCharacteristicIDs;
        }

        public void FillProductionMaterials(List<Guid> productionProductCharacteristicIDs, bool IsFillEnd = true)
        {
            ProductionProductCharacteristicIDs = productionProductCharacteristicIDs;
            FillProductionMaterials(IsFillEnd);
        }

        public void FillProductionMaterials(bool IsFillEnd = true)
        {
            {
                Clear();

                bool isCompositionCalculationParameter = false;
                var docMaterialProductions = new ItemsChangeObservableCollection<DocMaterialProductionDirectCalculationItem>(Materials);
                Materials.Clear();
                foreach (var item in docMaterialProductions)
                {
                    Materials.Add(item);
                }
                var ids = String.Join(",", ProductionProductCharacteristicIDs?.ToArray());
                var MaterialProductionsLoad =
                        new ItemsChangeObservableCollection<DocMaterialProductionItem>(
                            GammaBase.FillDocMaterialProductions(PlaceID, ShiftID, CloseDate, isCompositionCalculationParameter, ids)
                            //.Where(m => (m.IsCompositionCalculation ?? false))    
                            .Select(m => new DocMaterialProductionItem()
                            {
                                NomenclatureID = (Guid)m.NomenclatureID,
                                CharacteristicID = m.CharacteristicID,
                                NomenclatureName = m.NomenclatureName,
                                QuantityIsReadOnly = false, //!m.WithdrawByFact ?? true,
                                StandardQuantity = m.Quantity ?? 0,
                                //BaseQuantity = m.Quantity ?? 0,
                                //DocMaterialProductionItemID = SqlGuidUtil.NewSequentialid(),
                                MeasureUnit = m.MeasureUnit,
                                MeasureUnitID = m.MeasureUnitID,
                                WithdrawByFact = m.WithdrawByFact ?? true,
                                ParentID = m.ParentID,
                                ParentName = m.ParentName
                            }).OrderBy(m => m.NomenclatureName));


                foreach (DocMaterialProductionItem addedItem in MaterialProductionsLoad)
                {
                    var materialItem = Materials.FirstOrDefault(d => d.NomenclatureID == addedItem.NomenclatureID && (d.CharacteristicID == addedItem.CharacteristicID || (d.CharacteristicID == null && addedItem.CharacteristicID == null)));
                    if (materialItem == null)
                    {
                        Materials.Add(new DocMaterialProductionDirectCalculationItem()
                        {
                            NomenclatureID = addedItem.NomenclatureID,
                            CharacteristicID = addedItem.CharacteristicID,
                            NomenclatureName = addedItem.NomenclatureName,
                            QuantityIsReadOnly = addedItem.QuantityIsReadOnly,
                            DocMaterialProductionItemID = SqlGuidUtil.NewSequentialid(),
                            MeasureUnit = addedItem.MeasureUnit,
                            MeasureUnitID = addedItem.MeasureUnitID,
                            WithdrawByFact = addedItem.WithdrawByFact,
                            StandardQuantity = addedItem.StandardQuantity,
                            ParentID = addedItem.ParentID,
                            ParentName = addedItem.ParentName
                        });
                    }
                    else
                    {
                        materialItem.StandardQuantity = addedItem.StandardQuantity;
                    };
                }
                
                var materialsRemainderAtBegin =
                        new ItemsChangeObservableCollection<WithdrawalMaterial>(
                            GammaBase.FillDocMaterialProductionsAtBegin(PlaceID, ShiftID, CloseDate, isCompositionCalculationParameter)
                            //.Take(0)    
                            .Select(m => new WithdrawalMaterial()
                            {
                                NomenclatureID = (Guid)m.NomenclatureID,
                                CharacteristicID = m.CharacteristicID,
                                NomenclatureName = m.NomenclatureName,
                                QuantityIsReadOnly = (m.QuantityIsReadOnly == 1),
                                Quantity = m.Quantity,
                                BaseQuantity = m.Quantity,
                                DocWithdrawalMaterialID = SqlGuidUtil.NewSequentialid(),
                                MeasureUnit = m.MeasureUnit,
                                MeasureUnitID = m.MeasureUnitID,
                                WithdrawByFact = m.WithdrawByFact
                            }).OrderBy(m => m.NomenclatureName));

                foreach (WithdrawalMaterial addedItem in materialsRemainderAtBegin.Where(x => x.Quantity != 0))
                {
                    var item = Materials.FirstOrDefault(d => d.NomenclatureID == addedItem.NomenclatureID && (d.CharacteristicID == addedItem.CharacteristicID || (d.CharacteristicID == null && addedItem.CharacteristicID == null)));
                    if (item == null)
                    {
                        var standardQuantity = Materials.Where(d => d.AvailableNomenclatures.Any(n => n.NomenclatureID == addedItem.NomenclatureID && (n.CharacteristicID == addedItem.CharacteristicID || (n.CharacteristicID == null && addedItem.CharacteristicID == null)))).FirstOrDefault();
                        Materials.Add(new DocMaterialProductionDirectCalculationItem()
                        {
                            NomenclatureID = (Guid)addedItem.NomenclatureID,
                            CharacteristicID = addedItem.CharacteristicID,
                            NomenclatureName = addedItem.NomenclatureName,
                            QuantityIsReadOnly = addedItem.QuantityIsReadOnly,
                            QuantityRemainderAtBegin = addedItem.Quantity,
                            DocMaterialProductionItemID = SqlGuidUtil.NewSequentialid(),
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
                
                //Guid? fromDocID = null;

                //if (Docs?.Where(d => d.DocTypeID == (int)DocTypes.DocMaterialProduction).FirstOrDefault() != null)
                //{
                //    fromDocID = Docs.FirstOrDefault(d => d.DocTypeID == (int)DocTypes.DocMaterialProduction).DocID;
                //}
                //else
                //{
                //    fromDocID = DB.GetDocMaterialInFromDocID(PlaceID, ShiftID, CloseDate);
                //    if (fromDocID != null)
                //    {
                //        var doc = GammaBase.Docs.Where(d => d.DocID == fromDocID).FirstOrDefault();
                //        if (doc != null)
                //        {
                //            //var addedDoc = new Doc(doc.DocID, doc.Number, doc.Date, (doc.ShiftID ?? 0), doc.Places.Name, doc.Users.Name, doc.Persons.Name, doc.IsConfirmed, doc.DocTypeID);
                //            var addedDoc = new Doc();
                //            addedDoc.DocID = doc.DocID;
                //            addedDoc.Number = doc.Number;
                //            addedDoc.Date = doc.Date;
                //            addedDoc.ShiftID = doc.ShiftID ?? 0;
                //            addedDoc.Place = doc.Places?.Name;
                //            addedDoc.User = doc.Users?.Name;
                //            addedDoc.Person = doc.Persons?.Name;
                //            addedDoc.IsConfirmed = doc.IsConfirmed;
                //            addedDoc.DocTypeID = doc.DocTypeID;
                //            if (Docs == null)
                //                Docs = new List<Doc>() { addedDoc };
                //            else
                //                Docs.Add(addedDoc);
                //            //Docs.Add(new Doc { DocID = doc.DocID, Date = doc.Date, DocTypeID = doc.DocTypeID, Number = doc.Number, Person = doc.Persons.Name, Place = doc.Places.Name, ShiftID = doc.ShiftID ?? 0, User = doc.Users.Name, IsConfirmed = doc.IsConfirmed } );
                //        }
                //    }
                //}
                //if (fromDocID != null && fromDocID != Guid.Empty)
                //{
                //    //IsVisibleColumnQunatityIn = true;
                //    var materialsIn =
                //            new ItemsChangeObservableCollection<WithdrawalMaterial>(
                //                GammaBase.FillDocMaterialProductionsIn(PlaceID, ShiftID, CloseDate, isCompositionCalculationParameter, fromDocID)
                //                //.Take(0)    
                //                .Select(m => new WithdrawalMaterial()
                //                {
                //                    NomenclatureID = (Guid)m.NomenclatureID,
                //                    CharacteristicID = m.CharacteristicID,
                //                    NomenclatureName = m.NomenclatureName,
                //                    QuantityIsReadOnly = (m.QuantityIsReadOnly == 1),
                //                    Quantity = m.Quantity,
                //                    BaseQuantity = m.Quantity,
                //                    DocWithdrawalMaterialID = SqlGuidUtil.NewSequentialid(),
                //                    MeasureUnit = m.MeasureUnit,
                //                    MeasureUnitID = m.MeasureUnitID,
                //                    WithdrawByFact = m.WithdrawByFact
                //                }).OrderBy(m => m.NomenclatureName));

                //    foreach (WithdrawalMaterial addedItem in materialsIn.Where(x => x.Quantity != 0))
                //    {
                //        var item = Materials.FirstOrDefault(d => d.NomenclatureID == addedItem.NomenclatureID && (d.CharacteristicID == addedItem.CharacteristicID || (d.CharacteristicID == null && addedItem.CharacteristicID == null)));
                //        if (item == null)
                //        {
                //            var standardQuantity = Materials.Where(d => d.AvailableNomenclatures.Any(n => n.NomenclatureID == addedItem.NomenclatureID && (n.CharacteristicID == addedItem.CharacteristicID || (n.CharacteristicID == null && addedItem.CharacteristicID == null)))).FirstOrDefault();
                //            Materials.Add(new DocMaterialProductionDirectCalculationItem()
                //            {
                //                NomenclatureID = (Guid)addedItem.NomenclatureID,
                //                CharacteristicID = addedItem.CharacteristicID,
                //                NomenclatureName = addedItem.NomenclatureName,
                //                QuantityIsReadOnly = addedItem.QuantityIsReadOnly,
                //                QuantityIn = addedItem.Quantity,
                //                DocMaterialProductionItemID = SqlGuidUtil.NewSequentialid(),
                //                MeasureUnit = addedItem.MeasureUnit,
                //                MeasureUnitID = addedItem.MeasureUnitID,
                //                WithdrawByFact = addedItem.WithdrawByFact,
                //                NomenclatureKindID = addedItem.NomenclatureKindID,
                //                StandardQuantity = standardQuantity?.StandardQuantity
                //            });
                //        }
                //        else
                //            item.QuantityIn = addedItem.Quantity;
                //    }
                //}

                ////InProducts = new ItemsChangeObservableCollection<MovementProduct>(GammaBase.FillDocCloseShiftMovementProducts(PlaceID, ShiftID, CloseDate)
                ////                .Where(d => (bool)d.IsMovementIn).Select(d => new MovementProduct
                ////                {
                ////                    NomenclatureName = d.NomenclatureName,
                ////                    Number = d.Number,
                ////                    ProductId = d.ProductID,
                ////                    Quantity = (d.Quantity ?? 0),
                ////                    ProductKindName = d.ProductKindName,
                ////                    OrderTypeName = d.OrderTypeName,
                ////                    DocMovementId = d.DocMovementID,
                ////                    OutPlaceName = d.OutPlace,
                ////                    OutPlaceZoneName = d.OutPlaceZone,
                ////                    OutDate = d.OutDate,
                ////                    NomenclatureId = d.NomenclatureID,
                ////                    CharacteristicId = d.CharacteristicID,
                ////                    NomenclatureKindID = d.NomenclatureKindID,
                ////                    MeasureUnit = d.MeasureUnit,
                ////                    MeasureUnitID = d.MeasureUnitID
                ////                }));
                ////foreach (MovementProduct addedItem in InProducts)
                ////{
                ////    var item = Materials.FirstOrDefault(d => d.NomenclatureID == addedItem.NomenclatureId && (d.CharacteristicID == addedItem.CharacteristicId || (d.CharacteristicID == null && addedItem.CharacteristicId == null)));
                ////    if (item == null)
                ////        Materials.Add(new DocMaterialProductionDirectCalculationItem()
                ////        {
                ////            NomenclatureID = (Guid)addedItem.NomenclatureId,
                ////            CharacteristicID = addedItem.CharacteristicId,
                ////            NomenclatureName = addedItem.NomenclatureName,
                ////            QuantityIsReadOnly = true,
                ////            QuantityIn = addedItem.Quantity,
                ////            DocMaterialProductionItemID = SqlGuidUtil.NewSequentialid(),
                ////            MeasureUnit = addedItem.MeasureUnit,
                ////            MeasureUnitID = addedItem.MeasureUnitID,
                ////            WithdrawByFact = true,
                ////            NomenclatureKindID = addedItem.NomenclatureKindID
                ////        });
                ////    else
                ////        item.QuantityIn = (item.QuantityIn ?? 0) + addedItem.Quantity;
                ////}

                ////OutProducts = new ItemsChangeObservableCollection<MovementProduct>(GammaBase.FillDocCloseShiftMovementProducts(PlaceID, ShiftID, CloseDate)
                ////            .Where(d => (bool)d.IsMovementOut).Select(d => new MovementProduct
                ////            {
                ////                NomenclatureName = d.NomenclatureName,
                ////                Number = d.Number,
                ////                ProductId = d.ProductID,
                ////                Quantity = (d.Quantity ?? 0),
                ////                ProductKindName = d.ProductKindName,
                ////                OrderTypeName = d.OrderTypeName,
                ////                DocMovementId = d.DocMovementID,
                ////                InPlaceName = d.InPlace,
                ////                InPlaceZoneName = d.InPlaceZone,
                ////                InDate = d.InDate,
                ////                NomenclatureId = d.NomenclatureID,
                ////                CharacteristicId = d.CharacteristicID,
                ////                NomenclatureKindID = d.NomenclatureKindID,
                ////                MeasureUnit = d.MeasureUnit,
                ////                MeasureUnitID = d.MeasureUnitID
                ////            }));
                ////foreach (MovementProduct addedItem in OutProducts)
                ////{
                ////    var item = Materials.FirstOrDefault(d => d.NomenclatureID == addedItem.NomenclatureId && (d.CharacteristicID == addedItem.CharacteristicId || (d.CharacteristicID == null && addedItem.CharacteristicId == null)));
                ////    if (item == null)
                ////        Materials.Add(new DocMaterialProductionDirectCalculationItem()
                ////        {
                ////            NomenclatureID = (Guid)addedItem.NomenclatureId,
                ////            CharacteristicID = addedItem.CharacteristicId,
                ////            NomenclatureName = addedItem.NomenclatureName,
                ////            QuantityIsReadOnly = true,
                ////            QuantityOut = addedItem.Quantity,
                ////            DocMaterialProductionItemID = SqlGuidUtil.NewSequentialid(),
                ////            MeasureUnit = addedItem.MeasureUnit,
                ////            MeasureUnitID = addedItem.MeasureUnitID,
                ////            WithdrawByFact = true,
                ////            NomenclatureKindID = addedItem.NomenclatureKindID
                ////        });
                ////    else
                ////        item.QuantityOut = (item.QuantityOut ?? 0) + addedItem.Quantity;
                ////}

                ////UtilizationSpools = new ItemsChangeObservableCollection<PaperBase>(GammaBase.FillDocCloseShiftUtilizationSpools(PlaceID, ShiftID, CloseDate).Select(p => new PaperBase()
                ////{
                ////    CharacteristicID = p.CharacteristicID,
                ////    NomenclatureID = p.NomenclatureID,
                ////    Nomenclature = p.NomenclatureName,
                ////    Number = p.Number,
                ////    ProductID = p.ProductID,
                ////    Weight = (p.Quantity ?? 0),// * 1000
                ////    BaseMeasureUnit = p.BaseMeasureUnit,
                ////    BaseMeasureUnitID = p.BaseMeasureUnitID ?? Guid.Empty
                ////}));

                ////foreach (PaperBase addedItem in UtilizationSpools)
                ////{
                ////    var item = Materials.FirstOrDefault(d => d.NomenclatureID == addedItem.NomenclatureID && (d.CharacteristicID == addedItem.CharacteristicID || (d.CharacteristicID == null && addedItem.CharacteristicID == null)));
                ////    if (item == null)
                ////        Materials.Add(new DocMaterialProductionDirectCalculationItem()
                ////        {
                ////            NomenclatureID = (Guid)addedItem.NomenclatureID,
                ////            CharacteristicID = addedItem.CharacteristicID,
                ////            NomenclatureName = addedItem.Nomenclature,
                ////            QuantityIsReadOnly = true,
                ////            QuantityUtil = addedItem.Weight,
                ////            DocWithdrawalMaterialID = SqlGuidUtil.NewSequentialid(),
                ////            MeasureUnit = addedItem.BaseMeasureUnit,
                ////            MeasureUnitID = addedItem.BaseMeasureUnitID,
                ////            WithdrawByFact = true
                ////        });
                ////    else
                ////        item.QuantityUtil = (item.QuantityUtil ?? 0) + addedItem.Weight;
                ////}

                //var WithdrawalMaterialsLoad =
                //        new ItemsChangeObservableCollection<WithdrawalMaterial>(
                //            GammaBase.FillDocCloseShiftMaterials(PlaceID, ShiftID, CloseDate)
                //            //.Take(0)    
                //            .Select(m => new WithdrawalMaterial()
                //            {
                //                NomenclatureID = (Guid)m.NomenclatureID,
                //                CharacteristicID = m.CharacteristicID,
                //                NomenclatureName = m.NomenclatureName,
                //                QuantityIsReadOnly = false, //!m.WithdrawByFact ?? true,
                //                Quantity = m.Quantity ?? 0,
                //                BaseQuantity = m.Quantity ?? 0,
                //                DocWithdrawalMaterialID = SqlGuidUtil.NewSequentialid(),
                //                MeasureUnit = m.MeasureUnit,
                //                MeasureUnitID = m.MeasureUnitID,
                //                WithdrawByFact = m.WithdrawByFact ?? true
                //            }).OrderBy(m => m.NomenclatureName));


                //foreach (WithdrawalMaterial addedItem in WithdrawalMaterialsLoad)
                //{
                //    if (Materials.Count(d => d.NomenclatureID == addedItem.NomenclatureID && (d.CharacteristicID == addedItem.CharacteristicID || (d.CharacteristicID == null && addedItem.CharacteristicID == null))) == 0)
                //        Materials.Add(new DocMaterialProductionDirectCalculationItem()
                //        {
                //            NomenclatureID = addedItem.NomenclatureID,
                //            CharacteristicID = addedItem.CharacteristicID,
                //            NomenclatureName = addedItem.NomenclatureName,
                //            QuantityIsReadOnly = addedItem.QuantityIsReadOnly,
                //            DocMaterialProductionItemID = SqlGuidUtil.NewSequentialid(),
                //            MeasureUnit = addedItem.MeasureUnit,
                //            MeasureUnitID = addedItem.MeasureUnitID,
                //            WithdrawByFact = addedItem.WithdrawByFact
                //        });
                //}

                //var withdrawalMaterialsRemainderAtBegin =
                //        new ItemsChangeObservableCollection<WithdrawalMaterial>(
                //            GammaBase.FillDocCloseShiftMaterialsAtBegin(PlaceID, ShiftID, CloseDate)
                //            //.Take(0)    
                //            .Select(m => new WithdrawalMaterial()
                //            {
                //                NomenclatureID = (Guid)m.NomenclatureID,
                //                CharacteristicID = m.CharacteristicID,
                //                NomenclatureName = m.NomenclatureName,
                //                //QuantityIsReadOnly =  m.QuantityIsReadOnly,
                //                Quantity = m.Quantity ?? 0,
                //                BaseQuantity = m.Quantity ?? 0,
                //                DocWithdrawalMaterialID = SqlGuidUtil.NewSequentialid(),
                //                MeasureUnit = m.MeasureUnit,
                //                MeasureUnitID = m.MeasureUnitID,
                //                WithdrawByFact = m.WithdrawByFact ?? true
                //            }).OrderBy(m => m.NomenclatureName));

                //foreach (WithdrawalMaterial addedItem in withdrawalMaterialsRemainderAtBegin.Where(x => x.Quantity != 0))
                //{
                //    var item = Materials.FirstOrDefault(d => d.NomenclatureID == addedItem.NomenclatureID && (d.CharacteristicID == addedItem.CharacteristicID || (d.CharacteristicID == null && addedItem.CharacteristicID == null)));
                //    if (item == null)
                //        Materials.Add(new DocMaterialProductionDirectCalculationItem()
                //        {
                //            NomenclatureID = (Guid)addedItem.NomenclatureID,
                //            CharacteristicID = addedItem.CharacteristicID,
                //            NomenclatureName = addedItem.NomenclatureName,
                //            QuantityIsReadOnly = addedItem.QuantityIsReadOnly,
                //            QuantityRemainderAtBegin = addedItem.Quantity,
                //            DocMaterialProductionItemID = SqlGuidUtil.NewSequentialid(),
                //            MeasureUnit = addedItem.MeasureUnit,
                //            MeasureUnitID = addedItem.MeasureUnitID,
                //            WithdrawByFact = addedItem.WithdrawByFact,
                //            NomenclatureKindID = addedItem.NomenclatureKindID
                //        });
                //    else
                //        item.QuantityRemainderAtBegin = addedItem.Quantity;
                //}

                //var withdrawalMaterialsRemainderAtEnd =
                //        new ItemsChangeObservableCollection<WithdrawalMaterial>(
                //            GammaBase.FillDocCloseShiftMaterialsAtEnd(PlaceID, ShiftID, CloseDate)
                //            //.Take(0)    
                //            .Select(m => new WithdrawalMaterial()
                //            {
                //                NomenclatureID = (Guid)m.NomenclatureID,
                //                CharacteristicID = m.CharacteristicID,
                //                NomenclatureName = m.NomenclatureName,
                //                //QuantityIsReadOnly = true, //!m.WithdrawByFact ?? true,
                //                Quantity = m.Quantity ?? 0,
                //                BaseQuantity = m.Quantity ?? 0,
                //                DocWithdrawalMaterialID = SqlGuidUtil.NewSequentialid(),
                //                MeasureUnit = m.MeasureUnit,
                //                MeasureUnitID = m.MeasureUnitID,
                //                WithdrawByFact = m.WithdrawByFact ?? true
                //            }).OrderBy(m => m.NomenclatureName));

                //foreach (WithdrawalMaterial addedItem in withdrawalMaterialsRemainderAtEnd.Where(x => x.Quantity != 0))
                //{
                //    var item = Materials.FirstOrDefault(d => d.NomenclatureID == addedItem.NomenclatureID && (d.CharacteristicID == addedItem.CharacteristicID || (d.CharacteristicID == null && addedItem.CharacteristicID == null)));
                //    if (item == null)
                //        Materials.Add(new DocMaterialProductionDirectCalculationItem()
                //        {
                //            NomenclatureID = (Guid)addedItem.NomenclatureID,
                //            CharacteristicID = addedItem.CharacteristicID,
                //            NomenclatureName = addedItem.NomenclatureName,
                //            QuantityIsReadOnly = addedItem.QuantityIsReadOnly,
                //            QuantityRemainderAtEnd = addedItem.Quantity,
                //            DocMaterialProductionItemID = SqlGuidUtil.NewSequentialid(),
                //            MeasureUnit = addedItem.MeasureUnit,
                //            MeasureUnitID = addedItem.MeasureUnitID,
                //            WithdrawByFact = addedItem.WithdrawByFact,
                //            NomenclatureKindID = addedItem.NomenclatureKindID
                //        });
                //    else
                //        item.QuantityRemainderAtEnd = addedItem.Quantity;
                //}

                ////foreach (DocCloseShiftMaterial addedItem in DocCloseShiftMaterials.Where(x => x.QuantityWithdrawalMaterial != 0))
                ////{
                ////    if (WithdrawalMaterials.Count(d => d.NomenclatureID == addedItem.NomenclatureID && (d.CharacteristicID == addedItem.CharacteristicID || (d.CharacteristicID == null && addedItem.CharacteristicID == null))) == 0)
                ////        WithdrawalMaterials.Add(new WithdrawalMaterial(new ArrayList() { productionProductCharacteristicIDs, PlaceID })
                ////        {
                ////            NomenclatureID = addedItem.NomenclatureID,
                ////            CharacteristicID = addedItem.CharacteristicID,
                ////            NomenclatureName = addedItem.NomenclatureName,
                ////            QuantityIsReadOnly = addedItem.QuantityIsReadOnly,
                ////            Quantity = addedItem.QuantityWithdrawalMaterial ?? 0,
                ////            BaseQuantity = addedItem.QuantityWithdrawalMaterial ?? 0,
                ////            DocWithdrawalMaterialID = SqlGuidUtil.NewSequentialid(),
                ////            MeasureUnit = addedItem.MeasureUnit,
                ////            MeasureUnitID = addedItem.MeasureUnitID,
                ////            WithdrawByFact = addedItem.WithdrawByFact
                ////        });
                ////}


            }
        }

        public void ClearFromButton(bool IsFillEnd = true)
        {
            //Docs.Clear();
            //foreach (var item in Materials)
            //{
            //    item.QuantityIn = null;
            //}

            Clear(IsFillEnd);
        }

        public void Clear(bool IsFillEnd = true)
        {
            foreach (var item in Materials)
            {
                //if (item.QuantityIsReadOnly)
                //{
                //    item.QuantityIn = null;
                //    //item.QuantityOut = null;
                //    //item.QuantityUtil = null;
                //}
                //if (IsFillEnd) item.QuantityRemainderAtEnd = null;
                item.StandardQuantity = null;
            }
            var items = Materials?.Where(d => !(d.QuantityIn > 0 || d.QuantityOut > 0 || d.QuantityUtil > 0 || d.QuantityExperimental > 0 || d.QuantityRemainderAtEnd > 0)).ToArray();
            foreach (var item in items)
            {
                Materials.Remove(item);
            }
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


        public void MaterialNomenclatureChanged(C1CNomenclature nomenclatureInfo)//, List<Guid> productionProductCharacteristicIDs)
        {
            var characteristicID = nomenclatureInfo.C1CCharacteristics.Select(x => x.C1CCharacteristicID).FirstOrDefault() == Guid.Empty ? (Guid?)null : nomenclatureInfo.C1CCharacteristics.Select(x => x.C1CCharacteristicID).FirstOrDefault();
            var nomenclatureName = nomenclatureInfo.Name + " " + nomenclatureInfo.C1CCharacteristics.Select(x => x.Name).FirstOrDefault();

            if (Materials.FirstOrDefault(d => d.NomenclatureID == nomenclatureInfo.C1CNomenclatureID) == null)
                Materials.Add(new DocMaterialProductionDirectCalculationItem
                {
                    NomenclatureID = nomenclatureInfo.C1CNomenclatureID,
                    CharacteristicID = characteristicID,
                    NomenclatureName = nomenclatureName,
                    QuantityIsReadOnly = false,
                    MeasureUnitID = nomenclatureInfo.C1CMeasureUnitStorage.C1CMeasureUnitID,
                    MeasureUnit = nomenclatureInfo.C1CMeasureUnitStorage.Name,
                    DocMaterialProductionItemID = SqlGuidUtil.NewSequentialid(),
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


        ////public void MaterialChanged(int selectedMaterialTabIndex, WithdrawalMaterial selectedMaterial)//, List<Guid> productionProductCharacteristicIDs)
        ////{
        ////    var docCloseShiftMaterial = DocCloseShiftMaterials.Where(d => d.NomenclatureID == selectedMaterial?.NomenclatureID && (d.CharacteristicID == selectedMaterial?.CharacteristicID || (d.CharacteristicID == null && selectedMaterial?.CharacteristicID == null))).Sum(d => d.QuantityWithdrawalMaterial) ?? 0;
            ////var withdrawalMaterial = WithdrawalMaterials.FirstOrDefault(d => d.NomenclatureID == selectedMaterial?.NomenclatureID && (d.CharacteristicID == selectedMaterial?.CharacteristicID || (d.CharacteristicID == null && selectedMaterial?.CharacteristicID == null)));
            //////productionProductCharacteristicIDs = new List<Guid>(Spools
            //////        .Select(p => p.CharacteristicID).Distinct().ToList());
            ////if (selectedMaterialTabIndex != 1)
            ////{
            ////    if (withdrawalMaterial == null)
            ////    {
            ////        withdrawalMaterial = new WithdrawalMaterial(new ArrayList() { ProductionProductCharacteristicIDs, PlaceID })
            ////        {
            ////            NomenclatureID = selectedMaterial.NomenclatureID,
            ////            NomenclatureName = selectedMaterial.NomenclatureName,
            ////            CharacteristicID = selectedMaterial.CharacteristicID,
            ////            DocWithdrawalMaterialID = SqlGuidUtil.NewSequentialid(),
            ////            Quantity = docCloseShiftMaterial,
            ////            BaseQuantity = docCloseShiftMaterial,
            ////            MeasureUnit = selectedMaterial.MeasureUnit,
            ////            MeasureUnitID = selectedMaterial.MeasureUnitID,
            ////            WithdrawByFact = selectedMaterial.WithdrawByFact
            ////        };
            ////        WithdrawalMaterials.Add(withdrawalMaterial);
            ////    }
            ////    else
            ////    {
            ////        withdrawalMaterial.Quantity = docCloseShiftMaterial;
            ////        withdrawalMaterial.BaseQuantity = docCloseShiftMaterial;

            ////    }
            ////    selectedMaterial = withdrawalMaterial;
            ////    var items = WithdrawalMaterials.ToArray();
            ////    foreach (var item in items)
            ////    {
            ////        if (DocCloseShiftMaterials.Where(d => d.NomenclatureID == item.NomenclatureID && (d.CharacteristicID == item.CharacteristicID || (d.CharacteristicID == null && item.CharacteristicID == null))).Sum(d => d.QuantityWithdrawalMaterial) == 0)
            ////            WithdrawalMaterials.Remove(item);
            ////    }
            ////}
            //////else
            //////{
            //////    selectedMaterial = SelectedWithdrawalMaterial;
            //////}
            ////var material = WithdrawalMaterials.Where(d => d.NomenclatureID == selectedMaterial?.NomenclatureID && (d.CharacteristicID == selectedMaterial?.CharacteristicID || (d.CharacteristicID == null && selectedMaterial?.CharacteristicID == null))).Sum(d => d.Quantity);
            ////var materialBase = WithdrawalMaterials.Where(d => d.NomenclatureID == selectedMaterial?.NomenclatureID && (d.CharacteristicID == selectedMaterial?.CharacteristicID || (d.CharacteristicID == null && selectedMaterial?.CharacteristicID == null))).Sum(d => d.BaseQuantity);
            ////if (selectedMaterial.Quantity == docCloseShiftMaterial)
            ////{
            ////    var items = WithdrawalMaterials.Where(d => d.DocWithdrawalMaterialID != selectedMaterial?.DocWithdrawalMaterialID && d.NomenclatureID == selectedMaterial?.NomenclatureID && (d.CharacteristicID == selectedMaterial?.CharacteristicID || (d.CharacteristicID == null && selectedMaterial?.CharacteristicID == null))).ToArray();
            ////    foreach (var item in items)
            ////    {
            ////        WithdrawalMaterials.Remove(item);
            ////    }
            ////}
            ////else
            ////{
            ////    if (docCloseShiftMaterial - material != 0)
            ////        WithdrawalMaterials.Add(new WithdrawalMaterial(new ArrayList() { ProductionProductCharacteristicIDs, PlaceID })
            ////        {
            ////            NomenclatureID = selectedMaterial.NomenclatureID,
            ////            NomenclatureName = selectedMaterial.NomenclatureName,
            ////            CharacteristicID = selectedMaterial.CharacteristicID,
            ////            DocWithdrawalMaterialID = SqlGuidUtil.NewSequentialid(),
            ////            Quantity = docCloseShiftMaterial - material,
            ////            BaseQuantity = docCloseShiftMaterial - materialBase,
            ////            MeasureUnit = selectedMaterial.MeasureUnit,
            ////            MeasureUnitID = selectedMaterial.MeasureUnitID,
            ////            WithdrawByFact = selectedMaterial.WithdrawByFact
            ////        });
            ////}


            ////RaisePropertiesChanged("DocMaterialProductionDirectCalculationMaterials.WithdrawalMaterials");
        ////}


    }
}
