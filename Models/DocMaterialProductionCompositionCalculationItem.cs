using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gamma.Models
{
    public class DocMaterialProductionCompositionCalculationItem : DocMaterialProductionItem
    {
        private decimal? _quantityRemainderInGRVAtEnd { get; set; }
        public decimal? QuantityRemainderInGRVAtEnd
        {
            get { return _quantityRemainderInGRVAtEnd; }
            set
            {
                if (_quantityRemainderInGRVAtEnd != value)
                {
                    _quantityRemainderInGRVAtEnd = value;
                    RefreshQuntity();
                }
            }
        }
        public bool IsVisibleRemainderInGRVAtEnd { get; set; } = false;

        protected override void RefreshQuntity()
        {
            QuantitySend = (QuantityDismiss ?? 0) + (QuantityRemainderAtBegin ?? 0) + (QuantityIn ?? 0) - (QuantityRemainderAtEnd ?? 0) - (QuantityRemainderInGRVAtEnd ?? 0);
        }
    }
}
