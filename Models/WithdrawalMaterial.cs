using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using DevExpress.Mvvm;

namespace Gamma.Models
{
    //TODO Добавить нормальный конструктор к WithdrawalMaterial

    /// <summary>
    /// Класс для грида материалов на списание
    /// </summary>
    public class WithdrawalMaterial : ViewModelBase
    {
        public Guid DocWithdrawalMaterialID { get; set; } = SqlGuidUtil.NewSequentialid();
        private Guid _nomenclatureID;

        public Guid NomenclatureID
        {
            get { return _nomenclatureID; }
            set
            {
                _nomenclatureID = value;
                NomenclatureName = value != new Guid() ? DB.GammaDb.C1CNomenclature.First(n => n.C1CNomenclatureID == value).Name : string.Empty;
                if (AvailableNomenclatures == null)
                {
                    AvailableNomenclatures = new List<NomenclatureAnalog>();
                    using (var gammaBase = DB.GammaDb)
                    {
                        var nomenclatureInfo =
                            gammaBase.C1CNomenclature.Include(n => n.C1CMeasureUnits).First(n => n.C1CNomenclatureID == _nomenclatureID);
                        AvailableNomenclatures.Add(new NomenclatureAnalog()
                        {
                            NomenclatureID = _nomenclatureID,
                            Coefficient = 1,
                            NomenclatureName = nomenclatureInfo.Name,
                            MeasureUnit = nomenclatureInfo.C1CMeasureUnits.FirstOrDefault()?.Name,
                            MeasureUnitID = nomenclatureInfo.C1CMeasureUnits.FirstOrDefault().C1CMeasureUnitID,
                            IsMarked = nomenclatureInfo.IsArchive ?? false
                        }); 
                        var analogs =
                        gammaBase.C1CNomenclatureAnalogs.Where(a => a.C1CNomenclatureID == _nomenclatureID)
                            .Select(a => new
                            {
                                NomenclatureId = a.C1CNomenclatureAnalogID,
                                IsMarked = a.C1CAnalogNomenclature.IsArchive ?? false,
                                MeasureUnit = a.C1CAnalogMeasureUnits.Name,
                                MeasureUnitID = a.C1CMeasureUnitAnalogID,
                                Coefficient = (a.AmountAnalog/a.Amount)??0}).Distinct()
                            .ToList();
                        foreach (var analog in analogs)
                        {
                            AvailableNomenclatures.Add(new NomenclatureAnalog()
                            {
                                NomenclatureID = (Guid)analog.NomenclatureId,
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
                        }}
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
                    NomenclatureName = value != new Guid() ? DB.GammaDb.C1CNomenclature.First(n => n.C1CNomenclatureID == value).Name : string.Empty;
                }
            }
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
    }

}
