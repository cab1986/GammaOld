using DevExpress.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gamma.Models
{
    public class RepackProduct : ViewModelBase
    {
        public Guid DocRepackID { get; set; }
        public string DocRepackNumber { get; set; }
        public Guid ProductID { get; set; }
        public string ProductNumber { get; set; }
        public Guid NomenclatureID { get; set; }
        public Guid? CharacteristicID { get; set; }
        public string NomenclatureName { get; set; }
        public decimal Quantity { get; set; }
        public decimal? QuantityGood { get; set; }
        public decimal? QuantityBroke { get; set; }
        public DateTime Date { get; set; }
        public ProductKind ProductKind { get; set; }
        public int ShiftID { get; set; }
        public Guid? ProductionTaskID { get; set; }
        public Guid? DocBrokeID { get; set; }
        public string DocBrokeNumber { get; set; }

        public DelegateCommand ShowRepackCommand { get; private set; }

        public RepackProduct()
        {
            ShowRepackCommand = new DelegateCommand(ShowRepack);
        }

        private void ShowRepack()
        {
            MessageManager.OpenDocRepack(DocRepackID);
        }
    }
}