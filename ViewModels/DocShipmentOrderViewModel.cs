using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Windows;
using Gamma.Attributes;
using Gamma.Interfaces;
using Gamma.Models;

namespace Gamma.ViewModels
{
    public class DocShipmentOrderViewModel : SaveImplementedViewModel, ICheckedAccess
    {
        public DocShipmentOrderViewModel(Guid docShipmentOrderId, GammaEntities gammaBase = null) : base(gammaBase)
        {Bars.Add(ReportManager.GetReportBar("DocShipmentOrder",VMId));
            Persons = GammaBase.Persons.ToList();
            DocShipmentOrderID = docShipmentOrderId;
            var docShipmentOrderInfo =
                GammaBase.C1CDocShipmentOrder.Include(d => d.DocShipmentOrderInfo).Include(d => d.C1CConsignees)
                    .FirstOrDefault(d => d.C1CDocShipmentOrderID == docShipmentOrderId);
            if (docShipmentOrderInfo == null)
            {
                MessageBox.Show("Не удалось найти приказ в базе", "Ошибка получения приказа", MessageBoxButton.OK,
                    MessageBoxImage.Hand);
                CloseWindow();
                return;
            }
            Date = docShipmentOrderInfo.C1CDate;
            Number = docShipmentOrderInfo.C1CNumber;
            Consignee = docShipmentOrderInfo.C1CConsignees.Description;
            Title = $"Приказ на отгрузку № {Number}";
            if (docShipmentOrderInfo.DocShipmentOrderInfo != null)
            {
                VehicleNumber = docShipmentOrderInfo.DocShipmentOrderInfo.VehicleNumber;
                ActivePersonId = docShipmentOrderInfo.DocShipmentOrderInfo.ActivePersonID;
            }
            FillDocShipmentOrderGoods(docShipmentOrderId);
            IsReadOnly = docShipmentOrderInfo.Posted || !DB.HaveWriteAccess("DocShipmentOrderInfo");
        }

        public string Title { get; set; }

        private Guid VMId { get; set; } = Guid.NewGuid();

        private void FillDocShipmentOrderGoods(Guid docShipmentOrderId)
        {
            DocShipmentOrderGoods =
                new ObservableCollection<DocShipmentOrderGood>(GammaBase.vDocShipmentOrders.Where(
                    d => d.C1CDocShipmentOrderID == docShipmentOrderId)
                    .Select(d => new DocShipmentOrderGood
                    {
                        NomenclatureId = d.C1CNomenclatureID,
                        CharacteristicId = d.C1CCharacteristicID,
                        NomenclatureName = d.NomenclatureName,
                        Amount = d.Quantity,
                        CollectedQuantity = d.CollectedQuantity
                    }));
            foreach (var good in DocShipmentOrderGoods)
            {
                good.DocShipmentOrderProducts = new List<DocShipmentOrderProduct>(GammaBase.vProductsInfo
                    .Where(p => GammaBase.DocProducts.Where(dp => dp.Docs.DocShipments.Select(ds => ds.C1CDocShipmentOrderID).Contains(docShipmentOrderId)
                    && p.C1CNomenclatureID == good.NomenclatureId && p.C1CCharacteristicID == good.CharacteristicId).Select(dp => dp.ProductID).Contains(p.ProductID)).Select(p => new DocShipmentOrderProduct
                    {
                        Number = p.Number,
                        Quantity = p.BaseMeasureUnitQuantity??0
                    }));
            }
        }

        public List<BarViewModel> Bars { get; set; } = new List<BarViewModel>();

        public DateTime? Date { get; set; }
        public string Number { get; set; }
        public string Consignee { get; set; }

        [UIAuth(UIAuthLevel.ReadOnly)]
        public string VehicleNumber { get; set; }
        [UIAuth(UIAuthLevel.ReadOnly)]
        public int? ActivePersonId { get; set; }
        public List<Persons> Persons { get; set; }

        private Guid DocShipmentOrderID { get; set; }

        public override void SaveToModel(GammaEntities gammaBase = null)
        {
            if (!DB.HaveWriteAccess("DocShipmentOrderInfo")) return;
            base.SaveToModel(gammaBase);
            var docShipmentOrderInfo =
                GammaBase.DocShipmentOrderInfo.FirstOrDefault(d => d.C1CDocShipmentOrderID == DocShipmentOrderID);
            if (docShipmentOrderInfo == null)
            {
                docShipmentOrderInfo = new DocShipmentOrderInfo();
                GammaBase.DocShipmentOrderInfo.Add(docShipmentOrderInfo);
            }
            docShipmentOrderInfo.VehicleNumber = VehicleNumber;
            docShipmentOrderInfo.ActivePersonID = ActivePersonId;
            GammaBase.SaveChanges();
        }

        public ObservableCollection<DocShipmentOrderGood> DocShipmentOrderGoods { get; set; }

        public bool IsReadOnly { get; private set; }

        public class DocShipmentOrderGood
        {
            public Guid? NomenclatureId { get; set;}
            public Guid? CharacteristicId { get; set; }
            public string NomenclatureName { get; set; }
            public string Amount { get; set; }
            public decimal CollectedQuantity { get; set; }
            public List<DocShipmentOrderProduct> DocShipmentOrderProducts { get; set; }
        }

        public class DocShipmentOrderProduct
        {
            public string Number { get; set; }
            public decimal Quantity { get; set; }
        }
    }
}
