using System;
using DevExpress.Mvvm;

namespace Gamma.Models
{
    public class WithdrawalProduct : ViewModelBase
    {
        public Guid ProductId { get; set; }
        public string Number { get; set; }
        public string NomenclatureName { get; set; }
        public decimal? Quantity { get; set; }
        public string MeasureUnit { get; set; }
        public bool CompleteWithdrawal { get; set; }
        public decimal MaxQuantity { get; set; }
    }
}
