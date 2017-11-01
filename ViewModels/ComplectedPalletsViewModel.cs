using System;
using System.Collections.Generic;
using System.Linq;
using DevExpress.Mvvm;
using Gamma.Interfaces;
using Gamma.Models;

namespace Gamma.ViewModels
{
    public class ComplectedPalletsViewModel : RootViewModel, IItemManager
    {
        public ComplectedPalletsViewModel()
        {
            Intervals = new List<string> { "Последние 500", "За последний день", "Поиск" };
            RefreshCommand = new DelegateCommand(Find);
            NewItemCommand = new DelegateCommand(ComplectPallet);
            Find();
        }

        public List<string> Intervals { get; set; }

        private List<ComplectedPallet> _pallets;
        private int _intervalId;


        public List<ComplectedPallet> Pallets
        {
            get { return _pallets; }
            set
            {
                _pallets = value;
                RaisePropertiesChanged("Pallets");
            }
        }

        private void ComplectPallet()
        {
            MessageManager.CreateNewProduct(DocProductKinds.DocProductPallet);
        }

        public string Number { get; set; }

        public DateTime? DateBegin
        {
            get; set;    
        }

        public DateTime? DateEnd { get; set; }

        public ComplectedPallet SelectedPallet { get; set; }

        public DelegateCommand NewItemCommand { get; }
        public DelegateCommand<object> EditItemCommand { get; }
        public DelegateCommand DeleteItemCommand { get; }
        public DelegateCommand RefreshCommand { get; }

        private void Find()
        {
            using (var gammaBase = DB.GammaDb)
            {
                switch (IntervalId)
                {
                    case 0:
                        Pallets = gammaBase.Docs.Where(
                            d =>
                                d.DocProduction.DocOrderId != null &&
                                d.DocProduction.DocProductionProducts.FirstOrDefault().Products.ProductKindID ==
                                (int) ProductKind.ProductPallet).
                            OrderByDescending(d => d.Date).Take(500).
                            Select(d => new ComplectedPallet
                            {
                                DocId = d.DocID,
                                Number = d.DocProduction.DocProductionProducts.FirstOrDefault().Products.Number,
                                Date = d.Date,
                                ProductId = d.DocProduction.DocProductionProducts.FirstOrDefault().ProductID,
                                DocOrderId = (Guid)d.DocProduction.DocOrderId,
                                OrderNumber = gammaBase.v1COrders.FirstOrDefault(order => order.C1COrderID == d.DocProduction.DocOrderId).Number,
                                PalletItems = d.DocProduction.DocProductionProducts.FirstOrDefault().Products.ProductPallets.ProductItems
                                    .Select(p => new PalletItem
                                    {
                                        NomenclatureId = p.C1CNomenclatureID,
                                        CharacteristicId = p.C1CCharacteristicID,
                                        Quantity = p.Quantity ??0,
                                        NomenclatureName = p.C1CNomenclature.Name + " " + p.C1CCharacteristics.Name
                                    }).ToList()
                            }).ToList();
                        break;
                }
            }
            
        }

        public int IntervalId
        {
            get { return _intervalId; }
            set
            {
                if (_intervalId == value) return;
                _intervalId = value < 0 ? 0 : value;
                if (_intervalId < 2) Find();
            }
        }
    }
}
