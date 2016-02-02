using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gamma.Common
{
    public class Nomenclature1CFolder
    {
        public string Name { get; set; }
        public Guid FolderID { get; set; }
        public Guid? ParentFolderID { get; set; }
    }
}
