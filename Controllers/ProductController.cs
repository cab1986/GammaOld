using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.Entities;
using Gamma.Models;

namespace Gamma.Controllers
{
	public class ProductController
	{
		/// <summary>
		/// Create pallet with one nomenclature item
		/// and add to existing document
		/// </summary>
		/// <param name="docProductionId">DocId of existing production doc</param>
		/// <param name="nomenclatureId"></param>
		/// <param name="characteristicId"></param>
		/// <returns></returns>
		public Product AddNewPalletToDocProduction(Guid docProductionId, Guid nomenclatureId, Guid characteristicId)
		{
			using (var context = DB.GammaDb)
			{
				var quantity = context.C1CCharacteristics.First(c => c.C1CCharacteristicID == characteristicId)
									.C1CMeasureUnitsPallet.Coefficient ?? 1;
				var productId = SqlGuidUtil.NewSequentialid();
				var pallet = new Products
				{
					ProductID = productId,
					StateID = (byte)ProductState.Good,
					ProductKindID = (byte)ProductKind.ProductPallet,
					ProductPallets = new ProductPallets
					{
						ProductID = productId,
						ProductItems = new List<ProductItems>
						{
							new ProductItems
							{
								ProductItemID = SqlGuidUtil.NewSequentialid(),
								ProductID = productId,
								C1CNomenclatureID = nomenclatureId,
								C1CCharacteristicID = characteristicId,
								Quantity = (int)quantity
							}
						}
					}
				};
				context.Products.Add(pallet);
				context.DocProductionProducts.Add(new DocProductionProducts
				{
					ProductID = productId,
					DocID = docProductionId,
					C1CNomenclatureID = nomenclatureId,
					C1CCharacteristicID = characteristicId,
					Quantity = quantity
				});
				context.SaveChanges();
				return context.Products.Where(p => p.ProductID == productId)
					.Select(p => new Product
					{
						ProductId = p.ProductID,
						Number = p.Number,
						Barcode = p.BarCode,
						Quantity = quantity
					})
					.First();
			}
		}
	}
}
