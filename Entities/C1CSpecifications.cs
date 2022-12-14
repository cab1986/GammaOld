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
    
    public partial class C1CSpecifications
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public C1CSpecifications()
        {
            this.C1CSpecificationInputNomenclature = new HashSet<C1CSpecificationInputNomenclature>();
            this.C1CSpecificationOutputNomenclature = new HashSet<C1CSpecificationOutputNomenclature>();
            this.C1CNomenclatureAnalogs = new HashSet<C1CNomenclatureAnalogs>();
            this.C1CMainSpecifications = new HashSet<C1CMainSpecifications>();
        }
    
        public System.Guid C1CSpecificationID { get; set; }
        public string Description { get; set; }
        public Nullable<System.Guid> ParentID { get; set; }
        public Nullable<bool> Marked { get; set; }
        public Nullable<bool> Folder { get; set; }
        public Nullable<byte> SpecificationType { get; set; }
        public Nullable<System.DateTime> ValidTill { get; set; }
        public string C1CCode { get; set; }
        public Nullable<bool> C1CDeleted { get; set; }
        public string Status { get; set; }
        public Nullable<System.DateTime> ValidFrom { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<C1CSpecificationInputNomenclature> C1CSpecificationInputNomenclature { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<C1CSpecificationOutputNomenclature> C1CSpecificationOutputNomenclature { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<C1CNomenclatureAnalogs> C1CNomenclatureAnalogs { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<C1CMainSpecifications> C1CMainSpecifications { get; set; }
    }
}
