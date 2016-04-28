using System;
using System.Collections.Generic;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Text;
using Gamma.Attributes;
using Gamma.Interfaces;

namespace Gamma.Common
{
    public class SpoolRemainder: ICheckedAccess
    {
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
        public int Weight { get; set; }

        public int MaxWeight { get; set; }

        private string GetProductSpoolNomenclature(Guid productid)
        {
            return
                DB.GammaBase.ProductSpools.Where(p => p.ProductID == productid)
                    .Select(p => "№ " + p.Products.Number + " " + p.C1CNomenclature.Name + " " +
                                 p.C1CCharacteristics.Name + " Масса: " +
                                 SqlFunctions.StringConvert((double)p.Weight) + " кг").First();
        }

        private int GetRemainderMaxWeight(Guid productid)
        {
            return DB.GammaBase.ProductSpools.First(ps => ps.ProductID == productid).Weight;
        }

        public bool IsReadOnly { get; set; }
    }
}
