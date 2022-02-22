using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.Entities;
using System.Data.Entity;
using Gamma.Models;

namespace Gamma.Controllers
{
	public class DocumentController
	{
		/// <summary>
		/// Withdraw one product
		/// </summary>
		/// <param name="productId"></param>
		/// <param name="docId">DocWithdrawalId, if null new document</param>
		/// <returns>True if success</returns>
		public bool WithdrawProduct(Guid productId, Guid docId, bool isConfirmed, GammaEntities currentContext = null )
		{
            var quantity = (currentContext ?? DB.GammaDb).vProductsInfo.First(p => p.ProductID == productId).Quantity ?? 0;
            return WithdrawProductQuantity(productId, docId, isConfirmed, quantity, currentContext);
			/*using (var context = currentContext ?? DB.GammaDb)
			{
				var docWithdrawal = context.Docs.Include(d => d.DocWithdrawal)
					.Include(d => d.DocWithdrawal.DocWithdrawalProducts)
					.FirstOrDefault(d => d.DocID == docId);
				if (docWithdrawal == null)
				{
					docWithdrawal = ConstructDoc(docId, DocTypes.DocWithdrawal, isConfirmed, WorkSession.PlaceID);
					context.Docs.Add(docWithdrawal);
				}
				if (docWithdrawal.DocWithdrawal == null)
				{
					docWithdrawal.DocWithdrawal = new DocWithdrawal
					{
						DocID = docId,
						OutPlaceID = docWithdrawal.PlaceID
					};
				}
				if (docWithdrawal.DocWithdrawal.DocWithdrawalProducts == null)
				{
					docWithdrawal.DocWithdrawal.DocWithdrawalProducts = new List<DocWithdrawalProducts>();
				}
				docWithdrawal.DocWithdrawal.DocWithdrawalProducts.Add(new DocWithdrawalProducts
				{
					DocID = docId,
					CompleteWithdrawal = true,
					ProductID = productId,
					Quantity = context.vProductsInfo.First(p => p.ProductID == productId).Quantity
				});
				try
				{
					context.SaveChanges();
				}
				catch
				{
					return false;
				}
			}
			return true;*/
		}

        /// <summary>
		/// Withdraw one product quantity
		/// </summary>
		/// <param name="productId"></param>
		/// <param name="docId">DocWithdrawalId, if null new document</param>
		/// <returns>True if success</returns>
		public bool WithdrawProductQuantity(Guid productId, Guid docId, bool isConfirmed, decimal quantity, GammaEntities currentContext = null)
        {
            using (var context = currentContext ?? DB.GammaDb)
            {
                var docWithdrawal = context.Docs.Include(d => d.DocWithdrawal)
                    .Include(d => d.DocWithdrawal.DocWithdrawalProducts)
                    .FirstOrDefault(d => d.DocID == docId);
                if (docWithdrawal == null)
                {
                    docWithdrawal = ConstructDoc(docId, DocTypes.DocWithdrawal, isConfirmed, WorkSession.PlaceID);
                    context.Docs.Add(docWithdrawal);
                }
                if (docWithdrawal.DocWithdrawal == null)
                {
                    docWithdrawal.DocWithdrawal = new DocWithdrawal
                    {
                        DocID = docId,
                        OutPlaceID = docWithdrawal.PlaceID
                    };
                }
                if (docWithdrawal.DocWithdrawal.DocWithdrawalProducts == null)
                {
                    docWithdrawal.DocWithdrawal.DocWithdrawalProducts = new List<DocWithdrawalProducts>();
                }
                docWithdrawal.DocWithdrawal.DocWithdrawalProducts.Add(new DocWithdrawalProducts
                {
                    DocID = docId,
                    CompleteWithdrawal = (context.vProductsInfo.First(p => p.ProductID == productId).Quantity == quantity),
                    ProductID = productId,
                    Quantity = quantity
                });
                try
                {
                    context.SaveChanges();
                }
                catch
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Withdraw one product quantity
        /// </summary>
        /// <param name="docBrokeId"></param>
        /// <param name="productId"></param>
        /// <param name="docId">DocWithdrawalId, if null new document</param>
        /// <returns>True if success</returns>
        public CreateWithdrawalResult WithdrawProductQuantityFromDocBroke(Guid docBrokeId, Guid productId, byte stateId, Guid docId, bool isConfirmed, decimal quantity, GammaEntities currentContext = null)
        {
            Docs docWithdrawal = null;
            using (var context = currentContext ?? DB.GammaDb)
            {
                if (WithdrawProductQuantity(productId, docId, isConfirmed, quantity))
                {
                    docWithdrawal = context.Docs.Include(d => d.DocWithdrawal)
                    //.Include(d => d.DocWithdrawal.DocBrokeDecisionProductWithdrawalProducts)
                    .FirstOrDefault(d => d.DocID == docId);
                    if (docWithdrawal != null)
                        docWithdrawal.DocWithdrawal.DocBrokeDecisionProductWithdrawalProducts.Add(new DocBrokeDecisionProductWithdrawalProducts
                        {
                            DocID = docBrokeId,
                            DocWithdrawalID = docId,
                            ProductID = productId,
                            StateID = stateId
                        });
                }
                else
                    return null;
                try
                {
                    context.SaveChanges();
                }
                catch
                {
                    return null;
                }
            }
            return new CreateWithdrawalResult(docWithdrawal.DocID, docWithdrawal.Number, docWithdrawal.Date, quantity);
        }

       /* public bool AddDocBrokeDecision(Docs docBrokeDecision, Guid? docBrokeID)
        {
            using (var context = DB.GammaDb)
            {
                if (!context.Docs.Any(p => p.DocID == docBrokeDecision.DocID))
                    context.Docs.Add(new Docs
                    {
                        DocID = docBrokeDecision.DocID,
                        DocBrokeDecision = new DocBrokeDecision
                        {
                            DocBrokeID = docBrokeID
                        }
                    });

                try
                {
                    context.SaveChanges();
                }
                catch
                {
                    return false;
                }
                return true;
            }
        }*/

        #region Private methods

        public Docs ConstructDoc(Guid id, DocTypes type, bool isConfirmed, int? placeId = null, byte? shidtId = null, DateTime? date = null)
		{
            var doc = new Docs
            {
                DocID = id,
                Date = date ?? DB.CurrentDateTime,
                DocTypeID = (int)type,
                PrintName = WorkSession.PrintName,
                UserID = WorkSession.UserID,
                PlaceID = placeId?? WorkSession.PlaceID,
                ShiftID = WorkSession.ShiftID,
                IsConfirmed = isConfirmed
			};
			return doc;
		}

		#endregion
	}
}
