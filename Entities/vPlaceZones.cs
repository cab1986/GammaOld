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
    
    public partial class vPlaceZones
    {
        public System.Guid PlaceZoneID { get; set; }
        public int PlaceID { get; set; }
        public string Name { get; set; }
        public Nullable<System.Guid> PlaceZoneParentID { get; set; }
        public string Barcode { get; set; }
        public string SortOrder { get; set; }
        public Nullable<bool> v { get; set; }
        public Nullable<bool> MayBeProductsHere { get; set; }
        public Nullable<System.Guid> PlaceZoneRootID { get; set; }
        public Nullable<int> Sleeps { get; set; }
    }
}
