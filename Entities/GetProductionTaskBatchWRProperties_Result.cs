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
    
    public partial class GetProductionTaskBatchWRProperties_Result
    {
        public System.Guid ProductionTaskID { get; set; }
        public Nullable<byte> NumFilmLayers { get; set; }
        public Nullable<bool> IsWithCarton { get; set; }
        public Nullable<bool> IsEndProtected { get; set; }
        public string GroupPackConfig { get; set; }
        public bool IsActual { get; set; }
    }
}