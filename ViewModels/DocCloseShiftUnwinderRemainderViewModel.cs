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
            var sourceSpools = gammaBase.SourceSpools.FirstOrDefault(ss => ss.PlaceID == placeID);
            if (sourceSpools == null) return;
            if (sourceSpools.Unwinder1Spool != null)
            {
                AddSpoolRemainder((Guid)sourceSpools.Unwinder1Spool);
            }
            if (sourceSpools.Unwinder2Spool != null)
            {
                AddSpoolRemainder((Guid)sourceSpools.Unwinder2Spool);
            }
            if (sourceSpools.Unwinder3Spool != null)
            {
                AddSpoolRemainder((Guid)sourceSpools.Unwinder3Spool);
            }
            /*            SpoolRemainders[0].ProductID = sourceSpools.Unwinder1Spool;
                        SpoolRemainders[1].ProductID = sourceSpools.Unwinder2Spool;
                        SpoolRemainders[2].ProductID = sourceSpools.Unwinder3Spool;
                        SpoolRemainders[0].Weight = SpoolRemainders[0].MaxWeight;
                        SpoolRemainders[1].Weight = SpoolRemainders[1].MaxWeight;
                        SpoolRemainders[2].Weight = SpoolRemainders[2].MaxWeight;
            */
        }
        private void AddSpoolRemainder(Guid productId)
        {
            var spoolRemainder = new SpoolRemainder
            {
                ProductID = productId
            };
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
            foreach (var remainder in remainders)
            {
                var spoolRemainder = new SpoolRemainder
                {
                    ProductID = remainder.ProductID,
                    Weight = (int) remainder.Quantity,
                    IsReadOnly = IsConfirmed,
                    Index = SpoolRemainders.Count
                };
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
            gammaBase = gammaBase ?? DB.GammaDb;
            var remainders = SpoolRemainders.Where(sr => sr.ProductID != null).ToList();
            foreach (var remainder in remainders)
            {
                var docRemainder =
                    gammaBase.DocCloseShiftRemainders.FirstOrDefault(d => d.ProductID == remainder.ProductID && d.DocID == itemID);
                if (docRemainder == null)
                {
                    docRemainder = new DocCloseShiftRemainders()
                    {
                        DocID = itemID,
                        DocCloseShiftRemainderID = SqlGuidUtil.NewSequentialid(),
                        ProductID = remainder.ProductID
                    };
                    gammaBase.DocCloseShiftRemainders.Add(docRemainder);
                }
                docRemainder.Quantity = remainder.Weight;
            }
            gammaBase.SaveChanges();
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
        public bool IsReadOnly => !((DB.HaveWriteAccess("DocCloseShiftRemainders") || WorkSession.DBAdmin) && !IsConfirmed);
    }
}
