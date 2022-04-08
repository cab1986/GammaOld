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
            return WithdrawProductQuantity(productId, docId, DocTypes.DocWithdrawal, isConfirmed, quantity, null, currentContext);
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
		public bool WithdrawProductQuantity(Guid productId, Guid docId, DocTypes docType, bool isConfirmed, decimal quantity, int? placeID = null , GammaEntities currentContext = null)
        {
            using (var context = currentContext ?? DB.GammaDb)
            {
                var docWithdrawal = context.Docs.Include(d => d.DocWithdrawal)
                    .Include(d => d.DocWithdrawal.DocWithdrawalProducts)
                    .FirstOrDefault(d => d.DocID == docId);
                if (docWithdrawal == null)
                {
                    docWithdrawal = ConstructDoc(docId, docType, isConfirmed, placeID ?? WorkSession.PlaceID);
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
        /// <param name="docBrokeDecisionId"></param>
        /// <param name="productId"></param>
        /// <param name="docId">DocWithdrawalId, if null new document</param>
        /// <returns>True if success</returns>
        public CreateWithdrawalResult WithdrawProductQuantityFromDocBroke(Guid docBrokeDecisionId, Guid productId, byte stateId, Guid docId, bool isConfirmed, decimal quantity, int? placeID = null, GammaEntities currentContext = null)
        {
            Docs docWithdrawal = null;
            using (var context = currentContext ?? DB.GammaDb)
            {
                if (WithdrawProductQuantity(productId, docId, (stateId == (int) ProductState.Broke ? DocTypes.DocUtilization : DocTypes.DocWithdrawal), isConfirmed, quantity, placeID))
                {
                    docWithdrawal = context.Docs.Include(d => d.DocWithdrawal)
                    //.Include(d => d.DocWithdrawal.DocBrokeDecisionProductWithdrawalProducts)
                    .FirstOrDefault(d => d.DocID == docId);
                    if (docWithdrawal != null)
                    {
                        docWithdrawal.DocWithdrawal.DocBrokeDecisionProductWithdrawalProducts.Add(new DocBrokeDecisionProductWithdrawalProducts
                        {
                            DocID = docBrokeDecisionId,
                            DocWithdrawalID = docId,
                            ProductID = productId,
                            StateID = stateId
                        });
                    }
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

        public Docs ConstructDoc(Guid id, DocTypes type, bool isConfirmed, int? placeId = null, byte? shiftId = null, DateTime? date = null)
		{
            var doc = new Docs
            {
                DocID = id,
                Date = date ?? DB.CurrentDateTime,
                DocTypeID = (int)type,
                PrintName = WorkSession.PrintName,
                UserID = WorkSession.UserID,
                PlaceID = placeId?? WorkSession.PlaceID,
                ShiftID = shiftId ?? WorkSession.ShiftID,
                IsConfirmed = isConfirmed
			};
			return doc;
		}

		#endregion
	}
}
