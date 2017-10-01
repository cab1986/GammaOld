using System;

namespace Gamma.Models
{
	public class ComplectationProduct
	{
		/// <summary>
		/// DocWithdrawalId or DocProductionId
		/// </summary>
		public Guid DocId { get; set; }
		public Guid ProductId { get; set; }
		public string Number { get; set; }
		public string Barcode { get; set; }
		public decimal Quantity { get; set; }
	}
}
