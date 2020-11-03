using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gamma.Models
{
    class ReportParameters
    {
        public Guid? ID { get; set; }
        public DateTime? BeginDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? PlaceID { get; set; }
        public Guid? PlaceZoneID { get; set; }
        public int? ProductKindID { get; set; }
        public int? StateID { get; set; }
        public Guid? NomenclatureID { get; set; }
        public Guid? CharacteristicID { get; set; }
        public bool? IsVisibleDetailBand { get; set; }

        public override string ToString()
        {
            var retString = (ID != null ? "ID=" + ID + "; " : "") +
                (BeginDate != null ? "BeginDate=" + BeginDate + "; " : "") +
                (EndDate != null ? "EndDate=" + EndDate + "; " : "") +
                (PlaceID != null ? "PlaceID=" + PlaceID + "; " : "") +
                (PlaceZoneID != null ? "PlaceZoneID=" + PlaceZoneID + "; " : "") +
                (ProductKindID != null ? "ProductKindID=" + ProductKindID + "; " : "") +
                (StateID != null ? "StateID=" + StateID + "; " : "") +
                (NomenclatureID != null ? "NomenclatureID=" + NomenclatureID + "; " : "") +
                (CharacteristicID != null ? "CharacteristicID=" + CharacteristicID + "; " : "") +
                (IsVisibleDetailBand != null ? "IsVisibleDetailBand=" + IsVisibleDetailBand + "; " : "");
            return retString;// base.ToString();
        }
    }
}
