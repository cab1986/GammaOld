﻿using System;
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

		public DocComplectationsListViewModel()
		{
			Intervals = new List<string> { "Активные", "Последние 500", "Поиск" };
			RefreshCommand = new DelegateCommand(Find);
			EditItemCommand = new DelegateCommand(() => OpenDocComplectation(SelectedDocComplectation), SelectedDocComplectation != null);
            IntervalId = 0;
            Find();
		}

		public int IntervalId
		{
			get { return _intervalId; }
			set
			{
				if (_intervalId == value) return;
				_intervalId = value;
				if (_intervalId < 2) Find();
			}
		}

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

		private void OpenDocComplectation(DocComplectationListItem selectedDocComplectation)
		{
			if (selectedDocComplectation == null)
			{
				return;
			}
			if (selectedDocComplectation.DocId == null)
			{
				SelectedDocComplectation.DocId = SqlGuidUtil.NewSequentialid();
				var doc = documentController.ConstructDoc((Guid)SelectedDocComplectation.DocId, DocTypes.DocComplectation, selectedDocComplectation.PlaceId ?? WorkSession.PlaceID);
				doc.DocComplectation = new DocComplectation
				{
					C1CDocComplectationID = SelectedDocComplectation.Doc1CId,
					DocComplectationID = (Guid) SelectedDocComplectation.DocId
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
		private void Find()
		{
			UIServices.SetBusyState();
			SelectedDocComplectation = null;
			using (var gammaBase = DB.GammaDb)
			{
				switch (IntervalId)
				{
					case 0:
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
						break;
					case 1:
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
						break;
					case 2:
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
