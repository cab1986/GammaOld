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
    
    public partial class C1CRejectionReasons
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public C1CRejectionReasons()
        {
            this.DocChangeStateProducts = new HashSet<DocChangeStateProducts>();
            this.DocBrokeProductRejectionReasons = new HashSet<DocBrokeProductRejectionReasons>();
            this.ProductKinds = new HashSet<ProductKinds>();
        }
    
        public System.Guid C1CRejectionReasonID { get; set; }
        public Nullable<bool> IsMarked { get; set; }
        public Nullable<bool> IsFolder { get; set; }
        public Nullable<System.Guid> ParentID { get; set; }
        public string Description { get; set; }
        public string FullDescription { get; set; }
        public Nullable<System.Guid> C1CNewRejectionReasonID { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<DocChangeStateProducts> DocChangeStateProducts { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<DocBrokeProductRejectionReasons> DocBrokeProductRejectionReasons { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ProductKinds> ProductKinds { get; set; }
    }
}
