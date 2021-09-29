using System;
using System.Collections.Generic;
using DevExpress.Mvvm;

namespace Gamma.Models
{
    public class PlaceAuxiliaryMaterial : ViewModelBase
    {
        public Guid? PlaceAuxiliaryMaterialID { get; set; }
        public Guid NomenclatureID { get; set; }
        public Guid? CharacteristicID { get; set; }
        public string NomenclatureName { get; set; }
        public string CharacteristicName { get; set; }
        public int PlaceID { get; set; }
        public string PlaceName { get; set; }

    }
}