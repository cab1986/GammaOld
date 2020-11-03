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
    }
}
