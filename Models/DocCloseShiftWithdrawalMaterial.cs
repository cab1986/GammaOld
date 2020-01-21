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
                        QuantityIsReadOnly = d.QuantityIsReadOnly ?? false
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

            //Получение списка утилизированных тамбуров
            UtilizationSpools = new ItemsChangeObservableCollection<PaperBase>(GammaBase.DocCloseShiftUtilizationProducts.Where(d => d.DocID == docId)
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
                    var item = DocCloseShiftMaterials.FirstOrDefault(d => d.NomenclatureID == addedItem.NomenclatureId && (d.CharacteristicID == addedItem.CharacteristicId || (d.CharacteristicID == null && addedItem.CharacteristicId == null)));
                    if (item == null)
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
                            NomenclatureKindID = addedItem.NomenclatureKindID
                        });
                    else
                        item.QuantityIn = (item.QuantityIn ?? 0) + addedItem.Quantity;
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
                    var item = DocCloseShiftMaterials.FirstOrDefault(d => d.NomenclatureID == addedItem.NomenclatureId && (d.CharacteristicID == addedItem.CharacteristicId || (d.CharacteristicID == null && addedItem.CharacteristicId == null)));
                    if (item == null)
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
                            NomenclatureKindID = addedItem.NomenclatureKindID
                        });
                    else
                        item.QuantityOut = (item.QuantityOut ?? 0) + addedItem.Quantity;
                }

                UtilizationSpools = new ItemsChangeObservableCollection<PaperBase>(GammaBase.FillDocCloseShiftUtilizationSpools(PlaceID, ShiftID, CloseDate).Select(p => new PaperBase()
                {
                    CharacteristicID = p.CharacteristicID,
                    NomenclatureID = p.NomenclatureID,
                    Nomenclature = p.NomenclatureName,
                    Number = p.Number,
                    ProductID = p.ProductID,
                    Weight = (p.Quantity ?? 0),// * 1000
                    BaseMeasureUnit = p.BaseMeasureUnit,
                    BaseMeasureUnitID = p.BaseMeasureUnitID
                }));

                foreach (PaperBase addedItem in UtilizationSpools)
                {
                    var item = DocCloseShiftMaterials.FirstOrDefault(d => d.NomenclatureID == addedItem.NomenclatureID && (d.CharacteristicID == addedItem.CharacteristicID || (d.CharacteristicID == null && addedItem.CharacteristicID == null)));
                    if (item == null)
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
                            WithdrawByFact = true
                        });
                    else
                        item.QuantityUtil = (item.QuantityUtil ?? 0) + addedItem.Weight;
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
                    if (DocCloseShiftMaterials.Count(d => d.NomenclatureID == addedItem.NomenclatureID && (d.CharacteristicID == addedItem.CharacteristicID || (d.CharacteristicID == null && addedItem.CharacteristicID == null))) == 0)
                        DocCloseShiftMaterials.Add(new DocCloseShiftMaterial()
                        {
                            NomenclatureID = addedItem.NomenclatureID,
                            CharacteristicID = addedItem.CharacteristicID,
                            NomenclatureName = addedItem.NomenclatureName,
                            QuantityIsReadOnly = addedItem.QuantityIsReadOnly,
                            DocWithdrawalMaterialID = SqlGuidUtil.NewSequentialid(),
                            MeasureUnit = addedItem.MeasureUnit,
                            MeasureUnitID = addedItem.MeasureUnitID,
                            WithdrawByFact = addedItem.WithdrawByFact
                        });
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
                                //QuantityIsReadOnly =  m.QuantityIsReadOnly,
                                Quantity = m.Quantity ?? 0,
                                BaseQuantity = m.Quantity ?? 0,
                                DocWithdrawalMaterialID = SqlGuidUtil.NewSequentialid(),
                                MeasureUnit = m.MeasureUnit,
                                MeasureUnitID = m.MeasureUnitID,
                                WithdrawByFact = m.WithdrawByFact ?? true
                            }).OrderBy(m => m.NomenclatureName));

                foreach (WithdrawalMaterial addedItem in withdrawalMaterialsRemainderAtBegin.Where(x => x.Quantity != 0))
                {
                    var item = DocCloseShiftMaterials.FirstOrDefault(d => d.NomenclatureID == addedItem.NomenclatureID && (d.CharacteristicID == addedItem.CharacteristicID || (d.CharacteristicID == null && addedItem.CharacteristicID == null)));
                    if (item == null)
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
                            NomenclatureKindID = addedItem.NomenclatureKindID
                        });
                    else
                        item.QuantityRemainderAtBegin = addedItem.Quantity;
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
                                //QuantityIsReadOnly = true, //!m.WithdrawByFact ?? true,
                                Quantity = m.Quantity ?? 0,
                                BaseQuantity = m.Quantity ?? 0,
                                DocWithdrawalMaterialID = SqlGuidUtil.NewSequentialid(),
                                MeasureUnit = m.MeasureUnit,
                                MeasureUnitID = m.MeasureUnitID,
                                WithdrawByFact = m.WithdrawByFact ?? true
                            }).OrderBy(m => m.NomenclatureName));

                foreach (WithdrawalMaterial addedItem in withdrawalMaterialsRemainderAtEnd.Where(x => x.Quantity != 0))
                {
                    var item = DocCloseShiftMaterials.FirstOrDefault(d => d.NomenclatureID == addedItem.NomenclatureID && (d.CharacteristicID == addedItem.CharacteristicID || (d.CharacteristicID == null && addedItem.CharacteristicID == null)));
                    if (item == null)
                        DocCloseShiftMaterials.Add(new DocCloseShiftMaterial()
                        {
                            NomenclatureID = (Guid)addedItem.NomenclatureID,
                            CharacteristicID = addedItem.CharacteristicID,
                            NomenclatureName = addedItem.NomenclatureName,
                            QuantityIsReadOnly = addedItem.QuantityIsReadOnly,
                            QuantityRemainderAtEnd = addedItem.Quantity,
                            DocWithdrawalMaterialID = SqlGuidUtil.NewSequentialid(),
                            MeasureUnit = addedItem.MeasureUnit,
                            MeasureUnitID = addedItem.MeasureUnitID,
                            WithdrawByFact = addedItem.WithdrawByFact,
                            NomenclatureKindID = addedItem.NomenclatureKindID
                        });
                    else
                        item.QuantityRemainderAtEnd = addedItem.Quantity;
                }

                foreach (DocCloseShiftMaterial addedItem in DocCloseShiftMaterials.Where(x => x.QuantityWithdrawalMaterial != 0))
                {
                    if (WithdrawalMaterials.Count(d => d.NomenclatureID == addedItem.NomenclatureID && (d.CharacteristicID == addedItem.CharacteristicID || (d.CharacteristicID == null && addedItem.CharacteristicID == null))) == 0)
                        WithdrawalMaterials.Add(new WithdrawalMaterial(new ArrayList() { productionProductCharacteristicIDs, PlaceID })
                        {
                            NomenclatureID = addedItem.NomenclatureID,
                            CharacteristicID = addedItem.CharacteristicID,
                            NomenclatureName = addedItem.NomenclatureName,
                            QuantityIsReadOnly = addedItem.QuantityIsReadOnly,
                            Quantity = addedItem.QuantityWithdrawalMaterial ?? 0,
                            BaseQuantity = addedItem.QuantityWithdrawalMaterial ?? 0,
                            DocWithdrawalMaterialID = SqlGuidUtil.NewSequentialid(),
                            MeasureUnit = addedItem.MeasureUnit,
                            MeasureUnitID = addedItem.MeasureUnitID,
                            WithdrawByFact = addedItem.WithdrawByFact
                        });
                }


            }
        }

        public void Clear()
        {
            WithdrawalMaterials?.Clear();
            foreach (var item in DocCloseShiftMaterials.Where(ps => ps.QuantityIsReadOnly))
            {
                item.QuantityIn = null;
                item.QuantityOut = null;
                item.QuantityRemainderAtEnd = null;
                item.QuantityUtil = null;
            }
            var items = DocCloseShiftMaterials?.Where(d => !(d.QuantityIn > 0 || d.QuantityOut > 0 || d.QuantityRePack > 0 || d.QuantityUtil > 0 || d.QuantityExperimental > 0 || d.QuantityRemainderAtEnd > 0)).ToArray();
            foreach (var item in items)
            {
                DocCloseShiftMaterials.Remove(item);
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
            var docCloseShiftMaterial = DocCloseShiftMaterials.Where(d => d.NomenclatureID == selectedMaterial?.NomenclatureID && (d.CharacteristicID == selectedMaterial?.CharacteristicID || (d.CharacteristicID == null && selectedMaterial?.CharacteristicID == null))).Sum(d => d.QuantityWithdrawalMaterial) ?? 0;
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


            RaisePropertiesChanged("DocCloseShiftWithdrawalMaterials.WithdrawalMaterials");
        }


    }
}
