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
    
    public partial class ProductionTaskConvertingSamples
    {
        public System.Guid ProductionTaskConvertingSampleID { get; set; }
        public System.Guid ProductionTaskID { get; set; }
        public System.DateTime Date { get; set; }
        public int ShiftID { get; set; }
        public System.Guid C1CNomenclatureID { get; set; }
        public System.Guid C1CCharacteristicID { get; set; }
        public decimal Quantity { get; set; }
        public Nullable<System.Guid> C1CMeasureUnitID { get; set; }
    
        public virtual C1CCharacteristics C1CCharacteristics { get; set; }
        public virtual C1CMeasureUnits C1CMeasureUnits { get; set; }
        public virtual C1CNomenclature C1CNomenclature { get; set; }
        public virtual ProductionTasks ProductionTasks { get; set; }
    }
}