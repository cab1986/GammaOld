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
		public Product AddNewPalletToDocProduction(Guid docProductionId, Guid nomenclatureId, Guid characteristicId, Guid? placeZoneID = null)
		{
			using (var context = DB.GammaDb)
			{
				var quantity = context.C1CCharacteristics.First(c => c.C1CCharacteristicID == characteristicId)
									.C1CMeasureUnitsPallet.Coefficient ?? 1;
                return AddNewPalletToDocProduction(docProductionId, nomenclatureId, characteristicId, quantity, placeZoneID);
                /*var productId = SqlGuidUtil.NewSequentialid();
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
						ProductID = p.ProductID,
						Number = p.Number,
						Barcode = p.BarCode,
						Quantity = quantity,
                        ProductKind = (ProductKind)p.ProductKindID
					})
					.First();
                    */
            }
		}

        /// <summary>
		/// Create palletR with one nomenclature item
		/// and add to existing document
		/// </summary>
		/// <param name="docProductionId">DocId of existing production doc</param>
		/// <param name="nomenclatureId"></param>
		/// <param name="characteristicId"></param>
		/// <returns></returns>
		public Product AddNewPalletToDocProduction(Guid docProductionId, Guid nomenclatureId, Guid characteristicId, decimal quantity, Guid? placeZoneID = null)
        {
            using (var context = DB.GammaDb)
            {
                var quantityNomenclature = context.C1CCharacteristics.First(c => c.C1CCharacteristicID == characteristicId)
                                    .C1CMeasureUnitsPallet.Coefficient ?? 1;
                byte productKind;
                if (quantity == quantityNomenclature)
                    productKind = (byte)ProductKind.ProductPallet;
                else
                    productKind = (byte)ProductKind.ProductPalletR;
                var productId = SqlGuidUtil.NewSequentialid();
                var pallet = new Products
                {
                    ProductID = productId,
                    StateID = (byte)ProductState.Good,
                    ProductKindID = productKind,
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
                if (placeZoneID != null)
                {
                    context.Rests.Where(r => r.ProductID == productId).FirstOrDefault().PlaceZoneID = placeZoneID;
                    context.SaveChanges();
                }
                return context.Products.Where(p => p.ProductID == productId)
                    .Select(p => new Product
                    {
                        ProductID = p.ProductID,
                        Number = p.Number,
                        Barcode = p.BarCode,
                        Quantity = quantity,
                        ProductKind = (ProductKind)p.ProductKindID
                    })
                    .First();
            }
        }

        /// <summary>
        /// Create spool with one nomenclature item
        /// and add to existing document
        /// </summary>
        public Product AddNewProductToDocProduction(Docs docProduction, Guid? docWithdrawalId, ProductKind productKind, Guid nomenclatureId, Guid characteristicId, decimal quantity, int? diameter, byte? breakNumber, Guid? placeZoneID = null)
        {
            using (var context = DB.GammaDb)
            {
                if (context.Docs.Where(p => p.DocID == docProduction.DocID).Count() == 0)
                    context.Docs.Add(docProduction);
                if (context.DocProduction.Where(p => p.DocID == docProduction.DocID).Count() == 0)
                    context.DocProduction.Add(new DocProduction
                    {
                        DocID = docProduction.DocID
                    });
                context.SaveChanges();
            }
            return AddNewProductToDocProduction(docProduction.DocID, docWithdrawalId, productKind, nomenclatureId, characteristicId, quantity, diameter, breakNumber, null, placeZoneID);
        }

        /// <summary>
        /// Create spool with one nomenclature item
        /// and add to existing document
        /// </summary>
        //public Product AddNewProductToDocProduction(Guid docProductionId, Guid? docWithdrawalId, ProductKind productKind, Guid nomenclatureId, Guid characteristicId, decimal quantity, int diameter, byte breakNumber, Guid? placeZoneID = null)
        //{
        //    return AddNewProductToDocProduction(docProductionId, docWithdrawalId, productKind, nomenclatureId, characteristicId, quantity, diameter, breakNumber, null, placeZoneID);
        //}
                
        /// <summary>
        /// Create spool with one nomenclature item
        /// and add to existing document
        /// </summary>
        public Product AddNewProductToDocProduction(Guid docProductionId, Guid? docWithdrawalId, ProductKind productKind, Guid nomenclatureId, Guid characteristicId, decimal quantity, int? diameter, byte? breakNumber, decimal? length, Guid? placeZoneID = null)
        {
            using (var context = DB.GammaDb)
            {
                if (productKind == ProductKind.ProductPallet || productKind == ProductKind.ProductPalletR)
                {
                    var quantityInPallet = context.C1CCharacteristics.FirstOrDefault(c => c.C1CCharacteristicID == characteristicId).C1CMeasureUnitsPallet.Coefficient;
                    if (quantity == quantityInPallet && productKind != ProductKind.ProductPallet)
                        productKind = ProductKind.ProductPallet;
                    else if (quantity != quantityInPallet && productKind != ProductKind.ProductPalletR)
                        productKind = ProductKind.ProductPalletR;
                }
                decimal decimalWeight = quantity;/// 1000;
                var productId = SqlGuidUtil.NewSequentialid();
                Products product =
                    productKind == ProductKind.ProductSpool ? new Products
                    {
                        ProductID = productId,
                        StateID = (byte)ProductState.Good,
                        ProductKindID = (byte)productKind,
                        ProductSpools = new ProductSpools
                        {
                            ProductID = productId,
                            C1CNomenclatureID = nomenclatureId,
                            C1CCharacteristicID = characteristicId,
                            DecimalWeight = decimalWeight,
                            Diameter = (int)diameter,
                            Length = length,
                            BreakNumber = breakNumber
                        }
                    } :
                productKind == ProductKind.ProductPallet || productKind == ProductKind.ProductPalletR ? new Products
                {
                    ProductID = productId,
                    StateID = (byte)ProductState.Good,
                    ProductKindID = (byte)productKind,
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
                } : null
                ;
                if (product == null)
                    return null;
                else
                {
                    context.Products.Add(product);
                    context.DocProductionProducts.Add(new DocProductionProducts
                    {
                        ProductID = productId,
                        DocID = docProductionId,
                        C1CNomenclatureID = nomenclatureId,
                        C1CCharacteristicID = characteristicId,
                        Quantity = decimalWeight
                    });
                    if (docWithdrawalId != null && docWithdrawalId != Guid.Empty)
                    {
                        var docWithdrawal = context.DocWithdrawal.FirstOrDefault(d => d.DocID == docWithdrawalId);
                        if (docWithdrawal != null)
                            context.DocProduction.FirstOrDefault(d => d.DocID == docProductionId)?.DocWithdrawal.Add(docWithdrawal);
                    };
                    context.SaveChanges();
                    if (placeZoneID != null)
                    {
                        context.Rests.Where(r => r.ProductID == productId).FirstOrDefault().PlaceZoneID = placeZoneID;
                        context.SaveChanges();
                    }
                    return context.Products.Where(p => p.ProductID == productId)
                        .Select(p => new Product
                        {
                            ProductID = p.ProductID,
                            Number = p.Number,
                            Barcode = p.BarCode,
                            Quantity = decimalWeight,
                            ProductKind = (ProductKind)p.ProductKindID
                        })
                        .First();
                }
            }
        }
    }
}
