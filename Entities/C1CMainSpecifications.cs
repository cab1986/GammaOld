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
    
    public partial class C1CMainSpecifications
    {
        public System.Guid C1CMainSpecificationID { get; set; }
        public Nullable<System.DateTime> Period { get; set; }
        public System.Guid C1CNomenclatureID { get; set; }
        public Nullable<System.Guid> C1CCharacteristicID { get; set; }
        public Nullable<System.Guid> C1CSpecificationID { get; set; }
        public Nullable<System.Guid> C1CPlaceID { get; set; }
        public Nullable<bool> C1CDeleted { get; set; }
    
        public virtual C1CCharacteristics C1CCharacteristics { get; set; }
        public virtual C1CPlaces C1CPlaces { get; set; }
        public virtual C1CSpecifications C1CSpecifications { get; set; }
        public virtual C1CNomenclature C1CNomenclature { get; set; }
    }
}
