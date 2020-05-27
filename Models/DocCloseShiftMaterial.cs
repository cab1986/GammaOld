// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using DevExpress.Mvvm;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Collections;

namespace Gamma.Models
{
    /// <summary>
    /// Класс для грида материалов на списание
    /// </summary>
    public class DocCloseShiftMaterial : WithdrawalMaterial
    {
        public DocCloseShiftMaterial()
        {

        }

        public DocCloseShiftMaterial(ArrayList productionProducts)
        {
            SetAvailableProductionProducts = (List<Guid>)productionProducts[0];
            PlaceID = (int)productionProducts[1];
        }


        private decimal? _standardQuantity { get; set; }
        /// <summary>
        /// рассчитанное по норме количество материала на всю произведенную продукцию
        /// </summary>
        public decimal? StandardQuantity
        {
            get { return _standardQuantity; }
            set
            {
                if (_standardQuantity != value)
                {
                    _standardQuantity = value;
                    RefreshQuntity();
                }
                RefreshStandardQuantityVsQuantityWithdrawalMaterialPercent();
            }
        }

        public string Border { get; set; } = "White";
        public string StandardQuantityVsQuantityWithdrawalMaterialPercent { get; set; }

        private decimal? _quantityIn { get; set; }
        public decimal? QuantityIn
        {
            get { return _quantityIn; }
            set
            {
                if (_quantityIn != value)
                {
                    _quantityIn = value;
                    RefreshQuntity();
                }
            }
        }

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

        private decimal? _quantityRemainderAtBegin { get; set; }
        public decimal? QuantityRemainderAtBegin
        {
            get { return _quantityRemainderAtBegin; }
            set
            {
                if (_quantityRemainderAtBegin != value)
                {
                    _quantityRemainderAtBegin = value;
                    RefreshQuntity();
                }
            }
        }

        private decimal? _quantityRemainderAtEnd { get; set; }
        public decimal? QuantityRemainderAtEnd
        {
            get { return _quantityRemainderAtEnd; }
            set
            {
                if (_quantityRemainderAtEnd != value)
                {
                    _quantityRemainderAtEnd = value;
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

        private decimal? _quantityRePack { get; set; }
        public decimal? QuantityRePack
        {
            get { return _quantityRePack; }
            set
            {
                if (_quantityRePack != value)
                {
                    _quantityRePack = value;
                    RefreshQuntity();
                }
            }
        }

        /*private decimal? _quantityDismiss { get; set; }
        public decimal? QuantityDismiss
        {
            get { return _quantityDismiss; }
            set
            {
                if (_quantityDismiss != value)
                {
                    _quantityDismiss = value;
                    RefreshQuntity();
                }
            }
        }*/

        private decimal? _quantityWithdrawalMaterial { get; set; }
        public decimal? QuantityWithdrawalMaterial
        {
            get { return _quantityWithdrawalMaterial; }
            set
            {
                if (_quantityWithdrawalMaterial != value)
                {
                    _quantityWithdrawalMaterial = value;
                    RefreshQuntity();
                }
                RefreshStandardQuantityVsQuantityWithdrawalMaterialPercent();
            }
        }

        private void RefreshQuntity()
        {
            QuantityWithdrawalMaterial = (QuantityRemainderAtBegin ?? 0) + (QuantityIn ?? 0) /*+ (QuantityDismiss ?? 0)*/ - (QuantityOut ?? 0) - (QuantityUtil ?? 0) - (QuantityExperimental ?? 0) - (QuantityRePack ?? 0) - (QuantityRemainderAtEnd ?? 0);
        }

        private void RefreshStandardQuantityVsQuantityWithdrawalMaterialPercent()
        {
            Border = (WithdrawByFact ?? false) && (StandardQuantity ?? 0) != 0 && (QuantityWithdrawalMaterial ?? 0) != 0 && (((StandardQuantity > QuantityWithdrawalMaterial ? StandardQuantity - QuantityWithdrawalMaterial : QuantityWithdrawalMaterial - StandardQuantity) / StandardQuantity) > (decimal)0.05) ? "Red" : "White";
            StandardQuantityVsQuantityWithdrawalMaterialPercent = (WithdrawByFact ?? false) && (StandardQuantity ?? 0) != 0 && (QuantityWithdrawalMaterial ?? 0) != 0 && (((StandardQuantity > QuantityWithdrawalMaterial ? StandardQuantity - QuantityWithdrawalMaterial : QuantityWithdrawalMaterial - StandardQuantity) / StandardQuantity) > (decimal)0.05) ? "`" : "";
        }
    }
}
