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
            IsVisibleColumnQunatityIn = GammaBase.DocMaterialTankGroups.Any(t => t.PlaceID == placeID && t.DocMaterialProductionTypeID == (int)DocMaterialProductionTypes.In);
            IsVisibleColumnQuantityGRVAtEnd = GammaBase.DocMaterialTankGroups.Any(t => t.PlaceID == placeID && t.Name.Contains("ГРВ"));
        }

        private int PlaceID;
        private int ShiftID;
        DateTime CloseDate;

        private GammaEntities GammaBase { get; }

        public List<DocMaterialTankGroups> TankGroups { get; set; }

        public DocMaterialTankGroupContainer TankGroupContainer { get; set; }

       // public ItemsChangeObservableCollection<WithdrawalMaterial> WithdrawalMaterials = new ItemsChangeObservableCollection<WithdrawalMaterial>();

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

        public void LoadProductionMaterials(Guid docId, List<Guid> productionProductCharacteristicIDs)
        {
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
                        IsNotSendMaterialIntoNextPlace = ((d.QuantitySend ?? 0) == 0) && ((d.QuantityRemainderAtEnd ?? 0) != 0)
                    }).OrderBy(d => d.NomenclatureName));

            IsNotSendMaterialIntoNextPlace = DocMaterialProductionCompositionCalculations.Count != 0 && DocMaterialProductionCompositionCalculations.Max(m => m.IsNotSendMaterialIntoNextPlace);

            foreach (var item in DocMaterialProductionCompositionCalculations)
            {
                TankGroupContainer.AddComposition(item.NomenclatureID, item.QuantityDismiss, item.QuantityIn);
                item.IsNotSendMaterialIntoNextPlace = IsNotSendMaterialIntoNextPlace;
            }

            var doc = GammaBase.Docs.First(d => d.DocID == docId);
            Docs = doc.DocMaterialProductDocs.Select(dc => new Doc() { DocID = dc.DocID, Date = dc.Date, DocTypeID = dc.DocTypeID, Number = dc.Number, Person = dc.Persons?.Name, Place = dc.Places?.Name, ShiftID = dc.ShiftID ?? 0, User = dc.Users?.Name, IsConfirmed = dc.IsConfirmed }).ToList();
            //IsVisibleColumnQunatityIn = Docs.Any(d => d.DocTypeID == (int)DocTypes.DocMaterialProduction);


            ////Получение списка списанных материалов
            //WithdrawalMaterials = new ItemsChangeObservableCollection<WithdrawalMaterial>(GammaBase.DocWithdrawalMaterials
            //    //.Join(GammaBase.Docs, dm => dm.DocWithdrawal.Docs.DocID, dw => dw.DocCloseShift.DocID, (dm, dw) => new { })
            //    .Where(dm => dm.DocWithdrawal.DocCloseShift.FirstOrDefault().DocID == docId)
            //    .Select(d => new WithdrawalMaterial()
            //    {
            //        CharacteristicID = d.C1CCharacteristicID,
            //        NomenclatureID = d.C1CNomenclatureID,
            //        NomenclatureName = d.C1CNomenclature.Name + d.C1CCharacteristicID == null || d.C1CCharacteristicID == Guid.Empty ? string.Empty : " " + d.C1CCharacteristics.Name,
            //        DocWithdrawalMaterialID = d.DocWithdrawalMaterialID,
            //        Quantity = d.Quantity,
            //        BaseQuantity = d.Quantity,
            //        MeasureUnit = d.C1CNomenclature.C1CMeasureUnitStorage.Name,
            //        MeasureUnitID = d.C1CNomenclature.C1CMeasureUnitStorage.C1CMeasureUnitID,
            //        QuantityIsReadOnly = !d.WithdrawByFact ?? true,
            //        ProductionProductCharacteristicID = d.DocWithdrawal.DocProduction.FirstOrDefault().DocProductionProducts.FirstOrDefault().C1CCharacteristicID,
            //        WithdrawByFact = d.WithdrawByFact
            //    }).OrderBy(d => d.NomenclatureName));
            //foreach (var item in WithdrawalMaterials)
            //{
            //    item.PlaceID = PlaceID;
            //    item.SetAvailableProductionProducts = productionProductCharacteristicIDs;

            //}

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

                    TankGroupContainer.RefreshComposition(addedItem.NomenclatureID, materialItem?.QuantityDismiss, materialItem?.QuantityIn);
                }
                    //TankGroupContainer.RecalcAllNomenclatureInComposition();

                var materialsRemainderAtBegin =
                        new ItemsChangeObservableCollection<WithdrawalMaterialBaseItem>(
                            GammaBase.FillDocMaterialProductionsAtBegin(PlaceID, ShiftID, CloseDate, isCompositionCalculationParameter)
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
                    }
                    else
                        item.QuantityRemainderAtBegin = addedItem.Quantity;
                }

                Guid? fromDocID = null;
                
                if (Docs?.Where(d => d.DocTypeID == (int)DocTypes.DocMaterialProduction).FirstOrDefault() != null)
                {
                    fromDocID = Docs.FirstOrDefault(d => d.DocTypeID == (int)DocTypes.DocMaterialProduction).DocID;
                }
                else
                {
                    fromDocID = DB.GetDocMaterialInFromDocID(PlaceID, ShiftID, CloseDate);
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
                            TankGroupContainer.RefreshComposition(addedItem.NomenclatureID, null, addedItem.Quantity);
                        }
                        else
                        {
                            item.QuantityIn = addedItem.Quantity;
                            TankGroupContainer.RefreshComposition(addedItem.NomenclatureID, item.QuantityDismiss, item.QuantityIn);
                        }
                        
                    }
                    //TankGroupContainer.RecalcAllNomenclatureInComposition();
                }
                //if (IsFillEnd)
                //{
                //    var withdrawalMaterialsRemainderAtEnd =
                //            new ItemsChangeObservableCollection<WithdrawalMaterial>(
                //                GammaBase.FillDocCloseShiftMaterialsAtEnd(PlaceID, ShiftID, CloseDate)
                //                //.Take(0)    
                //                .Select(m => new WithdrawalMaterial()
                //                {
                //                    NomenclatureID = (Guid)m.NomenclatureID,
                //                    CharacteristicID = m.CharacteristicID,
                //                    NomenclatureName = m.NomenclatureName,
                //                    QuantityIsReadOnly = m.QuantityIsReadOnly ?? false,
                //                    Quantity = m.Quantity ?? 0,
                //                    BaseQuantity = m.Quantity ?? 0,
                //                    DocWithdrawalMaterialID = SqlGuidUtil.NewSequentialid(),
                //                    MeasureUnit = m.MeasureUnit,
                //                    MeasureUnitID = m.MeasureUnitID,
                //                    WithdrawByFact = m.WithdrawByFact ?? true
                //                }).OrderBy(m => m.NomenclatureName));

                //    var docUnwinderRemainders = GammaBase.DocUnwinderRemainders.Where(r => r.DocWithdrawalID != null && r.Docs1.PlaceID == PlaceID && r.Docs1.ShiftID == ShiftID &&
                //            r.Docs1.Date >= SqlFunctions.DateAdd("hh", -1, DB.GetShiftBeginTime((DateTime)SqlFunctions.DateAdd("hh", -1, CloseDate))) &&
                //            r.Docs1.Date <= SqlFunctions.DateAdd("hh", 1, DB.GetShiftEndTime((DateTime)SqlFunctions.DateAdd("hh", -1, CloseDate))));

                //    foreach (WithdrawalMaterial addedItem in withdrawalMaterialsRemainderAtEnd.Where(x => x.Quantity != 0))
                //    {
                //       /* var endProducts = EndProducts.Where(p => p.CharacteristicID == addedItem.CharacteristicID).Select(p => p.ProductID).ToList();
                //        //var spoolsUnwinderRemainder = endProducts == null || spoolUnwinderRemainders == null ? null : spoolUnwinderRemainders.SpoolRemainders.Where(s => endProducts.Contains((Guid)s.ProductID));
                //        var spoolsUnwinderRemainder = docUnwinderRemainders.Where(r => endProducts.Contains((Guid)r.ProductID));
                //        var endProductsQuantity = spoolsUnwinderRemainder == null ? 0 : EndProducts.Where(p => spoolsUnwinderRemainder.Select(s => s.ProductID).ToList().Contains((Guid) p.ProductID)).Sum(p => p.Quantity);
                //        var spoolUnwinderRemainderQuantity = spoolsUnwinderRemainder?.Count() == 0 ? 0 : spoolsUnwinderRemainder?.Sum(s => s.Quantity) ?? 0;
                //        var item = DocMaterialProductions.FirstOrDefault(d => d.NomenclatureID == addedItem.NomenclatureID && (d.CharacteristicID == addedItem.CharacteristicID || (d.CharacteristicID == null && addedItem.CharacteristicID == null)));
                //        if (item == null)
                //        {
                //            var standardQuantity = DocMaterialProductions.Where(d => d.AvailableNomenclatures.Any(n => n.NomenclatureID == addedItem.NomenclatureID && (n.CharacteristicID == addedItem.CharacteristicID || (n.CharacteristicID == null && addedItem.CharacteristicID == null)))).FirstOrDefault();
                //            DocMaterialProductions.Add(new DocMaterialProductionItem()
                //            {
                //                NomenclatureID = (Guid)addedItem.NomenclatureID,
                //                CharacteristicID = addedItem.CharacteristicID,
                //                NomenclatureName = addedItem.NomenclatureName,
                //                QuantityIsReadOnly = addedItem.QuantityIsReadOnly,
                //                QuantityRemainderAtEnd = addedItem.Quantity - endProductsQuantity + spoolUnwinderRemainderQuantity,
                //                //DocWithdrawalMaterialID = SqlGuidUtil.NewSequentialid(),
                //                MeasureUnit = addedItem.MeasureUnit,
                //                MeasureUnitID = addedItem.MeasureUnitID,
                //                //WithdrawByFact = addedItem.WithdrawByFact,
                //                NomenclatureKindID = addedItem.NomenclatureKindID,
                //                StandardQuantity = standardQuantity?.StandardQuantity
                //            });
                //        }
                //        else
                //        {
                //            item.QuantityRemainderAtEnd = addedItem.Quantity - endProductsQuantity + spoolUnwinderRemainderQuantity;
                //            item.QuantityIsReadOnly = addedItem.QuantityIsReadOnly;
                //        }*/
                //    }
                //}
                ///*
                //foreach (DocMaterialProductionItem addedItem in DocMaterialProductions.Where(x => x.StandardQuantity == null || x.StandardQuantity == 0 ))
                //{
                //    var standardQuantity = DocMaterialProductions.Where(d => d.StandardQuantity > 0 && d.AvailableNomenclatures.Any(n => n.NomenclatureID == addedItem.NomenclatureID && (n.CharacteristicID == addedItem.CharacteristicID || (n.CharacteristicID == null && addedItem.CharacteristicID == null)))).FirstOrDefault();
                //    if (standardQuantity != null)
                //        addedItem.StandardQuantity = standardQuantity.StandardQuantity;
                //}

                //foreach (DocMaterialProductionItem addedItem in DocMaterialProductions.Where(x => ((x.WithdrawByFact ?? true) ? x.QuantityWithdrawalMaterial : x.StandardQuantity) != 0))
                //{
                //    if (WithdrawalMaterials.Count(d => d.NomenclatureID == addedItem.NomenclatureID && (d.CharacteristicID == addedItem.CharacteristicID || (d.CharacteristicID == null && addedItem.CharacteristicID == null))) == 0)
                //        WithdrawalMaterials.Add(new WithdrawalMaterial(new ArrayList() { productionProductCharacteristicIDs, PlaceID })
                //        {
                //            NomenclatureID = addedItem.NomenclatureID,
                //            CharacteristicID = addedItem.CharacteristicID,
                //            NomenclatureName = addedItem.NomenclatureName,
                //            QuantityIsReadOnly = addedItem.QuantityIsReadOnly,
                //            Quantity = ((addedItem.WithdrawByFact ?? true) ? addedItem.QuantityWithdrawalMaterial : addedItem.StandardQuantity) ?? 0,
                //            BaseQuantity = ((addedItem.WithdrawByFact ?? true) ? addedItem.QuantityWithdrawalMaterial : addedItem.StandardQuantity) ?? 0,
                //            DocWithdrawalMaterialID = SqlGuidUtil.NewSequentialid(),
                //            MeasureUnit = addedItem.MeasureUnit,
                //            MeasureUnitID = addedItem.MeasureUnitID,
                //            WithdrawByFact = addedItem.WithdrawByFact
                //        });
                //}
                //*/
            }
        }

        public void ClearFromButton(bool IsFillEnd = true)
        {
            Docs?.Clear();
            foreach (var item in DocMaterialProductionCompositionCalculations)
            {
                item.QuantityIn = null;
            }

                Clear(IsFillEnd);
        }


        public void Clear(bool IsFillEnd = true)
        {
            //WithdrawalMaterials?.Clear();
            
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
            var docMaterialProduction = DocMaterialProductionCompositionCalculations.Where(p => p.NomenclatureID == msg.NomenclatureID).FirstOrDefault();
            if (docMaterialProduction != null)
                docMaterialProduction.QuantityRemainderAtEnd = Math.Round(msg.Quantity,3);
            var n = new DocMaterialProductionCompositionCalculationItem { WithdrawByFact = false };
            DocMaterialProductionCompositionCalculations.Add(n);
            DocMaterialProductionCompositionCalculations.Remove(n);
        }
        /*public void RecalcQuantityFromTankReaminder (RecalcQuantityFromTankReaminderMessage msg)
        {
            var quantityInMaterial = (decimal?)0;
            var docMaterialProductions = DocMaterialProductions.Where(p => (msg.NomenclatureID.Count == 0 || (msg.NomenclatureID.Count > 0 && msg.NomenclatureID.Contains((Guid)p.ParentID))) && !msg.ExceptNomenclatureID.Contains((Guid)p.ParentID));
            switch (msg.DocMaterialProductionTypeID)
            {
                case (3):
                    quantityInMaterial = docMaterialProductions.Sum(m => m.QuantityDismiss);
                    break;
                case (4):
                    quantityInMaterial = docMaterialProductions.Sum(m => m.QuantityIn);
                    break;
            } 
            foreach (var item in docMaterialProductions)
            {
                switch (msg.DocMaterialProductionTypeID)
                {
                    case (3):
                        if ((quantityInMaterial ?? 0) != 0 && (item.QuantityIn ?? 0) != 0)
                            item.QuantityRemainderAtEnd = Math.Round((decimal)(item.QuantityIn / quantityInMaterial * msg.Quantity), 3);
                        else
                            item.QuantityRemainderAtEnd = 0;
                        break;
                    case (4):
                        if ((quantityInMaterial ?? 0) != 0 && (item.QuantityIn ?? 0) != 0)
                            item.QuantityRemainderAtEnd = Math.Round((decimal)(item.QuantityDismiss / quantityInMaterial * msg.Quantity), 3);
                        else
                            item.QuantityRemainderAtEnd = 0;
                        break;
                }
            }
            var n = new DocMaterialProductionItem { WithdrawByFact = false };
            DocMaterialProductions.Add(n);
            DocMaterialProductions.Remove(n);

        }*/

        public void MaterialNomenclatureChanged(C1CNomenclature nomenclatureInfo)//, List<Guid> productionProductCharacteristicIDs)
        {
            //В сырье не учитываем хараеткристики
            //var characteristicID = nomenclatureInfo.C1CCharacteristics.Select(x => x.C1CCharacteristicID).FirstOrDefault() == Guid.Empty ? (Guid?)null : nomenclatureInfo.C1CCharacteristics.Select(x => x.C1CCharacteristicID).FirstOrDefault();
            var nomenclatureName = nomenclatureInfo.Name;// + " " + nomenclatureInfo.C1CCharacteristics.Select(x => x.Name).FirstOrDefault();
            var parentID = nomenclatureInfo.C1CParentID;
            var parentName = parentID == null ? "" : GammaBase.C1CNomenclature.Where(n => n.C1CNomenclatureID == nomenclatureInfo.C1CParentID).FirstOrDefault().Name;

            if (DocMaterialProductionCompositionCalculations.FirstOrDefault(d => d.NomenclatureID == nomenclatureInfo.C1CNomenclatureID) == null)
                DocMaterialProductionCompositionCalculations.Add(new DocMaterialProductionCompositionCalculationItem
                {
                    NomenclatureID = nomenclatureInfo.C1CNomenclatureID,
                    //CharacteristicID = characteristicID,
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


        public void MaterialChanged(int selectedMaterialTabIndex, DocMaterialProductionItem selectedMaterial)//, List<Guid> productionProductCharacteristicIDs)
        {

            TankGroupContainer.RefreshComposition(selectedMaterial.NomenclatureID, selectedMaterial.QuantityDismiss, selectedMaterial.QuantityIn);

            /*
            var docCloseShiftMaterial = 0;// DocMaterialProductions.Where(d => d.NomenclatureID == selectedMaterial?.NomenclatureID && (d.CharacteristicID == selectedMaterial?.CharacteristicID || (d.CharacteristicID == null && selectedMaterial?.CharacteristicID == null))).Sum(d => ((d.WithdrawByFact ?? true) ? d.QuantityWithdrawalMaterial : d.StandardQuantity)) ?? 0;
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
                    //if (DocMaterialProductions.Where(d => d.NomenclatureID == item.NomenclatureID && (d.CharacteristicID == item.CharacteristicID || (d.CharacteristicID == null && item.CharacteristicID == null))).Sum(d => d.QuantityWithdrawalMaterial) == 0)
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

    */
            //RaisePropertiesChanged("DocCloseShiftWithdrawalMaterials.WithdrawalMaterials");
        }

        public bool _isNotSendMaterialIntoNextPlace { get; set; } = false;
        public bool IsNotSendMaterialIntoNextPlace
        {
            get { return _isNotSendMaterialIntoNextPlace; }
            set
            {
                _isNotSendMaterialIntoNextPlace = value;
                foreach (var item in DocMaterialProductionCompositionCalculations)
                {
                    item.IsNotSendMaterialIntoNextPlace = _isNotSendMaterialIntoNextPlace;
                }
                TankGroupContainer.RecalcAllNomenclatureInComposition();
                {
                    var item = new DocMaterialProductionCompositionCalculationItem { WithdrawByFact = false };
                    DocMaterialProductionCompositionCalculations.Add(item);
                    DocMaterialProductionCompositionCalculations.Remove(item);
                }
            }
        }

        /*public void ChangeIsSendIntoNextPlaceDocMaterialProduction()
        {
            foreach (var item in DocMaterialProductionCompositionCalculations)
            {
                item.IsNotSendMaterialIntoNextPlace = !item.IsNotSendMaterialIntoNextPlace;
            }
            TankGroupContainer.RecalcAllNomenclatureInComposition();
            {
                var item = new DocMaterialProductionCompositionCalculationItem { WithdrawByFact = false };
                DocMaterialProductionCompositionCalculations.Add(item);
                DocMaterialProductionCompositionCalculations.Remove(item);
            }
        }*/
    }
}
