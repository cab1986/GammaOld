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
    
    public partial class DocCloseShiftDowntimes
    {
        public System.Guid DocCloseShiftDowntimeID { get; set; }
        public System.Guid DocID { get; set; }
        public System.Guid C1CDowntimeTypeID { get; set; }
        public Nullable<System.Guid> C1CDowntimeTypeDetailID { get; set; }
        public Nullable<System.DateTime> DateBegin { get; set; }
        public Nullable<System.DateTime> DateEnd { get; set; }
        public int Duration { get; set; }
        public string Comment { get; set; }
        public Nullable<System.Guid> ProductionTaskID { get; set; }
        public Nullable<System.Guid> C1CEquipmentNodeID { get; set; }
        public Nullable<System.Guid> C1CEquipmentNodeDetailID { get; set; }
    
        public virtual C1CDowntimeTypeDetails C1CDowntimeTypeDetails { get; set; }
        public virtual C1CDowntimeTypes C1CDowntimeTypes { get; set; }
        public virtual Docs Docs { get; set; }
        public virtual C1CEquipmentNodeDetails C1CEquipmentNodeDetails { get; set; }
        public virtual C1CEquipmentNodes C1CEquipmentNodes { get; set; }
    }
}