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
    public class DocCloseShiftRwRemainderViewModel : SaveImplementedViewModel, ICheckedAccess
    {
        public DocCloseShiftRwRemainderViewModel(int placeID) // Конструктор для нового закрытия смены
        {
            ShowProductCommand = new DelegateCommand<int>(ShowProduct);
            var sourceSpools = DB.GammaBase.SourceSpools.FirstOrDefault(ss => ss.PlaceID == placeID);
            if (sourceSpools == null) return;
            SpoolRemainders[0].ProductID = sourceSpools.Unwinder1Spool;
            SpoolRemainders[1].ProductID = sourceSpools.Unwinder2Spool;
            SpoolRemainders[2].ProductID = sourceSpools.Unwinder3Spool;
            SpoolRemainders[0].Weight = SpoolRemainders[0].MaxWeight;
            SpoolRemainders[1].Weight = SpoolRemainders[1].MaxWeight;
            SpoolRemainders[2].Weight = SpoolRemainders[2].MaxWeight;
        }
        //<Summary>
        //Конструктор для существующего закрытия смены
        //</Summary>
        public DocCloseShiftRwRemainderViewModel(Guid docID)
        {
            var doc = DB.GammaBase.Docs.Include(d => d.DocCloseShiftRemainders).First(d => d.DocID == docID);
            IsConfirmed = doc.IsConfirmed;
            var remainders = doc.DocCloseShiftRemainders.ToList();
            for (int i = 0; i < remainders.Count; i++)
            {
                SpoolRemainders[i].ProductID = remainders[i].ProductID;
                SpoolRemainders[i].Weight = (int) remainders[i].Quantity;
            }
        }

        public override void SaveToModel(Guid itemID)
        {
            base.SaveToModel(itemID);
            var remainders = SpoolRemainders.Where(sr => sr.ProductID != null).ToList();
            foreach (var remainder in remainders)
            {
                var docRemainder =
                    DB.GammaBase.DocCloseShiftRemainders.FirstOrDefault(d => d.ProductID == remainder.ProductID && d.DocID == itemID);
                if (docRemainder == null)
                {
                    docRemainder = new DocCloseShiftRemainders()
                    {
                        DocID = itemID,
                        DocCloseShiftRemainderID = SqlGuidUtil.NewSequentialid(),
                        ProductID = remainder.ProductID
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
                    if (SpoolRemainders[0].ProductID == null) return;
                    MessageManager.OpenDocProduct(DocProductKinds.DocProductSpool, (Guid) SpoolRemainders[0].ProductID);
                    break;
                case 2:
                    if (SpoolRemainders[1].ProductID == null) return;
                    MessageManager.OpenDocProduct(DocProductKinds.DocProductSpool, (Guid) SpoolRemainders[1].ProductID);
                    break;
                case 3:
                    if (SpoolRemainders[2].ProductID == null) return;
                    MessageManager.OpenDocProduct(DocProductKinds.DocProductSpool, (Guid) SpoolRemainders[2].ProductID);
                    break;
            }
        }

        public class SpoolRemainder
        {
            private Guid? _productid;

            public Guid? ProductID
            {
                get { return _productid; }
                set
                {
                    _productid = value;
                    Nomenclature = ProductID != null ? GetProductSpoolNomenclature((Guid) ProductID) : string.Empty;
                    MaxWeight = ProductID != null ? GetRemainderMaxWeight((Guid) ProductID) : 0;
                }
            }
            public string Nomenclature { get; set; }
            [UIAuth(UIAuthLevel.ReadOnly)]
            public int Weight { get; set; }
            public int MaxWeight { get; set; }
            private string GetProductSpoolNomenclature(Guid productid)
            {
                return
                    DB.GammaBase.ProductSpools.Where(p => p.ProductID == productid)
                        .Select(p => "№ " + p.Products.Number + " " + p.C1CNomenclature.Name + " " +
                                     p.C1CCharacteristics.Name + " Масса: " + SqlFunctions.StringConvert((double)p.Weight) + " кг").First();
            }

            private int GetRemainderMaxWeight(Guid productid)
            {
                return DB.GammaBase.ProductSpools.First(ps => ps.ProductID == productid).Weight;
            }
        }

        private bool IsConfirmed { get; set; }
        public bool IsReadOnly => !((DB.HaveWriteAccess("DocCloseShiftRemainders") || WorkSession.DBAdmin) && !IsConfirmed);
    }
}
