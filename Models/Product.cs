using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gamma.Models
{
	public class Product
	{
		public Guid ProductId { get; set; }
		public string Number { get; set; }
		public string Barcode { get; set; }
		public decimal Quantity { get; set; }
	}
}
