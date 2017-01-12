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
