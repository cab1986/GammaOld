using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Gamma.ViewModels;
using System.Collections.Generic;
using Gamma.Common;
using Gamma.Entities;

namespace Gamma.DialogViewModels
{
    class ChooseMeasureUnitDialogModel : DbEditItemWithNomenclatureViewModel
    {
        //private decimal _measureUnitCoefficient;
        //private int _groupPacksCount;
        public Dictionary<byte, string> WithdrawalTypes { get; set; }
        private byte? _withdrawalTypeID { get; set; }
        public byte? WithdrawalTypeID
        {
            get
            {
                return _withdrawalTypeID;
            }
            set
            {
                _withdrawalTypeID = value;
            }
        }
        
        public string MeasureUnitName { get; set; }
        private Guid _measureUnitID { get; set; }
        public Guid MeasureUnitID
        {
            get
            {
                return _measureUnitID;
            }
            set
            {
                _measureUnitID = value;
                MeasureUnitName = MeasureUnits.FirstOrDefault(m => m.C1CMeasureUnitID == _measureUnitID).Name;
            }
        }

        public List<C1CMeasureUnits> MeasureUnits { get; set; }

        public ChooseMeasureUnitDialogModel()
        {
            WithdrawalTypes = new WithdrawalTypes().ToDictionary();
            //PlaceGroupID = (int) PlaceGroup.Convertings;
        }

        public ChooseMeasureUnitDialogModel(Guid nomenclatureID) : this()
        {
            using (var gammaBase = DB.GammaDb)
            {
                var nomInfo =
                    gammaBase.C1CNomenclature.FirstOrDefault(p => p.C1CNomenclatureID == nomenclatureID );
                if (nomInfo == null) return;
                MeasureUnits =
                    gammaBase.C1CMeasureUnits.Where(c => c.C1CMeasureUnitID == nomInfo.C1CMeaureUnitStorage || c.C1CMeasureUnitID == nomInfo.C1CMeasureUnitVolume ).ToList();
            }
        }
        /*
        public override Guid? CharacteristicID
        {
            get { return base.CharacteristicID; }
            set
            {
                base.CharacteristicID = value;
                if (value == null) return;
                using (var gammaBase = DB.GammaDb)
                {
                    MeasureUnitCoefficient =
                        gammaBase.C1CCharacteristics.FirstOrDefault(c => c.C1CCharacteristicID == CharacteristicID)?
                            .C1CMeasureUnitsPackage.Coefficient ?? 0;
                }
            }
        }

        private decimal MeasureUnitCoefficient
        {
            get { return _measureUnitCoefficient; }
            set
            {
                _measureUnitCoefficient = value;
                Quantity = GroupPacksCount*value;
            }
        }

        [Range(1, 100, ErrorMessage = @"Необходимо указать корректное количество")]
        public int GroupPacksCount
        {
            get { return _groupPacksCount; }
            set
            {
                _groupPacksCount = value;
                Quantity = GroupPacksCount*MeasureUnitCoefficient;
            }
        }

        public decimal Quantity { get; private set; }*/
    }
}
