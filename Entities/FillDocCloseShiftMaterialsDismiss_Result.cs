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
    
    public partial class FillDocCloseShiftMaterialsDismiss_Result
    {
        public System.Guid NomenclatureID { get; set; }
        public Nullable<System.Guid> CharacteristicID { get; set; }
        public bool WithdrawByFact { get; set; }
        public Nullable<System.Guid> MeasureUnitID { get; set; }
        public string MeasureUnit { get; set; }
        public decimal Quantity { get; set; }
        public string NomenclatureName { get; set; }
        public Nullable<bool> QuantityIsReadOnly { get; set; }
    }
}
