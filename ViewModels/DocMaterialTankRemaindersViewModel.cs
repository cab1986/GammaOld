// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.Models;
using System.Data.Entity;
using DevExpress.Mvvm;
using Gamma.Interfaces;
using Gamma.Common;
using Gamma.Entities;

namespace Gamma.ViewModels
{
    public class DocMaterialTankRemaindersViewModel : SaveImplementedViewModel, ICheckedAccess, IFillClearGrid
    {
        /// <summary>
        /// Конструктор для новых остатков на раскатах
        /// </summary>
        /// <param name="placeID">ID передела</param>
        /// <param name="gammaBase">Контекст базы данных</param>
        public DocMaterialTankRemaindersViewModel(int placeID, DocMaterialTankGroupContainer tankGroupContainer, GammaEntities gammaBase = null)
        {
            gammaBase = gammaBase ?? DB.GammaDb;
            PlaceID = placeID;
            //FillGrid();
            TankGroupContainer = tankGroupContainer;
        }

        public DocMaterialTankGroupContainer TankGroupContainer { get; set; }
        private int PlaceID { get; set; }
        private byte? ShiftID { get; set; }
        private DateTime DocDate { get; set; }

        /*private void AddSpoolRemainder(Guid productId, DateTime date, bool isSourceProduct, Guid? docWithdrawalId, int index)
        {
            if (!SpoolRemainders.Any(s => s.ProductID == productId && s.Index == index))
            {
                var newSpoolRemainders = new List<SpoolRemainder>();
                newSpoolRemainders.AddRange(SpoolRemainders.Where(s => s.Index != index));// не копируем предыдущий сохраненный тамбур на этом раскате
                var spoolRemainder = new SpoolRemainder(date, ShiftID, productId, isSourceProduct, docWithdrawalId);
                MessageManager.RecalcQuantityEndFromUnwinderReaminderEvent(spoolRemainder.ProductID, spoolRemainder.NomenclatureID, spoolRemainder.CharacteristicID, -2, spoolRemainder.Weight);
                //Вызвать обновление кол-ва в материалах обязательно до изменения кол-ва в тамбуре на раскате, так как в первом вызове это добавление нового тамбура на раскате в рапорт пока без веса.
                spoolRemainder.Weight = spoolRemainder.MaxWeight;
                spoolRemainder.Index = index;

                newSpoolRemainders.Add(spoolRemainder);
                SpoolRemainders = newSpoolRemainders;
            }
        }
        */
        //<Summary>
        //Конструктор для существующего закрытия смены
        //</Summary>7
        public DocMaterialTankRemaindersViewModel(Guid docID, GammaEntities gammaBase = null)
        {
            gammaBase = gammaBase ?? DB.GammaDb;
            var doc = gammaBase.Docs.Include(d => d.DocCloseShiftRemainders).First(d => d.DocID == docID);
            IsConfirmed = doc.IsConfirmed;
            PlaceID = (int)doc.PlaceID;
            ShiftID = doc.ShiftID;
            DocDate = doc.Date;
            //var remainders = doc.DocCloseShiftRemainders.Where(dr => dr.IsSourceProduct ?? false).ToList();
           /* foreach (var spoolRemainder in remainders.Select(remainder => new SpoolRemainder(doc.Date, doc.ShiftID, remainder.ProductID, remainder.IsSourceProduct ?? false, remainder.DocWithdrawalID)
            {
                Weight = (int)remainder.Quantity,
                IsReadOnly = IsConfirmed || (remainder.DocWithdrawalID != null && !DB.AllowEditDoc((Guid)remainder.DocWithdrawalID)),
            }))
            {
                if (spoolRemainder.MaxWeight < spoolRemainder.Weight) spoolRemainder.MaxWeight = spoolRemainder.Weight;
                spoolRemainder.Index = SpoolRemainders.Count + 1;
                var newSpoolRemainders = new List<SpoolRemainder>(SpoolRemainders);
                newSpoolRemainders.Add(spoolRemainder);
                SpoolRemainders = newSpoolRemainders;

            }*/
        }
        /// <summary>
        /// Сохранение остатков в БД
        /// </summary>
        /// <param name="itemID">ID документа закрытия смены</param>
        public override bool SaveToModel(Guid itemID)
        {
#if DEBUG
            Console.WriteLine(IsReadOnly.ToString());
#endif
            if (IsReadOnly) return true;
            UIServices.SetBusyState();
            
            using (var gammaBase = DB.GammaDb)
            {
                //gammaBase.DocMaterialTankRemainders.RemoveRange(gammaBase.DocMaterialTankRemainders.Where(r => r.DocID == itemID));
                var tankIds = new List<int>();
                foreach (var tankGroup in TankGroupContainer.TankGroups)
                {
                    foreach (var tank in tankGroup.Tanks)
                    {
                        var tankRemainder = gammaBase.DocMaterialTankRemainders.Where(t => t.DocID == itemID && t.DocMaterialTankID == tank.DocMaterialTankID).FirstOrDefault();
                        if (tankRemainder == null)
                        {
                            gammaBase.DocMaterialTankRemainders.Add(new DocMaterialTankRemainders()
                            {
                                DocMaterialTankRemainderID = SqlGuidUtil.NewSequentialid(),
                                DocMaterialTankID = tank.DocMaterialTankID,
                                Concentration = tank.Concentration,
                                Level = (int)tank.Level,
                                DocID = itemID
                            });
                        }
                        else
                        {
                            tankRemainder.Concentration = tank.Concentration;
                            tankRemainder.Level = (int)tank.Level;
                        }
                        tankIds.Add(tank.DocMaterialTankID);
                    }
                }
                gammaBase.DocMaterialTankRemainders.RemoveRange(gammaBase.DocMaterialTankRemainders.Where(r => r.DocID == itemID && !tankIds.Contains(r.DocMaterialTankID)));


                //d.Products.ProductKindID == 0 - мне надо удалить остатки на раскате, по другому никак не определить именно тамбура на раскате, так как для конвертингов остатки на раскате - это полуфабрикат переходящий, а для БДМ - это выработка переходящая.Поэтому ни IsSourceProduct, ни RemainderTypeID не подходит
                //удаляем списание, так как удаляем запись в остатках
                //gammaBase.DocWithdrawalProducts.RemoveRange(gammaBase.DocWithdrawalProducts.Where(d => gammaBase.DocCloseShiftRemainders.Any(r => r.DocWithdrawalID == d.DocID && r.ProductID == d.ProductID && r.DocID == itemID && (r.Products.ProductKindID == 0))));
                var remainderProductWithdrawalIDs = gammaBase.DocCloseShiftRemainders.Where(d => d.DocID == itemID && (d.Products.ProductKindID == 0)).Select(d => d.DocWithdrawalID).ToList();
            /*    gammaBase.DocCloseShiftRemainders.RemoveRange(gammaBase.DocCloseShiftRemainders.Where(d => d.DocID == itemID && (d.Products.ProductKindID == 0)));
                //gammaBase.SaveChanges();
                var remainders = SpoolRemainders.Where(sr => sr.ProductID != null).ToList();
                var withdrawalIds = remainders.Select(s => s.DocWithdrawalId).ToList();
                var removeDocWithdrawalProducts = gammaBase.DocWithdrawalProducts.Where(r => remainderProductWithdrawalIDs.Contains(r.DocID) && !withdrawalIds.Contains(r.DocID));
                foreach (var item in gammaBase.DocWithdrawalProducts.Where(r => remainderProductWithdrawalIDs.Contains(r.DocID) && !withdrawalIds.Contains(r.DocID)))
                {
                    //если есть привязанная произведенная продукция, то обнуляем, чтобы не потерять привязку.
                    if (gammaBase.DocProductionProducts.Any(p => p.DocProduction.DocWithdrawal.Any(w => w.DocID == item.DocID)))
                    {
                        item.Quantity = null;
                        item.CompleteWithdrawal = null;
                    }
                    else
                        gammaBase.DocWithdrawalProducts.Remove(item);
                }
                foreach (var remainder in remainders)
                {
                    // Занесение остатков в таблицу остатков закрытия смены
                    //var docRemainder =
                    //    gammaBase.DocCloseShiftRemainders.FirstOrDefault(d => d.ProductID == remainder.ProductID && d.DocID == itemID);
                    //if (docRemainder == null)
                    //{
                    var docRemainder = new DocCloseShiftRemainders()
                    {
                        DocID = itemID,
                        DocCloseShiftRemainderID = SqlGuidUtil.NewSequentialid(),
                        ProductID = remainder.ProductID,
                        IsSourceProduct = remainder.IsSourceProduct,
                        DocWithdrawalID = remainder.DocWithdrawalId,
                        IsMaterial = true
                    };
                    gammaBase.DocCloseShiftRemainders.Add(docRemainder);
                    //}
                    docRemainder.Quantity = remainder.Weight;
                    // Списание части тамбура                
                    if (remainder.IsSourceProduct)// Чтобы в любом случае закрылась строка списания, пусть и с 0 расходом тамбура на раскате.&& (remainder.MaxWeight - remainder.Weight) != 0)
                    {
                        DocWithdrawalProducts docWithdrawalProduct;
                        ////закоментировал, так как выше мы удаляем списание, то в каждом случае надо создать новое
                        if (remainder.DocWithdrawalId != null)
                        {
                            docWithdrawalProduct =
                                gammaBase.DocWithdrawalProducts.Include(d => d.DocWithdrawal.Docs)
                                    .First(d => d.DocID == remainder.DocWithdrawalId);
                        }
                        else
                        {
                            docWithdrawalProduct =
                                gammaBase.DocWithdrawalProducts.OrderByDescending(d => d.DocWithdrawal.Docs.Date).Include(d => d.DocWithdrawal.Docs)
                                .FirstOrDefault(d => d.ProductID == remainder.ProductID);
                            if (docWithdrawalProduct == null || docWithdrawalProduct.Quantity != null || docWithdrawalProduct.CompleteWithdrawal == true)
                            {
                                var docId = SqlGuidUtil.NewSequentialid();
                                docWithdrawalProduct = new DocWithdrawalProducts
                                {
                                    DocID = docId,
                                    // ReSharper disable once PossibleInvalidOperationException
                                    // Проверка на null есть при выборке из SpoolRemainders
                                    ProductID = (Guid)remainder.ProductID,
                                    DocWithdrawal = new DocWithdrawal
                                    {
                                        DocID = docId,
                                        OutPlaceID = WorkSession.PlaceID,
                                        Docs = new Docs
                                        {
                                            DocID = docId,
                                            IsConfirmed = true,
                                            Date = DB.CurrentDateTime,
                                            DocTypeID = (int)DocTypes.DocWithdrawal,
                                            PlaceID = WorkSession.PlaceID,
                                            PrintName = WorkSession.PrintName,
                                            ShiftID = WorkSession.ShiftID,
                                            UserID = WorkSession.UserID
                                        }
                                    }
                                };
                                gammaBase.DocWithdrawalProducts.Add(docWithdrawalProduct);
                            }
                        };
                        if (DB.AllowEditDoc(docWithdrawalProduct.DocID))
                        {
                            docWithdrawalProduct.Quantity = (remainder.MaxWeight - remainder.Weight) / 1000;
                            docWithdrawalProduct.CompleteWithdrawal = false;
                            docWithdrawalProduct.DocWithdrawal.Docs.IsConfirmed = true;
                        }
                        docRemainder.DocWithdrawalID = docWithdrawalProduct.DocID;
                        remainder.DocWithdrawalId = docWithdrawalProduct.DocID;
                    }
                }
            */    gammaBase.SaveChanges();
            }
            return true;
        }

