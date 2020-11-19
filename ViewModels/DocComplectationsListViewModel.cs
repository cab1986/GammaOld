using System;
using System.Collections.Generic;
using System.Linq;
using DevExpress.Mvvm;
using Gamma.Common;
using Gamma.Controllers;
using Gamma.Entities;
using Gamma.Models;
using Gamma.Interfaces;

namespace Gamma.ViewModels
{
	public class DocComplectationsListViewModel : RootViewModel, IItemManager
    {
		#region Fields

		private DocumentController documentController = new DocumentController();

		private int _intervalId;

		private List<DocComplectationListItem> _docComplectations;

		#endregion

		public DocComplectationsListViewModel(bool isSource1C)
		{
			Intervals = new List<string> { "Активные", "Последние 500", "Поиск" };
			RefreshCommand = new DelegateCommand(() => Find(IsSource1C));
			EditItemCommand = new DelegateCommand(() => OpenDocComplectation(SelectedDocComplectation), SelectedDocComplectation != null);
            NewItemCommand = new DelegateCommand(() => CreateDocComplectation());
            IntervalId = 0;
            IsSource1C = isSource1C;
            Find(isSource1C);
		}

		public int IntervalId
		{
			get { return _intervalId; }
			set
			{
				if (_intervalId == value) return;
				_intervalId = value;
				if (_intervalId < 2) Find(IsSource1C);
			}
		}

        private bool IsSource1C { get; set; }

        public DateTime? DateBegin { get; set; }
		public DateTime? DateEnd { get; set; }
		public string Number { get; set; }

		#region IItemManager

		public DelegateCommand DeleteItemCommand { get; }

		public DelegateCommand<object> EditItemCommand { get; private set; }

		public DelegateCommand NewItemCommand { get; }

		public DelegateCommand RefreshCommand { get; private set; }

		#endregion

		public List<DocComplectationListItem> DocComplectations
		{
			get { return _docComplectations; }
			set
			{
				_docComplectations = value;
				RaisePropertyChanged("DocComplectations");
			}
		}

		public DocComplectationListItem SelectedDocComplectation { get; set; }

		public List<string> Intervals { get; private set; }

        #region Private Methods

        private void CreateDocComplectation()
        {

            /*var docId = SqlGuidUtil.NewSequentialid();
            var doc = documentController.ConstructDoc((Guid)docId, DocTypes.DocComplectation, WorkSession.PlaceID);
            doc.DocComplectation = new DocComplectation
            {
                //C1CDocComplectationID = SelectedDocComplectation.Doc1CId,
                DocComplectationID = (Guid)docId
            };
            using (var db = DB.GammaDb)
            {
                db.Docs.Add(doc);
                db.SaveChanges();
            }
            MessageManager.OpenDocComplectation((Guid)docId);*/
            var newDocComplectation = new DocComplectationListItem() { PlaceId = 8 };
            OpenDocComplectation(newDocComplectation);
        }

        private void OpenDocComplectation(DocComplectationListItem selectedDocComplectation)
		{
			if (selectedDocComplectation == null)
			{
				return;
			}
			if (selectedDocComplectation.DocId == null)
			{
				selectedDocComplectation.DocId = SqlGuidUtil.NewSequentialid();
				var doc = documentController.ConstructDoc((Guid)selectedDocComplectation.DocId, DocTypes.DocComplectation, selectedDocComplectation.PlaceId ?? WorkSession.PlaceID);
				doc.DocComplectation = new DocComplectation
				{
					C1CDocComplectationID = selectedDocComplectation.Doc1CId,
					DocComplectationID = (Guid) selectedDocComplectation.DocId
				};
				using (var db = DB.GammaDb)
				{
					db.Docs.Add(doc);
					db.SaveChanges();
				}
			}
			MessageManager.OpenDocComplectation((Guid)selectedDocComplectation.DocId);
		}

