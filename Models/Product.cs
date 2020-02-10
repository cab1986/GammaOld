using Gamma.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gamma.Models
{
	public class Product:IProduct
	{
		public Guid ProductID { get; set; }
		public string Number { get; set; }
		public string Barcode { get; set; }
		public decimal Quantity { get; set; }
        public ProductKind ProductKind { get; set; }
        public Guid NomenclatureID { get; set; }
        public Guid CharacteristicID { get; set; }
        public string NomenclatureName { get; set; }
        /// <summary>
        /// ID документа выработки
        /// </summary>
        public Guid DocID { get; set; }
        /// <summary>
        /// Дата документа выработки
        /// </summary>
        public DateTime Date { get; set; }
    }
}
