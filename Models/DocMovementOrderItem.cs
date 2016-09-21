using System;
using System.Collections.ObjectModel;

namespace Gamma.Models
{
    public class DocMovementOrderItem
    {
        public Guid DocId { get; set; }
        public string Number { get; set; }
        public DateTime Date { get; set; }
        public string PlaceTo { get; set; }
        public string PlaceFrom { get; set; }
        public ObservableCollection<DocNomenclatureItem> NomenclatureItems { get; set; }
    }
}
