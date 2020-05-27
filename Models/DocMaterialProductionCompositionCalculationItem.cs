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
            if (!IsNotSendMaterialIntoNextPlace)
                QuantitySend = (QuantityDismiss ?? 0) + (QuantityRemainderAtBegin ?? 0) + (QuantityIn ?? 0) - (QuantityRemainderAtEnd ?? 0) - (QuantityRemainderInGRVAtEnd ?? 0);
            else
            {
                QuantityRemainderAtEnd = (QuantityDismiss ?? 0) + (QuantityRemainderAtBegin ?? 0) + (QuantityIn ?? 0) - (QuantityRemainderInGRVAtEnd ?? 0);
                QuantitySend = 0;
            }
        }

        public bool _isNotSendMaterialIntoNextPlace { get; set; } = false;
        public bool IsNotSendMaterialIntoNextPlace
        {
            get { return _isNotSendMaterialIntoNextPlace; }
            set
            {
                _isNotSendMaterialIntoNextPlace = value;
                RefreshQuntity();
            }
        }
    }
}
