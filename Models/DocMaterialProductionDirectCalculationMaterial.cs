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
                        ParentName = d.ParentName,
                        QuantitySendAtBegin = d.QuantitySendAtBegin,
                        QuantitySendAtEnd = d.QuantitySendAtEnd,
                        IsNotSendMaterialIntoNextPlace = (d.QuantitySendAtEnd ?? 0) != 0
                    }).OrderBy(d => d.NomenclatureName));

            IsNotSendMaterialIntoNextPlace = Materials.Count != 0 && Materials.Max(m => m.IsNotSendMaterialIntoNextPlace);

            foreach (var item in Materials.Where(m => m.IsNotSendMaterialIntoNextPlace != IsNotSendMaterialIntoNextPlace))
            {
                item.IsNotSendMaterialIntoNextPlace = IsNotSendMaterialIntoNextPlace;
            }

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
                        new ItemsChangeObservableCollection<WithdrawalMaterialBaseItem>(
                            GammaBase.FillDocMaterialProductionsAtBegin(PlaceID, ShiftID, CloseDate, isCompositionCalculationParameter, ids)
                            //.Take(0)    
                            .Select(m => new WithdrawalMaterialBaseItem()
                            {
                                NomenclatureID = (Guid)m.NomenclatureID,
                                CharacteristicID = m.CharacteristicID,
                                NomenclatureName = m.NomenclatureName,
                                QuantityIsReadOnly = (m.QuantityIsReadOnly == 1),
                                Quantity = m.Quantity,
                                BaseQuantity = m.QuantitySend, //не используем базовое кол-во, поэтому пока сохраняем туда Расход на начало
                                //DocWithdrawalMaterialID = SqlGuidUtil.NewSequentialid(),
                                MeasureUnit = m.MeasureUnit,
                                MeasureUnitID = m.MeasureUnitID,
                                WithdrawByFact = m.WithdrawByFact
                            }).OrderBy(m => m.NomenclatureName));

                foreach (WithdrawalMaterialBaseItem addedItem in materialsRemainderAtBegin.Where(x => x.Quantity != 0 || x.BaseQuantity != 0))
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
                            QuantitySendAtBegin = addedItem.BaseQuantity,
                            DocMaterialProductionItemID = SqlGuidUtil.NewSequentialid(),
                            MeasureUnit = addedItem.MeasureUnit,
                            MeasureUnitID = addedItem.MeasureUnitID,
                            WithdrawByFact = addedItem.WithdrawByFact,
                            NomenclatureKindID = addedItem.NomenclatureKindID,
                            StandardQuantity = standardQuantity?.StandardQuantity
                        });
                    }
                    else
                    {
                        item.QuantityRemainderAtBegin = addedItem.Quantity;
                        item.QuantitySendAtBegin = addedItem.BaseQuantity;
                    }
                }

            }
        }

        public void ClearFromButton(bool IsFillEnd = true)
        {
            if (Materials != null)
            {
                Materials?.Clear();
                var item = new DocMaterialProductionDirectCalculationItem { WithdrawByFact = false };
                Materials.Add(item);
                Materials.Remove(item);
            }
        }

        public void Clear(bool IsFillEnd = true)
        {
            foreach (var item in Materials)
            {
                
                item.StandardQuantity = null;
            }
            var items = Materials?.Where(d => !(d.QuantityIn > 0 || d.QuantityOut > 0 || d.QuantityUtil > 0 || d.QuantityExperimental > 0 || d.QuantityRemainderAtEnd > 0 || d.QuantitySendAtEnd > 0)).ToArray();
            if (items?.Count() > 0)
                foreach (var item in items)
                {
                    Materials.Remove(item);
                }
            else
            {
                var item = new DocMaterialProductionDirectCalculationItem { WithdrawByFact = false };
                Materials.Add(item);
                Materials.Remove(item);
            }
        }


        public void MaterialNomenclatureChanged(C1CNomenclature nomenclatureInfo)//, List<Guid> productionProductCharacteristicIDs)
        {
            var characteristicID = nomenclatureInfo.C1CCharacteristics.Select(x => x.C1CCharacteristicID).FirstOrDefault() == Guid.Empty ? (Guid?)null : nomenclatureInfo.C1CCharacteristics.Select(x => x.C1CCharacteristicID).FirstOrDefault();
            var nomenclatureName = nomenclatureInfo.Name + " " + nomenclatureInfo.C1CCharacteristics.Select(x => x.Name).FirstOrDefault();
            var parentID = nomenclatureInfo.C1CParentID;
            var parentName = parentID == null ? "" : GammaBase.C1CNomenclature.Where(n => n.C1CNomenclatureID == nomenclatureInfo.C1CParentID).FirstOrDefault().Name;

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
                    WithdrawByFact = true,
                    ParentID = parentID,
                    ParentName = parentName
                });
        }

        public bool _isNotSendMaterialIntoNextPlace { get; set; } = false;
        public bool IsNotSendMaterialIntoNextPlace
        {
            get { return _isNotSendMaterialIntoNextPlace; }
            set
            {
                _isNotSendMaterialIntoNextPlace = value;
                foreach (var item in Materials)
                {
                    item.IsNotSendMaterialIntoNextPlace = _isNotSendMaterialIntoNextPlace;
                }
                {
                    var item = new DocMaterialProductionDirectCalculationItem { WithdrawByFact = false };
                    Materials.Add(item);
                    Materials.Remove(item);
                }
            }
        }
    }
}
