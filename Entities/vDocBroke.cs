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
    
    public partial class vDocBroke
    {
        public System.Guid DocID { get; set; }
        public string Number { get; set; }
        public System.DateTime Date { get; set; }
        public Nullable<int> DocTypeID { get; set; }
        public string DocTypeName { get; set; }
        public string Comment { get; set; }
        public bool IsConfirmed { get; set; }
        public Nullable<System.DateTime> LastUploadedTo1C { get; set; }
        public Nullable<bool> IsInFuturePeriod { get; set; }
        public string IsInFuturePeriodName { get; set; }
        public string UserCreate { get; set; }
        public string Places { get; set; }
    }
}
