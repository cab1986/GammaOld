using System;
using System.Linq;
using Gamma.Models;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using DevExpress.Mvvm;
using Gamma.Interfaces;
using Gamma.Attributes;

namespace Gamma.ViewModels
{
    public class DocCloseShiftRWRemainderViewModel : SaveImplementedViewModel, ICheckedAccess
    {
        public DocCloseShiftRWRemainderViewModel(int placeId) // Конструктор для нового закрытия смены
        {
            ShowProductCommand = new DelegateCommand<int>(ShowProduct);
            var sourceSpools = DB.GammaBase.SourceSpools.FirstOrDefault(ss => ss.PlaceID == placeId);
            if (sourceSpools == null) return;
            SpoolRemainders[0].ProductId = sourceSpools.Unwinder1Spool;
            SpoolRemainders[1].ProductId = sourceSpools.Unwinder2Spool;
            SpoolRemainders[2].ProductId = sourceSpools.Unwinder3Spool;
            SpoolRemainders[0].Weight = SpoolRemainders[0].MaxWeight;
            SpoolRemainders[1].Weight = SpoolRemainders[1].MaxWeight;
            SpoolRemainders[2].Weight = SpoolRemainders[2].MaxWeight;
        }
        //<Summary>
        //Конструктор для существующего закрытия смены
        //</Summary>
        public DocCloseShiftRWRemainderViewModel(Guid docId)
        {
            var doc = DB.GammaBase.Docs.Include(d => d.DocCloseShiftRemainders).First(d => d.DocID == docId);
            IsConfirmed = doc.IsConfirmed;
            var remainders = doc.DocCloseShiftRemainders.ToList();
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

        public SpoolRemainder[] SpoolRemainders { get; set; } = {new SpoolRemainder(), new SpoolRemainder(), new SpoolRemainder()};

        public DelegateCommand<int> ShowProductCommand { get; set; }


        private void ShowProduct(int i)
        {
            switch (i)
            {
                case 1:
                    if (SpoolRemainders[0].ProductId == null) return;
                    MessageManager.OpenDocProduct(DocProductKinds.DocProductSpool, (Guid)SpoolRemainders[0].ProductId);
                    break;
                case 2:
                    if (SpoolRemainders[1].ProductId == null) return;
                    MessageManager.OpenDocProduct(DocProductKinds.DocProductSpool, (Guid)SpoolRemainders[1].ProductId);
                    break;
                case 3:
                    if (SpoolRemainders[2].ProductId == null) return;
                    MessageManager.OpenDocProduct(DocProductKinds.DocProductSpool, (Guid)SpoolRemainders[2].ProductId);
                    break;
            }
        }

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
            [UIAuth(UIAuthLevel.ReadOnly)]
            public int Weight { get; set; }
            public int MaxWeight { get; set; }
            private string GetProductSpoolNomenclature(Guid productId)
            {
                return
                    DB.GammaBase.ProductSpools.Where(p => p.ProductID == productId)
                        .Select(p => "№ " + p.Products.Number + " " + p.C1CNomenclature.Name + " " +
                                     p.C1CCharacteristics.Name + " Масса: " + SqlFunctions.StringConvert((double)p.Weight) + " кг").First();
            }

            private int GetRemainderMaxWeight(Guid productId)
            {
                return DB.GammaBase.ProductSpools.First(ps => ps.ProductID == productId).Weight;
            }
        }

        private bool IsConfirmed { get; set; }
        public bool IsReadOnly => !((DB.HaveWriteAccess("DocCloseShiftRemainders") || WorkSession.DBAdmin) && !IsConfirmed);
    }
}
