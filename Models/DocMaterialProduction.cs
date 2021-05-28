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
    public class DocMaterialProduction : ViewModelBase
    {
        public DocMaterialProduction(int placeID, int shiftID, DateTime closeDate, DocMaterialTankGroupContainer tankGroupContainer)
        {
            GammaBase = DB.GammaDb;
            PlaceID = placeID;
            ShiftID = shiftID;
            CloseDate = closeDate;
            TankGroupContainer = tankGroupContainer;
            //Messenger.Default.Register<RecalcQuantityFromTankReaminderMessage>(this, RecalcQuantityFromTankReaminder);
            Messenger.Default.Register<RecalcMaterialProductionQuantityEndFromTankReaminderMessage>(this, RecalcQuantityEndFromTankReaminder);
            IsVisibleColumnQunatityIn = GammaBase.DocMaterialTankGroups.Any(t => t.PlaceID == placeID && (t.DocMaterialProductionTypeID == (int)DocMaterialProductionTypes.In || t.DocMaterialProductionTypeID == (int)DocMaterialProductionTypes.InToCompositionTank));
            IsVisibleColumnQuantityGRVAtEnd = GammaBase.DocMaterialTankGroups.Any(t => t.PlaceID == placeID && t.Name.Contains("ГРВ"));
        }

        private int PlaceID;
        private int ShiftID;
        DateTime CloseDate;

        private GammaEntities GammaBase { get; }

        public List<DocMaterialTankGroups> TankGroups { get; set; }

        public DocMaterialTankGroupContainer TankGroupContainer { get; set; }

        private ItemsChangeObservableCollection<DocMaterialProductionCompositionCalculationItem> _docMaterialProductionCompositionCalculations = new ItemsChangeObservableCollection<DocMaterialProductionCompositionCalculationItem>();

        public ItemsChangeObservableCollection<DocMaterialProductionCompositionCalculationItem> DocMaterialProductionCompositionCalculations
        {
            get { return _docMaterialProductionCompositionCalculations; }
            set
            {
                _docMaterialProductionCompositionCalculations = value;
                RaisePropertiesChanged("DocMaterialProductions.DocMaterialProductionCompositionCalculations");
            }
        }

        public bool IsVisibleColumnQunatityIn { get; set; } = false;
        public bool IsVisibleColumnQuantityGRVAtEnd { get; set; } = false;

        private List<Guid> ProductionProductCharacteristicIDs { get; set; }

        public List<Doc> Docs { get; private set; }

        public bool CheckSetNotSendMaterialIntoNextPlace (bool value)
        {
            if (DocMaterialProductionCompositionCalculations == null)
                return false;
            else
            {
                var s1 = DocMaterialProductionCompositionCalculations.Sum(m => m.QuantityRemainderAtBegin);
                var s2 = DocMaterialProductionCompositionCalculations.Sum(m => m.QuantityIn);
                var s3 = DocMaterialProductionCompositionCalculations.Sum(m => m.QuantityDismiss);
                var s4 = DocMaterialProductionCompositionCalculations.Sum(m => m.QuantityRemainderAtEnd);
                var s5 = TankGroupContainer.Quantity;
                var b1 = s1 + s2 + s3 <= s5;
                return !value || (value && DocMaterialProductionCompositionCalculations.Count != 0 && (DocMaterialProductionCompositionCalculations.Sum(m => m.QuantityRemainderAtBegin) + DocMaterialProductionCompositionCalculations.Sum(m => m.QuantityIn) + DocMaterialProductionCompositionCalculations.Sum(m => m.QuantityDismiss) <= DocMaterialProductionCompositionCalculations.Sum(m => m.QuantityRemainderAtEnd)));
            }

        }

        public bool _isNotSendMaterialIntoNextPlace { get; set; } = false;
        public bool IsNotSendMaterialIntoNextPlace
        {
            get { return _isNotSendMaterialIntoNextPlace; }
            set
            {
                _isNotSendMaterialIntoNextPlace = value;
                //if (value)
                {
                    if (!isLoading)
                    {
                        foreach (var item in DocMaterialProductionCompositionCalculations)
                        {
                            item.IsNotSendMaterialIntoNextPlace = value;
                        }
                        if (!value)
                        {
                            foreach (var item in DocMaterialProductionCompositionCalculations)
                            {
                                TankGroupContainer.RefreshComposition(item.NomenclatureID, item.ParentID, item.IsFullSendMaterialIntoNextPlace ? 0 : ((item.QuantityDismiss ?? 0) + (item.QuantityRemainderAtBegin ?? 0)), item.IsFullSendMaterialIntoNextPlace ? 0 : item.QuantityIn, item.IsNotSendMaterialIntoNextPlace, false);
                            }
                            TankGroupContainer.RecalcAllNomenclatureInComposition(true);
                            //RecalcQuantityEndFromTankReaminder(DocMaterialProductionCompositionCalculations.FirstOrDefault().NomenclatureID, DocMaterialProductionCompositionCalculations.FirstOrDefault().QuantityRemainderAtEnd ?? 0);
                        }
                        var ritem = new DocMaterialProductionCompositionCalculationItem { WithdrawByFact = false };
                        DocMaterialProductionCompositionCalculations.Add(ritem);
                        DocMaterialProductionCompositionCalculations.Remove(ritem);
                    }
                }
                RaisePropertiesChanged("DocMaterialProductionCompositionCalculations");
            }
        }

        private bool isLoading { get; set; } = false;

        public void LoadProductionMaterials(Guid docId, List<Guid> productionProductCharacteristicIDs)
        {
            isLoading = true;
            ProductionProductCharacteristicIDs = productionProductCharacteristicIDs;
            DocMaterialProductionCompositionCalculations = new ItemsChangeObservableCollection<DocMaterialProductionCompositionCalculationItem>(GammaBase.vDocMaterialProductionCompositionCalculations
                    .Where(dm => dm.DocID == docId)
                    .Select(d => new DocMaterialProductionCompositionCalculationItem
                    {
                        CharacteristicID = d.C1CCharacteristicID,
                        NomenclatureID = d.C1CNomenclatureID,
                        NomenclatureName = d.NomenclatureName,
                        MeasureUnit = d.MeasureUnitName,
                        MeasureUnitID = d.C1CMeasureUnitID,
                        QuantityRemainderAtBegin = d.QuantityRemainderAtBegin,
                        QuantityIn = d.QuantityIn,
                        QuantityRemainderAtEnd = d.QuantityRemainderAtEnd,
                        QuantityDismiss = d.QuantityDismiss,
                        QuantityRemainderInGRVAtEnd = d.QuantityRemainderInGRAVAtEnd,
                        QuantitySend = d.QuantitySend,
                        QuantityIsReadOnly = d.QuantityIsReadOnly ?? false,
                        StandardQuantity = d.StandardQuantity,
                        WithdrawByFact = d.WithdrawByFact,
                        ParentID = d.ParentID,
                        ParentName = d.ParentName,
                        IsNotSendMaterialIntoNextPlace = d.IsNotSendMaterialIntoNextPlace ?? false,// ((d.QuantitySend ?? 0) == 0) && ((d.QuantityRemainderAtEnd ?? 0) != 0)
                        IsFullSendMaterialIntoNextPlace = d.IsFullSendMaterialIntoNextPlace ?? false,
                        IsNotCalculatedQuantityRemainderAtEnd = d.IsNotCalculatedQuantityRemainderAtEnd ?? false
                    }).OrderBy(d => d.NomenclatureName));

            //IsNotSendMaterialIntoNextPlace = DocMaterialProductionCompositionCalculations.Count != 0 && DocMaterialProductionCompositionCalculations.Max(m => m.IsNotSendMaterialIntoNextPlace);
            IsNotCalculatedQuantityRemainderAtEnd = DocMaterialProductionCompositionCalculations.Count != 0 && DocMaterialProductionCompositionCalculations.Max(m => m.IsNotCalculatedQuantityRemainderAtEnd);
            IsNotSendMaterialIntoNextPlace = DocMaterialProductionCompositionCalculations.Count != 0 && DocMaterialProductionCompositionCalculations.Min(m => m.IsNotSendMaterialIntoNextPlace);
            //foreach (var item in DocMaterialProductionCompositionCalculations)
            //{
            //    TankGroupContainer.AddComposition(item.NomenclatureID, item.ParentID, item.IsFullSendMaterialIntoNextPlace ? 0 : ((item.QuantityDismiss ?? 0) + (item.QuantityRemainderAtBegin ?? 0)), item.IsFullSendMaterialIntoNextPlace ? 0 : item.QuantityIn);                
            //}
            //var isNotSend = true;
            foreach (var item in DocMaterialProductionCompositionCalculations)
            {
                //    if (!item.IsNotSendMaterialIntoNextPlace)
                //        isNotSend = false;
                TankGroupContainer.RefreshComposition(item.NomenclatureID, item.ParentID, item.IsFullSendMaterialIntoNextPlace ? 0 : ((item.QuantityDismiss ?? 0) + (item.QuantityRemainderAtBegin ?? 0)), item.IsFullSendMaterialIntoNextPlace ? 0 : item.QuantityIn, item.IsNotSendMaterialIntoNextPlace, false);
            }

            //if (isNotSend)
            //    IsNotSendMaterialIntoNextPlace = isNotSend;

            var doc = GammaBase.Docs.First(d => d.DocID == docId);
            Docs = doc.DocMaterialProductDocs.Select(dc => new Doc() { DocID = dc.DocID, Date = dc.Date, DocTypeID = dc.DocTypeID, Number = dc.Number, Person = dc.Persons?.Name, Place = dc.Places?.Name, ShiftID = dc.ShiftID ?? 0, User = dc.Users?.Name, IsConfirmed = dc.IsConfirmed }).ToList();
            isLoading = false;

        }

        public void SetProductionProductCharacteristics(List<Guid> productionProductCharacteristicIDs)
        {
            ProductionProductCharacteristicIDs = productionProductCharacteristicIDs;
        }

        private void FillEndProducts(bool IsFillEnd = true)
        {
            if (IsFillEnd)
                using (var gammaBase = DB.GammaDb)
                {
                }
        }

        public void FillProductionMaterials(List<Guid> productionProductCharacteristicIDs, bool IsFillEnd = true)
        {
            ProductionProductCharacteristicIDs = productionProductCharacteristicIDs;
            FillProductionMaterials(IsFillEnd);
        }

        public void FillProductionMaterials(bool IsFillEnd = true)
        {
            {
                Clear(IsFillEnd);

                bool isCompositionCalculationParameter = true;
                var docMaterialProductions = new ItemsChangeObservableCollection<DocMaterialProductionCompositionCalculationItem>(DocMaterialProductionCompositionCalculations);
                DocMaterialProductionCompositionCalculations.Clear();
                foreach (var item in docMaterialProductions)
                {
                    DocMaterialProductionCompositionCalculations.Add(item);
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
                    var materialItem = DocMaterialProductionCompositionCalculations.FirstOrDefault(d => d.NomenclatureID == addedItem.NomenclatureID && (d.CharacteristicID == addedItem.CharacteristicID || (d.CharacteristicID == null && addedItem.CharacteristicID == null)));
                    if (materialItem == null)
                    {
                        DocMaterialProductionCompositionCalculations.Add(new DocMaterialProductionCompositionCalculationItem()
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

                    TankGroupContainer.RefreshComposition(addedItem.NomenclatureID, addedItem.ParentID, (materialItem?.QuantityDismiss ?? 0) + (materialItem?.QuantityRemainderAtBegin ?? 0), materialItem?.QuantityIn, null, true);
                }
                    //TankGroupContainer.RecalcAllNomenclatureInComposition();

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
                                BaseQuantity = m.Quantity,
                                MeasureUnit = m.MeasureUnit,
                                MeasureUnitID = m.MeasureUnitID,
                                WithdrawByFact = m.WithdrawByFact,
                                ParentID = m.ParentID,
                                ParentName = m.ParentName
                            }).OrderBy(m => m.NomenclatureName));

                foreach (WithdrawalMaterialBaseItem addedItem in materialsRemainderAtBegin.Where(x => x.Quantity != 0))
                {
                    var item = DocMaterialProductionCompositionCalculations.FirstOrDefault(d => d.NomenclatureID == addedItem.NomenclatureID && (d.CharacteristicID == addedItem.CharacteristicID || (d.CharacteristicID == null && addedItem.CharacteristicID == null)));
                    if (item == null)
                    {
                        var standardQuantity = DocMaterialProductionCompositionCalculations.Where(d => d.AvailableNomenclatures.Any(n => n.NomenclatureID == addedItem.NomenclatureID && (n.CharacteristicID == addedItem.CharacteristicID || (n.CharacteristicID == null && addedItem.CharacteristicID == null)))).FirstOrDefault();
                        DocMaterialProductionCompositionCalculations.Add(new DocMaterialProductionCompositionCalculationItem()
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
                            StandardQuantity = standardQuantity?.StandardQuantity,
                            ParentID = addedItem.ParentID,
                            ParentName = addedItem.ParentName
                        });
                        TankGroupContainer.RefreshComposition(addedItem.NomenclatureID, addedItem.ParentID, addedItem.Quantity, null, null, true);
                    }
                    else
                    {
                        item.QuantityRemainderAtBegin = addedItem.Quantity;
                        TankGroupContainer.RefreshComposition(addedItem.NomenclatureID, addedItem.ParentID, (item.QuantityDismiss ?? 0) + (item.QuantityRemainderAtBegin ?? 0), item.QuantityIn, null, true);
                    }
                }

                Guid? fromDocID = null;
                
                if (Docs?.Where(d => d.DocTypeID == (int)DocTypes.DocMaterialProduction).FirstOrDefault() != null)
                {
                    fromDocID = Docs.FirstOrDefault(d => d.DocTypeID == (int)DocTypes.DocMaterialProduction).DocID;
                }
                else
                {
                    fromDocID = DB.GetDocMaterialInFromDocID(PlaceID, ShiftID, CloseDate, ids);
                    if (fromDocID != null)
                    {
                        var doc = GammaBase.Docs.Where(d => d.DocID == fromDocID).FirstOrDefault();
                        if (doc != null)
                        {
                            //var addedDoc = new Doc(doc.DocID, doc.Number, doc.Date, (doc.ShiftID ?? 0), doc.Places.Name, doc.Users.Name, doc.Persons.Name, doc.IsConfirmed, doc.DocTypeID);
                            var addedDoc = new Doc();
                            addedDoc.DocID = doc.DocID;
                            addedDoc.Number = doc.Number;
                            addedDoc.Date = doc.Date;
                            addedDoc.ShiftID = doc.ShiftID ?? 0;
                            addedDoc.Place = doc.Places?.Name;
                            addedDoc.User = doc.Users?.Name;
                            addedDoc.Person = doc.Persons?.Name;
                            addedDoc.IsConfirmed = doc.IsConfirmed;
                            addedDoc.DocTypeID = doc.DocTypeID;
                            if (Docs == null)
                                Docs = new List<Doc>() { addedDoc };
                            else
                                Docs.Add(addedDoc);
                            //Docs.Add(new Doc { DocID = doc.DocID, Date = doc.Date, DocTypeID = doc.DocTypeID, Number = doc.Number, Person = doc.Persons.Name, Place = doc.Places.Name, ShiftID = doc.ShiftID ?? 0, User = doc.Users.Name, IsConfirmed = doc.IsConfirmed } );
                        }
                    }
                }
                if (fromDocID != null && fromDocID != Guid.Empty)
                { 
                    IsVisibleColumnQunatityIn = true;
                    var materialsIn =
                            new ItemsChangeObservableCollection<WithdrawalMaterialBaseItem>(
                                GammaBase.FillDocMaterialProductionsIn(PlaceID, ShiftID, CloseDate, isCompositionCalculationParameter, fromDocID)
                                //.Take(0)    
                                .Select(m => new WithdrawalMaterialBaseItem()
                                {
                                    NomenclatureID = (Guid)m.NomenclatureID,
                                    CharacteristicID = m.CharacteristicID,
                                    NomenclatureName = m.NomenclatureName,
                                    QuantityIsReadOnly = (m.QuantityIsReadOnly == 1),
                                    Quantity = m.Quantity,
                                    BaseQuantity = m.Quantity,
                                    MeasureUnit = m.MeasureUnit,
                                    MeasureUnitID = m.MeasureUnitID,
                                    WithdrawByFact = m.WithdrawByFact,
                                    ParentID = m.ParentID,
                                    ParentName = m.ParentName
                                }).OrderBy(m => m.NomenclatureName));

                    foreach (WithdrawalMaterialBaseItem addedItem in materialsIn.Where(x => x.Quantity != 0))
                    {
                        var item = DocMaterialProductionCompositionCalculations.FirstOrDefault(d => d.NomenclatureID == addedItem.NomenclatureID && (d.CharacteristicID == addedItem.CharacteristicID || (d.CharacteristicID == null && addedItem.CharacteristicID == null)));
                        if (item == null)
                        {
                            var standardQuantity = DocMaterialProductionCompositionCalculations.Where(d => d.AvailableNomenclatures.Any(n => n.NomenclatureID == addedItem.NomenclatureID && (n.CharacteristicID == addedItem.CharacteristicID || (n.CharacteristicID == null && addedItem.CharacteristicID == null)))).FirstOrDefault();
                            DocMaterialProductionCompositionCalculations.Add(new DocMaterialProductionCompositionCalculationItem()
                            {
                                NomenclatureID = (Guid)addedItem.NomenclatureID,
                                CharacteristicID = addedItem.CharacteristicID,
                                NomenclatureName = addedItem.NomenclatureName,
                                QuantityIsReadOnly = addedItem.QuantityIsReadOnly,
                                QuantityIn = addedItem.Quantity,
                                DocMaterialProductionItemID = SqlGuidUtil.NewSequentialid(),
                                MeasureUnit = addedItem.MeasureUnit,
                                MeasureUnitID = addedItem.MeasureUnitID,
                                WithdrawByFact = addedItem.WithdrawByFact,
                                NomenclatureKindID = addedItem.NomenclatureKindID,
                                StandardQuantity = standardQuantity?.StandardQuantity,
                                ParentID = addedItem.ParentID,
                                ParentName = addedItem.ParentName
                            });
                            TankGroupContainer.RefreshComposition(addedItem.NomenclatureID, addedItem.ParentID, null, addedItem.Quantity, null, true);
                        }
                        else
                        {
                            item.QuantityIn = addedItem.Quantity;
                            TankGroupContainer.RefreshComposition(addedItem.NomenclatureID, addedItem.ParentID, (item.QuantityDismiss ?? 0) + (item.QuantityRemainderAtBegin ?? 0), item.QuantityIn, null, true);
                        }
                        
                    }
                    //TankGroupContainer.RecalcAllNomenclatureInComposition();
                }

            }
        }

        public void ClearFromButton(bool IsFillEnd = true)
        {
            Docs?.Clear();
            DocMaterialProductionCompositionCalculations?.Clear();
            var item = new DocMaterialProductionCompositionCalculationItem { WithdrawByFact = false };
            DocMaterialProductionCompositionCalculations.Add(item);
            DocMaterialProductionCompositionCalculations.Remove(item);
        }
        
        public void Clear(bool IsFillEnd = true)
        {
            foreach (var item in DocMaterialProductionCompositionCalculations)
            {
                if (item.QuantityIsReadOnly)
                {
                    item.QuantityIn = null;
                    //item.QuantityOut = null;
                    //item.QuantityUtil = null;
                }
                if (IsFillEnd) item.QuantityRemainderAtEnd = null;
                item.StandardQuantity = null;
            }
            var items = DocMaterialProductionCompositionCalculations?.Where(d => !(d.QuantityDismiss > 0 || d.QuantityRemainderInGRVAtEnd > 0)).ToArray();
            if (items?.Count() > 0)
                foreach (var item in items)
                {
                    DocMaterialProductionCompositionCalculations.Remove(item);
                }
            else
            {
                var item = new DocMaterialProductionCompositionCalculationItem { WithdrawByFact = false };
                DocMaterialProductionCompositionCalculations.Add(item);
                DocMaterialProductionCompositionCalculations.Remove(item);
            }
            
        }

        public void RecalcQuantityEndFromTankReaminder(RecalcMaterialProductionQuantityEndFromTankReaminderMessage msg)
        {
            RecalcQuantityEndFromTankReaminder(msg.NomenclatureID, msg.Quantity);
        }

        public void RecalcQuantityEndFromTankReaminder(Guid nomenclatureID, decimal quantity)
        {
            var docMaterialProduction = DocMaterialProductionCompositionCalculations.Where(p => p.NomenclatureID == nomenclatureID).FirstOrDefault();
            if (docMaterialProduction != null)
                docMaterialProduction.QuantityRemainderAtEnd = Math.Round(quantity,3);
            var sumQuantityRemainderAtEnd = DocMaterialProductionCompositionCalculations.Sum(p => p.QuantityRemainderAtEnd);
            foreach (var item in DocMaterialProductionCompositionCalculations)
                item.SumQuantityRemainderAtEnd = sumQuantityRemainderAtEnd ?? 0;
            var n = new DocMaterialProductionCompositionCalculationItem { WithdrawByFact = false };
            DocMaterialProductionCompositionCalculations.Add(n);
            DocMaterialProductionCompositionCalculations.Remove(n);
        }
       
        public void MaterialNomenclatureChanged(C1CNomenclature nomenclatureInfo)//, List<Guid> productionProductCharacteristicIDs)
        {
            //В сырье не учитываем характеристики
            //var characteristicID = nomenclatureInfo.C1CCharacteristics.Select(x => x.C1CCharacteristicID).FirstOrDefault() == Guid.Empty ? (Guid?)null : nomenclatureInfo.C1CCharacteristics.Select(x => x.C1CCharacteristicID).FirstOrDefault();
            var nomenclatureName = nomenclatureInfo.Name;// + " " + nomenclatureInfo.C1CCharacteristics.Select(x => x.Name).FirstOrDefault();
            var parentID = nomenclatureInfo.C1CParentID;
            var parentName = parentID == null ? "" : GammaBase.C1CNomenclature.Where(n => n.C1CNomenclatureID == nomenclatureInfo.C1CParentID).FirstOrDefault().Name;
            var measureUnitID = (parentName == "Целлюлоза" || parentName == "Брак и Тех. Отходы" || parentName == "Макулатура") && nomenclatureInfo.C1CMeasureUnitStorage.Name != "т" ? GammaBase.C1CMeasureUnits.Where(m => m.Name == "т").FirstOrDefault().C1CMeasureUnitID : nomenclatureInfo.C1CMeasureUnitStorage.C1CMeasureUnitID;
            var measureUnitName = GammaBase.C1CMeasureUnits.Where(m => m.C1CMeasureUnitID == measureUnitID).FirstOrDefault().Name;
            if (DocMaterialProductionCompositionCalculations.FirstOrDefault(d => d.NomenclatureID == nomenclatureInfo.C1CNomenclatureID) == null)
                DocMaterialProductionCompositionCalculations.Add(new DocMaterialProductionCompositionCalculationItem
                {
                    NomenclatureID = nomenclatureInfo.C1CNomenclatureID,
                    //CharacteristicID = characteristicID,
                    NomenclatureName = nomenclatureName,
                    QuantityIsReadOnly = false,
                    MeasureUnitID = measureUnitID,
                    MeasureUnit = (measureUnitName == "т" || measureUnitName == "т.") ? "кг  " : measureUnitName,
                    DocMaterialProductionItemID = SqlGuidUtil.NewSequentialid(),
                    WithdrawByFact = true,
                    ParentID = parentID,
                    ParentName = parentName
                });

            
        }


        public void MaterialChanged(int selectedMaterialTabIndex, DocMaterialProductionCompositionCalculationItem selectedMaterial)//, List<Guid> productionProductCharacteristicIDs)
        {

            TankGroupContainer.RefreshComposition(selectedMaterial.NomenclatureID, selectedMaterial.ParentID, selectedMaterial.IsFullSendMaterialIntoNextPlace ? 0 : ((selectedMaterial.QuantityDismiss ?? 0) + (selectedMaterial.QuantityRemainderAtBegin ?? 0)), selectedMaterial.IsFullSendMaterialIntoNextPlace ? 0 : selectedMaterial.QuantityIn, selectedMaterial.IsNotSendMaterialIntoNextPlace, true);

        }

        public bool _isNotCalculatedQuantityRemainderAtEnd { get; set; } = false;
        public bool IsNotCalculatedQuantityRemainderAtEnd
        {
            get { return _isNotCalculatedQuantityRemainderAtEnd; }
            set
            {
                _isNotCalculatedQuantityRemainderAtEnd = value;
                if (!isLoading)
                {
                    foreach (var item in DocMaterialProductionCompositionCalculations)
                    {
                        item.IsNotCalculatedQuantityRemainderAtEnd = value;
                    }
                    if (!value)
                        TankGroupContainer.RecalcAllNomenclatureInComposition(true);
                    {
                        var item = new DocMaterialProductionCompositionCalculationItem { WithdrawByFact = false };
                        DocMaterialProductionCompositionCalculations.Add(item);
                        DocMaterialProductionCompositionCalculations.Remove(item);
                    }
                }
            }
        }
    }
}
