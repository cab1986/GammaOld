// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Gamma.Interfaces;
using Gamma.Models;
using System.Data.Entity;
using System.Windows;
using DevExpress.Mvvm;

namespace Gamma.ViewModels
{
    public class DocInventarisationViewModel : SaveImplementedViewModel, IBarImplemented
    {
        public DocInventarisationViewModel(Guid docId)
        {
            DocId = docId;
            Bars.Add(ReportManager.GetReportBar("DocInventarisation", VMID));
            using (var gammaBase = DB.GammaDb)
            {
                var doc = gammaBase.Docs.Include(d => d.DocInventarisationProducts).Include(d => d.Persons).Include(d => d.Places)
                    .FirstOrDefault(d => d.DocID == docId);
                if (doc == null)
                {
                    MessageBox.Show("Не удалось получить информацию о документе");
                    CloseWindow();
                    return;
                }
                Number = doc.Number;
                Date = doc.Date;
                Place = doc.Places.Name;
                IsConfirmed = doc.IsConfirmed;
                var productIds =
                    doc.DocInventarisationProducts.Where(ip => ip.ProductID.HasValue)
                        .Select(ip => ip.ProductID)
                        .ToArray();
                var products =
                    gammaBase.vProductsInfo.Where(
                        p => productIds.Contains(p.ProductID)).ToArray();
                foreach (var ip in doc.DocInventarisationProducts)
                {
                    var product = products.FirstOrDefault(p => p.ProductID == ip.ProductID);
                    Items.Add(new InventarisationItem()
                    {
                        NomenclatureName = ip.NomenclatureName ?? product?.NomenclatureName ?? "",
                        Number = product?.Number ?? ip.Barcode,
                        Quantity = ip.Quantity ?? product?.Quantity ?? 0,
                        MeasureUnit = product?.BaseMeasureUnit ??"",
                        ProductKind = (ProductKind)product?.ProductKindID,
                        ProductID = ip.ProductID
                    });
                }
            }
            Messenger.Default.Register<PrintReportMessage>(this, PrintReport);
            ShowProductCommand = new DelegateCommand(ShowProduct, SelectedProduct != null);
        }

        private void PrintReport(PrintReportMessage msg)
        {
            if (msg.VMID != VMID) return;
            ReportManager.PrintReport(msg.ReportID, DocId);
        }

        private Guid DocId { get; set; }

        public ObservableCollection<BarViewModel> Bars { get; set; } = new ObservableCollection<BarViewModel>();
        public Guid? VMID { get; } = Guid.NewGuid();

        public string Number { get; set; }
        public DateTime Date { get; set; }
        public string Place { get; set; }
        public bool IsConfirmed { get; set; }
        public ObservableCollection<InventarisationItem> Items { get; set; } = new ObservableCollection<InventarisationItem>();

        public InventarisationItem SelectedProduct { get; set; }
        public DelegateCommand ShowProductCommand { get; private set; }

        private void ShowProduct()
        {
            if (SelectedProduct.ProductID != null && SelectedProduct.ProductID != Guid.Empty)
                switch (SelectedProduct.ProductKind)
                {
                    case ProductKind.ProductSpool:
                        MessageManager.OpenDocProduct(DocProductKinds.DocProductSpool, (Guid)SelectedProduct.ProductID);
                        break;
                    case ProductKind.ProductGroupPack:
                        MessageManager.OpenDocProduct(DocProductKinds.DocProductGroupPack, (Guid)SelectedProduct.ProductID);
                        break;
                    case ProductKind.ProductPallet:
                        MessageManager.OpenDocProduct(DocProductKinds.DocProductPallet, (Guid)SelectedProduct.ProductID);
                        break;
                    case ProductKind.ProductPalletR:
                        MessageManager.OpenDocProduct(DocProductKinds.DocProductPalletR, (Guid)SelectedProduct.ProductID);
                        break;
                    default:
                        //MessageBox.Show("Ошибка программы, действие не предусмотрено");
                        return;
                }
        }

        public override bool SaveToModel()
        {
            using (var gammaBase = DB.GammaDb)
            {
                var doc = gammaBase.Docs.FirstOrDefault(d => d.DocID == DocId);
                if (doc == null) return false;
                doc.Number = Number;
                doc.IsConfirmed = IsConfirmed;
                gammaBase.SaveChanges();
            }
            return true;
        }
    }
}
