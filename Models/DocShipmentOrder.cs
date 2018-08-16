// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Collections.ObjectModel;

namespace Gamma.Models
{
    public class DocShipmentOrder
    {
        public Guid DocShipmentOrderId { get; set; }
        public string Number { get; set; }
        public DateTime Date { get; set; }
        public string Shipper { get; set; }
        public string Consignee { get; set; }
        public string Buyer { get; set; }
        public string VehicleNumber { get; set; }
        public string ActivePerson { get; set; }
        public string OrderType { get; set; }
        public DateTime? OutDate { get; set; }
        public string Warehouse { get; set; }

        public ObservableCollection<DocNomenclatureItem> DocShipmentOrderGoods { get; set; }
    }
}