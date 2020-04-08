﻿// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
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
        public DocMaterialProduction(int placeID, int shiftID, DateTime closeDate)
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

        public ItemsChangeObservableCollection<WithdrawalMaterial> WithdrawalMaterials = new ItemsChangeObservableCollection<WithdrawalMaterial>();

        private ItemsChangeObservableCollection<DocMaterialProductionItem> _docMaterialProductions = new ItemsChangeObservableCollection<DocMaterialProductionItem>();

        public ItemsChangeObservableCollection<DocMaterialProductionItem> DocMaterialProductions
        {
            get { return _docMaterialProductions; }
            set
            {
                _docMaterialProductions = value;
                RaisePropertiesChanged("DocMaterialProductions.DocMaterialProductions");
            }
        }
        private List<Guid> ProductionProductCharacteristicIDs { get; set; }

        public void LoadProductionMaterials(Guid docId, List<Guid> productionProductCharacteristicIDs)
        {
            ProductionProductCharacteristicIDs = productionProductCharacteristicIDs;
            DocMaterialProductions = new ItemsChangeObservableCollection<DocMaterialProductionItem>(GammaBase.vDocMaterialProductions
                    .Where(dm => dm.DocID == docId)
                    .Select(d => new DocMaterialProductionItem
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
                        ParentName = d.ParentName
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

                var docMaterialProductions = new ItemsChangeObservableCollection<DocMaterialProductionItem>(DocMaterialProductions);
                DocMaterialProductions.Clear();
                foreach (var item in docMaterialProductions)
                {
                    DocMaterialProductions.Add(item);
                }
                var ids = String.Join(",", ProductionProductCharacteristicIDs?.ToArray());
                  var MaterialProductionsLoad =
                        new ItemsChangeObservableCollection<DocMaterialProductionItem>(
                            GammaBase.FillDocMaterialProductions(PlaceID, ShiftID, CloseDate, ids)
                            //.Take(0)    
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
                                ParentName = m.ParentName
                            }).OrderBy(m => m.NomenclatureName));


                foreach (DocMaterialProductionItem addedItem in MaterialProductionsLoad)
                {
                    var materialItem = DocMaterialProductions.FirstOrDefault(d => d.NomenclatureID == addedItem.NomenclatureID && (d.CharacteristicID == addedItem.CharacteristicID || (d.CharacteristicID == null && addedItem.CharacteristicID == null)));
                    if (materialItem == null)
                    {
                        DocMaterialProductions.Add(new DocMaterialProductionItem()
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
                            ParentName = addedItem.ParentName
                        });
                    }
                    else
                    {
                        materialItem.StandardQuantity = addedItem.StandardQuantity;
                    };
                }
                
                //var withdrawalMaterialsRemainderAtBegin =
                //        new ItemsChangeObservableCollection<WithdrawalMaterial>(
                //            GammaBase.FillDocCloseShiftMaterialsAtBegin(PlaceID, ShiftID, CloseDate)
                //            //.Take(0)    
                //            .Select(m => new WithdrawalMaterial()
                //            {
                //                NomenclatureID = (Guid)m.NomenclatureID,
                //                CharacteristicID = m.CharacteristicID,
                //                NomenclatureName = m.NomenclatureName,
                //                QuantityIsReadOnly =  m.QuantityIsReadOnly ?? false,
                //                Quantity = m.Quantity ?? 0,
                //                BaseQuantity = m.Quantity ?? 0,
                //                DocWithdrawalMaterialID = SqlGuidUtil.NewSequentialid(),
                //                MeasureUnit = m.MeasureUnit,
                //                MeasureUnitID = m.MeasureUnitID,
                //                WithdrawByFact = m.WithdrawByFact ?? true
                //            }).OrderBy(m => m.NomenclatureName));

                //foreach (WithdrawalMaterial addedItem in withdrawalMaterialsRemainderAtBegin.Where(x => x.Quantity != 0))
                //{
                //    var item = DocMaterialProductions.FirstOrDefault(d => d.NomenclatureID == addedItem.NomenclatureID && (d.CharacteristicID == addedItem.CharacteristicID || (d.CharacteristicID == null && addedItem.CharacteristicID == null)));
                //    if (item == null)
                //    {
                //        //var standardQuantity = DocMaterialProductions.Where(d => d.AvailableNomenclatures.Any(n => n.NomenclatureID == addedItem.NomenclatureID && (n.CharacteristicID == addedItem.CharacteristicID || (n.CharacteristicID == null && addedItem.CharacteristicID == null)))).FirstOrDefault();
                //        DocMaterialProductions.Add(new DocMaterialProductionItem()
                //        {
                //            NomenclatureID = (Guid)addedItem.NomenclatureID,
                //            CharacteristicID = addedItem.CharacteristicID,
                //            NomenclatureName = addedItem.NomenclatureName,
                //            QuantityIsReadOnly = addedItem.QuantityIsReadOnly,
                //            QuantityRemainderAtBegin = addedItem.Quantity,
                //            //DocWithdrawalMaterialID = SqlGuidUtil.NewSequentialid(),
                //            MeasureUnit = addedItem.MeasureUnit,
                //            MeasureUnitID = addedItem.MeasureUnitID,
                //            //WithdrawByFact = addedItem.WithdrawByFact,
                //            NomenclatureKindID = addedItem.NomenclatureKindID//,
                //            //StandardQuantity = standardQuantity?.StandardQuantity
                //        });
                //    }
                //    else
                //        item.QuantityRemainderAtBegin = addedItem.Quantity;
                //}

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

        public void Clear(bool IsFillEnd = true)
        {
            WithdrawalMaterials?.Clear();
            foreach (var item in DocMaterialProductions)
            {
                if (item.QuantityIsReadOnly)
                {
                    item.QuantityIn = null;
                    //item.QuantityOut = null;
                    //item.QuantityUtil = null;
                    if (IsFillEnd) item.QuantityRemainderAtEnd = null;
                }
                item.StandardQuantity = null;
            }
            var items = DocMaterialProductions?.Where(d => !(d.QuantityIn > 0 || d.QuantityDismiss > 0 || d.QuantityRemainderInGRVAtEnd > 0  || d.QuantityRemainderAtEnd > 0)).ToArray();
            foreach (var item in items)
            {
                DocMaterialProductions.Remove(item);
            }
            
        }

        public void RecalcQuantityEndFromUnwinderReaminder (RecalcQuantityEndFromUnwinderReaminderMessage msg)
        {
            var item = DocMaterialProductions.FirstOrDefault(d => d.NomenclatureID == msg.NomenclatureID && (d.CharacteristicID == msg.CharacteristicID || (d.CharacteristicID == null && msg.CharacteristicID == null)));
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
                /*
                var quantityEndProduct = (msg.Quantity == -1 || msg.Quantity == -2) ? 
                    EndProducts.Where(p => p.ProductID == msg.ProductID).Select(s => s.Quantity).FirstOrDefault()
                    : 0 ;
                item.QuantityRemainderAtEnd = (item.QuantityRemainderAtEnd ?? 0) + (msg.Delta ?? 0) + (msg.Quantity == -1 ? quantityEndProduct : msg.Quantity == -2 ? -quantityEndProduct : 0);
                */
                var n = new DocMaterialProductionItem { };// WithdrawByFact = false };
                DocMaterialProductions.Add(n);
                DocMaterialProductions.Remove(n);
            }
        }

        public void MaterialNomenclatureChanged(C1CNomenclature nomenclatureInfo)//, List<Guid> productionProductCharacteristicIDs)
        {
            var characteristicID = nomenclatureInfo.C1CCharacteristics.Select(x => x.C1CCharacteristicID).FirstOrDefault() == Guid.Empty ? (Guid?)null : nomenclatureInfo.C1CCharacteristics.Select(x => x.C1CCharacteristicID).FirstOrDefault();
            var nomenclatureName = nomenclatureInfo.Name + " " + nomenclatureInfo.C1CCharacteristics.Select(x => x.Name).FirstOrDefault();

            if (DocMaterialProductions.FirstOrDefault(d => d.NomenclatureID == nomenclatureInfo.C1CNomenclatureID) == null)
                DocMaterialProductions.Add(new DocMaterialProductionItem
                {
                    NomenclatureID = nomenclatureInfo.C1CNomenclatureID,
                    CharacteristicID = characteristicID,
                    NomenclatureName = nomenclatureName,
                    QuantityIsReadOnly = false,
                    MeasureUnitID = nomenclatureInfo.C1CMeasureUnitStorage.C1CMeasureUnitID,
                    MeasureUnit = nomenclatureInfo.C1CMeasureUnitStorage.Name//,
                    //DocWithdrawalMaterialID = SqlGuidUtil.NewSequentialid(),
                    //WithdrawByFact = true
                });

            
        }


        public void MaterialChanged(int selectedMaterialTabIndex, WithdrawalMaterial selectedMaterial)//, List<Guid> productionProductCharacteristicIDs)
        {
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


            //RaisePropertiesChanged("DocCloseShiftWithdrawalMaterials.WithdrawalMaterials");
        }


    }
}