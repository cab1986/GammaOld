// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
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