        public bool IsChanged { get; private set; }

        /// <summary>
        /// Очистка тамбуров с раската
        /// </summary>
        public void ClearGrid()
        {
            /*foreach (var items in TankGroupContainer.TankGroups)
            {
                items.Composition.Clear();
            }*/
            /*var tankGroups = new List<DocMaterialTankGroup>();
            for (int i = tankGroups.Count(); i < 4; i++)
            {
                tankGroups.Add(new DocMaterialTankGroup(0));
            }
            TankGroupContainer.TankGroups = tankGroups;*/
            TankGroupContainer.Clear();
        }

        public void ClearGridWithIndex(byte index)
        {
          /*  var spool = SpoolRemainders.Where(s => s.Index == index).FirstOrDefault();
            if (spool != null)
                MessageManager.RecalcQuantityEndFromUnwinderReaminderEvent(spool.ProductID, spool.NomenclatureID, spool.CharacteristicID, -1, -spool.Weight);
            var newSpoolRemainders = new List<SpoolRemainder>();
            newSpoolRemainders.AddRange(SpoolRemainders.Where(s => s.Index != index));// не копируем предыдущий сохраненный тамбур на этом раскате
            SpoolRemainders = newSpoolRemainders;//SpoolRemainders?.Clear();*/
        }

        public void FillGridWithNoFillEnd()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Перезаполнение тамбуров с раската
        /// </summary>
        public void FillGrid()
        {
            using (var gammaBase = DB.GammaDb)
            {
                //    ClearGrid();
                if (TankGroupContainer.TankGroups[0].Tanks?.Count > 0)
                {
                    foreach (var tankG in TankGroupContainer.TankGroups)
                    {
                        tankG.Composition?.Clear();
                    }
                }
                else
                {
                    /*var tankGroups = new List<DocMaterialTankGroup>();
                    var tanks = gammaBase.DocMaterialTanks.Where(dr => dr.DocMaterialTankGroups.PlaceID == PlaceID).ToList();
                    var groupNumber = 0;
                    foreach (var groupID in tanks.Select(r => r.DocMaterialTankGroupID).Distinct())
                    {
                        DocMaterialTankGroup tankGroup = new DocMaterialTankGroup(groupID);
                        foreach (var tank in tanks.Where(r => r.DocMaterialTankGroupID == groupID).Select(remainder => new MaterialProductionTankRemainder(remainder.DocMaterialTankID, remainder.Name, remainder.Volume)
                        { }))
                        {
                            tankGroup.Tanks.Add(tank);
                        };
                        tankGroup.Name = tankGroup.Name + " V=" + tankGroup.Tanks.Sum(t => t.Volume) + "м3";
                        tankGroups.Add(tankGroup);
                        groupNumber += 1;
                    }
                    for (int i = tankGroups.Count(); i < 4; i++)
                    {
                        tankGroups.Add(new DocMaterialTankGroup(0));
                    }
                    TankGroupContainer.TankGroups = tankGroups;*/
                    TankGroupContainer.TankGroups = new DocMaterialTankGroupContainer(PlaceID).TankGroups;
                }
                //var date = DB.CurrentDateTime;
                //ClearGrid();
                /*var sourceSpools = gammaBase.SourceSpools.FirstOrDefault(ss => ss.PlaceID == PlaceID);
                if (sourceSpools == null)
                {
                    //ClearGrid();
                    gammaBase.CriticalLogs.Add(new CriticalLogs { LogID = SqlGuidUtil.NewSequentialid(), LogDate = DB.CurrentDateTime, LogUserID = WorkSession.UserName, Log = "Загрузка раскатов в рапорте закрытия смены @PlaceID " + PlaceID.ToString() + ", @ShiftID " + ShiftID?.ToString() + ", @Date " + DocDate.ToString() + " Нет тамбуров на раскатах!" });
                }
                else
                {
                    var spoolWithdrawals = SpoolRemainders.Where(s => s.DocWithdrawalId != null);
                    if (sourceSpools.Unwinder1Spool != null)
                    {
                        AddSpoolRemainder((Guid)sourceSpools.Unwinder1Spool, date, true, spoolWithdrawals.Where(s => s.ProductID == sourceSpools.Unwinder1Spool && s.DocWithdrawalId != null).Select(s => s.DocWithdrawalId).FirstOrDefault(), 1);
                    }
                    if (sourceSpools.Unwinder2Spool != null)
                    {
                        AddSpoolRemainder((Guid)sourceSpools.Unwinder2Spool, date, true, spoolWithdrawals.Where(s => s.ProductID == sourceSpools.Unwinder2Spool && s.DocWithdrawalId != null).Select(s => s.DocWithdrawalId).FirstOrDefault(), 2);
                    }
                    if (sourceSpools.Unwinder3Spool != null)
                    {
                        AddSpoolRemainder((Guid)sourceSpools.Unwinder3Spool, date, true, spoolWithdrawals.Where(s => s.ProductID == sourceSpools.Unwinder3Spool && s.DocWithdrawalId != null).Select(s => s.DocWithdrawalId).FirstOrDefault(), 3);
                    }
                    if (sourceSpools.Unwinder4Spool != null)
                    {
                        AddSpoolRemainder((Guid)sourceSpools.Unwinder4Spool, date, true, spoolWithdrawals.Where(s => s.ProductID == sourceSpools.Unwinder4Spool && s.DocWithdrawalId != null).Select(s => s.DocWithdrawalId).FirstOrDefault(), 4);
                    }
                    if (sourceSpools.Unwinder1Spool == null)
                    {
                        ClearGridWithIndex(1);
                    }
                    if (sourceSpools.Unwinder2Spool == null)
                    {
                        ClearGridWithIndex(2);
                    }
                    if (sourceSpools.Unwinder3Spool == null)
                    {
                        ClearGridWithIndex(3);
                    }
                    if (sourceSpools.Unwinder4Spool == null)
                    {
                        ClearGridWithIndex(4);
                    }
                    IsChanged = true;
                    gammaBase.CriticalLogs.Add(new CriticalLogs { LogID = SqlGuidUtil.NewSequentialid(), LogDate = DB.CurrentDateTime, LogUserID = WorkSession.UserName, Log = "Загрузка раскатов в рапорте закрытия смены @PlaceID " + PlaceID.ToString() + ", @ShiftID " + ShiftID?.ToString() + ", @Date " + DocDate.ToString() });
                }*/
                //gammaBase.SaveChanges();
            }
        }

