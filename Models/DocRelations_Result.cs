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
    
    public partial class DocRelations_Result
    {
        public string RelationKind { get; set; }
        public string ProductKind { get; set; }
        public string Number { get; set; }
        public System.DateTime Date { get; set; }
        public System.Guid DocID { get; set; }
        public System.Guid ProductID { get; set; }
        public Nullable<byte> ProductKindID { get; set; }
    }
}