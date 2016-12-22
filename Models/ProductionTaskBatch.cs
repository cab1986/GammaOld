// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;

namespace Gamma.Models
{
    public class ProductionTaskBatch
    {
        public DateTime? Date { get; set; }
        public Guid ProductionTaskBatchID { get; set; }
        public DateTime? DateBegin { get; set; }
        public string Nomenclature { get; set; }
        public decimal? Quantity { get; set; }
        public decimal? MadeQuantity { get; set; }
        public string Number { get; set; }
        public string State { get; set; }
        public byte BatchKindID { get; set; }
        public int PlaceID { get; set; }
        public string Place { get; set; }
    }
}
