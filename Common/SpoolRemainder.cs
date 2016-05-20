using System;
using System.Data.Entity.SqlServer;
using System.Linq;
using Gamma.Attributes;
using Gamma.Interfaces;
using Gamma.Models;

namespace Gamma.Common
{
    public class SpoolRemainder: ICheckedAccess
    {
        public SpoolRemainder(GammaEntities gammaBase = null)
        {
            GammaBase = gammaBase ?? DB.GammaDb;
        }
        private GammaEntities GammaBase { get; }
        public int Index { get; set; }
        private Guid? _productid;

        public Guid? ProductID
        {
            get { return _productid; }
            set
            {
                _productid = value;
                Nomenclature = ProductID != null ? GetProductSpoolNomenclature((Guid)ProductID) : string.Empty;
                MaxWeight = ProductID != null ? GetRemainderMaxWeight((Guid)ProductID) : 0;
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
                                 p.C1CCharacteristics.Name + " Масса: " +
                                 SqlFunctions.StringConvert((double)p.DecimalWeight) + " кг").First();
        }

        private decimal GetRemainderMaxWeight(Guid productid)
        {
            return GammaBase.ProductSpools.First(ps => ps.ProductID == productid).DecimalWeight??0;
        }

        public bool IsReadOnly { get; set; }
    }
}
