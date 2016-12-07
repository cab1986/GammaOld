using System;
using System.Linq;
using Gamma.Attributes;
using Gamma.Interfaces;

namespace Gamma.Models
{
    public class SpoolRemainder: ICheckedAccess
    {
        public SpoolRemainder(DateTime docDate, Guid? productId, bool isSourceProduct, GammaEntities gammaBase = null)
        {
            GammaBase = gammaBase ?? DB.GammaDb;
            DocDate = docDate;
            IsSourceProduct = isSourceProduct;
            ProductID = productId;            
        }

        private DateTime DocDate { get; set; }
        private GammaEntities GammaBase { get; }
        public int Index { get; set; }
        private Guid? _productid;

        public Guid? ProductID
        {
            get { return _productid; }
            set
            {
                _productid = value;
                MaxWeight = ProductID != null ? GetRemainderMaxWeight((Guid)ProductID, DocDate) : 0;
                Nomenclature = ProductID != null ? GetProductSpoolNomenclature((Guid)ProductID) + MaxWeight + "кг": string.Empty;
                
            }
        }

        public string Nomenclature { get; set; }

        [UIAuth(UIAuthLevel.ReadOnly)]
        public decimal Weight { get; set; }

        public decimal MaxWeight { get; set; }

        private string GetProductSpoolNomenclature(Guid productid)
        {
            return
                GammaBase.ProductSpools.Where(p => p.ProductID == productid)
                    .Select(p => "№ " + p.Products.Number + " " + p.C1CNomenclature.Name + " " +
                                 p.C1CCharacteristics.Name + " Масса: ").First();

        }

        private decimal GetRemainderMaxWeight(Guid productId, DateTime docDate)
        {
            var date =
                GammaBase.DocWithdrawalProducts.Where(
                    dw => dw.ProductID == productId && dw.DocWithdrawal.Docs.Date < docDate)
                    .OrderByDescending(dw => dw.DocWithdrawal.Docs.Date)
                    .Select(dw => dw.DocWithdrawal.Docs)
                    .FirstOrDefault()?
                    .Date;
            date = date ?? docDate;
            return DB.CalculateSpoolWeightBeforeDate(productId, (DateTime)date, GammaBase)*1000;
        }

        public bool IsReadOnly { get; set; }
        public bool IsSourceProduct { get; set; }
        public Guid? DocWithdrawalId { get; set; }
    }
}
