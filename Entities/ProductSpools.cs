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
    
    public partial class ProductSpools
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ProductSpools()
        {
            this.SourceSpools = new HashSet<SourceSpools>();
            this.SourceSpools1 = new HashSet<SourceSpools>();
            this.SourceSpools2 = new HashSet<SourceSpools>();
            this.SpoolInstallLog = new HashSet<SpoolInstallLog>();
        }
    
        public System.Guid ProductID { get; set; }
        public System.Guid C1CNomenclatureID { get; set; }
        public System.Guid C1CCharacteristicID { get; set; }
        public Nullable<int> RealFormat { get; set; }
        public int Diameter { get; set; }
        public int Weight { get; set; }
        public decimal DecimalWeight { get; set; }
        public Nullable<decimal> Length { get; set; }
        public Nullable<decimal> RealBasisWeight { get; set; }
        public Nullable<byte> ToughnessKindID { get; set; }
        public Nullable<byte> BreakNumber { get; set; }
    
        public virtual C1CCharacteristics C1CCharacteristics { get; set; }
        public virtual C1CNomenclature C1CNomenclature { get; set; }
        public virtual Products Products { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SourceSpools> SourceSpools { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SourceSpools> SourceSpools1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SourceSpools> SourceSpools2 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SpoolInstallLog> SpoolInstallLog { get; set; }
    }
}