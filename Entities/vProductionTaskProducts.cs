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
    
    public partial class vProductionTaskProducts
    {
        public Nullable<System.Guid> ProductionTaskID { get; set; }
        public Nullable<int> PlaceID { get; set; }
        public System.Guid DocID { get; set; }
        public byte ProductKindID { get; set; }
        public System.Guid ProductID { get; set; }
        public string Number { get; set; }
        public System.DateTime Date { get; set; }
        public string NomenclatureName { get; set; }
        public Nullable<System.Guid> NomenclatureID { get; set; }
        public Nullable<System.Guid> CharacteristicID { get; set; }
        public Nullable<int> Quantity { get; set; }
    }
}