		/// <summary>
		/// Refresh documents list with current search settings
		/// </summary>
		private void Find(bool isSource1C)
		{
			UIServices.SetBusyState();
			SelectedDocComplectation = null;
			using (var gammaBase = DB.GammaDb)
			{
				switch (IntervalId)
				{
					case 0:
                        if (isSource1C)
                            DocComplectations = gammaBase.C1CDocComplectation.Where(dc => gammaBase.Branches.FirstOrDefault(b => b.BranchID == WorkSession.BranchID).C1CSubdivisionID == dc.C1CWarehouses.C1CSubdivisionID
                            && (bool)dc.C1CDocComplectationNomenclature.FirstOrDefault().C1CNomenclature.C1CMeasureUnitQualifiers.IsInteger
                            && (!dc.DocComplectation.Any() || !dc.DocComplectation.FirstOrDefault().Docs.IsConfirmed))
							.OrderByDescending(dc => dc.Date).Take(100)
							.Select(dc => new DocComplectationListItem
							{
								DocId = dc.DocComplectation.Any() ? (Guid?)dc.DocComplectation.FirstOrDefault().DocComplectationID : null,
								Doc1CId = dc.C1CDocComplectationID,
								Number = dc.C1CCode,
								Date = (DateTime)dc.Date,
								PlaceId = gammaBase.Places.FirstOrDefault(p => p.C1CPlaceID == dc.C1CWarehouseID).PlaceID
							}).ToList();
                        else
                            DocComplectations = gammaBase.DocComplectation
                                .Where(dc => dc.C1CDocComplectationID == Guid.Empty && (!dc.Docs.IsConfirmed))
                            .OrderByDescending(dc => dc.Docs.Date).Take(100)
                            .Select(dc => new DocComplectationListItem
                            {
                                DocId = dc.DocComplectationID,
                                Doc1CId = dc.C1CDocComplectationID,
                                Number = dc.Docs.Number,
                                Date = (DateTime)dc.Docs.Date,
                                PlaceId = dc.Docs.PlaceID
                            }).ToList();
                        break;
					case 1:
                        if (isSource1C)
                            DocComplectations = gammaBase.C1CDocComplectation
							.Where(dc => gammaBase.Branches.FirstOrDefault(b => b.BranchID == WorkSession.BranchID).C1CSubdivisionID == dc.C1CWarehouses.C1CSubdivisionID
                                                    //&& dc.C1CDocComplectationNomenclature.Any(c => gammaBase.C1CNomenclature.FirstOrDefault(e => gammaBase.C1CMeasureUnitQualifiers.FirstOrDefault(f => f.IsInteger == true).C1CMeasureUnitQualifierID == e.C1CBaseMeasureUnitQualifier).C1CNomenclatureID == c.C1CNomenclatureID)
                                                    && (bool)dc.C1CDocComplectationNomenclature.FirstOrDefault().C1CNomenclature.C1CMeasureUnitQualifiers.IsInteger
                                                    )
                            .OrderByDescending(dc => dc.Date).Take(500)
							.Select(dc => new DocComplectationListItem
							{
								DocId = dc.DocComplectation.Any() ? (Guid?)dc.DocComplectation.FirstOrDefault().DocComplectationID : null,
								Doc1CId = dc.C1CDocComplectationID,
								Number = dc.C1CCode,
								Date = (DateTime)dc.Date,
								PlaceId = gammaBase.Places.FirstOrDefault(p => p.C1CPlaceID == dc.C1CWarehouseID).PlaceID
							}).ToList();
                        else
                            DocComplectations = gammaBase.DocComplectation
                                .Where(dc => dc.C1CDocComplectationID == Guid.Empty)
                            .OrderByDescending(dc => dc.Docs.Date).Take(500)
                            .Select(dc => new DocComplectationListItem
                            {
                                DocId = dc.DocComplectationID,
                                Doc1CId = dc.C1CDocComplectationID,
                                Number = dc.Docs.Number,
                                Date = (DateTime)dc.Docs.Date,
                                PlaceId = dc.Docs.PlaceID
                            }).ToList();
                        break;
					case 2:
                        if (isSource1C)
                            DocComplectations = gammaBase.C1CDocComplectation
							.Where(dc => gammaBase.Branches.FirstOrDefault(b => b.BranchID == WorkSession.BranchID).C1CSubdivisionID == dc.C1CWarehouses.C1CSubdivisionID &&
                                (bool)dc.C1CDocComplectationNomenclature.FirstOrDefault().C1CNomenclature.C1CMeasureUnitQualifiers.IsInteger &&
                                (string.IsNullOrEmpty(Number) || dc.C1CCode.Contains(Number)) &&
								(DateBegin == null || dc.Date >= DateBegin) &&
								(DateEnd == null || dc.Date <= DateEnd)
								)
							.OrderByDescending(dc => dc.Date).Take(500)
							.Select(dc => new DocComplectationListItem
							{
								DocId = dc.DocComplectation.Any() ? (Guid?)dc.DocComplectation.FirstOrDefault().DocComplectationID : null,
								Doc1CId = dc.C1CDocComplectationID,
								Number = dc.C1CCode,
								Date = (DateTime)dc.Date,
								PlaceId = gammaBase.Places.FirstOrDefault(p => p.C1CPlaceID == dc.C1CWarehouseID).PlaceID
							}).ToList();
                        else
                            DocComplectations = gammaBase.DocComplectation
                            .Where(dc => dc.C1CDocComplectationID == Guid.Empty &&
                                (string.IsNullOrEmpty(Number) || dc.Docs.Number.Contains(Number)) &&
                                (DateBegin == null || dc.Docs.Date >= DateBegin) &&
                                (DateEnd == null || dc.Docs.Date <= DateEnd)
                                )
                            .OrderByDescending(dc => dc.Docs.Date).Take(500)
                            .Select(dc => new DocComplectationListItem
                            {
                                DocId = dc.DocComplectationID,
                                Doc1CId = dc.C1CDocComplectationID,
                                Number = dc.Docs.Number,
                                Date = (DateTime)dc.Docs.Date,
                                PlaceId = dc.Docs.PlaceID
                            }).ToList();
                        break;
				}
				foreach (var docComplectation in DocComplectations)
				{
					docComplectation.NomenclaturePositions = new List<ComplectationNomenclatureItem>(gammaBase
						.v1CDocComplectationPositions.Where(p => p.C1CDocComplectationID == docComplectation.Doc1CId)
						.Select(p => new ComplectationNomenclatureItem
						{
							Nomenclature = p.Nomenclature,
							CharacteristicFrom = p.OldCharacteristic,
							CharacteristicTo = p.NewCharacteristic,
							Quantity = p.DocQuantity ?? 0,
							UnpackedQuantity = p.Withdraw ?? 0,
							ComplectedQuantity = p.Complected ?? 0
						}));
				}
			}
		}

        #endregion
    }
}	
