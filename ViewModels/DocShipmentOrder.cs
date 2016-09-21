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
        /*
        private int? _activePersonId;

        public int? ActivePersonId
        {
            get { return _activePersonId; }
            set
            {
                if (_activePersonId == value) return;
                _activePersonId = value;
                SetActivePersonOrder(DocShipmentOrderId, _activePersonId);
            }
        }

        private void SetActivePersonOrder(Guid docShipmentOrderId, int? activePersonId)
        {
            using (var gammaBase = DB.GammaDb)
            {
                var activeOrder =
                    gammaBase.DocShipmentOrderInfo.FirstOrDefault(ao => ao.C1CDocShipmentOrderID == docShipmentOrderId);               
                if (activeOrder == null)
                {
                    activeOrder = new DocShipmentOrderInfo
                    {
                        C1CDocShipmentOrderID = docShipmentOrderId
                    };
                    gammaBase.DocShipmentOrderInfo.Add(activeOrder);
                }
                activeOrder.ActivePersonID = activePersonId;

                // Ќа случай, если в базе уже удален приказ с данным ID
                try
                {
                    gammaBase.SaveChanges();
                }
                catch (Exception)
                {
                    
                }
                
            }
        }
        */
        public ObservableCollection<DocNomenclatureItem> DocShipmentOrderGoods { get; set; }
    }
}