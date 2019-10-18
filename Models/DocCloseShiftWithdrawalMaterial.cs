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

        private ItemsChangeObservableCollection<WithdrawalMaterial> _withdrawalMaterialsRemainderAtBegin = new ItemsChangeObservableCollection<WithdrawalMaterial>();

        public ItemsChangeObservableCollection<WithdrawalMaterial> WithdrawalMaterialsRemainderAtBegin
        {
            get { return _withdrawalMaterialsRemainderAtBegin; }
            set
            {
                _withdrawalMaterialsRemainderAtBegin = value;
                RaisePropertiesChanged("DocCloseShiftWithdrawalMaterials.WithdrawalMaterialsRemainderAtBegin");
            }
        }

        private ItemsChangeObservableCollection<WithdrawalMaterial> _withdrawalMaterialsRemainderAtEnd = new ItemsChangeObservableCollection<WithdrawalMaterial>();

        public ItemsChangeObservableCollection<WithdrawalMaterial> WithdrawalMaterialsRemainderAtEnd
        {
            get { return _withdrawalMaterialsRemainderAtEnd; }
            set
            {
                _withdrawalMaterialsRemainderAtEnd = value;
                RaisePropertiesChanged("DocCloseShiftWithdrawalMaterials.WithdrawalMaterialsRemainderAtEnd");
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
                        MeasureUnitID = d.C1CNomenclature.C1CMeasureUnitStorage.C1CMeasureUnitID,
                        WithdrawByFact = d.WithdrawByFact
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
                    MeasureUnitID = d.C1CNomenclature.C1CMeasureUnitStorage.C1CMeasureUnitID,
                    WithdrawByFact = d.WithdrawByFact
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
            WithdrawalMaterialsRemainderAtBegin = new ItemsChangeObservableCollection<WithdrawalMaterial>(GammaBase.DocCloseShiftMaterials
                .Where(dm => dm.DocID == docId && dm.DocCloseShiftMaterialTypeID == 4)
                .Select(d => new WithdrawalMaterial
                {
                    CharacteristicID = d.C1CCharacteristicID,
                    NomenclatureID = d.C1CNomenclatureID,
                    NomenclatureName = d.C1CNomenclature.Name + d.C1CCharacteristicID == null || d.C1CCharacteristicID == Guid.Empty ? string.Empty : " " + d.C1CCharacteristics.Name,
                    DocWithdrawalMaterialID = d.DocCloseShiftMaterialID,
                    Quantity = d.Quantity,
                    BaseQuantity = d.Quantity,
                    MeasureUnit = d.C1CNomenclature.C1CMeasureUnitStorage.Name,
                    MeasureUnitID = d.C1CNomenclature.C1CMeasureUnitStorage.C1CMeasureUnitID,
                    WithdrawByFact = d.WithdrawByFact,
                    QuantityIsReadOnly = true
                }).OrderBy(d => d.NomenclatureName));
            WithdrawalMaterialsRemainderAtEnd = new ItemsChangeObservableCollection<WithdrawalMaterial>(GammaBase.DocCloseShiftMaterials
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
                    MeasureUnitID = d.C1CNomenclature.C1CMeasureUnitStorage.C1CMeasureUnitID,
                    WithdrawByFact = d.WithdrawByFact
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
                    ProductionProductCharacteristicID = d.DocWithdrawal.DocProduction.FirstOrDefault().DocProductionProducts.FirstOrDefault().C1CCharacteristicID,
                    WithdrawByFact = d.WithdrawByFact
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
            {
                Clear();
                foreach (var removeItem in WithdrawalMaterialsIn.Where(ps => ps.QuantityIsReadOnly).ToList())
                {
                        WithdrawalMaterialsIn.Remove(removeItem);
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
                                MeasureUnitID = addedItem.MeasureUnitID,
                                WithdrawByFact = true
                            });
                        else
                        {
                        if (!item.QuantityIsReadOnly)
                        {
                            WithdrawalMaterialsIn.Remove(item);
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
                                MeasureUnitID = addedItem.MeasureUnitID,
                                WithdrawByFact = true
                            });
                        }
                        else
                        {
                            item.Quantity = item.Quantity + addedItem.Quantity;
                            item.BaseQuantity = item.BaseQuantity + addedItem.Quantity;
                        }
                        };

                        item = WithdrawalMaterialsOut.FirstOrDefault(d => d.NomenclatureID == addedItem.NomenclatureId && (d.CharacteristicID == addedItem.CharacteristicId || (d.CharacteristicID == null && addedItem.CharacteristicId == null)));
                        if (item != null)
                        {
                            if (!item.QuantityIsReadOnly) item.QuantityIsReadOnly = true;
                        }

                        item = WithdrawalMaterialsRemainderAtEnd.FirstOrDefault(d => d.NomenclatureID == addedItem.NomenclatureId && (d.CharacteristicID == addedItem.CharacteristicId || (d.CharacteristicID == null && addedItem.CharacteristicId == null)));
                        if (item == null)
                            WithdrawalMaterialsRemainderAtEnd.Add(new WithdrawalMaterial()
                            {
                                NomenclatureID = (Guid)addedItem.NomenclatureId,
                                CharacteristicID = addedItem.CharacteristicId,
                                NomenclatureName = addedItem.NomenclatureName,
                                QuantityIsReadOnly = true,
                                Quantity = 0,//addedItem.Quantity,
                                BaseQuantity = 0,//addedItem.Quantity,//addedItem.BaseQuantity,
                                DocWithdrawalMaterialID = SqlGuidUtil.NewSequentialid(),
                                MeasureUnit = addedItem.MeasureUnit,
                                MeasureUnitID = addedItem.MeasureUnitID,
                                WithdrawByFact = true
                            });

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

                foreach (var removeItem in WithdrawalMaterialsOut.Where(ps => ps.QuantityIsReadOnly).ToList())
                {
                    WithdrawalMaterialsOut.Remove(removeItem);
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
                                MeasureUnitID = addedItem.MeasureUnitID,
                                WithdrawByFact = true
                            });
                        else
                        {
                        if (!item.QuantityIsReadOnly)
                        {
                            WithdrawalMaterialsOut.Remove(item);
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
                                MeasureUnitID = addedItem.MeasureUnitID,
                                WithdrawByFact = true
                            });
                        }
                        else
                        {
                            item.Quantity = item.Quantity + addedItem.Quantity;
                            item.BaseQuantity = item.BaseQuantity + addedItem.Quantity;
                        }
                        };

                        item = WithdrawalMaterialsIn.FirstOrDefault(d => d.NomenclatureID == addedItem.NomenclatureId && (d.CharacteristicID == addedItem.CharacteristicId || (d.CharacteristicID == null && addedItem.CharacteristicId == null)));
                        if (item != null)
                        {
                            if (!item.QuantityIsReadOnly) item.QuantityIsReadOnly = true;
                        }

                        item = WithdrawalMaterialsRemainderAtEnd.FirstOrDefault(d => d.NomenclatureID == addedItem.NomenclatureId && (d.CharacteristicID == addedItem.CharacteristicId || (d.CharacteristicID == null && addedItem.CharacteristicId == null)));
                        if (item == null)
                            WithdrawalMaterialsRemainderAtEnd.Add(new WithdrawalMaterial()
                            {
                                NomenclatureID = (Guid)addedItem.NomenclatureId,
                                CharacteristicID = addedItem.CharacteristicId,
                                NomenclatureName = addedItem.NomenclatureName,
                                QuantityIsReadOnly = true,
                                Quantity = 0,//addedItem.Quantity,
                                BaseQuantity = 0,//addedItem.Quantity,//addedItem.BaseQuantity,
                                DocWithdrawalMaterialID = SqlGuidUtil.NewSequentialid(),
                                MeasureUnit = addedItem.MeasureUnit,
                                MeasureUnitID = addedItem.MeasureUnitID,
                                WithdrawByFact = true
                            });

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
                                MeasureUnitID = m.MeasureUnitID,
                                WithdrawByFact = m.WithdrawByFact ?? true
                            }).OrderBy(m => m.NomenclatureName));


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
                            MeasureUnitID = addedItem.MeasureUnitID,
                            WithdrawByFact = addedItem.WithdrawByFact
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
                            MeasureUnitID = addedItem.MeasureUnitID,
                            WithdrawByFact = addedItem.WithdrawByFact
                        });
                    /*if (WithdrawalMaterialsRemainderAtBegin.Count(d => d.NomenclatureID == addedItem.NomenclatureID && (d.CharacteristicID == addedItem.CharacteristicID || (d.CharacteristicID == null && addedItem.CharacteristicID == null))) == 0)
                        WithdrawalMaterialsRemainderAtBegin.Add(new WithdrawalMaterial()
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
                        });*/
                    if (WithdrawalMaterialsRemainderAtEnd.Count(d => d.NomenclatureID == addedItem.NomenclatureID && (d.CharacteristicID == addedItem.CharacteristicID || (d.CharacteristicID == null && addedItem.CharacteristicID == null))) == 0)
                        WithdrawalMaterialsRemainderAtEnd.Add(new WithdrawalMaterial()
                        {
                            NomenclatureID = addedItem.NomenclatureID,
                            CharacteristicID = addedItem.CharacteristicID,
                            NomenclatureName = addedItem.NomenclatureName,
                            QuantityIsReadOnly = (addedItem.WithdrawByFact == false ? true : addedItem.QuantityIsReadOnly),
                            Quantity = (addedItem.WithdrawByFact == false ? 0 : addedItem.Quantity),//addedItem.Quantity,
                            BaseQuantity = (addedItem.WithdrawByFact == false ? 0 : addedItem.BaseQuantity),//addedItem.BaseQuantity,
                            DocWithdrawalMaterialID = SqlGuidUtil.NewSequentialid(),
                            MeasureUnit = addedItem.MeasureUnit,
                            MeasureUnitID = addedItem.MeasureUnitID,
                            WithdrawByFact = addedItem.WithdrawByFact
                        });
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
                            MeasureUnitID = addedItem.MeasureUnitID,
                            WithdrawByFact = addedItem.WithdrawByFact
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
                            MeasureUnitID = addedItem.MeasureUnitID,
                            WithdrawByFact = addedItem.WithdrawByFact
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

                var withdrawalMaterialsRemainderAtBegin =
                        new ItemsChangeObservableCollection<WithdrawalMaterial>(
                            GammaBase.FillDocCloseShiftMaterialsAtBegin(PlaceID, ShiftID, CloseDate)
                            //.Take(0)    
                            .Select(m => new WithdrawalMaterial()
                            {
                                NomenclatureID = (Guid)m.NomenclatureID,
                                CharacteristicID = m.CharacteristicID,
                                NomenclatureName = m.NomenclatureName,
                                QuantityIsReadOnly = true, //!m.WithdrawByFact ?? true,
                            Quantity = m.Quantity ?? 0,
                                BaseQuantity = m.Quantity ?? 0,
                                DocWithdrawalMaterialID = SqlGuidUtil.NewSequentialid(),
                                MeasureUnit = m.MeasureUnit,
                                MeasureUnitID = m.MeasureUnitID,
                                WithdrawByFact = m.WithdrawByFact ?? true
                            }).OrderBy(m => m.NomenclatureName));

                foreach (WithdrawalMaterial addedItem in withdrawalMaterialsRemainderAtBegin.Where(x => x.Quantity != 0))
                {
                    WithdrawalMaterialsRemainderAtBegin.Add(new WithdrawalMaterial(new ArrayList() { productionProductCharacteristicIDs, PlaceID })
                    {
                        NomenclatureID = addedItem.NomenclatureID,
                        CharacteristicID = addedItem.CharacteristicID,
                        NomenclatureName = addedItem.NomenclatureName,
                        QuantityIsReadOnly = true,//ddedItem.QuantityIsReadOnly,
                        Quantity = addedItem.Quantity,
                        BaseQuantity = addedItem.BaseQuantity,
                        DocWithdrawalMaterialID = SqlGuidUtil.NewSequentialid(),
                        MeasureUnit = addedItem.MeasureUnit,
                        MeasureUnitID = addedItem.MeasureUnitID,
                        WithdrawByFact = addedItem.WithdrawByFact
                    });
                }

                foreach (WithdrawalMaterial addedItem in WithdrawalMaterialsRemainderAtBegin.Where(x => x.Quantity != 0))
                {
                        if (WithdrawalMaterialsIn.Count(d => d.NomenclatureID == addedItem.NomenclatureID && (d.CharacteristicID == addedItem.CharacteristicID || (d.CharacteristicID == null && addedItem.CharacteristicID == null))) == 0)
                            WithdrawalMaterialsIn.Add(new WithdrawalMaterial()
                            {
                                NomenclatureID = addedItem.NomenclatureID,
                                CharacteristicID = addedItem.CharacteristicID,
                                NomenclatureName = addedItem.NomenclatureName,
                                QuantityIsReadOnly = false, //addedItem.QuantityIsReadOnly,
                                Quantity = 0,//addedItem.Quantity,
                                BaseQuantity = 0,//addedItem.BaseQuantity,
                                DocWithdrawalMaterialID = SqlGuidUtil.NewSequentialid(),
                                MeasureUnit = addedItem.MeasureUnit,
                                MeasureUnitID = addedItem.MeasureUnitID,
                                WithdrawByFact = addedItem.WithdrawByFact
                            });
                        if (WithdrawalMaterialsOut.Count(d => d.NomenclatureID == addedItem.NomenclatureID && (d.CharacteristicID == addedItem.CharacteristicID || (d.CharacteristicID == null && addedItem.CharacteristicID == null))) == 0)
                            WithdrawalMaterialsOut.Add(new WithdrawalMaterial()
                            {
                                NomenclatureID = addedItem.NomenclatureID,
                                CharacteristicID = addedItem.CharacteristicID,
                                NomenclatureName = addedItem.NomenclatureName,
                                QuantityIsReadOnly = false,// addedItem.QuantityIsReadOnly,
                                Quantity = 0,//addedItem.Quantity,
                                BaseQuantity = 0,//addedItem.BaseQuantity,
                                DocWithdrawalMaterialID = SqlGuidUtil.NewSequentialid(),
                                MeasureUnit = addedItem.MeasureUnit,
                                MeasureUnitID = addedItem.MeasureUnitID,
                                WithdrawByFact = addedItem.WithdrawByFact
                            });
                        /*if (WithdrawalMaterialsRemainderAtBegin.Count(d => d.NomenclatureID == addedItem.NomenclatureID && (d.CharacteristicID == addedItem.CharacteristicID || (d.CharacteristicID == null && addedItem.CharacteristicID == null))) == 0)
                            WithdrawalMaterialsRemainderAtBegin.Add(new WithdrawalMaterial()
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
                            });*/
                        if (WithdrawalMaterialsRemainderAtEnd.Count(d => d.NomenclatureID == addedItem.NomenclatureID && (d.CharacteristicID == addedItem.CharacteristicID || (d.CharacteristicID == null && addedItem.CharacteristicID == null))) == 0)
                            WithdrawalMaterialsRemainderAtEnd.Add(new WithdrawalMaterial()
                            {
                                NomenclatureID = addedItem.NomenclatureID,
                                CharacteristicID = addedItem.CharacteristicID,
                                NomenclatureName = addedItem.NomenclatureName,
                                QuantityIsReadOnly = false,// (addedItem.WithdrawByFact == false ? true : addedItem.QuantityIsReadOnly),
                                Quantity = 0,//(addedItem.WithdrawByFact == false ? 0 : addedItem.Quantity),//addedItem.Quantity,
                                BaseQuantity = 0,//(addedItem.WithdrawByFact == false ? 0 : addedItem.BaseQuantity),//addedItem.BaseQuantity,
                                DocWithdrawalMaterialID = SqlGuidUtil.NewSequentialid(),
                                MeasureUnit = addedItem.MeasureUnit,
                                MeasureUnitID = addedItem.MeasureUnitID,
                                WithdrawByFact = addedItem.WithdrawByFact
                            });
                 


                    if (WithdrawalMaterials.Count(d => d.NomenclatureID == addedItem.NomenclatureID && (d.CharacteristicID == addedItem.CharacteristicID || (d.CharacteristicID == null && addedItem.CharacteristicID == null))) == 0)
                    {
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
                            MeasureUnitID = addedItem.MeasureUnitID,
                            WithdrawByFact = addedItem.WithdrawByFact
                        });
                    }
                    else
                    {
                        var item = WithdrawalMaterials.FirstOrDefault(d => d.NomenclatureID == addedItem.NomenclatureID && (d.CharacteristicID == addedItem.CharacteristicID || (d.CharacteristicID == null && addedItem.CharacteristicID == null)));
                        if (item != null)
                        {
                            item.Quantity = item.Quantity + addedItem.Quantity;
                            item.BaseQuantity = item.BaseQuantity + addedItem.BaseQuantity;
                        }
                    }
                }

                var withdrawalMaterialsRemainderAtEnd =
                        new ItemsChangeObservableCollection<WithdrawalMaterial>(
                            GammaBase.FillDocCloseShiftMaterialsAtEnd(PlaceID, ShiftID, CloseDate)
                            //.Take(0)    
                            .Select(m => new WithdrawalMaterial()
                            {
                                NomenclatureID = (Guid)m.NomenclatureID,
                                CharacteristicID = m.CharacteristicID,
                                NomenclatureName = m.NomenclatureName,
                                QuantityIsReadOnly = true, //!m.WithdrawByFact ?? true,
                                Quantity = m.Quantity ?? 0,
                                BaseQuantity = m.Quantity ?? 0,
                                DocWithdrawalMaterialID = SqlGuidUtil.NewSequentialid(),
                                MeasureUnit = m.MeasureUnit,
                                MeasureUnitID = m.MeasureUnitID,
                                WithdrawByFact = m.WithdrawByFact ?? true
                            }).OrderBy(m => m.NomenclatureName));

                foreach (WithdrawalMaterial addedItem in withdrawalMaterialsRemainderAtEnd.Where(x => x.Quantity != 0))
                {
                    var item = WithdrawalMaterialsRemainderAtEnd.FirstOrDefault(d => d.NomenclatureID == addedItem.NomenclatureID && (d.CharacteristicID == addedItem.CharacteristicID || (d.CharacteristicID == null && addedItem.CharacteristicID == null)));
                    if (item == null)
                    {
                        WithdrawalMaterialsRemainderAtEnd.Add(new WithdrawalMaterial(new ArrayList() { productionProductCharacteristicIDs, PlaceID })
                        {
                            NomenclatureID = addedItem.NomenclatureID,
                            CharacteristicID = addedItem.CharacteristicID,
                            NomenclatureName = addedItem.NomenclatureName,
                            QuantityIsReadOnly = addedItem.QuantityIsReadOnly,
                            Quantity = addedItem.Quantity,
                            BaseQuantity = addedItem.BaseQuantity,
                            DocWithdrawalMaterialID = SqlGuidUtil.NewSequentialid(),
                            MeasureUnit = addedItem.MeasureUnit,
                            MeasureUnitID = addedItem.MeasureUnitID,
                            WithdrawByFact = addedItem.WithdrawByFact
                        });
                    }
                    else
                    {
                        item.Quantity = addedItem.Quantity;
                        item.BaseQuantity = addedItem.Quantity;
                    };
                }

                foreach (WithdrawalMaterial addedItem in WithdrawalMaterialsRemainderAtEnd.Where(x => x.Quantity != 0))
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
                            MeasureUnitID = addedItem.MeasureUnitID,
                            WithdrawByFact = addedItem.WithdrawByFact
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
        }

        public void Clear()
        {
            WithdrawalMaterials?.Clear();
            if ((WithdrawalMaterialsIn?.Count(d => d.Quantity != 0) == 0 && WithdrawalMaterialsOut?.Count(d => d.Quantity != 0) == 0))
            {
                WithdrawalMaterialsIn?.Clear();
                WithdrawalMaterialsOut?.Clear();
                //MessageBox.Show("Внимание! Во вкладке Принято или Отдано есть строки с кол-вом больше 0. Загрузка материалов не произведена! Обнулите материалы в этих вкладках.", "Ошибка сохранения", MessageBoxButton.OK, MessageBoxImage.Asterisk);
            }
            WithdrawalMaterialsRemainderAtBegin?.Clear();
            if (WithdrawalMaterialsRemainderAtEnd?.Count(d => d.Quantity != 0) == 0)
                WithdrawalMaterialsRemainderAtEnd?.Clear();
        }

      
        public void MaterialNomenclatureChanged(C1CNomenclature nomenclatureInfo)//, List<Guid> productionProductCharacteristicIDs)
        {
            var characteristicID = nomenclatureInfo.C1CCharacteristics.Select(x => x.C1CCharacteristicID).FirstOrDefault() == Guid.Empty ? (Guid?)null : nomenclatureInfo.C1CCharacteristics.Select(x => x.C1CCharacteristicID).FirstOrDefault();
            var nomenclatureName = nomenclatureInfo.Name + " " + nomenclatureInfo.C1CCharacteristics.Select(x => x.Name).FirstOrDefault();

            if (WithdrawalMaterialsIn.FirstOrDefault(d => d.NomenclatureID == nomenclatureInfo.C1CNomenclatureID) == null)
                WithdrawalMaterialsIn.Add(new WithdrawalMaterial
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
                });
            if (WithdrawalMaterialsOut.FirstOrDefault(d => d.NomenclatureID == nomenclatureInfo.C1CNomenclatureID) == null)
                WithdrawalMaterialsOut.Add(new WithdrawalMaterial
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
                });
            if (WithdrawalMaterialsRemainderAtBegin.FirstOrDefault(d => d.NomenclatureID == nomenclatureInfo.C1CNomenclatureID) == null)
                WithdrawalMaterialsRemainderAtBegin.Add(new WithdrawalMaterial
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
                });
            if (WithdrawalMaterialsRemainderAtEnd.FirstOrDefault(d => d.NomenclatureID == nomenclatureInfo.C1CNomenclatureID) == null)
                WithdrawalMaterialsRemainderAtEnd.Add(new WithdrawalMaterial
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
                });
            if (WithdrawalMaterials.FirstOrDefault(d => d.NomenclatureID == nomenclatureInfo.C1CNomenclatureID) == null)
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
                });
        }


        public void MaterialChanged(int selectedMaterialTabIndex, WithdrawalMaterial selectedMaterial)//, List<Guid> productionProductCharacteristicIDs)
        {
            var materialIn = WithdrawalMaterialsIn.Where(d => d.NomenclatureID == selectedMaterial?.NomenclatureID && (d.CharacteristicID == selectedMaterial?.CharacteristicID || (d.CharacteristicID == null && selectedMaterial?.CharacteristicID == null))).Sum(d => d.Quantity);
            var materialOut = WithdrawalMaterialsOut.Where(d => d.NomenclatureID == selectedMaterial?.NomenclatureID && (d.CharacteristicID == selectedMaterial?.CharacteristicID || (d.CharacteristicID == null && selectedMaterial?.CharacteristicID == null))).Sum(d => d.Quantity);
            var materialRemainderAtBegin = WithdrawalMaterialsRemainderAtBegin.Where(d => d.NomenclatureID == selectedMaterial?.NomenclatureID && (d.CharacteristicID == selectedMaterial?.CharacteristicID || (d.CharacteristicID == null && selectedMaterial?.CharacteristicID == null))).Sum(d => d.Quantity);
            var materialRemainderAtEnd = WithdrawalMaterialsRemainderAtEnd.Where(d => d.NomenclatureID == selectedMaterial?.NomenclatureID && (d.CharacteristicID == selectedMaterial?.CharacteristicID || (d.CharacteristicID == null && selectedMaterial?.CharacteristicID == null))).Sum(d => d.Quantity);
            var materialBaseIn = WithdrawalMaterialsIn.Where(d => d.NomenclatureID == selectedMaterial?.NomenclatureID && (d.CharacteristicID == selectedMaterial?.CharacteristicID || (d.CharacteristicID == null && selectedMaterial?.CharacteristicID == null))).Sum(d => d.BaseQuantity);
            var materialBaseOut = WithdrawalMaterialsOut.Where(d => d.NomenclatureID == selectedMaterial?.NomenclatureID && (d.CharacteristicID == selectedMaterial?.CharacteristicID || (d.CharacteristicID == null && selectedMaterial?.CharacteristicID == null))).Sum(d => d.BaseQuantity);
            var materialBaseRemainderAtBegin = WithdrawalMaterialsRemainderAtBegin.Where(d => d.NomenclatureID == selectedMaterial?.NomenclatureID && (d.CharacteristicID == selectedMaterial?.CharacteristicID || (d.CharacteristicID == null && selectedMaterial?.CharacteristicID == null))).Sum(d => d.BaseQuantity);
            var materialBaseRemainderAtEnd = WithdrawalMaterialsRemainderAtEnd.Where(d => d.NomenclatureID == selectedMaterial?.NomenclatureID && (d.CharacteristicID == selectedMaterial?.CharacteristicID || (d.CharacteristicID == null && selectedMaterial?.CharacteristicID == null))).Sum(d => d.BaseQuantity);
            var withdrawalMaterial = WithdrawalMaterials.FirstOrDefault(d => d.NomenclatureID == selectedMaterial?.NomenclatureID && (d.CharacteristicID == selectedMaterial?.CharacteristicID || (d.CharacteristicID == null && selectedMaterial?.CharacteristicID == null)));// && d.CharacteristicID == selectedMaterial?.CharacteristicID);
            //productionProductCharacteristicIDs = new List<Guid>(Spools
            //        .Select(p => p.CharacteristicID).Distinct().ToList());
            if (selectedMaterialTabIndex != 4)
            {
                if (withdrawalMaterial == null)
                {
                    withdrawalMaterial = new WithdrawalMaterial(new ArrayList() { ProductionProductCharacteristicIDs, PlaceID })
                    {
                        NomenclatureID = selectedMaterial.NomenclatureID,
                        NomenclatureName = selectedMaterial.NomenclatureName,
                        CharacteristicID = selectedMaterial.CharacteristicID,
                        DocWithdrawalMaterialID = SqlGuidUtil.NewSequentialid(),
                        Quantity = materialRemainderAtBegin + materialIn - materialOut - materialRemainderAtEnd,
                        BaseQuantity = materialBaseRemainderAtBegin + materialBaseIn - materialBaseOut - materialBaseRemainderAtEnd,
                        MeasureUnit = selectedMaterial.MeasureUnit,
                        MeasureUnitID = selectedMaterial.MeasureUnitID,
                        WithdrawByFact = selectedMaterial.WithdrawByFact
                    };
                    WithdrawalMaterials.Add(withdrawalMaterial);
                }
                else
                {
                    withdrawalMaterial.Quantity = materialRemainderAtBegin + materialIn - materialOut - materialRemainderAtEnd;
                    withdrawalMaterial.BaseQuantity = materialBaseRemainderAtBegin + materialBaseIn - materialBaseOut - materialBaseRemainderAtEnd;

                }
                selectedMaterial = withdrawalMaterial;
                var items = WithdrawalMaterials.ToArray();
                foreach (var item in items)
                {
                    var qIn = WithdrawalMaterialsIn.Where(d => d.NomenclatureID == item.NomenclatureID && (d.CharacteristicID == item.CharacteristicID || (d.CharacteristicID == null && item.CharacteristicID == null))).Sum(d => d.Quantity);
                    var qOut = WithdrawalMaterialsOut.Where(d => d.NomenclatureID == item.NomenclatureID && (d.CharacteristicID == item.CharacteristicID || (d.CharacteristicID == null && item.CharacteristicID == null))).Sum(d => d.Quantity);
                    var qBegin = WithdrawalMaterialsRemainderAtBegin.Where(d => d.NomenclatureID == item.NomenclatureID && (d.CharacteristicID == item.CharacteristicID || (d.CharacteristicID == null && item.CharacteristicID == null))).Sum(d => d.Quantity);
                    var qEnd = WithdrawalMaterialsRemainderAtEnd.Where(d => d.NomenclatureID == item.NomenclatureID && (d.CharacteristicID == item.CharacteristicID || (d.CharacteristicID == null && item.CharacteristicID == null))).Sum(d => d.Quantity);
                    if (qBegin + qIn - qOut - qEnd == 0)
                        WithdrawalMaterials.Remove(item);
                }
            }
            //else
            //{
            //    selectedMaterial = SelectedWithdrawalMaterial;
            //}
            var material = WithdrawalMaterials.Where(d => d.NomenclatureID == selectedMaterial?.NomenclatureID && (d.CharacteristicID == selectedMaterial?.CharacteristicID || (d.CharacteristicID == null && selectedMaterial?.CharacteristicID == null))).Sum(d => d.Quantity);
            var materialBase = WithdrawalMaterials.Where(d => d.NomenclatureID == selectedMaterial?.NomenclatureID && (d.CharacteristicID == selectedMaterial?.CharacteristicID || (d.CharacteristicID == null && selectedMaterial?.CharacteristicID == null))).Sum(d => d.BaseQuantity);
            if (selectedMaterial.Quantity == (materialRemainderAtBegin + materialIn - materialOut - materialRemainderAtEnd))
            {
                var items = WithdrawalMaterials.Where(d => d.DocWithdrawalMaterialID != selectedMaterial?.DocWithdrawalMaterialID && d.NomenclatureID == selectedMaterial?.NomenclatureID && (d.CharacteristicID == selectedMaterial?.CharacteristicID || (d.CharacteristicID == null && selectedMaterial?.CharacteristicID == null))).ToArray();
                foreach (var item in items)
                {
                    WithdrawalMaterials.Remove(item);
                }
            }
            else
            {
                if (materialRemainderAtBegin + materialIn - materialOut - materialRemainderAtEnd - material != 0)
                    WithdrawalMaterials.Add(new WithdrawalMaterial(new ArrayList() { ProductionProductCharacteristicIDs, PlaceID })
                    {
                        NomenclatureID = selectedMaterial.NomenclatureID,
                        NomenclatureName = selectedMaterial.NomenclatureName,
                        CharacteristicID = selectedMaterial.CharacteristicID,
                        DocWithdrawalMaterialID = SqlGuidUtil.NewSequentialid(),
                        Quantity = materialRemainderAtBegin + materialIn - materialOut - materialRemainderAtEnd - material,
                        BaseQuantity = materialBaseRemainderAtBegin + materialBaseIn - materialBaseOut - materialBaseRemainderAtEnd - materialBase,
                        MeasureUnit = selectedMaterial.MeasureUnit,
                        MeasureUnitID = selectedMaterial.MeasureUnitID,
                        WithdrawByFact = selectedMaterial.WithdrawByFact
                    });
            }

            
            RaisePropertiesChanged("DocCloseShiftWithdrawalMaterials.WithdrawalMaterials");
        }

        
    }
}
