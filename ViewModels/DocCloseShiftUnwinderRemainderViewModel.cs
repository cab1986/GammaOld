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

        private void AddSpoolRemainder(Guid productId, DateTime date, bool isSourceProduct, Guid? docWithdrawalId, int index)
        {
            if (!SpoolRemainders.Any(s => s.ProductID == productId && s.Index == index))
            {
                var newSpoolRemainders = new List<SpoolRemainder>();
                newSpoolRemainders.AddRange(SpoolRemainders.Where(s => s.Index != index));// не копируем предыдущий сохраненный тамбур на этом раскате
                var spoolRemainder = new SpoolRemainder(date, ShiftID, productId, isSourceProduct, docWithdrawalId);
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
            gammaBase = gammaBase ?? DB.GammaDb;
            ShowProductCommand = new DelegateCommand<int>(ShowProduct);
            var doc = gammaBase.Docs.Include(d => d.DocCloseShiftRemainders).First(d => d.DocID == docID);
            IsConfirmed = doc.IsConfirmed;
            PlaceID = (int) doc.PlaceID;
            ShiftID = doc.ShiftID;
            DocDate = doc.Date;
            var remainders = doc.DocCloseShiftRemainders.Where(dr => dr.IsSourceProduct ?? false).ToList();
            foreach (var spoolRemainder in remainders.Select(remainder => new SpoolRemainder(doc.Date, doc.ShiftID, remainder.ProductID, remainder.IsSourceProduct ?? false, remainder.DocWithdrawalID)
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
        }
        /// <summary>
        /// Сохранение остатков в БД
        /// </summary>
        /// <param name="itemID">ID документа закрытия смены</param>
        public override bool SaveToModel(Guid itemID)
        {
            if (IsReadOnly) return true;
            UIServices.SetBusyState();
            using (var gammaBase = DB.GammaDb)
            {
                //d.Products.ProductKindID == 0 - мне надо удалить остатки на раскате, по другому никак не определить именно тамбура на раскате, так как для конвертингов остатки на раскате - это полуфабрикат переходящий, а для БДМ - это выработка переходящая.Поэтому ни IsSourceProduct, ни RemainderTypeID не подходит
                //удаляем списание, так как удаляем запись в остатках
                gammaBase.DocWithdrawalProducts.RemoveRange(gammaBase.DocWithdrawalProducts.Where(d => gammaBase.DocCloseShiftRemainders.Any(r => r.DocWithdrawalID == d.DocID && r.ProductID == d.ProductID && r.DocID == itemID && (r.Products.ProductKindID == 0))));
                gammaBase.DocCloseShiftRemainders.RemoveRange(gammaBase.DocCloseShiftRemainders.Where(d => d.DocID == itemID && (d.Products.ProductKindID == 0)));
                //gammaBase.SaveChanges();
                var remainders = SpoolRemainders.Where(sr => sr.ProductID != null).ToList();
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
                            //DocWithdrawalID = remainder.DocWithdrawalId
                        };
                        gammaBase.DocCloseShiftRemainders.Add(docRemainder);
                    //}
                    docRemainder.Quantity = remainder.Weight;
                    // Списание части тамбура                
                    if (remainder.IsSourceProduct )// Чтобы в любом случае закрылась строка списания, пусть и с 0 расходом тамбура на раскате.&& (remainder.MaxWeight - remainder.Weight) != 0)
                    {
                        DocWithdrawalProducts docWithdrawalProduct;
                        //закоментировал, так как выше мы удаляем списание, то в каждом случае надо создать новое
                        //if (remainder.DocWithdrawalId != null)
                        //{
                        //    docWithdrawalProduct =
                        //        gammaBase.DocWithdrawalProducts.Include(d => d.DocWithdrawal.Docs)
                        //            .First(d => d.DocID == remainder.DocWithdrawalId);
                        //}
                        //else
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
            var newSpoolRemainders = new List<SpoolRemainder>();
            newSpoolRemainders.AddRange(SpoolRemainders.Where(s => s.Index != index));// не копируем предыдущий сохраненный тамбур на этом раскате
            SpoolRemainders = newSpoolRemainders;//SpoolRemainders?.Clear();
        }
        
        /// <summary>
        /// Перезаполнение тамбуров с раската
        /// </summary>
        public void FillGrid()
        {
            using (var gammaBase = DB.GammaDb)
            {
                var date = DB.CurrentDateTime;
                //ClearGrid();
                var sourceSpools = gammaBase.SourceSpools.FirstOrDefault(ss => ss.PlaceID == PlaceID);
                if (sourceSpools == null)
                {
                    //ClearGrid();
                    gammaBase.CriticalLogs.Add(new CriticalLogs { LogID = SqlGuidUtil.NewSequentialid(), LogDate = DB.CurrentDateTime, LogUserID = WorkSession.UserName, Log = "Загрузка раскатов в рапорте закрытия смены @PlaceID " + PlaceID.ToString() + ", @ShiftID " + ShiftID?.ToString() + ", @Date " + DocDate.ToString() +" Нет тамбуров на раскатах!"});
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
                    gammaBase.CriticalLogs.Add(new CriticalLogs { LogID = SqlGuidUtil.NewSequentialid(), LogDate = DB.CurrentDateTime, LogUserID = WorkSession.UserName, Log = "Загрузка раскатов в рапорте закрытия смены @PlaceID " + PlaceID.ToString() + ", @ShiftID " + ShiftID?.ToString() + ", @Date " + DocDate.ToString() });
                }
                gammaBase.SaveChanges();
            }
        }

        //public List<SpoolRemainder> SpoolRemainders { get; set; } = new List<SpoolRemainder>(); // = {new SpoolRemainder() {Index = 1}, new SpoolRemainder() {Index = 2}, new SpoolRemainder() {Index = 3} };

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
    }
}
