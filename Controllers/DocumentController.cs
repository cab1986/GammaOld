using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.Entities;
using System.Data.Entity;

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
			return true;
		}

		#region Private methods

		public Docs ConstructDoc(Guid id, DocTypes type, bool isConfirmed, int placeId)
		{
            var doc = new Docs
            {
                DocID = id,
                Date = DB.CurrentDateTime,
                DocTypeID = (int)type,
                PrintName = WorkSession.PrintName,
                UserID = WorkSession.UserID,
                PlaceID = placeId,
                ShiftID = WorkSession.ShiftID,
                IsConfirmed = isConfirmed
			};
			return doc;
		}

		#endregion
	}
}
