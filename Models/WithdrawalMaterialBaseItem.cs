using DevExpress.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gamma.Models
{
    public class WithdrawalMaterialBaseItem : ViewModelBase
    {
        private int _placeID { get; set; }
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

        private Guid _nomenclatureID;
        public virtual Guid NomenclatureID
        {
            get { return _nomenclatureID; }
            set
            {
                _nomenclatureID = value;
                RaisePropertiesChanged("NomenclatureID");
            }
        }

        private Guid? _characteristicID;
        public virtual Guid? CharacteristicID
        {
            get { return _characteristicID; }
            set
            {
                _characteristicID = value;
            }
        }

        public Guid? ProductID { get; set; }
        public string NomenclatureName { get; set; }
        public decimal BaseQuantity { get; set; }
        public decimal Quantity { get; set; }
        public bool IsFloatValue { get; set; }
        public Guid? DocMovementID { get; set; }
        public int? NomenclatureKindID { get; set; }

        public string ParentName { get; set; }
        public Guid? ParentID { get; set; }

        private bool _quantityIsReadOnly { get; set; }
        /// <summary>
        /// Можно ли менять количество
        /// </summary>
        public virtual bool QuantityIsReadOnly
        {
            get { return _quantityIsReadOnly; }
            set
            {
                _quantityIsReadOnly = value;
            }
        }

        /// <summary>
        /// Списание по факту (или по нормативам)
        /// </summary>
        public bool? WithdrawByFact { get; set; }

        public string MeasureUnit { get; set; }

        public Guid? MeasureUnitID { get; set; }

    }
}
