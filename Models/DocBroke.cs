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
    
    public partial class DocBroke
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public DocBroke()
        {
            this.DocBrokeDecisionProducts = new HashSet<DocBrokeDecisionProducts>();
            this.DocBrokeProducts = new HashSet<DocBrokeProducts>();
        }
    
        public System.Guid DocID { get; set; }
        public Nullable<System.Guid> PlaceDiscoverID { get; set; }
        public Nullable<System.Guid> PlaceStoreID { get; set; }
        public Nullable<bool> IsInFuturePeriod { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<DocBrokeDecisionProducts> DocBrokeDecisionProducts { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<DocBrokeProducts> DocBrokeProducts { get; set; }
        public virtual Docs Docs { get; set; }
    }
}
