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
    
    public partial class C1CCharacteristics
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public C1CCharacteristics()
        {
            this.C1CCharacteristicProperties = new HashSet<C1CCharacteristicProperties>();
            this.C1CNomenclatureAnalogs = new HashSet<C1CNomenclatureAnalogs>();
            this.C1CNomenclatureAnalogs1 = new HashSet<C1CNomenclatureAnalogs>();
            this.C1CNomenclatureAnalogs2 = new HashSet<C1CNomenclatureAnalogs>();
            this.C1CSpecificationInputNomenclature = new HashSet<C1CSpecificationInputNomenclature>();
            this.C1CSpecificationOutputNomenclature = new HashSet<C1CSpecificationOutputNomenclature>();
            this.DocWithdrawalMaterials = new HashSet<DocWithdrawalMaterials>();
            this.NomenclatureBarcodes = new HashSet<NomenclatureBarcodes>();
            this.ProductionTaskRWCutting = new HashSet<ProductionTaskRWCutting>();
            this.ProductionTasks = new HashSet<ProductionTasks>();
            this.ProductItems = new HashSet<ProductItems>();
            this.ProductGroupPacks = new HashSet<ProductGroupPacks>();
            this.DocCloseShiftNomenclatureRests = new HashSet<DocCloseShiftNomenclatureRests>();
            this.C1CMainSpecifications = new HashSet<C1CMainSpecifications>();
            this.DocCloseShiftWastes = new HashSet<DocCloseShiftWastes>();
            this.ProductSpools = new HashSet<ProductSpools>();
            this.ProductBales = new HashSet<ProductBales>();
            this.DocCloseShiftSamples = new HashSet<DocCloseShiftSamples>();
            this.C1CDocComplectationNomenclature = new HashSet<C1CDocComplectationNomenclature>();
            this.C1CDocComplectationNomenclature1 = new HashSet<C1CDocComplectationNomenclature>();
        }
    
        public System.Guid C1CCharacteristicID { get; set; }
        public System.Guid C1CNomenclatureID { get; set; }
        public string C1CCode { get; set; }
        public Nullable<System.Guid> MeasureUnitPackage { get; set; }
        public Nullable<System.Guid> MeasureUnitPallet { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public string PrintName { get; set; }
        public string PackageLabelPath { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<C1CCharacteristicProperties> C1CCharacteristicProperties { get; set; }
        public virtual C1CMeasureUnits C1CMeasureUnitsPackage { get; set; }
        public virtual C1CMeasureUnits C1CMeasureUnitsPallet { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<C1CNomenclatureAnalogs> C1CNomenclatureAnalogs { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<C1CNomenclatureAnalogs> C1CNomenclatureAnalogs1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<C1CNomenclatureAnalogs> C1CNomenclatureAnalogs2 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<C1CSpecificationInputNomenclature> C1CSpecificationInputNomenclature { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<C1CSpecificationOutputNomenclature> C1CSpecificationOutputNomenclature { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<DocWithdrawalMaterials> DocWithdrawalMaterials { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<NomenclatureBarcodes> NomenclatureBarcodes { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ProductionTaskRWCutting> ProductionTaskRWCutting { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ProductionTasks> ProductionTasks { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ProductItems> ProductItems { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ProductGroupPacks> ProductGroupPacks { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<DocCloseShiftNomenclatureRests> DocCloseShiftNomenclatureRests { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<C1CMainSpecifications> C1CMainSpecifications { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<DocCloseShiftWastes> DocCloseShiftWastes { get; set; }
        public virtual C1CNomenclature C1CNomenclature { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ProductSpools> ProductSpools { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ProductBales> ProductBales { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<DocCloseShiftSamples> DocCloseShiftSamples { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<C1CDocComplectationNomenclature> C1CDocComplectationNomenclature { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<C1CDocComplectationNomenclature> C1CDocComplectationNomenclature1 { get; set; }
    }
}
