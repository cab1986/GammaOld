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
    using System.Collections.Generic;
    
    public partial class vProductsBaseInfo
    {
        public System.Guid ProductID { get; set; }
        public byte ProductKindID { get; set; }
        public string Number { get; set; }
        public string BarCode { get; set; }
        public System.Guid C1CNomenclatureID { get; set; }
        public Nullable<System.Guid> C1CCharacteristicID { get; set; }
        public Nullable<decimal> Quantity { get; set; }
        public Nullable<decimal> BaseMeasureUnitQuantity { get; set; }
        public Nullable<decimal> GrossQuantity { get; set; }
        public Nullable<decimal> BaseMeasureUnitGrossQuantity { get; set; }
        public string BaseMeasureUnitName { get; set; }
    }
}
