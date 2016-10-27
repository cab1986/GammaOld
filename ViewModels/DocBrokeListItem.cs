using System;

namespace Gamma.ViewModels
{
    public class DocBrokeListItem{
        public Guid DocId { get; set; }
        public string Number { get; set; }
        public DateTime Date { get; set; }
        public string PlaceDiscover { get; set; }
        public string PlaceStore { get; set; }
    }
}