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
    
    public partial class DocTypes
    {
        public DocTypes()
        {
            this.Docs = new HashSet<Docs>();
        }
    
        public int DocTypeID { get; set; }
        public string Name { get; set; }
    
        public virtual ICollection<Docs> Docs { get; set; }
    }
}