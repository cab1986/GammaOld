using System;
using System.Collections.Generic;

namespace Gamma.Models
{
	public class DocComplectationListItem
	{
		#region Properties

		public Guid? DocId { get; set; }

		public Guid Doc1CId { get; set; }

		public DateTime Date { get; set; }

		public string Number { get; set; }

		public List<ComplectationNomenclatureItem> NomenclaturePositions { get; set; } = new List<ComplectationNomenclatureItem>();

		public int? PlaceId { get; set; }

        public bool IsConfirmed { get; set; }

        public bool IsReturned { get; set; }

        public string UserName { get; set; }

        public string ShiftID { get; set; }

    #endregion
}

	public class ComplectationNomenclatureItem
	{
		public string Nomenclature { get; set; }

		public string CharacteristicFrom { get; set; }

		public string CharacteristicTo { get; set; }

		public decimal Quantity { get; set; }

		public decimal UnpackedQuantity { get; set; }

		public decimal ComplectedQuantity { get; set; }
	}
}
