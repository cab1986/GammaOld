using System;
using System.Collections.ObjectModel;
using Gamma.Models;

namespace Gamma.ViewModels
{
    public class DocShipmentOrder
    {
        public Guid DocShipmentOrderId { get; set; }
        public string Number { get; set; }
        public DateTime Date { get; set; }
        public string Consignee { get; set; }
        public string VehicleNumber { get; set; }
        public string ActivePerson { get; set; }
        public string OrderType { get; set; }
        
        public ObservableCollection<DocNomenclatureItem> DocShipmentOrderGoods { get; set; }
    }
}