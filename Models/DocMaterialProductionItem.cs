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
    public class DocMaterialProductionItem : ViewModelBase
    {

        public Guid DocMaterialProductionItemID { get; set; } = SqlGuidUtil.NewSequentialid();

        public string PlaceName { get; private set; }

        private int _placeID { get; set; }
        public int PlaceID
        {
            get { return _placeID; }
            set
            {
                if (_placeID != value)
                    PlaceName = WorkSession.Places.FirstOrDefault(p => p.PlaceID == value).Name;
                _placeID = value;
            }
        }

        public string DocNumberDate { get; set; }
        public DateTime? DocDate { get; set; }

        private Guid _docID { get; set; }
        public Guid DocID
        {
            get { return _docID; }
            set { _docID = value; }
        }

        private Guid _nomenclatureID;
        public Guid NomenclatureID
        {
            get { return _nomenclatureID; }
            set
            {
                if ((_nomenclatureID == null || _nomenclatureID == Guid.Empty) && value != null && value != Guid.Empty)
                {
                    _nomenclatureID = value;
                    RefreshAvilableNomenclatures();
                }
                else
                    _nomenclatureID = value;
                
                {
                    var choosenNomenclature = AvailableNomenclatures?.FirstOrDefault(an => an.NomenclatureID == _nomenclatureID && (an.CharacteristicID == CharacteristicID || CharacteristicID == null || CharacteristicID == Guid.Empty));
                    if (choosenNomenclature == null)
                    {
                        //Quantity = 0;
                        MeasureUnit = string.Empty;
                    }
                    else
                    {
                        //Quantity = BaseQuantity * choosenNomenclature.Coefficient;
                        MeasureUnit = choosenNomenclature.MeasureUnit;
                        MeasureUnitID = choosenNomenclature.MeasureUnitID;
                    }
                }
                //RefreshAvailableProductionProducts();
                
                RaisePropertiesChanged("NomenclatureID");
            }
        }

        private List<NomenclatureAnalog> _availableNomenclatures;

        public List<NomenclatureAnalog> AvailableNomenclatures
        {
            get { return _availableNomenclatures; }
            set
            {
                _availableNomenclatures = value;
                if (_availableNomenclatures.Count == 1)
                {
                    NomenclatureID = _availableNomenclatures[0].NomenclatureID;
                }
                else if (_availableNomenclatures.Count > 1)
                {
                    var actualNomenclatures = _availableNomenclatures.Where(n => !n.IsMarked).ToList();
                    if (actualNomenclatures.Count > 0)
                        NomenclatureID = actualNomenclatures[0].NomenclatureID;
                }
                RaisePropertiesChanged("AvailableNomenclatures");
            }
        }

        private void RefreshAvilableNomenclatures()
        {
            AvailableNomenclatures = new List<NomenclatureAnalog>();
            using (var gammaBase = DB.GammaDbWithNoCheckConnection)
            {
                var nomenclatureInfo =
                    gammaBase.C1CNomenclature.Include(n => n.C1CMeasureUnitStorage).First(n => n.C1CNomenclatureID == _nomenclatureID);
               {
                    AvailableNomenclatures.Add(new NomenclatureAnalog()
                    {
                        NomenclatureID = _nomenclatureID,
                        Coefficient = 1,
                        NomenclatureName = nomenclatureInfo.Name,
                        MeasureUnit = nomenclatureInfo.C1CMeasureUnitStorage.Name,
                        MeasureUnitID = nomenclatureInfo.C1CMeasureUnitStorage.C1CMeasureUnitID,
                        IsMarked = nomenclatureInfo.IsArchive ?? false
                    });
                }

                var analogs =
                gammaBase.GetNomenclatureAnalogs(DocDate).Where(a => a.C1CNomenclatureID == _nomenclatureID && (a.C1CCharacteristicID == CharacteristicID || CharacteristicID == null))
                    .Select(a => new
                    {
                        NomenclatureId = a.C1CNomenclatureAnalogID,
                        CharacteristicID = a.C1CCharacteristicAnalogID,
                        NomenclatureName = a.NomenclatureAnalogName,
                        IsMarked = a.NomenclatureAnalogIsArchive ?? false,
                        MeasureUnit = a.MeasureUnitAnalogName,
                        MeasureUnitID = a.C1CMeasureUnitAnalogID,
                        Coefficient = (a.AmountAnalog / a.Amount) ?? 0
                    }).Distinct()
                    .ToList();
                foreach (var analog in analogs)
                {

                    if (AvailableNomenclatures.Where(n => n.NomenclatureID == analog.NomenclatureId && n.CharacteristicID == analog.CharacteristicID).Count() == 0)
                        AvailableNomenclatures.Add(new NomenclatureAnalog()
                        {
                            NomenclatureID = (Guid)analog.NomenclatureId,
                            CharacteristicID = analog.CharacteristicID,
                            NomenclatureName = analog.NomenclatureName,
                            IsMarked = analog.IsMarked,
                            MeasureUnit = analog.MeasureUnit,
                            MeasureUnitID = (Guid)analog.MeasureUnitID,
                            Coefficient = analog.Coefficient
                        });                    

                }
                if (nomenclatureInfo.IsArchive ?? false)
                {
                    var activeNomenclature = AvailableNomenclatures.FirstOrDefault(an => !an.IsMarked);
                    if (activeNomenclature != null)
                        NomenclatureID = activeNomenclature.NomenclatureID;
                }

            }
        }

        private string _nomenclatureIDDiezCharacteristicID { get; set; }
        public string NomenclatureIDDiezCharacteristicID
        {
            get { return NomenclatureID.ToString() + "#" + CharacteristicID?.ToString(); /*return _nomenclatureIDDiezCharacteristicID; */}
            set
            {
                /*_nomenclatureIDDiezCharacteristicID = value;*/
                var isDeletedNomenclatureInComposition = (NomenclatureID != null && NomenclatureID != Guid.Empty && NomenclatureIDDiezCharacteristicID != value);
                try
                {
                    NomenclatureID = Guid.Parse(value.Substring(0, value.IndexOf("#")));
                }
                catch
                {
                    NomenclatureID = Guid.Empty;
                }

                try
                {
                    var ch = value.Substring(value.IndexOf("#") + 1);
                    CharacteristicID = Guid.Parse(value.Substring(value.IndexOf("#") + 1));
                }
                catch
                {
                    CharacteristicID = null;
                }
                if (isDeletedNomenclatureInComposition)
                    MessageManager.DeleteNomenclatureInCompositionFromTankGroupEvent(NomenclatureID);
            }
        }

        /// <summary>
        /// Аналог номенклатуры
        /// </summary>
        public class NomenclatureAnalog
        {
            private Guid _nomenclatureID;

            public Guid NomenclatureID
            {
                get { return _nomenclatureID; }
                set
                {
                    _nomenclatureID = value;
                    //NomenclatureName = value != new Guid() ? DB.GammaDb.C1CNomenclature.First(n => n.C1CNomenclatureID == value).Name : string.Empty;
                }
            }
            public Guid? CharacteristicID { get; set; }
            // ReSharper disable once UnusedAutoPropertyAccessor.Global
            public string NomenclatureName { get; set; }
            /// <summary>
            /// Метка архивного
            /// </summary>
            public bool IsMarked { get; set; }
            public decimal Coefficient { get; set; }
            public string MeasureUnit { get; set; }
            public Guid? MeasureUnitID { get; set; }

            public string NomenclatureIDDiezCharacteristicID
            {
                get { return NomenclatureID.ToString() + "#" + CharacteristicID?.ToString(); }
            }
        }

        private Guid? _characteristicID;
        public Guid? CharacteristicID
        {
            get { return _characteristicID; }
            set
            {
                if ((NomenclatureID != null && NomenclatureID != Guid.Empty) && (_characteristicID == null || _characteristicID == Guid.Empty) && value != null && value != Guid.Empty)
                {
                    _characteristicID = value;
                    RefreshAvilableNomenclatures();
                }
                else
                    _characteristicID = value;
                
                var choosenNomenclature = AvailableNomenclatures?.FirstOrDefault(an => an.NomenclatureID == _nomenclatureID && (an.CharacteristicID == CharacteristicID || CharacteristicID == null || CharacteristicID == Guid.Empty));
                if (choosenNomenclature == null)
                {
                    //Quantity = 0;
                    MeasureUnit = string.Empty;
                }
                else
                {
                    //Quantity = BaseQuantity * choosenNomenclature.Coefficient;
                    MeasureUnit = choosenNomenclature.MeasureUnit;
                    MeasureUnitID = choosenNomenclature.MeasureUnitID;
                }
                
            }
        }

        public string NomenclatureName { get; set; }
        //public decimal BaseQuantity { get; set; }
        //public decimal Quantity { get; set; }
        public bool IsFloatValue { get; set; }
        public int? NomenclatureKindID { get; set; }

        public string ParentName { get; set; }
        public Guid? ParentID { get; set; }

        /// <summary>
        /// Списание по факту (или по нормативам)
        /// </summary>
        public bool? WithdrawByFact { get; set; }


        private bool _quantityIsReadOnly { get; set; }
        /// <summary>
        /// Можно ли менять количество
        /// </summary>
        public bool QuantityIsReadOnly
        {
            get { return _quantityIsReadOnly; }
            set
            {
                _quantityIsReadOnly = value;
            }
        }

        public string MeasureUnit { get; set; }
        public Guid? MeasureUnitID { get; set; }

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
        public bool IsVisibleIn { get; set; } = false;

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

        private decimal? _quantityDismiss { get; set; }
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
        }

        //private decimal? _quantityRemainderInGRVAtEnd { get; set; }
        //public decimal? QuantityRemainderInGRVAtEnd
        //{
        //    get { return _quantityRemainderInGRVAtEnd; }
        //    set
        //    {
        //        if (_quantityRemainderInGRVAtEnd != value)
        //        {
        //            _quantityRemainderInGRVAtEnd = value;
        //            RefreshQuntity();
        //        }
        //    }
        //}
        //public bool IsVisibleRemainderInGRVAtEnd { get; set; } = false;

        private decimal? _quantitySend { get; set; }
        public decimal? QuantitySend
        {
            get { return _quantitySend; }
            set
            {
                if (_quantitySend != value)
                {
                    _quantitySend = value;
                    RefreshQuntity();
                }
                RefreshStandardQuantityVsQuantityWithdrawalMaterialPercent();
            }
        }

        protected virtual void RefreshQuntity()
        {
            QuantitySend = (QuantityDismiss ?? 0) + (QuantityRemainderAtBegin ?? 0) + (QuantityIn ?? 0) - (QuantityRemainderAtEnd ?? 0);// - (QuantityRemainderInGRVAtEnd ?? 0)
        }

        protected virtual void RefreshStandardQuantityVsQuantityWithdrawalMaterialPercent()
        {
            Border = "White";// (WithdrawByFact ?? false) && (StandardQuantity ?? 0) != 0 && (QuantityWithdrawalMaterial ?? 0) != 0 && (((StandardQuantity > QuantityWithdrawalMaterial ? StandardQuantity - QuantityWithdrawalMaterial : QuantityWithdrawalMaterial - StandardQuantity) / StandardQuantity) > (decimal)0.05) ? "Red" : "White";
            StandardQuantityVsQuantityWithdrawalMaterialPercent = "";// (WithdrawByFact ?? false) && (StandardQuantity ?? 0) != 0 && (QuantityWithdrawalMaterial ?? 0) != 0 && (((StandardQuantity > QuantityWithdrawalMaterial ? StandardQuantity - QuantityWithdrawalMaterial : QuantityWithdrawalMaterial - StandardQuantity) / StandardQuantity) > (decimal)0.05) ? "`" : "";
        }
    }
}
