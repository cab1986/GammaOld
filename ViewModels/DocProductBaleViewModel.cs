using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Gamma.Attributes;
using Gamma.Interfaces;
using System.Data.Entity;

namespace Gamma.ViewModels
{
    public sealed class DocProductBaleViewModel:DbEditItemWithNomenclatureViewModel, IBarImplemented, ICheckedAccess
    {
        private DocProductBaleViewModel()
        {
            Bars.Add(ReportManager.GetReportBar("ProductBale", VMID));
        }

        public DocProductBaleViewModel(Guid productId) : this()
        {
            using (var gammaBase = DB.GammaDb)
            {
                var productBale = gammaBase.ProductBales.Include(p => p.Products.DocProductionProducts.Select(dp => dp.DocProduction.Docs)).FirstOrDefault(p => p.ProductID == productId);
                if (productBale == null)
                {
                    MessageBox.Show("Не удалось загрузить информацию о кипе", "Ошибка загрузки", MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }
                NomenclatureID = productBale.C1CNomenclatureID;
                CharacteristicID = productBale.C1CCharacteristicID;
                Weight = Convert.ToInt32(productBale.Weight*1000);
                IsConfirmed = productBale.Products.DocProductionProducts.FirstOrDefault()?.DocProduction.Docs.IsConfirmed ?? false;
            }
        }

        private bool IsConfirmed { get; set; }

        [UIAuth(UIAuthLevel.ReadOnly)]
        public int Weight { get; set; }

        [UIAuth(UIAuthLevel.ReadOnly)]
        public override Guid? CharacteristicID
        {
            get { return base.CharacteristicID; }
            set { base.CharacteristicID = value; }
        }

        protected override bool CanChooseNomenclature()
        {
            return !IsReadOnly;
        }

        public ObservableCollection<BarViewModel> Bars { get; set; } = new ObservableCollection<BarViewModel>();
        public Guid? VMID { get; } = Guid.NewGuid();

        public bool IsReadOnly
        {
            get { return (!DB.HaveWriteAccess("ProductBales") || IsConfirmed) && IsValid; }
        }
    }
}
