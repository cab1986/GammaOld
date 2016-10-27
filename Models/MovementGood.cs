using System;
using System.Collections.Generic;
using Gamma.ViewModels;

namespace Gamma.Models
{
    public class MovementGood : DbEditItemWithNomenclatureViewModel
    {
        public string Amount { get; set; }
        public decimal OutQuantity { get; set; }
        public decimal InQuantity { get; set; }
        public List<MovementProduct> Products { get; set; }
    }
}
