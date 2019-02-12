﻿// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
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
    public class WithdrawalMaterial : ViewModelBase
    {
        public WithdrawalMaterial()
        {
            
        }

        public WithdrawalMaterial(ArrayList productionProducts)
        {
            SetAvailableProductionProducts = (List<Guid>)productionProducts[0];
            _placeID = (int)productionProducts[1];
        }

        private List<Guid> _productionProducts { get; set; }
        private int _placeID  { get; set; }
        public int PlaceID
        {
            get { return _placeID; }
            set { _placeID = value; }
        }

    public Guid DocWithdrawalMaterialID { get; set; } = SqlGuidUtil.NewSequentialid();
        private Guid _nomenclatureID;

        public Guid NomenclatureID
        {
            get { return _nomenclatureID; }
            set
            {
                _nomenclatureID = value;
                //NomenclatureName = value != new Guid() ? DB.GammaDb.C1CNomenclature.First(n => n.C1CNomenclatureID == value).Name : string.Empty;
                if (AvailableNomenclatures == null)
                {
                    AvailableNomenclatures = new List<NomenclatureAnalog>();
                    using (var gammaBase = DB.GammaDb)
                    {
                        var nomenclatureInfo =
                            gammaBase.C1CNomenclature.Include(n => n.C1CMeasureUnitStorage).First(n => n.C1CNomenclatureID == _nomenclatureID);
                        var characteristicInfo =
                            gammaBase.C1CCharacteristics.Where(n => n.C1CNomenclatureID == _nomenclatureID);
                        if (characteristicInfo?.Count() > 0)
                        {
                            foreach (var item in characteristicInfo)
                            {
                                AvailableNomenclatures.Add(new NomenclatureAnalog()
                                {
                                    NomenclatureID = item.C1CNomenclatureID,
                                    CharacteristicID = item.C1CCharacteristicID,
                                    Coefficient = 1,
                                    NomenclatureName = nomenclatureInfo.Name + " " + item.Name,
                                    MeasureUnit = nomenclatureInfo.C1CMeasureUnitStorage.Name,
                                    MeasureUnitID = nomenclatureInfo.C1CMeasureUnitStorage.C1CMeasureUnitID,
                                    IsMarked = nomenclatureInfo.IsArchive ?? false
                                });
                            }
                        }
                        else
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
                        gammaBase.C1CNomenclatureAnalogs.Where(a => a.C1CNomenclatureID == _nomenclatureID)
                            .Select(a => new
                            {
                                NomenclatureId = a.C1CNomenclatureAnalogID,
                                NomenclatureName = gammaBase.C1CNomenclature.Where(x => x.C1CNomenclatureID == a.C1CNomenclatureAnalogID).Select(x => x.Name).FirstOrDefault(),
                                IsMarked = a.C1CAnalogNomenclature.IsArchive ?? false,
                                MeasureUnit = a.C1CAnalogMeasureUnits.Name,
                                MeasureUnitID = a.C1CMeasureUnitAnalogID,
                                Coefficient = (a.AmountAnalog / a.Amount) ?? 0
                            }).Distinct()
                            .ToList();
                        foreach (var analog in analogs)
                        {
                            var analogCharacteristicInfo =
                            gammaBase.C1CCharacteristics.Where(n => n.C1CNomenclatureID == _nomenclatureID);
                            if (analogCharacteristicInfo?.Count() > 0)
                            {
                                foreach (var item in analogCharacteristicInfo)
                                {
                                    AvailableNomenclatures.Add(new NomenclatureAnalog()
                                    {
                                        NomenclatureID = (Guid)analog.NomenclatureId,
                                        CharacteristicID = item.C1CCharacteristicID,
                                        NomenclatureName = analog.NomenclatureName + " " + item.Name,
                                        IsMarked = analog.IsMarked,
                                        MeasureUnit = analog.MeasureUnit,
                                        MeasureUnitID = (Guid)analog.MeasureUnitID,
                                        Coefficient = analog.Coefficient
                                    });
                                }
                            }
                            else
                            {
                                AvailableNomenclatures.Add(new NomenclatureAnalog()
                                {
                                    NomenclatureID = (Guid)analog.NomenclatureId,
                                    NomenclatureName = analog.NomenclatureName,
                                    IsMarked = analog.IsMarked,
                                    MeasureUnit = analog.MeasureUnit,
                                    MeasureUnitID = (Guid)analog.MeasureUnitID,
                                    Coefficient = analog.Coefficient
                                });
                            }
                            
                        }
                        if (nomenclatureInfo.IsArchive ?? false)
                        {
                            var activeNomenclature = AvailableNomenclatures.FirstOrDefault(an => !an.IsMarked);
                            if (activeNomenclature != null)
                                NomenclatureID = activeNomenclature.NomenclatureID;
                        }

                    }
                }
                else
                {
                    var choosenNomenclature = AvailableNomenclatures.First(an => an.NomenclatureID == _nomenclatureID);
                    if (choosenNomenclature == null)
                    {
                        Quantity = 0;
                        MeasureUnit = string.Empty;
                    }
                    else
                    {
                        Quantity = BaseQuantity * choosenNomenclature.Coefficient;
                        MeasureUnit = choosenNomenclature.MeasureUnit;
                        MeasureUnitID = choosenNomenclature.MeasureUnitID;
                    }
                }
                RefreshAvailableProductionProducts();

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
        public Guid? CharacteristicID { get; set; }
        public string NomenclatureName { get; set; }
        public decimal BaseQuantity { get; set; }
        public decimal Quantity { get; set; }
        public bool IsFloatValue { get; set; }
        /// <summary>
        /// Можно ли менять количество
        /// </summary>
        public bool QuantityIsReadOnly { get; set; }

        public string MeasureUnit { get; set; }

        public Guid? MeasureUnitID { get; set; }

        public List<ProductionProducts> AvailableProductionProducts { get; set; }

        public List<Guid> SetAvailableProductionProducts
        {
            get { return AvailableProductionProducts.Select(x => x.CharacteristicID).ToList(); }
            set
            {
                _productionProducts = value;
                RefreshAvailableProductionProducts();
            }
        }

        public Guid? ProductionProductCharacteristicID { get; set; }

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
        }

        /// <summary>
        /// Произведенная продукция
        /// </summary>
        public class ProductionProducts
        {
            private Guid _characteristicID;

            public Guid CharacteristicID
            {
                get { return _characteristicID; }
                set
                {
                    _characteristicID = value;
                    //NomenclatureID = value != new Guid() ? DB.GammaDb.C1CCharacteristics.First(n => n.C1CCharacteristicID == value).C1CNomenclatureID : Guid.Empty;
                    //NomenclatureName = value != new Guid() ? DB.GammaDb.C1CNomenclature.First(n => n.C1CNomenclatureID == NomenclatureID).Name : string.Empty;
                }
            }

            // ReSharper disable once UnusedAutoPropertyAccessor.Global
            public string NomenclatureName { get; set; }
            public Guid NomenclatureID { get; set; }
        }

        private void RefreshAvailableProductionProducts()
        {
            if (_productionProducts != null)
            {
                var n = AvailableNomenclatures?.Select(x => x.NomenclatureID).ToList();
                if (n != null)
                {
                    AvailableProductionProducts = DB.GammaDb.vProductionMaterials.Where(x => _productionProducts.Contains((Guid)x.ProductCharacteristicID) && x.ProductPlaceID == _placeID && n.Contains((Guid)x.NomenclatureID)).Select(x => new ProductionProducts()
                    {
                        NomenclatureID = x.ProductNomenclatureID,
                        CharacteristicID = (Guid)x.ProductCharacteristicID,
                        NomenclatureName = x.ProductNomenclatureName
                    }).ToList();
                }
                if (AvailableProductionProducts == null || AvailableProductionProducts?.Count == 0)
                {
                    AvailableProductionProducts = DB.GammaDb.C1CCharacteristics.Where(x => _productionProducts.Contains(x.C1CCharacteristicID)).Select(x => new ProductionProducts()
                    {
                        NomenclatureID = x.C1CNomenclatureID,
                        CharacteristicID = x.C1CCharacteristicID,
                        NomenclatureName = x.C1CNomenclature.Name + " " + x.Name,
                    }).ToList();
                    /*var a = _productionProducts.ConvertAll(x => new ProductionProducts()
                    {
                        CharacteristicID = x
                    });
                    if (a != null)
                    {
                        AvailableProductionProducts = a;
                    }*/
                }
            }
            if (AvailableProductionProducts?.Count == 1 && (ProductionProductCharacteristicID == Guid.Empty || ProductionProductCharacteristicID == null))
            {
                ProductionProductCharacteristicID = AvailableProductionProducts.FirstOrDefault().CharacteristicID;
            }
        }
    }

}
