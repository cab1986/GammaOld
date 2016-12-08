using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.Models;
using System.Data.Entity;
using DevExpress.Mvvm;
using Gamma.Interfaces;
using Gamma.Common;

namespace Gamma.ViewModels
{
    public class DocCloseShiftUnwinderRemainderViewModel : SaveImplementedViewModel, ICheckedAccess
    {
        /// <summary>
        /// Конструктор для новых остатков на раскатах
        /// </summary>
        /// <param name="placeID">ID передела</param>
        /// <param name="gammaBase">Контекст базы данных</param>
        public DocCloseShiftUnwinderRemainderViewModel(int placeID, GammaEntities gammaBase = null)
        {
            gammaBase = gammaBase ?? DB.GammaDb;
            ShowProductCommand = new DelegateCommand<int>(ShowProduct);
            var date = DB.CurrentDateTime;
            var sourceSpools = gammaBase.SourceSpools.FirstOrDefault(ss => ss.PlaceID == placeID);
            var placeGroup = (PlaceGroups) gammaBase.Places.First(p => p.PlaceID == placeID).PlaceGroupID;
            var isSourceProduct = placeGroup == PlaceGroups.Convertings || placeGroup == PlaceGroups.Rw;
            if (sourceSpools == null) return;
            if (sourceSpools.Unwinder1Spool != null)
            {
                AddSpoolRemainder((Guid)sourceSpools.Unwinder1Spool, date, isSourceProduct);
            }
            if (sourceSpools.Unwinder2Spool != null)
            {
                AddSpoolRemainder((Guid)sourceSpools.Unwinder2Spool, date, isSourceProduct);
            }
            if (sourceSpools.Unwinder3Spool != null)
            {
                AddSpoolRemainder((Guid)sourceSpools.Unwinder3Spool, date, isSourceProduct);
            }
            /*            SpoolRemainders[0].ProductID = sourceSpools.Unwinder1Spool;
                        SpoolRemainders[1].ProductID = sourceSpools.Unwinder2Spool;
                        SpoolRemainders[2].ProductID = sourceSpools.Unwinder3Spool;
                        SpoolRemainders[0].Weight = SpoolRemainders[0].MaxWeight;
                        SpoolRemainders[1].Weight = SpoolRemainders[1].MaxWeight;
                        SpoolRemainders[2].Weight = SpoolRemainders[2].MaxWeight;
            */
        }

        private void AddSpoolRemainder(Guid productId, DateTime date, bool isSourceProduct)
        {
            var spoolRemainder = new SpoolRemainder(date, productId, isSourceProduct);
            spoolRemainder.Weight = spoolRemainder.MaxWeight;
            spoolRemainder.Index = SpoolRemainders.Count;
            SpoolRemainders.Add(spoolRemainder);
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
            var remainders = doc.DocCloseShiftRemainders.ToList();
            foreach (var spoolRemainder in remainders.Select(remainder => new SpoolRemainder(doc.Date, remainder.ProductID, remainder.IsSourceProduct ?? false)
            {
                Weight = (int) remainder.Quantity,
                IsReadOnly = IsConfirmed || (remainder.DocWithdrawalID != null && !DB.AllowEditDoc((Guid)remainder.DocWithdrawalID)),
                Index = SpoolRemainders.Count,
                DocWithdrawalId = remainder.DocWithdrawalID
            }))
            {
                if (spoolRemainder.MaxWeight < spoolRemainder.Weight) spoolRemainder.MaxWeight = spoolRemainder.Weight;
                SpoolRemainders.Add(spoolRemainder);
            }
        }
        /// <summary>
        /// Сохранение остатков в БД
        /// </summary>
        /// <param name="itemID">ID документа закрытия смены</param>
        /// <param name="gammaBase">Контекст базы данных</param>
        public override bool SaveToModel(Guid itemID, GammaEntities gammaBase = null)
        {
            if (!DB.HaveWriteAccess("DocCloseShiftRemainders")) return true;
            UIServices.SetBusyState();
            gammaBase = gammaBase ?? DB.GammaDb;
            var remainders = SpoolRemainders.Where(sr => sr.ProductID != null).ToList();
            foreach (var remainder in remainders)
            {
                // Занесение остатков в таблицу остатков закрытия смены
                var docRemainder =
                    gammaBase.DocCloseShiftRemainders.FirstOrDefault(d => d.ProductID == remainder.ProductID && d.DocID == itemID);
                if (docRemainder == null)
                {
                    docRemainder = new DocCloseShiftRemainders()
                    {
                        DocID = itemID,
                        DocCloseShiftRemainderID = SqlGuidUtil.NewSequentialid(),
                        ProductID = remainder.ProductID,
                        IsSourceProduct = remainder.IsSourceProduct
                    };
                    gammaBase.DocCloseShiftRemainders.Add(docRemainder);
                }
                docRemainder.Quantity = remainder.Weight;
                // Списание части тамбура                
                if (remainder.IsSourceProduct)
                {
                    DocWithdrawalProducts docWithdrawalProduct;
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
                        docWithdrawalProduct.Quantity = (remainder.MaxWeight  - remainder.Weight) / 1000;
                        docWithdrawalProduct.CompleteWithdrawal = false;
                        docWithdrawalProduct.DocWithdrawal.Docs.IsConfirmed = true;
                    }
                    docRemainder.DocWithdrawalID = docWithdrawalProduct.DocID;
                    remainder.DocWithdrawalId = docWithdrawalProduct.DocID;
                }
                gammaBase.SaveChanges();   
            }
            return true;
        }



        public List<SpoolRemainder> SpoolRemainders { get; set; } = new List<SpoolRemainder>(); // = {new SpoolRemainder() {Index = 1}, new SpoolRemainder() {Index = 2}, new SpoolRemainder() {Index = 3} };

        public DelegateCommand<int> ShowProductCommand { get; set; }


        private void ShowProduct(int i)
        {
            if (SpoolRemainders[i].ProductID == null) return;
            MessageManager.OpenDocProduct(DocProductKinds.DocProductSpool, (Guid)SpoolRemainders[i].ProductID);
        }

        private bool IsConfirmed { get; set; }
        public bool IsReadOnly => !(DB.HaveWriteAccess("DocCloseShiftRemainders") && !IsConfirmed);
    }
}
