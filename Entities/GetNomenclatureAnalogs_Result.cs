//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Gamma.Entities
{
    using System;
    
    public partial class GetNomenclatureAnalogs_Result
    {
        public System.Guid ID { get; set; }
        public Nullable<System.Guid> C1CNomenclatureID { get; set; }
        public Nullable<System.Guid> C1CCharacteristicID { get; set; }
        public Nullable<System.Guid> C1CNomenclatureAnalogID { get; set; }
        public Nullable<System.Guid> C1CCharacteristicAnalogID { get; set; }
        public Nullable<System.Guid> C1COutputNomenclatureID { get; set; }
        public Nullable<System.Guid> C1COutputCharacteristicID { get; set; }
        public Nullable<System.Guid> C1CSpecificationID { get; set; }
        public Nullable<System.Guid> C1CMeasureUnitID { get; set; }
        public Nullable<decimal> Amount { get; set; }
        public Nullable<System.Guid> C1CMeasureUnitAnalogID { get; set; }
        public Nullable<decimal> AmountAnalog { get; set; }
        public Nullable<decimal> Priority { get; set; }
        public Nullable<System.DateTime> StartDate { get; set; }
        public Nullable<System.DateTime> EndDate { get; set; }
        public string NomenclatureName { get; set; }
        public string NomenclatureAnalogName { get; set; }
        public Nullable<bool> NomenclatureAnalogIsArchive { get; set; }
        public string MeasureUnitName { get; set; }
        public string MeasureUnitAnalogName { get; set; }
    }
}
