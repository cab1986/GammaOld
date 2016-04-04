using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gamma.Models;
using  System.Data.Entity;

namespace Gamma.ViewModels
{
    public class DocCloseShiftRWRemainderViewModel : SaveImplementedViewModel
    {
        public DocCloseShiftRWRemainderViewModel(int placeId) // Конструктор для нового закрытия смены
        {
            var sourceSpools = DB.GammaBase.SourceSpools.FirstOrDefault(ss => ss.PlaceID == placeId);
            if (sourceSpools == null) return;
            SpoolRemainders[0].ProductId = sourceSpools.Unwinder1Spool;
            SpoolRemainders[1].ProductId = sourceSpools.Unwinder2Spool;
            SpoolRemainders[2].ProductId = sourceSpools.Unwinder3Spool;
            SpoolRemainders[0].Weight = SpoolRemainders[0].MaxWeight;
            SpoolRemainders[1].Weight = SpoolRemainders[1].MaxWeight;
            SpoolRemainders[2].Weight = SpoolRemainders[2].MaxWeight;
        }
        // Конструктор для существующего закрытия смены
        public DocCloseShiftRWRemainderViewModel(Guid docId)
        {
            var remainders = DB.GammaBase.DocCloseShiftRemainders.Where(d => d.DocID == docId).ToList();
            for (int i = 0; i < remainders.Count; i++)
            {
                SpoolRemainders[i].ProductId = remainders[i].ProductID;
                SpoolRemainders[i].Weight = (int) remainders[i].Quantity;
            }
        }

        public override void SaveToModel(Guid itemId)
        {
            base.SaveToModel(itemId);
            var remainders = SpoolRemainders.Where(sr => sr.ProductId != null).ToList();
            foreach (var remainder in remainders)
            {
                var docRemainder =
                    DB.GammaBase.DocCloseShiftRemainders.FirstOrDefault(d => d.ProductID == remainder.ProductId && d.DocID == itemId);
                if (docRemainder == null)
                {
                    docRemainder = new DocCloseShiftRemainders()
                    {
                        DocID = itemId,
                        DocCloseShiftRemainderID = SQLGuidUtil.NewSequentialId(),
                        ProductID = remainder.ProductId
                    };
                    DB.GammaBase.DocCloseShiftRemainders.Add(docRemainder);
                }
                docRemainder.Quantity = remainder.Weight;
            }
            DB.GammaBase.SaveChanges();
        }

        public SpoolRemainder[] SpoolRemainders { get; set; } = new SpoolRemainder[3];

        public class SpoolRemainder
        {
            private Guid? _productId;

            public Guid? ProductId
            {
                get { return _productId; }
                set
                {
                    _productId = value;
                    Nomenclature = ProductId != null ? GetProductSpoolNomenclature((Guid) ProductId) : string.Empty;
                    MaxWeight = ProductId != null ? GetRemainderMaxWeight((Guid) ProductId) : 0;
                }
            }
            public string Nomenclature { get; set; }
            public int Weight { get; set; }
            public int MaxWeight { get; set; }
            private string GetProductSpoolNomenclature(Guid productId)
            {
                return
                    DB.GammaBase.ProductSpools.Where(p => p.ProductID == productId)
                        .Select(p => "№ " + p.Products.Number + " " + p.C1CNomenclature.Name + " " +
                                     p.C1CCharacteristics.Name + " Масса: " + p.Weight.ToString() + " кг").First();
            }

            private int GetRemainderMaxWeight(Guid productId)
            {
                return DB.GammaBase.ProductSpools.First(ps => ps.ProductID == productId).Weight;
            }
        }
    }
}
