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
    
    public partial class vProductsInfo
    {
        public System.Guid DocID { get; set; }
        public System.Guid ProductID { get; set; }
        public Nullable<byte> ProductKindID { get; set; }
        public string Number { get; set; }
        public Nullable<System.DateTime> Date { get; set; }
        public string BarCode { get; set; }
        public Nullable<System.Guid> C1CNomenclatureID { get; set; }
        public Nullable<System.Guid> C1CCharacteristicID { get; set; }
        public Nullable<decimal> Quantity { get; set; }
        public Nullable<decimal> BaseMeasureUnitQuantity { get; set; }
        public string NomenclatureName { get; set; }
        public string Place { get; set; }
        public Nullable<int> PlaceID { get; set; }
        public Nullable<short> PlaceGroupID { get; set; }
        public Nullable<byte> ShiftID { get; set; }
        public Nullable<byte> StateID { get; set; }
        public Nullable<decimal> ChangeStateQuantity { get; set; }
        public string RejectionReason { get; set; }
        public Nullable<bool> IsConfirmed { get; set; }
        public Nullable<bool> IsWrittenOff { get; set; }
        public string State { get; set; }
    }
}
