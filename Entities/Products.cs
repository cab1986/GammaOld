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
    
    public partial class Products
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Products()
        {
            this.DocProductionProducts = new HashSet<DocProductionProducts>();
            this.DocBrokeDecisionProducts = new HashSet<DocBrokeDecisionProducts>();
            this.DocBrokeProductRejectionReasons = new HashSet<DocBrokeProductRejectionReasons>();
            this.DocCloseShiftRemainders = new HashSet<DocCloseShiftRemainders>();
            this.DocWithdrawalProducts = new HashSet<DocWithdrawalProducts>();
            this.DocCloseShift = new HashSet<Docs>();
        }
    
        public System.Guid ProductID { get; set; }
        public string Number { get; set; }
        public string BarCode { get; set; }
        public byte ProductKindID { get; set; }
        public Nullable<byte> StateID { get; set; }
    
        public virtual ProductGroupPacks ProductGroupPacks { get; set; }
        public virtual ProductKinds ProductKinds { get; set; }
        public virtual ProductStates ProductStates { get; set; }
        public virtual ProductPallets ProductPallets { get; set; }
        public virtual ProductSpools ProductSpools { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<DocProductionProducts> DocProductionProducts { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<DocBrokeDecisionProducts> DocBrokeDecisionProducts { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<DocBrokeProductRejectionReasons> DocBrokeProductRejectionReasons { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<DocCloseShiftRemainders> DocCloseShiftRemainders { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<DocWithdrawalProducts> DocWithdrawalProducts { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Docs> DocCloseShift { get; set; }
    }
}
