// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
namespace Gamma.Models
{
    public class DocNomenclatureItem
    {
        public string NomenclatureName { get; set; }
        public string Quantity { get; set; }
        public decimal OutQuantity { get; set; }
        public decimal InQuantity { get; set; }
        public string Quality { get; set; }
    }
}