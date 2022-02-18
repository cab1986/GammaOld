using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gamma.Models
{
    public class CreateWithdrawalResult
    {
        public CreateWithdrawalResult(Guid docID, string number, DateTime date, decimal quantity)
        {
            DocID = docID;
            Number = number;
            Date = date;
            Quantity = quantity;
        }
        public CreateWithdrawalResult(Guid productID, string number, DateTime date, ProductKind productKind, decimal quantity)
        {
            ProductID = productID;
            Number = number;
            Date = date;
            ProductKind = productKind;
            Quantity = quantity;
        }
        public Guid DocID { get; set; }
        public string Number { get; set; }
        public DateTime Date { get; set; }
        public Guid ProductID { get; set; }
        public ProductKind ProductKind { get; set; }
        public decimal Quantity { get; set; }
    }
}
