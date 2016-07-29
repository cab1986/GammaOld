using System;

namespace Gamma.Models
{
    public class Nomenclature1CFolder
    {
        public string Name { get; set; }
        public Guid FolderID { get; set; }
        public Guid? ParentFolderID { get; set; }
    }
}
