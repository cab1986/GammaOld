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
    
    public partial class ProductGroupPacks
    {
        public System.Guid ProductID { get; set; }
        public System.Guid C1CNomenclatureID { get; set; }
        public System.Guid C1CCharacteristicID { get; set; }
        public Nullable<decimal> Weight { get; set; }
        public Nullable<decimal> GrossWeight { get; set; }
        public Nullable<short> Diameter { get; set; }
        public Nullable<bool> ManualWeightInput { get; set; }
    
        public virtual C1CCharacteristics C1CCharacteristics { get; set; }
        public virtual Products Products { get; set; }
        public virtual C1CNomenclature C1CNomenclature { get; set; }
    }
}
