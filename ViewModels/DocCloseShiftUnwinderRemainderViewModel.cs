﻿// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
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
using System.Windows;
using System.Data.Entity.SqlServer;

namespace Gamma.ViewModels
{
    public class DocCloseShiftUnwinderRemainderViewModel : SaveImplementedViewModel, ICheckedAccess, IFillClearGrid
    {
        /// <summary>
        /// Конструктор для новых остатков на раскатах
        /// </summary>
        /// <param name="placeID">ID передела</param>
        /// <param name="gammaBase">Контекст базы данных</param>
        public DocCloseShiftUnwinderRemainderViewModel(int placeID, GammaEntities gammaBase = null)
        {
            ShowProductCommand = new DelegateCommand<int>(ShowProduct);
            PlaceID = placeID;
            //FillGrid();
        }

        private int PlaceID { get; set; }
        private byte? ShiftID { get; set; }
        private DateTime DocDate { get; set; }
        private Guid DocID { get; set; }

        private void AddSpoolRemainder(Guid productId, DateTime date, bool isSourceProduct, Guid? docWithdrawalId, int index)
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

        //<Summary>
        //Конструктор для существующего закрытия смены
        //</Summary>
        public DocCloseShiftUnwinderRemainderViewModel(Guid docID, GammaEntities gammaBase = null)
        {
            gammaBase = gammaBase ?? DB.GammaDbWithNoCheckConnection;
            ShowProductCommand = new DelegateCommand<int>(ShowProduct);
            var doc = gammaBase.Docs.Where(d => d.DocID == docID).Select(d => new { d.IsConfirmed, d.PlaceID, d.ShiftID, d.Date, d.DocID }).First();
            IsConfirmed = doc.IsConfirmed;
            PlaceID = (int) doc.PlaceID;
            ShiftID = doc.ShiftID;
            DocDate = doc.Date;
            DocID = doc.DocID;
            var remainders = gammaBase.DocUnwinderRemainders.Where(r => r.DocID == DocID).ToList();
            foreach (var spoolRemainder in remainders.Select(remainder => new SpoolRemainder(doc.Date, doc.ShiftID, remainder.ProductID, false, remainder.DocWithdrawalID)
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
                
            }
            foreach (var spool in SpoolRemainders)
            {
                SpoolRemaindersPrev.Add(new remainderPrev() { ProductID = spool.ProductID, Weight = spool.Weight, WithdrawalID = spool.DocWithdrawalId });
            }
        }
        /// <summary>
        /// Сохранение остатков в БД
        /// </summary>
        /// <param name="itemID">ID документа закрытия смены</param>
        public override bool SaveToModel(Guid docID)
        {
            if (IsReadOnly) return true;
            UIServices.SetBusyState();
            using (var gammaBase = DB.GammaDb)
            {
                var doc = gammaBase.Docs.Where(d => d.DocID == docID).Select(d => new { d.IsConfirmed, d.PlaceID, d.ShiftID, d.Date, d.DocID }).First();
                PlaceID = (int)doc.PlaceID;
                ShiftID = doc.ShiftID;
                DocDate = doc.Date;
                DocID = doc.DocID;

                foreach (var item in SpoolRemaindersPrev.Where(s => !SpoolRemainders.Any(r => r.ProductID == s.ProductID)))
                {
                    var removeDocWithdrawalProducts = gammaBase.DocWithdrawalProducts.FirstOrDefault(r => r.DocID == item.WithdrawalID && r.ProductID == item.ProductID);
                    if (removeDocWithdrawalProducts != null)
                    {
                        //если есть привязанная произведенная продукция, то обнуляем, чтобы не потерять привязку.
                        if (gammaBase.DocProductionProducts.Any(p => p.DocProduction.DocWithdrawal.Any(w => w.DocID == removeDocWithdrawalProducts.DocID)))
                        {
                            removeDocWithdrawalProducts.Quantity = null;
                            removeDocWithdrawalProducts.CompleteWithdrawal = null;
                        }
                        else
                            gammaBase.DocWithdrawalProducts.Remove(removeDocWithdrawalProducts);
                    }
                    gammaBase.DocUnwinderRemainders.RemoveRange(gammaBase.DocUnwinderRemainders.Where(r => r.DocID == docID && r.ProductID == item.ProductID));
                }

                foreach (var item in SpoolRemainders)
                {
                    var itemPrev = SpoolRemaindersPrev.FirstOrDefault(r => r.ProductID == item.ProductID);
                    if (itemPrev == null)
                    {
                        var docRemainder = new DocUnwinderRemainders()
                        {
                            DocID = docID,
                            DocUnwinderRemainderID = SqlGuidUtil.NewSequentialid(),
                            ProductID = item.ProductID,
                            DocWithdrawalID = item.DocWithdrawalId,
                            Quantity = item.Weight
                        };
                        gammaBase.DocUnwinderRemainders.Add(docRemainder);
                        DocWithdrawalProducts docWithdrawalProduct;
                        {
                            docWithdrawalProduct =
                                gammaBase.DocWithdrawalProducts.OrderByDescending(d => d.DocWithdrawal.Docs.Date).Include(d => d.DocWithdrawal.Docs)
                                .FirstOrDefault(d => d.ProductID == item.ProductID);
                            if (docWithdrawalProduct == null || docWithdrawalProduct.Quantity != null || docWithdrawalProduct.CompleteWithdrawal == true)
                            {
                                var docId = SqlGuidUtil.NewSequentialid();
                                docWithdrawalProduct = new DocWithdrawalProducts
                                {
                                    DocID = docId,
                                    // ReSharper disable once PossibleInvalidOperationException
                                    // Проверка на null есть при выборке из SpoolRemainders
                                    ProductID = (Guid)item.ProductID,
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
                            docWithdrawalProduct.Quantity = (item.MaxWeight - item.Weight) / 1000;
                            docWithdrawalProduct.CompleteWithdrawal = false;
                            docWithdrawalProduct.DocWithdrawal.Docs.IsConfirmed = true;
                        }
                        item.DocWithdrawalId = docWithdrawalProduct.DocID;
                        docRemainder.DocWithdrawalID = docWithdrawalProduct.DocID;
                    }
                    else if (item.Weight != itemPrev.Weight)
                    {
                        var docUnwinderRemainder = gammaBase.DocUnwinderRemainders.FirstOrDefault(r => r.DocID == docID && r.ProductID == item.ProductID);
                        if (docUnwinderRemainder != null)
                            docUnwinderRemainder.Quantity = item.Weight;
                        var docWithdrawalProduct =
                                gammaBase.DocWithdrawalProducts.FirstOrDefault(d => d.ProductID == item.ProductID && d.DocID == item.DocWithdrawalId);
                        docWithdrawalProduct.Quantity = (item.MaxWeight - item.Weight) / 1000;
                    }
                }
                var spoolRemainders = SpoolRemainders.Where(s => s.ProductID != null).Select(s => (Guid)s.ProductID).ToList();
                var spoolRemaindersPrev = SpoolRemaindersPrev.Where(s => s.ProductID != null).Select(s => (Guid)s.ProductID).ToList();
                var docDatePrev = gammaBase.DocCloseShiftRemainders.Where(d => d.ProductID != null && d.DocCloseShifts.PlaceID == PlaceID && d.DocCloseShifts.ShiftID == ShiftID &&
                        d.DocCloseShifts.Date >= SqlFunctions.DateAdd("hh", 1, DB.GetShiftBeginTime((DateTime)SqlFunctions.DateAdd("hh", -1, DocDate))) &&
                        d.DocCloseShifts.Date <= SqlFunctions.DateAdd("hh", -1, DB.GetShiftEndTime((DateTime)SqlFunctions.DateAdd("hh", -1, DocDate)))
                        && spoolRemaindersPrev.Contains((Guid)d.ProductID)).OrderByDescending(d => d.DocCloseShifts.Date).Select(d => new { d.DocCloseShifts.DocID, d.DocCloseShifts.Date }).FirstOrDefault();
                var docDate = gammaBase.DocCloseShiftRemainders.Where(d => d.ProductID != null && d.DocCloseShifts.PlaceID == PlaceID && d.DocCloseShifts.ShiftID == ShiftID &&
                        d.DocCloseShifts.Date >= SqlFunctions.DateAdd("hh", 1, DB.GetShiftBeginTime((DateTime)SqlFunctions.DateAdd("hh", -1, DocDate))) &&
                        d.DocCloseShifts.Date <= SqlFunctions.DateAdd("hh", -1, DB.GetShiftEndTime((DateTime)SqlFunctions.DateAdd("hh", -1, DocDate)))
                        && spoolRemainders.Contains((Guid)d.ProductID)).OrderByDescending(d => d.DocCloseShifts.Date).Select(d => new { d.DocCloseShifts.DocID, d.DocCloseShifts.Date }).FirstOrDefault();
                if (docDate != null || docDatePrev != null)
                    MessageBox.Show("Остатки на раскатах сохранены в Рапорте закрытия смены от "+(docDate?.Date ?? docDatePrev?.Date).ToString()+"! Не забудьте перезаполнить Рапорт закрытия смены!", "Остатки на раскатах", MessageBoxButton.OK, MessageBoxImage.Asterisk);

                /*
                //d.Products.ProductKindID == 0 - мне надо удалить остатки на раскате, по другому никак не определить именно тамбура на раскате, так как для конвертингов остатки на раскате - это полуфабрикат переходящий, а для БДМ - это выработка переходящая.Поэтому ни IsSourceProduct, ни RemainderTypeID не подходит
                //удаляем списание, так как удаляем запись в остатках
                //gammaBase.DocWithdrawalProducts.RemoveRange(gammaBase.DocWithdrawalProducts.Where(d => gammaBase.DocCloseShiftRemainders.Any(r => r.DocWithdrawalID == d.DocID && r.ProductID == d.ProductID && r.DocID == itemID && (r.Products.ProductKindID == 0))));
                var remainderProductWithdrawalIDs = gammaBase.DocCloseShiftRemainders.Where(d => d.DocID == itemID && (d.Products.ProductKindID == 0)).Select(d => d.DocWithdrawalID).ToList();
                gammaBase.DocCloseShiftRemainders.RemoveRange(gammaBase.DocCloseShiftRemainders.Where(d => d.DocID == itemID && (d.Products.ProductKindID == 0)));
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
                      var  docRemainder = new DocCloseShiftRemainders()
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
                    if (remainder.IsSourceProduct )// Чтобы в любом случае закрылась строка списания, пусть и с 0 расходом тамбура на раскате.&& (remainder.MaxWeight - remainder.Weight) != 0)
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
    */
                gammaBase.SaveChanges();
            }
            return true;
        }

        public bool IsChanged { get; private set; }

        /// <summary>
        /// Очистка тамбуров с раската
        /// </summary>
         public void ClearGrid()
        {
            ClearGridWithIndex(1);
            ClearGridWithIndex(2);
            ClearGridWithIndex(3);
            ClearGridWithIndex(4);
        }

        public void ClearGridWithIndex(byte index)
        {
            var spool = SpoolRemainders.Where(s => s.Index == index).FirstOrDefault();
            if (spool != null)
                MessageManager.RecalcQuantityEndFromUnwinderReaminderEvent(spool.ProductID, spool.NomenclatureID, spool.CharacteristicID, -1, -spool.Weight);
            var newSpoolRemainders = new List<SpoolRemainder>();
            newSpoolRemainders.AddRange(SpoolRemainders.Where(s => s.Index != index));// не копируем предыдущий сохраненный тамбур на этом раскате
            SpoolRemainders = newSpoolRemainders;//SpoolRemainders?.Clear();
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
            var dlgResult = MessageBox.Show("Последняя паллета(или тамбур) в вашу смену произведена и больше продукции выпускаться не будет?. Вы уверены?", "Обновить тамбура на раскате", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
            if (dlgResult != MessageBoxResult.Yes)
                return;
            using (var gammaBase = DB.GammaDbWithNoCheckConnection)
            {
                var date = DB.CurrentDateTime;
                //ClearGrid();
                var sourceSpools = gammaBase.SourceSpools.FirstOrDefault(ss => ss.PlaceID == PlaceID);
                if (sourceSpools == null)
                {
                    //ClearGrid();
                    DB.AddLogMessageInformation("Загрузка раскатов в рапорте закрытия смены @PlaceID " + PlaceID.ToString() + ", @ShiftID " + ShiftID?.ToString() + ", @Date " + DocDate.ToString() +" Нет тамбуров на раскатах!");
                }
                else
                {
                    var spoolWithdrawals = SpoolRemainders.Where(s => s.DocWithdrawalId != null);
                    if (sourceSpools.Unwinder1Spool != null)
                    {
                        AddSpoolRemainder((Guid)sourceSpools.Unwinder1Spool, date, true, spoolWithdrawals.Where(s => s.ProductID == sourceSpools.Unwinder1Spool && s.DocWithdrawalId != null).Select(s => s.DocWithdrawalId).FirstOrDefault(),1);
                    }
                    if (sourceSpools.Unwinder2Spool != null)
                    {
                        AddSpoolRemainder((Guid)sourceSpools.Unwinder2Spool, date, true, spoolWithdrawals.Where(s => s.ProductID == sourceSpools.Unwinder2Spool && s.DocWithdrawalId != null).Select(s => s.DocWithdrawalId).FirstOrDefault(),2);
                    }
                    if (sourceSpools.Unwinder3Spool != null)
                    {
                        AddSpoolRemainder((Guid)sourceSpools.Unwinder3Spool, date, true, spoolWithdrawals.Where(s => s.ProductID == sourceSpools.Unwinder3Spool && s.DocWithdrawalId != null).Select(s => s.DocWithdrawalId).FirstOrDefault(),3);
                    }
                    if (sourceSpools.Unwinder4Spool != null)
                    {
                        AddSpoolRemainder((Guid)sourceSpools.Unwinder4Spool, date, true, spoolWithdrawals.Where(s => s.ProductID == sourceSpools.Unwinder4Spool && s.DocWithdrawalId != null).Select(s => s.DocWithdrawalId).FirstOrDefault(),4);
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
                    DB.AddLogMessageInformation("Загрузка раскатов в рапорте закрытия смены @PlaceID " + PlaceID.ToString() + ", @ShiftID " + ShiftID?.ToString() + ", @Date " + DocDate.ToString() );
                }
                gammaBase.SaveChanges();
            }
        }

        //public List<SpoolRemainder> SpoolRemainders { get; set; } = new List<SpoolRemainder>(); // = {new SpoolRemainder() {Index = 1}, new SpoolRemainder() {Index = 2}, new SpoolRemainder() {Index = 3} };

        public class remainderPrev
        {
            public Guid? ProductID { get; set; }
            public decimal Weight { get; set; }
            public Guid? WithdrawalID { get; set; }
        }
        private List<remainderPrev> SpoolRemaindersPrev = new List<remainderPrev>();
        private List<SpoolRemainder> _spoolRemainders = new List<SpoolRemainder>();

        public List<SpoolRemainder> SpoolRemainders
        {
            get { return _spoolRemainders; }
            set
            {
                _spoolRemainders = value;
                RaisePropertyChanged("SpoolRemainders");
            }
        }
        public DelegateCommand<int> ShowProductCommand { get; set; }


        private void ShowProduct(int i)
        {
            if (SpoolRemainders.Where(s => s.Index == i).FirstOrDefault().ProductID == null) return;
            MessageManager.OpenDocProduct(DocProductKinds.DocProductSpool, (Guid)SpoolRemainders.Where(s => s.Index == i).FirstOrDefault().ProductID);
        }

        private bool IsConfirmed { get; set; }
        public bool IsReadOnly => !(DB.HaveWriteAccess("DocCloseShiftRemainders"));// && !IsConfirmed);

        public void UpdateIsConfirmed (bool isConfirmed)
        {
            IsConfirmed = isConfirmed;
            var newSpoolRemainders = new List<SpoolRemainder>();
            foreach (var spoolRemainder in SpoolRemainders)
            {
                spoolRemainder.IsReadOnly = IsConfirmed || (spoolRemainder.DocWithdrawalId != null && !DB.AllowEditDoc((Guid)spoolRemainder.DocWithdrawalId));
                newSpoolRemainders.Add(spoolRemainder);
            }
            SpoolRemainders = newSpoolRemainders;
        }
    }
}
