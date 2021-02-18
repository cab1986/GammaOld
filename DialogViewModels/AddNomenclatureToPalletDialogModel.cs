using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Gamma.ViewModels;

namespace Gamma.DialogViewModels
{
    class AddNomenclatureToPalletDialogModel : DbEditItemWithNomenclatureViewModel
    {
        private decimal _measureUnitCoefficient;
        private int _groupPacksCount;

        public AddNomenclatureToPalletDialogModel()
        {
            PlaceGroupID = (int) PlaceGroup.Convertings;
        }

        public AddNomenclatureToPalletDialogModel(string barcode) : this()
        {
            using (var gammaBase = DB.GammaDb)
            {
                var nomInfo =
                    gammaBase.Products.FirstOrDefault(p => p.BarCode == barcode)?
                        .ProductPallets.ProductItems.FirstOrDefault() ?? gammaBase.Products.FirstOrDefault(p => p.Number == barcode)?
                        .ProductPallets.ProductItems.FirstOrDefault();
                if (nomInfo == null) return;
                NomenclatureID = nomInfo.C1CNomenclatureID;
                CharacteristicID = nomInfo.C1CCharacteristicID;
                MeasureUnitCoefficient =
                    gammaBase.C1CCharacteristics.FirstOrDefault(c => c.C1CCharacteristicID == CharacteristicID)?
                        .C1CMeasureUnitsPackage.Coefficient ?? 0;
            }
        }

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

        public decimal Quantity { get; private set; }
    }
}
