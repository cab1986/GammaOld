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
    
    public partial class DocProduction
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public DocProduction()
        {
            this.DocWithdrawal = new HashSet<DocWithdrawal>();
            this.DocProductionProducts = new HashSet<DocProductionProducts>();
        }
    
        public System.Guid DocID { get; set; }
        public Nullable<System.Guid> ProductionTaskID { get; set; }
        public Nullable<int> InPlaceID { get; set; }
        public Nullable<bool> HasWarnings { get; set; }
    
        public virtual Docs Docs { get; set; }
        public virtual ProductionTasks ProductionTasks { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<DocWithdrawal> DocWithdrawal { get; set; }
        public virtual Places Places { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<DocProductionProducts> DocProductionProducts { get; set; }
    }
}
