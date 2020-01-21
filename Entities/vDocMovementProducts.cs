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
    
    public partial class vDocMovementProducts
    {
        public System.Guid DocMovementID { get; set; }
        public System.Guid ProductID { get; set; }
        public byte ProductKindID { get; set; }
        public Nullable<System.Guid> DocOrderID { get; set; }
        public Nullable<byte> OrderTypeID { get; set; }
        public Nullable<int> InPlaceID { get; set; }
        public Nullable<int> OutPlaceID { get; set; }
        public Nullable<bool> IsShipped { get; set; }
        public Nullable<bool> IsAccepted { get; set; }
        public Nullable<bool> IsConfirmed { get; set; }
        public Nullable<System.DateTime> OutDate { get; set; }
        public Nullable<System.DateTime> InDate { get; set; }
        public Nullable<System.Guid> OutPersonID { get; set; }
        public Nullable<System.Guid> InPersonID { get; set; }
        public System.DateTime DocDate { get; set; }
        public string OutPerson { get; set; }
        public string InPerson { get; set; }
        public Nullable<decimal> Quantity { get; set; }
        public System.Guid C1CNomenclatureID { get; set; }
        public Nullable<System.Guid> C1CCharacteristicID { get; set; }
        public Nullable<System.Guid> C1CQualityID { get; set; }
        public string NomenclatureName { get; set; }
        public string ShortNomenclatureName { get; set; }
        public string Number { get; set; }
        public string BarCode { get; set; }
        public Nullable<System.Guid> InPlaceZoneID { get; set; }
        public string InPlaceZone { get; set; }
        public Nullable<System.Guid> OutPlaceZoneID { get; set; }
        public string OutPlaceZone { get; set; }
        public Nullable<decimal> CoefficientPackage { get; set; }
        public Nullable<decimal> CoefficientPallet { get; set; }
        public string ProductKindName { get; set; }
        public string OrderTypeName { get; set; }
        public string InPlace { get; set; }
        public string OutPlace { get; set; }
        public Nullable<byte> ShiftID { get; set; }
        public string NumberDocMovement { get; set; }
        public Nullable<byte> StateID { get; set; }
        public string StateName { get; set; }
    }
}
