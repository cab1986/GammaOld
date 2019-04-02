// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Linq;
using DevExpress.Mvvm;
using Gamma.Entities;
using Gamma.Common;
using System.Collections;
using System.Collections.Generic;

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

        private ItemsChangeObservableCollection<WithdrawalMaterial> _withdrawalMaterialsIn = new ItemsChangeObservableCollection<WithdrawalMaterial>();

        public ItemsChangeObservableCollection<WithdrawalMaterial> WithdrawalMaterialsIn
        {
            get { return _withdrawalMaterialsIn; }
            set
            {
                _withdrawalMaterialsIn = value;
                RaisePropertiesChanged("DocCloseShiftWithdrawalMaterials.WithdrawalMaterialsIn");
            }
        }

        private ItemsChangeObservableCollection<WithdrawalMaterial> _withdrawalMaterialsOut = new ItemsChangeObservableCollection<WithdrawalMaterial>();

        public ItemsChangeObservableCollection<WithdrawalMaterial> WithdrawalMaterialsOut
        {
            get { return _withdrawalMaterialsOut; }
            set
            {
                _withdrawalMaterialsOut = value;
                RaisePropertiesChanged("DocCloseShiftWithdrawalMaterials.WithdrawalMaterialsOut");
            }
        }

        private ItemsChangeObservableCollection<WithdrawalMaterial> _withdrawalMaterialsRemainder = new ItemsChangeObservableCollection<WithdrawalMaterial>();

        public ItemsChangeObservableCollection<WithdrawalMaterial> WithdrawalMaterialsRemainder
        {
            get { return _withdrawalMaterialsRemainder; }
            set
            {
                _withdrawalMaterialsRemainder = value;
                RaisePropertiesChanged("DocCloseShiftWithdrawalMaterials.WithdrawalMaterialsRemainder");
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

        private List<Guid> ProductionProductCharacteristicIDs { get; set; }

        public void LoadWithdrawalMaterials(Guid docId, List<Guid> productionProductCharacteristicIDs)
        {
            ProductionProductCharacteristicIDs = productionProductCharacteristicIDs;
            WithdrawalMaterialsIn = new ItemsChangeObservableCollection<WithdrawalMaterial>(GammaBase.DocCloseShiftMaterials
                    .Where(dm => dm.DocID == docId && dm.DocCloseShiftMaterialTypeID == 1)
                    .Select(d => new WithdrawalMaterial
                    {
                        CharacteristicID = d.C1CCharacteristicID,
                        NomenclatureID = d.C1CNomenclatureID,
                        NomenclatureName = d.C1CNomenclature.Name,
                        DocWithdrawalMaterialID = d.DocCloseShiftMaterialID,
                        Quantity = d.Quantity,
                        BaseQuantity = d.Quantity,
                        MeasureUnit = d.C1CNomenclature.C1CMeasureUnitStorage.Name,
                        MeasureUnitID = d.C1CNomenclature.C1CMeasureUnitStorage.C1CMeasureUnitID
                    }).OrderBy(d => d.NomenclatureName));
            WithdrawalMaterialsOut = new ItemsChangeObservableCollection<WithdrawalMaterial>(GammaBase.DocCloseShiftMaterials
                .Where(dm => dm.DocID == docId && dm.DocCloseShiftMaterialTypeID == 2)
                .Select(d => new WithdrawalMaterial
                {
                    CharacteristicID = d.C1CCharacteristicID,
                    NomenclatureID = d.C1CNomenclatureID,
                    NomenclatureName = d.C1CNomenclature.Name + d.C1CCharacteristicID == null || d.C1CCharacteristicID == Guid.Empty ? string.Empty : " " + d.C1CCharacteristics.Name,
                    DocWithdrawalMaterialID = d.DocCloseShiftMaterialID,
                    Quantity = d.Quantity,
                    BaseQuantity = d.Quantity,
                    MeasureUnit = d.C1CNomenclature.C1CMeasureUnitStorage.Name,
                    MeasureUnitID = d.C1CNomenclature.C1CMeasureUnitStorage.C1CMeasureUnitID
                }).OrderBy(d => d.NomenclatureName));

            InProducts = new ItemsChangeObservableCollection<MovementProduct>(GammaBase.DocCloseShiftMovementProducts.Where(d => d.DocID == docId && (bool)d.IsMovementIn)
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

            OutProducts = new ItemsChangeObservableCollection<MovementProduct>(GammaBase.DocCloseShiftMovementProducts.Where(d => d.DocID == docId && !((bool)d.IsMovementIn))
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

            /*foreach (MovementProduct addedItem in InProducts)
            {
                WithdrawalMaterialsIn.Add(new WithdrawalMaterial()
                {
                    ProductID = addedItem.ProductId,
                    NomenclatureID = (Guid)addedItem.NomenclatureId,
                    CharacteristicID = addedItem.CharacteristicId,
                    NomenclatureName = addedItem.NomenclatureName,
                    QuantityIsReadOnly = true,
                    Quantity = addedItem.Quantity,
                    BaseQuantity = addedItem.Quantity,//addedItem.BaseQuantity,
                    DocWithdrawalMaterialID = SqlGuidUtil.NewSequentialid(),
                    DocMovementID = addedItem.DocMovementId,
                    NomenclatureKindID = (int)addedItem.NomenclatureKindID,
                    MeasureUnit = addedItem.MeasureUnit,
                    MeasureUnitID = (Guid)addedItem.MeasureUnitID
                });
            }
            
            foreach (MovementProduct addedItem in OutProducts)
            {
                WithdrawalMaterialsOut.Add(new WithdrawalMaterial()
                {
                    ProductID = addedItem.ProductId,
                    NomenclatureID = (Guid)addedItem.NomenclatureId,
                    CharacteristicID = addedItem.CharacteristicId,
                    NomenclatureName = addedItem.NomenclatureName,
                    QuantityIsReadOnly = true,
                    Quantity = addedItem.Quantity,
                    BaseQuantity = addedItem.Quantity,//addedItem.BaseQuantity,
                    DocWithdrawalMaterialID = SqlGuidUtil.NewSequentialid(),
                    DocMovementID = addedItem.DocMovementId,
                    NomenclatureKindID = (int)addedItem.NomenclatureKindID,
                    MeasureUnit = addedItem.MeasureUnit,
                    MeasureUnitID = (Guid)addedItem.MeasureUnitID
                });
            }
            */
            WithdrawalMaterialsRemainder = new ItemsChangeObservableCollection<WithdrawalMaterial>(GammaBase.DocCloseShiftMaterials
                .Where(dm => dm.DocID == docId && dm.DocCloseShiftMaterialTypeID == 3)
                .Select(d => new WithdrawalMaterial
                {
                    CharacteristicID = d.C1CCharacteristicID,
                    NomenclatureID = d.C1CNomenclatureID,
                    NomenclatureName = d.C1CNomenclature.Name + d.C1CCharacteristicID == null || d.C1CCharacteristicID == Guid.Empty ? string.Empty : " " + d.C1CCharacteristics.Name,
                    DocWithdrawalMaterialID = d.DocCloseShiftMaterialID,
                    Quantity = d.Quantity,
                    BaseQuantity = d.Quantity,
                    MeasureUnit = d.C1CNomenclature.C1CMeasureUnitStorage.Name,
                    MeasureUnitID = d.C1CNomenclature.C1CMeasureUnitStorage.C1CMeasureUnitID
                }).OrderBy(d => d.NomenclatureName));
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
                    ProductionProductCharacteristicID = d.DocWithdrawal.DocProduction.FirstOrDefault().DocProductionProducts.FirstOrDefault().C1CCharacteristicID
                }).OrderBy(d => d.NomenclatureName));
            foreach (var item in WithdrawalMaterials)
            {
                item.PlaceID = PlaceID;
                item.SetAvailableProductionProducts = productionProductCharacteristicIDs;

            }
        }

        public void FillWithdrawalMaterials(List<Guid> productionProductCharacteristicIDs)
        {
            ProductionProductCharacteristicIDs = productionProductCharacteristicIDs;
            var WithdrawalMaterialsLoad =
                    new ItemsChangeObservableCollection<WithdrawalMaterial>(
                        GammaBase.FillDocCloseShiftMaterials(PlaceID, ShiftID, CloseDate)
                        //.Take(0)    
                        .Select(m => new WithdrawalMaterial()
                        {
                            NomenclatureID = (Guid)m.NomenclatureID,
                            CharacteristicID = m.CharacteristicID,
                            NomenclatureName = m.NomenclatureName,
                            QuantityIsReadOnly = false, //!m.WithdrawByFact ?? true,
                            Quantity = m.Quantity ?? 0,
                            BaseQuantity = m.Quantity ?? 0,
                            DocWithdrawalMaterialID = SqlGuidUtil.NewSequentialid(),
                            MeasureUnit = m.MeasureUnit,
                            MeasureUnitID = m.MeasureUnitID
                        }).OrderBy(m => m.NomenclatureName));

            Clear();

            foreach (WithdrawalMaterial addedItem in WithdrawalMaterialsLoad)
            {
                if (WithdrawalMaterialsIn.Count(d => d.NomenclatureID == addedItem.NomenclatureID && (d.CharacteristicID == addedItem.CharacteristicID || (d.CharacteristicID == null && addedItem.CharacteristicID == null))) == 0)
                    WithdrawalMaterialsIn.Add(new WithdrawalMaterial()
                    {
                        NomenclatureID = addedItem.NomenclatureID,
                        CharacteristicID = addedItem.CharacteristicID,
                        NomenclatureName = addedItem.NomenclatureName,
                        QuantityIsReadOnly = addedItem.QuantityIsReadOnly,
                        Quantity = addedItem.Quantity,
                        BaseQuantity = addedItem.BaseQuantity,
                        DocWithdrawalMaterialID = SqlGuidUtil.NewSequentialid(),
                        MeasureUnit = addedItem.MeasureUnit,
                        MeasureUnitID = addedItem.MeasureUnitID
                    });
                if (WithdrawalMaterialsOut.Count(d => d.NomenclatureID == addedItem.NomenclatureID && (d.CharacteristicID == addedItem.CharacteristicID || (d.CharacteristicID == null && addedItem.CharacteristicID == null))) == 0)
                    WithdrawalMaterialsOut.Add(new WithdrawalMaterial()
                    {
                        NomenclatureID = addedItem.NomenclatureID,
                        CharacteristicID = addedItem.CharacteristicID,
                        NomenclatureName = addedItem.NomenclatureName,
                        QuantityIsReadOnly = addedItem.QuantityIsReadOnly,
                        Quantity = addedItem.Quantity,
                        BaseQuantity = addedItem.BaseQuantity,
                        DocWithdrawalMaterialID = SqlGuidUtil.NewSequentialid(),
                        MeasureUnit = addedItem.MeasureUnit,
                        MeasureUnitID = addedItem.MeasureUnitID
                    });
                if (WithdrawalMaterialsRemainder.Count(d => d.NomenclatureID == addedItem.NomenclatureID && (d.CharacteristicID == addedItem.CharacteristicID || (d.CharacteristicID == null && addedItem.CharacteristicID == null))) == 0)
                    WithdrawalMaterialsRemainder.Add(new WithdrawalMaterial()
                    {
                        NomenclatureID = addedItem.NomenclatureID,
                        CharacteristicID = addedItem.CharacteristicID,
                        NomenclatureName = addedItem.NomenclatureName,
                        QuantityIsReadOnly = addedItem.QuantityIsReadOnly,
                        Quantity = addedItem.Quantity,
                        BaseQuantity = addedItem.BaseQuantity,
                        DocWithdrawalMaterialID = SqlGuidUtil.NewSequentialid(),
                        MeasureUnit = addedItem.MeasureUnit,
                        MeasureUnitID = addedItem.MeasureUnitID
                    });
            }

            InProducts = new ItemsChangeObservableCollection<MovementProduct>(GammaBase.FillDocCloseShiftMovementProducts(PlaceID, ShiftID, CloseDate)
                            .Where(d => (bool)d.IsMovementIn).Select(d => new MovementProduct
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
                var item = WithdrawalMaterialsIn.FirstOrDefault(d => d.NomenclatureID == addedItem.NomenclatureId && (d.CharacteristicID == addedItem.CharacteristicId || (d.CharacteristicID == null && addedItem.CharacteristicId == null)));
                if (item == null)
                    WithdrawalMaterialsIn.Add(new WithdrawalMaterial()
                    {
                        NomenclatureID = (Guid)addedItem.NomenclatureId,
                        CharacteristicID = addedItem.CharacteristicId,
                        NomenclatureName = addedItem.NomenclatureName,
                        QuantityIsReadOnly = true,
                        Quantity = addedItem.Quantity,
                        BaseQuantity = addedItem.Quantity,//addedItem.BaseQuantity,
                        DocWithdrawalMaterialID = SqlGuidUtil.NewSequentialid(),
                        MeasureUnit = addedItem.MeasureUnit,
                        MeasureUnitID = addedItem.MeasureUnitID
                    });
                else
                {
                    item.Quantity = item.Quantity + addedItem.Quantity;
                    item.BaseQuantity = item.BaseQuantity + addedItem.Quantity;
                };
               /* WithdrawalMaterialsIn.Add(new WithdrawalMaterial()
                {
                    ProductID = addedItem.ProductId,
                    NomenclatureID = (Guid)addedItem.NomenclatureId,
                    CharacteristicID = addedItem.CharacteristicId,
                    NomenclatureName = addedItem.NomenclatureName,
                    QuantityIsReadOnly = true,
                    Quantity = addedItem.Quantity,
                    BaseQuantity = addedItem.Quantity,//addedItem.BaseQuantity,
                    DocWithdrawalMaterialID = SqlGuidUtil.NewSequentialid(),
                    DocMovementID = addedItem.DocMovementId,
                    NomenclatureKindID = (int)addedItem.NomenclatureKindID,
                    MeasureUnit = addedItem.MeasureUnit,
                    MeasureUnitID = (Guid)addedItem.MeasureUnitID
                });*/
            }

            OutProducts = new ItemsChangeObservableCollection<MovementProduct>(GammaBase.FillDocCloseShiftMovementProducts(PlaceID, ShiftID, CloseDate)
                        .Where(d => (bool)d.IsMovementOut).Select(d => new MovementProduct
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
                var item = WithdrawalMaterialsOut.FirstOrDefault(d => d.NomenclatureID == addedItem.NomenclatureId && (d.CharacteristicID == addedItem.CharacteristicId || (d.CharacteristicID == null && addedItem.CharacteristicId == null)));
                if (item == null)
                    WithdrawalMaterialsOut.Add(new WithdrawalMaterial()
                    {
                        NomenclatureID = (Guid)addedItem.NomenclatureId,
                        CharacteristicID = addedItem.CharacteristicId,
                        NomenclatureName = addedItem.NomenclatureName,
                        QuantityIsReadOnly = true,
                        Quantity = addedItem.Quantity,
                        BaseQuantity = addedItem.Quantity,//addedItem.BaseQuantity,
                        DocWithdrawalMaterialID = SqlGuidUtil.NewSequentialid(),
                        MeasureUnit = addedItem.MeasureUnit,
                        MeasureUnitID = addedItem.MeasureUnitID
                    });
                else
                {
                    item.Quantity = item.Quantity + addedItem.Quantity;
                    item.BaseQuantity = item.BaseQuantity + addedItem.Quantity;
                };

                /*WithdrawalMaterialsOut.Add(new WithdrawalMaterial()
                {
                    ProductID = addedItem.ProductId,
                    NomenclatureID = (Guid)addedItem.NomenclatureId,
                    CharacteristicID = addedItem.CharacteristicId,
                    NomenclatureName = addedItem.NomenclatureName,
                    QuantityIsReadOnly = true,
                    Quantity = addedItem.Quantity,
                    BaseQuantity = addedItem.Quantity,//addedItem.BaseQuantity,
                    DocWithdrawalMaterialID = SqlGuidUtil.NewSequentialid(),
                    DocMovementID = addedItem.DocMovementId,
                    NomenclatureKindID = (int)addedItem.NomenclatureKindID,
                    MeasureUnit = addedItem.MeasureUnit,
                    MeasureUnitID = (Guid)addedItem.MeasureUnitID
                });*/
            }

            foreach (WithdrawalMaterial addedItem in WithdrawalMaterialsIn.Where(x => x.Quantity != 0))
            {
                if (WithdrawalMaterials.Count(d => d.NomenclatureID == addedItem.NomenclatureID && (d.CharacteristicID == addedItem.CharacteristicID || (d.CharacteristicID == null && addedItem.CharacteristicID == null))) == 0)
                    WithdrawalMaterials.Add(new WithdrawalMaterial(new ArrayList() { productionProductCharacteristicIDs, PlaceID })
                    {
                        NomenclatureID = addedItem.NomenclatureID,
                        CharacteristicID = addedItem.CharacteristicID,
                        NomenclatureName = addedItem.NomenclatureName,
                        QuantityIsReadOnly = addedItem.QuantityIsReadOnly,
                        Quantity = addedItem.Quantity,
                        BaseQuantity = addedItem.BaseQuantity,
                        DocWithdrawalMaterialID = SqlGuidUtil.NewSequentialid(),
                        MeasureUnit = addedItem.MeasureUnit,
                        MeasureUnitID = addedItem.MeasureUnitID
                    });
            }
            foreach (WithdrawalMaterial addedItem in WithdrawalMaterialsOut.Where(x => x.Quantity != 0))
            {
                if (WithdrawalMaterials.Count(d => d.NomenclatureID == addedItem.NomenclatureID && (d.CharacteristicID == addedItem.CharacteristicID || (d.CharacteristicID == null && addedItem.CharacteristicID == null))) == 0)
                {
                    WithdrawalMaterials.Add(new WithdrawalMaterial(new ArrayList() { productionProductCharacteristicIDs, PlaceID })
                    {
                        NomenclatureID = addedItem.NomenclatureID,
                        CharacteristicID = addedItem.CharacteristicID,
                        NomenclatureName = addedItem.NomenclatureName,
                        QuantityIsReadOnly = addedItem.QuantityIsReadOnly,
                        Quantity = -addedItem.Quantity,
                        BaseQuantity = -addedItem.BaseQuantity,
                        DocWithdrawalMaterialID = SqlGuidUtil.NewSequentialid(),
                        MeasureUnit = addedItem.MeasureUnit,
                        MeasureUnitID = addedItem.MeasureUnitID
                    });
                }
                else
                {
                    var item = WithdrawalMaterials.FirstOrDefault(d => d.NomenclatureID == addedItem.NomenclatureID && (d.CharacteristicID == addedItem.CharacteristicID || (d.CharacteristicID == null && addedItem.CharacteristicID == null)));
                    if (item != null)
                    {
                        item.Quantity = item.Quantity - addedItem.Quantity;
                        item.BaseQuantity = item.BaseQuantity - addedItem.BaseQuantity;
                    }
                }
            }
            foreach (WithdrawalMaterial addedItem in WithdrawalMaterialsRemainder.Where(x => x.Quantity != 0))
            {
                if (WithdrawalMaterials.Count(d => d.NomenclatureID == addedItem.NomenclatureID && (d.CharacteristicID == addedItem.CharacteristicID || (d.CharacteristicID == null && addedItem.CharacteristicID == null))) == 0)
                {
                    WithdrawalMaterials.Add(new WithdrawalMaterial(new ArrayList() { productionProductCharacteristicIDs, PlaceID })
                    {
                        NomenclatureID = addedItem.NomenclatureID,
                        CharacteristicID = addedItem.CharacteristicID,
                        NomenclatureName = addedItem.NomenclatureName,
                        QuantityIsReadOnly = addedItem.QuantityIsReadOnly,
                        Quantity = -addedItem.Quantity,
                        BaseQuantity = -addedItem.BaseQuantity,
                        DocWithdrawalMaterialID = SqlGuidUtil.NewSequentialid(),
                        MeasureUnit = addedItem.MeasureUnit,
                        MeasureUnitID = addedItem.MeasureUnitID
                    });
                }
                else
                {
                    var item = WithdrawalMaterials.FirstOrDefault(d => d.NomenclatureID == addedItem.NomenclatureID && (d.CharacteristicID == addedItem.CharacteristicID || (d.CharacteristicID == null && addedItem.CharacteristicID == null)));
                    if (item != null)
                    {
                        item.Quantity = item.Quantity - addedItem.Quantity;
                        item.BaseQuantity = item.BaseQuantity - addedItem.BaseQuantity;
                    }
                }
            }
        }

        public void Clear()
        {
            WithdrawalMaterials?.Clear();
            WithdrawalMaterialsIn?.Clear();
            WithdrawalMaterialsOut?.Clear();
            WithdrawalMaterialsRemainder?.Clear();
        }

      
        public void MaterialNomenclatureChanged(C1CNomenclature nomenclatureInfo)//, List<Guid> productionProductCharacteristicIDs)
        {
            if (WithdrawalMaterialsIn.FirstOrDefault(d => d.NomenclatureID == nomenclatureInfo.C1CNomenclatureID) == null)
                WithdrawalMaterialsIn.Add(new WithdrawalMaterial
                {
                    NomenclatureID = nomenclatureInfo.C1CNomenclatureID,
                    CharacteristicID = nomenclatureInfo.C1CCharacteristics.Select(x => x.C1CCharacteristicID).FirstOrDefault(),
                    NomenclatureName = nomenclatureInfo.Name + " " + nomenclatureInfo.C1CCharacteristics.Select(x => x.Name).FirstOrDefault(),
                    QuantityIsReadOnly = false,
                    Quantity = 0,
                    MeasureUnitID = nomenclatureInfo.C1CMeasureUnitStorage.C1CMeasureUnitID,
                    MeasureUnit = nomenclatureInfo.C1CMeasureUnitStorage.Name,
                    DocWithdrawalMaterialID = SqlGuidUtil.NewSequentialid()
                });
            if (WithdrawalMaterialsOut.FirstOrDefault(d => d.NomenclatureID == nomenclatureInfo.C1CNomenclatureID) == null)
                WithdrawalMaterialsOut.Add(new WithdrawalMaterial
                {
                    NomenclatureID = nomenclatureInfo.C1CNomenclatureID,
                    CharacteristicID = nomenclatureInfo.C1CCharacteristics.Select(x => x.C1CCharacteristicID).FirstOrDefault(),
                    NomenclatureName = nomenclatureInfo.Name + " " + nomenclatureInfo.C1CCharacteristics.Select(x => x.Name).FirstOrDefault(),
                    QuantityIsReadOnly = false,
                    Quantity = 0,
                    MeasureUnitID = nomenclatureInfo.C1CMeasureUnitStorage.C1CMeasureUnitID,
                    MeasureUnit = nomenclatureInfo.C1CMeasureUnitStorage.Name,
                    DocWithdrawalMaterialID = SqlGuidUtil.NewSequentialid()
                });
            if (WithdrawalMaterialsRemainder.FirstOrDefault(d => d.NomenclatureID == nomenclatureInfo.C1CNomenclatureID) == null)
                WithdrawalMaterialsRemainder.Add(new WithdrawalMaterial
                {
                    NomenclatureID = nomenclatureInfo.C1CNomenclatureID,
                    CharacteristicID = nomenclatureInfo.C1CCharacteristics.Select(x => x.C1CCharacteristicID).FirstOrDefault(),
                    NomenclatureName = nomenclatureInfo.Name + " " + nomenclatureInfo.C1CCharacteristics.Select(x => x.Name).FirstOrDefault(),
                    QuantityIsReadOnly = false,
                    Quantity = 0,
                    MeasureUnitID = nomenclatureInfo.C1CMeasureUnitStorage.C1CMeasureUnitID,
                    MeasureUnit = nomenclatureInfo.C1CMeasureUnitStorage.Name,
                    DocWithdrawalMaterialID = SqlGuidUtil.NewSequentialid()
                });
            if (WithdrawalMaterials.FirstOrDefault(d => d.NomenclatureID == nomenclatureInfo.C1CNomenclatureID) == null)
                WithdrawalMaterials.Add(new WithdrawalMaterial(new ArrayList() { ProductionProductCharacteristicIDs, PlaceID })
                {
                    NomenclatureID = nomenclatureInfo.C1CNomenclatureID,
                    CharacteristicID = nomenclatureInfo.C1CCharacteristics.Select(x => x.C1CCharacteristicID).FirstOrDefault(),
                    NomenclatureName = nomenclatureInfo.Name + " " + nomenclatureInfo.C1CCharacteristics.Select(x => x.Name).FirstOrDefault(),
                    QuantityIsReadOnly = false,
                    Quantity = 0,
                    MeasureUnitID = nomenclatureInfo.C1CMeasureUnitStorage.C1CMeasureUnitID,
                    MeasureUnit = nomenclatureInfo.C1CMeasureUnitStorage.Name,
                    DocWithdrawalMaterialID = SqlGuidUtil.NewSequentialid()
                });
        }


        public void MaterialChanged(int selectedMaterialTabIndex, WithdrawalMaterial selectedMaterial)//, List<Guid> productionProductCharacteristicIDs)
        {
            var materialIn = WithdrawalMaterialsIn.Where(d => d.NomenclatureID == selectedMaterial?.NomenclatureID).Sum(d => d.Quantity);
            var materialOut = WithdrawalMaterialsOut.Where(d => d.NomenclatureID == selectedMaterial?.NomenclatureID).Sum(d => d.Quantity);
            var materialRemainder = WithdrawalMaterialsRemainder.Where(d => d.NomenclatureID == selectedMaterial?.NomenclatureID).Sum(d => d.Quantity);
            var materialBaseIn = WithdrawalMaterialsIn.Where(d => d.NomenclatureID == selectedMaterial?.NomenclatureID).Sum(d => d.BaseQuantity);
            var materialBaseOut = WithdrawalMaterialsOut.Where(d => d.NomenclatureID == selectedMaterial?.NomenclatureID).Sum(d => d.BaseQuantity);
            var materialBaseRemainder = WithdrawalMaterialsRemainder.Where(d => d.NomenclatureID == selectedMaterial?.NomenclatureID).Sum(d => d.BaseQuantity);
            var withdrawalMaterial = WithdrawalMaterials.FirstOrDefault(d => d.NomenclatureID == selectedMaterial?.NomenclatureID && (d.CharacteristicID == selectedMaterial?.CharacteristicID || (d.CharacteristicID == null && selectedMaterial?.CharacteristicID == null)));// && d.CharacteristicID == selectedMaterial?.CharacteristicID);
            //productionProductCharacteristicIDs = new List<Guid>(Spools
            //        .Select(p => p.CharacteristicID).Distinct().ToList());
            if (selectedMaterialTabIndex != 3)
            {
                if (withdrawalMaterial == null)
                {
                    withdrawalMaterial = new WithdrawalMaterial(new ArrayList() { ProductionProductCharacteristicIDs, PlaceID })
                    {
                        NomenclatureID = selectedMaterial.NomenclatureID,
                        NomenclatureName = selectedMaterial.NomenclatureName,
                        CharacteristicID = selectedMaterial.CharacteristicID,
                        DocWithdrawalMaterialID = SqlGuidUtil.NewSequentialid(),
                        Quantity = materialIn - materialOut - materialRemainder,
                        BaseQuantity = materialBaseIn - materialBaseOut - materialBaseRemainder,
                        MeasureUnit = selectedMaterial.MeasureUnit,
                        MeasureUnitID = selectedMaterial.MeasureUnitID
                    };
                    WithdrawalMaterials.Add(withdrawalMaterial);
                }
                else
                {
                    withdrawalMaterial.Quantity = materialIn - materialOut - materialRemainder;
                    withdrawalMaterial.BaseQuantity = materialBaseIn - materialBaseOut - materialBaseRemainder;

                }
                selectedMaterial = withdrawalMaterial;
            }
            //else
            //{
            //    selectedMaterial = SelectedWithdrawalMaterial;
            //}
            var material = WithdrawalMaterials.Where(d => d.NomenclatureID == selectedMaterial?.NomenclatureID && (d.CharacteristicID == selectedMaterial?.CharacteristicID || (d.CharacteristicID == null && selectedMaterial?.CharacteristicID == null))).Sum(d => d.Quantity);
            var materialBase = WithdrawalMaterials.Where(d => d.NomenclatureID == selectedMaterial?.NomenclatureID && (d.CharacteristicID == selectedMaterial?.CharacteristicID || (d.CharacteristicID == null && selectedMaterial?.CharacteristicID == null))).Sum(d => d.BaseQuantity);
            if (selectedMaterial.Quantity == (materialIn - materialOut - materialRemainder))
            {
                var items = WithdrawalMaterials.Where(d => d.DocWithdrawalMaterialID != selectedMaterial?.DocWithdrawalMaterialID && d.NomenclatureID == selectedMaterial?.NomenclatureID && (d.CharacteristicID == selectedMaterial?.CharacteristicID || (d.CharacteristicID == null && selectedMaterial?.CharacteristicID == null))).ToArray();
                foreach (var item in items)
                {
                    WithdrawalMaterials.Remove(item);
                }
            }
            else
            {
                if (materialIn - materialOut - materialRemainder - material != 0)
                    WithdrawalMaterials.Add(new WithdrawalMaterial(new ArrayList() { ProductionProductCharacteristicIDs, PlaceID })
                    {
                        NomenclatureID = selectedMaterial.NomenclatureID,
                        NomenclatureName = selectedMaterial.NomenclatureName,
                        CharacteristicID = selectedMaterial.CharacteristicID,
                        DocWithdrawalMaterialID = SqlGuidUtil.NewSequentialid(),
                        Quantity = materialIn - materialOut - materialRemainder - material,
                        BaseQuantity = materialBaseIn - materialBaseOut - materialBaseRemainder - materialBase,
                        MeasureUnit = selectedMaterial.MeasureUnit,
                        MeasureUnitID = selectedMaterial.MeasureUnitID
                    });
            }

            RaisePropertiesChanged("DocCloseShiftWithdrawalMaterials.WithdrawalMaterials");
        }

        
    }
}
