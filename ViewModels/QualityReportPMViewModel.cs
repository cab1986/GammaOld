// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DevExpress.Mvvm;
using Gamma.Common;
using Gamma.Entities;
using Gamma.Models;

namespace Gamma.ViewModels
{
    public class QualityReportPMViewModel : DbEditItemWithNomenclatureViewModel
    {
        public QualityReportPMViewModel()
        {
            using (var gammaBase = DB.GammaDb)
            {
                Places = gammaBase.Places.Where(
                    p => p.PlaceGroupID == (int) PlaceGroup.PM && WorkSession.BranchIds.Contains(p.BranchID))
                    .Select(p => new Place
                    {
                        PlaceID = p.PlaceID,
                        PlaceName = p.Name
                    }).ToList();
            }
            if (Places.Count > 0) PlaceId = Places.First().PlaceID;
            FindCommand = new DelegateCommand(Find);
            BrokeProductsCommand = new DelegateCommand(BrokeProducts, ()=> DB.HaveWriteAccess("DocBroke"));
        }

        private void QualityReportItemsOnItemChanged(object sender, PropertyChangedEventArgs args)
        {
            var item = sender as QualityReportItem;
            if (item == null) return;
            switch (args.PropertyName)
            {
                case "IsBroke":
                    {
                        var qualityItems = QualityReportItems.Where(i => i.ProductId == item.ProductId);
                        foreach (var qualityItem in qualityItems)
                        {
                            qualityItem.IsBroke = item.IsBroke;
                        }
                    }
                    break;
            }
        }


        private ItemsChangeObservableCollection<QualityReportItem> _qualityReportItems = new ItemsChangeObservableCollection<QualityReportItem>();
        public DateTime? DateBegin { get; set; }
        public DateTime? DateEnd { get; set; }


        public List<Place> Places { get; set; }

        public int? PlaceId { get; set; }

        public ItemsChangeObservableCollection<QualityReportItem> QualityReportItems
        {
            get { return _qualityReportItems; }
            set
            {
                if (QualityReportItems != null) QualityReportItems.ItemChanged -= QualityReportItemsOnItemChanged;
                _qualityReportItems = value;
                if (QualityReportItems != null) QualityReportItems.ItemChanged += QualityReportItemsOnItemChanged;
                RaisePropertyChanged("QualityReportItems");
            }
        }

        public DelegateCommand FindCommand { get; private set; }

        private void Find()
        {
            UIServices.SetBusyState();
            using (var gammaBase = DB.GammaDb)
            {
                QualityReportItems = new ItemsChangeObservableCollection<QualityReportItem>(
                    gammaBase.vPMSpoolsToGroupPack
                        .Where(sg =>
                            (NomenclatureID == null || sg.C1CNomenclatureID == NomenclatureID)
                            && (CharacteristicID == null || sg.C1CCharacteristicID == CharacteristicID)
                            && (DateBegin == null || sg.PMDate >= DateBegin)
                            && (DateEnd == null || sg.PMDate <= DateEnd)
                            && (PlaceId == null || sg.PlaceID == PlaceId))
                            .OrderByDescending(sg => sg.PMDate).Take(500)
                        .Select(sg => new QualityReportItem
                        {
                            NomenclatureName = sg.NomenclatureName,
                            ProductId = sg.ProductID,
                            ProductGroupPackId = sg.ProductGroupPackID,
                            Date = sg.PMDate,
                            ShiftId = sg.ShiftId ?? 1,
                            GroupPackNumber = sg.WrNumber,
                            Weight = sg.Weight ?? 0,
                            SpoolNumber = sg.PMNumber
                        }));
            }
        }

        public DelegateCommand BrokeProductsCommand { get; private set; }

        private void BrokeProducts()
        {
            UIServices.SetBusyState();
            using (var gammaBase = DB.GammaDb)
            {
                var docId = SqlGuidUtil.NewSequentialid();
                var doc = new Docs
                {
                    DocID = docId,
                    DocTypeID = (int) DocTypes.DocBroke,
                    Date = DB.CurrentDateTime,
                    UserID = WorkSession.UserID,
                    PlaceID = WorkSession.PlaceID,
                    DocBroke = new DocBroke
                    {
                        DocID = docId,
                        DocBrokeProducts = new List<DocBrokeProducts>()
                    }
                };
                var productIds =
                    QualityReportItems.Where(ri => ri.IsBroke && ri.ProductGroupPackId != null)
                        .Select(ri => (Guid)ri.ProductGroupPackId)
                        .ToList();
                productIds.AddRange(QualityReportItems.Where(ri => ri.ProductGroupPackId == null && ri.IsBroke).Select(ri => ri.ProductId).ToList().Distinct());
                foreach (var productId in productIds)
                {
                    doc.DocBroke.DocBrokeProducts.Add(new DocBrokeProducts
                    {
                        ProductID = productId,
                        DocID = doc.DocID
                    });
                }
                gammaBase.Docs.Add(doc);
                gammaBase.SaveChanges();
                MessageManager.OpenDocBroke(doc.DocID);
            }
        }
    }
}
