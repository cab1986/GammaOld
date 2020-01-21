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

        private string _nomenclatureIDDiezCharacteristicID { get; set; }
        public string NomenclatureIDDiezCharacteristicID
        {
            get { return NomenclatureID.ToString() + "#" + CharacteristicID?.ToString(); /*return _nomenclatureIDDiezCharacteristicID; */}
            set
            {
                /*_nomenclatureIDDiezCharacteristicID = value;*/
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
            }
        }

        public Guid DocWithdrawalMaterialID { get; set; } = SqlGuidUtil.NewSequentialid();

        private void RefreshAvilableNomenclatures()
        {
            AvailableNomenclatures = new List<NomenclatureAnalog>();
            using (var gammaBase = DB.GammaDb)
            {
                var nomenclatureInfo =
                    gammaBase.C1CNomenclature.Include(n => n.C1CMeasureUnitStorage).First(n => n.C1CNomenclatureID == _nomenclatureID);
                var characteristicInfo =
                    gammaBase.C1CCharacteristics.Where(n => n.C1CNomenclatureID == _nomenclatureID && (n.C1CCharacteristicID == CharacteristicID || CharacteristicID == null));
                if (characteristicInfo?.Count() > 0)
                {
                    foreach (var item in characteristicInfo)
                    {
                        if (AvailableNomenclatures.Where(n => n.NomenclatureID == _nomenclatureID && n.CharacteristicID == CharacteristicID).Count() == 0)
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
                gammaBase.C1CNomenclatureAnalogs.Where(a => a.C1CNomenclatureID == _nomenclatureID && (a.C1CCharacteristicID == CharacteristicID || CharacteristicID == null))
                    .Select(a => new
                    {
                        NomenclatureId = a.C1CNomenclatureAnalogID,
                        CharacteristicID = a.C1CCharacteristicAnalogID,
                        NomenclatureName = gammaBase.C1CNomenclature.Where(x => x.C1CNomenclatureID == a.C1CNomenclatureAnalogID).Select(x => x.Name).FirstOrDefault() + gammaBase.C1CCharacteristics.Where(x => x.C1CCharacteristicID == a.C1CCharacteristicAnalogID).Select(x => " " + x.Name).FirstOrDefault(),
                        IsMarked = a.C1CAnalogNomenclature.IsArchive ?? false,
                        MeasureUnit = a.C1CAnalogMeasureUnits.Name,
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

                    //var avilableCharacteristicID = AvailableNomenclatures.Select(x => x.CharacteristicID).Distinct().ToList();
                    //var analogCharacteristicInfo =
                    //gammaBase.C1CCharacteristics.Where(n => n.C1CNomenclatureID == _nomenclatureID && !avilableCharacteristicID.Contains(n.C1CCharacteristicID));
                    /*var analogCharacteristicInfo =
                    gammaBase.C1CCharacteristics.Where(n => n.C1CNomenclatureID == analog.NomenclatureId);
                    if (analogCharacteristicInfo?.Count() > 0)
                    {
                        foreach (var item in analogCharacteristicInfo)
                        {
                            if (AvailableNomenclatures.Where(n => n.NomenclatureID == analog.NomenclatureId && n.CharacteristicID == item.C1CCharacteristicID).Count() == 0)
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
                        if (AvailableNomenclatures.Where(n => n.NomenclatureID == analog.NomenclatureId && n.CharacteristicID == null).Count() == 0)
                            AvailableNomenclatures.Add(new NomenclatureAnalog()
                            {
                                NomenclatureID = (Guid)analog.NomenclatureId,
                                NomenclatureName = analog.NomenclatureName,
                                IsMarked = analog.IsMarked,
                                MeasureUnit = analog.MeasureUnit,
                                MeasureUnitID = (Guid)analog.MeasureUnitID,
                                Coefficient = analog.Coefficient
                            });
                    }*/

                }
                if (nomenclatureInfo.IsArchive ?? false)
                {
                    var activeNomenclature = AvailableNomenclatures.FirstOrDefault(an => !an.IsMarked);
                    if (activeNomenclature != null)
                        NomenclatureID = activeNomenclature.NomenclatureID;
                }

            }
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

                //NomenclatureName = value != new Guid() ? DB.GammaDb.C1CNomenclature.First(n => n.C1CNomenclatureID == value).Name : string.Empty;
                //if (AvailableNomenclatures == null)

                //else
                {
                    var choosenNomenclature = AvailableNomenclatures?.First(an => an.NomenclatureID == _nomenclatureID && (an.CharacteristicID == CharacteristicID || CharacteristicID == null || CharacteristicID == Guid.Empty));
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

                var choosenNomenclature = AvailableNomenclatures?.First(an => an.NomenclatureID == _nomenclatureID && (an.CharacteristicID == CharacteristicID || CharacteristicID == null || CharacteristicID == Guid.Empty));
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
        }

        public Guid? ProductID { get; set; }
        public string NomenclatureName { get; set; }
        public decimal BaseQuantity { get; set; }
        public decimal Quantity { get; set; }
        public bool IsFloatValue { get; set; }
        public Guid? DocMovementID { get; set; }
        public int? NomenclatureKindID { get; set; }

        /// <summary>
        /// Можно ли менять количество
        /// </summary>
        public bool QuantityIsReadOnly { get; set; }

        /// <summary>
        /// Списание по факту (или по нормативам)
        /// </summary>
        public bool? WithdrawByFact { get; set; }

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

            public string NomenclatureIDDiezCharacteristicID
            {
                get { return NomenclatureID.ToString() + "#" + CharacteristicID?.ToString(); }
            }
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
                    }).Distinct().ToList();
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
