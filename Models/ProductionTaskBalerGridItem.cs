// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using Gamma.ViewModels;

namespace Gamma.Models
{
    public class ProductionTaskBalerGridItem : RootViewModel
    {
        public ProductionTaskBalerGridItem(Guid productionTaskBatchId,  string nomenclatureName)
        {
            NomenclatureName = nomenclatureName;
            ProductionTaskBatchID = productionTaskBatchId;
        }

        public Guid ProductionTaskBatchID { get; set; }

        public string NomenclatureName { get; set; }
    }
}
