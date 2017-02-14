using System;
using System.Collections.ObjectModel;
using System.Linq;
using Gamma.Interfaces;
using Gamma.Models;
using System.Data.Entity;
using System.Windows;

namespace Gamma.ViewModels
{
    public class DocInventarisationViewModel : SaveImplementedViewModel, IBarImplemented
    {
        public DocInventarisationViewModel(Guid docId)
        {
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
                Items = new ObservableCollection<InventarisationItem>(doc.DocInventarisationProducts.Join(gammaBase.vProductsInfo.DefaultIfEmpty(),
                        ip => ip.ProductID,
                        p => p.ProductID, (ip,p) => new {InventarisationProduct = ip, Product = p}).Select(p => new InventarisationItem()
                        {
                            NomenclatureName = p.Product?.NomenclatureName??"",
                            Barcode = p.InventarisationProduct.Barcode,
                            Quantity = p.Product?.Quantity,
                            MeasureUnit = p.Product?.BaseMeasureUnit
                        }));
            }
        }

        public ObservableCollection<BarViewModel> Bars { get; set; } = new ObservableCollection<BarViewModel>();
        public Guid? VMID { get; } = Guid.NewGuid();

        public string Number { get; set; }
        public DateTime Date { get; set; }
        public string Place { get; set; }
        public ObservableCollection<InventarisationItem> Items;
    }
}
