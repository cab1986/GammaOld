using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

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
            if (!IsNotSendMaterialIntoNextPlace && !IsFullSendMaterialIntoNextPlace)
                QuantitySend = (QuantityDismiss ?? 0) + (QuantityRemainderAtBegin ?? 0) + (QuantityIn ?? 0) - (QuantityRemainderAtEnd ?? 0) - (QuantityRemainderInGRVAtEnd ?? 0);
            else
            {
                if (IsNotSendMaterialIntoNextPlace)
                {
                    if (!IsNotCalculatedQuantityRemainderAtEnd)
                        QuantityRemainderAtEnd = (QuantityDismiss ?? 0) + (QuantityRemainderAtBegin ?? 0) + (QuantityIn ?? 0) - (QuantityRemainderInGRVAtEnd ?? 0);
                    QuantitySend = 0;
                } 
                else
                    if (IsFullSendMaterialIntoNextPlace)
                {
                    QuantityRemainderAtEnd = 0;
                    QuantitySend = (QuantityDismiss ?? 0) + (QuantityRemainderAtBegin ?? 0) + (QuantityIn ?? 0) - (QuantityRemainderInGRVAtEnd ?? 0); ;

                }
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

        public bool _isFullSendMaterialIntoNextPlace { get; set; } = false;
        public bool IsFullSendMaterialIntoNextPlace
        {
            get { return _isFullSendMaterialIntoNextPlace; }
            set
            {
                _isFullSendMaterialIntoNextPlace = value;
                RefreshQuntity();
            }
        }

        public bool _isNotCalculatedQuantityRemainderAtEnd { get; set; } = false;
        public bool IsNotCalculatedQuantityRemainderAtEnd
        {
            get { return _isNotCalculatedQuantityRemainderAtEnd; }
            set
            {
                _isNotCalculatedQuantityRemainderAtEnd = value;
                RefreshQuntity();
            }
        }

        private decimal _sumQuantityRemainderAtEnd { get; set; }
        public decimal SumQuantityRemainderAtEnd
        {
            get { return _sumQuantityRemainderAtEnd; }
            set
            {
                if (_sumQuantityRemainderAtEnd != value)
                {
                    _sumQuantityRemainderAtEnd = value;
                }
            }
        }
    }
}
