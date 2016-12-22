// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using Gamma.ViewModels;

namespace Gamma.Models
{
    public class DocMovementOrderNomenclatureItem : DbEditItemWithNomenclatureViewModel
    {
        public Guid DocId { get; set; }
        public decimal Quantity { get; set; }
        public decimal CollectedQuantity { get; set; }
    }
}
