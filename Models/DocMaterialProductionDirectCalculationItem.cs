using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gamma.Models
{
    public class DocMaterialProductionDirectCalculationItem : DocMaterialProductionItem
    {
        private decimal? _quantityOut { get; set; }
        public decimal? QuantityOut
        {
            get { return _quantityOut; }
            set
            {
                if (_quantityOut != value)
                {
                    _quantityOut = value;
                    RefreshQuntity();
                }
            }
        }

        private decimal? _quantityUtil { get; set; }
        public decimal? QuantityUtil
        {
            get { return _quantityUtil; }
            set
            {
                if (_quantityUtil != value)
                {
                    _quantityUtil = value;
                    RefreshQuntity();
                }
            }
        }

        private decimal? _quantityExperimental { get; set; }
        public decimal? QuantityExperimental
        {
            get { return _quantityExperimental; }
            set
            {
                if (_quantityExperimental != value)
                {
                    _quantityExperimental = value;
                    RefreshQuntity();
                }
            }
        }

        protected override void RefreshQuntity()
        {
            QuantitySend = (QuantityDismiss ?? 0) + (QuantityRemainderAtBegin ?? 0) + (QuantityIn ?? 0) - (QuantityRemainderAtEnd ?? 0) - (QuantityOut ?? 0) - (QuantityUtil ?? 0) - (QuantityExperimental ?? 0);
        }
    }
}
