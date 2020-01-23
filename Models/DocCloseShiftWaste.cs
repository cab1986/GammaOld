using DevExpress.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gamma.Models
{
    public class DocCloseShiftWaste : ViewModelBase
    {
        public Guid NomenclatureID { get; set; }
        public Guid? CharacteristicID { get; set; }
        public string NomenclatureName { get; set; }
        public Guid? MeasureUnitId { get; set; }
        public string MeasureUnit { get; set; }
        public decimal Quantity { get; set; }
        public Dictionary<Guid, string> MeasureUnits { get; set; }
        public Guid? ProductNomenclatureID { get; set; }
        public Guid? ProductCharacteristicID { get; set; }
        public string ProductName { get; set; }
    }
}