        //public List<SpoolRemainder> SpoolRemainders { get; set; } = new List<SpoolRemainder>(); // = {new SpoolRemainder() {Index = 1}, new SpoolRemainder() {Index = 2}, new SpoolRemainder() {Index = 3} };

        public bool TankGroup0Visible { get; set; } = false;
        public string TankGroup0Name { get; set; }
        private List<MaterialProductionTankRemainder> _tankGroup0Remainders = new List<MaterialProductionTankRemainder>();
        public List<MaterialProductionTankRemainder> TankGroup0Remainders
        {
            get { return _tankGroup0Remainders; }
            set
            {
                _tankGroup0Remainders = value;
                RaisePropertyChanged("TankGroup0Remainders");
            }
        }

        public bool TankGroup1Visible { get; set; } = false;
        public string TankGroup1Name { get; set; }
        private List<MaterialProductionTankRemainder> _tankGroup1Remainders = new List<MaterialProductionTankRemainder>();
        public List<MaterialProductionTankRemainder> TankGroup1Remainders
        {
            get { return _tankGroup1Remainders; }
            set
            {
                _tankGroup1Remainders = value;
                RaisePropertyChanged("TankGroup1Remainders");
            }
        }

        public bool TankGroup2Visible { get; set; } = false;
        public string TankGroup2Name { get; set; }
        private List<MaterialProductionTankRemainder> _tankGroup2Remainders = new List<MaterialProductionTankRemainder>();
        public List<MaterialProductionTankRemainder> TankGroup2Remainders
        {
            get { return _tankGroup2Remainders; }
            set
            {
                _tankGroup2Remainders = value;
                RaisePropertyChanged("TankGroup2Remainders");
            }
        }

        public bool TankGroup3Visible { get; set; } = false;
        public string TankGroup3Name { get; set; }
        private List<MaterialProductionTankRemainder> _tankGroup3Remainders = new List<MaterialProductionTankRemainder>();
        public List<MaterialProductionTankRemainder> TankGroup3Remainders
        {
            get { return _tankGroup3Remainders; }
            set
            {
                _tankGroup3Remainders = value;
                RaisePropertyChanged("TankGroup3Remainders");
            }
        }

        public DelegateCommand<int> ShowProductCommand { get; set; }


        private bool IsConfirmed { get; set; }
        public bool IsReadOnly => !(DB.HaveWriteAccess("DocMaterialTankRemainders"));// && !IsConfirmed);
    }
}
