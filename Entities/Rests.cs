//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан по шаблону.
//
//     Изменения, вносимые в этот файл вручную, могут привести к непредвиденной работе приложения.
//     Изменения, вносимые в этот файл вручную, будут перезаписаны при повторном создании кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Gamma.Entities
{
    using System;
    using System.Collections.Generic;
    
    public partial class Rests
    {
        public System.Guid ProductID { get; set; }
        public Nullable<int> PlaceID { get; set; }
        public int Quantity { get; set; }
        public Nullable<System.Guid> PlaceZoneID { get; set; }
    
        public virtual PlaceZones PlaceZones { get; set; }
        public virtual Places Places { get; set; }
    }
}
