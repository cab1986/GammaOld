//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан по шаблону.
//
//     Изменения, вносимые в этот файл вручную, могут привести к непредвиденной работе приложения.
//     Изменения, вносимые в этот файл вручную, будут перезаписаны при повторном создании кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Gamma.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class DocMovement
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public DocMovement()
        {
            this.DocInProducts = new HashSet<DocInProducts>();
            this.DocOutProducts = new HashSet<DocOutProducts>();
        }
    
        public System.Guid DocID { get; set; }
        public Nullable<int> OutPlaceID { get; set; }
        public Nullable<int> InPlaceID { get; set; }
        public Nullable<int> TransferPlaceID { get; set; }
        public Nullable<System.Guid> DocOrderID { get; set; }
        public Nullable<byte> OrderTypeID { get; set; }
        public Nullable<System.DateTime> OutDate { get; set; }
        public Nullable<System.DateTime> InDate { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<DocInProducts> DocInProducts { get; set; }
        public virtual Places OutPlaces { get; set; }
        public virtual Places TransferPlaces { get; set; }
        public virtual Places InPlaces { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<DocOutProducts> DocOutProducts { get; set; }
        public virtual Docs Docs { get; set; }
    }
}
