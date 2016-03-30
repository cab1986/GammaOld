using System.Collections.ObjectModel;
using Gamma.Interfaces;
using System;
using GalaSoft.MvvmLight.Command;
using System.Linq;
using Gamma.Attributes;
using Gamma.Models;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight;
using System.Collections.Generic;
using System.Windows;
using System.Data.Entity;
using Gamma.Common;
using System.ComponentModel.DataAnnotations;

namespace Gamma.ViewModels
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class DocProductUnloadViewModel : DBEditItemWithNomenclatureViewModel, IBarImplemented, ICheckedAccess
    {
        /// <summary>
        /// Инициализирует новую viewmodel.
        /// Если новый, то ID = ProductionTaskID иначе ID = DocID
        /// </summary>
        public DocProductUnloadViewModel(Guid docID, bool NewProduct) 
        {
            if (NewProduct)  // Если новый съем то получаем раскрой по из задания DocProduction и инициализруем коллекцию тамбуров съема
            {
                GetProductionTaskRWInfo(docID);
                UnloadSpools = new ObservableCollection<PaperBase>();
                var sourceSpools = DB.GammaBase.GetActiveSourceSpools(WorkSession.PlaceID).Select(p => p);
                if (sourceSpools == null)
                {
                    MessageBox.Show("Нет активных раскатов. Окно будет закрыто!", "Нет активных раскатов", MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    CloseWindow();
                }
                SourceSpools = sourceSpools.OfType<Guid>().ToList();
            }
            else // Получение данных по ID документа
            {
                IsConfirmed = DB.GammaBase.Docs.Where(d => d.DocID == docID).Select(d => d.IsConfirmed).FirstOrDefault();
                SourceSpools = DB.GammaBase.DocProducts.Where(dp => dp.Docs.DocWithdrawal.DocProduction.
                    Any(dprod => dprod.DocID == docID)).Select(dp => dp.ProductID).ToList();             
                DocProducts = new ObservableCollection<DocProducts>
                (DB.GammaBase.DocProducts.Include("Products").Include("Products.ProductSpools").Where(dp => dp.DocID == docID));
                Products = new ObservableCollection<Products>(DocProducts.Select(dp => dp.Products));
//                (from p in DB.GammaBase.Products where DocProducts.Where(dp => dp.ProductID == p.ProductID).Any() select p);
                UnloadSpools = new ObservableCollection<PaperBase>(from dp in DocProducts
                                                         join ps in DB.GammaBase.ProductSpools.Include(p => p.C1CNomenclature)
                                                                   .Include(p => p.C1CCharacteristics) on dp.ProductID equals ps.ProductID
                                                         select new PaperBase
                                                         {
                                                             ProductID = ps.ProductID,
                                                             Number = dp.Products.Number,
                                                             Nomenclature = ps.C1CNomenclature.Name + " " + ps.C1CCharacteristics.Name,
                                                             CharacteristicID = (Guid)ps.C1CCharacteristicID,
                                                             NomenclatureID = ps.C1CNomenclatureID,
                                                             Weight = ps.Weight,
                                                             Diameter = ps.Diameter
                                                         }
                                    );
                ProductSpools = new ObservableCollection<ProductSpools>
                (DocProducts.Select(dp => dp.Products.ProductSpools));
                if (UnloadSpools.Count == 0)
                {
                    GetProductionTaskRWInfo(docID);
                }
                else
                {
                    Diameter = UnloadSpools[0].Diameter;
                    NomenclatureID = UnloadSpools[0].NomenclatureID;
                    Cuttings = new ObservableCollection<Cutting>
                    (
                    from u in UnloadSpools
                    group u by u.CharacteristicID into grouped
                    select new Cutting
                    {
                        CharacteristicID = grouped.Key,
                        Quantity = grouped.Count()
                    }
                    );
                }
            }
            CreateSpoolsCommand = new RelayCommand(CreateSpools, () => !IsReadOnly && IsValid);
            EditSpoolCommand = new RelayCommand(EditSpool);
            Bars.Add(ReportManager.GetReportBar("Unload",VMID));
        }
        private List<Guid> SourceSpools { get; set; }
        private void GetProductionTaskRWInfo(Guid docID)
        {
            var productionTaskID = DB.GammaBase.DocProduction.Where(d => d.DocID == docID).Select(d => d.ProductionTaskID).FirstOrDefault();
            NomenclatureID = DB.GammaBase.ProductionTasks.Where(p => p.ProductionTaskID == productionTaskID).
                Select(p => p.C1CNomenclatureID).FirstOrDefault();
            Diameter = DB.GammaBase.ProductionTaskSGB.Where(p => p.ProductionTaskID == productionTaskID).Select(p => p.Diameter).FirstOrDefault() ?? 0;
            Cuttings = new ObservableCollection<Cutting>
            (
                from ptcut in DB.GammaBase.ProductionTaskRWCutting
                where ptcut.ProductionTaskID == productionTaskID
                group ptcut by ptcut.C1CCharacteristicID into grouped
                select new Cutting
                {
                    CharacteristicID = (Guid)grouped.Key,
                    Quantity = grouped.Count()
                }
            );
        }
        private ObservableCollection<BarViewModel> _bars = new ObservableCollection<BarViewModel>();
        public ObservableCollection<BarViewModel> Bars
        {
            get
            {
                return _bars;
            }
            set
            {
                _bars = value;
                RaisePropertyChanged("Bars");
            }
        }
        public RelayCommand CreateSpoolsCommand { get; private set; }
        
        private void CreateSpools()
        {
            UIServices.SetBusyState();
            if (UnloadSpools.Count > 0) 
            {
                UnloadSpools.Clear();
                DB.GammaBase.DocProducts.RemoveRange(DocProducts);
                DB.GammaBase.ProductSpools.RemoveRange(ProductSpools);
                DB.GammaBase.Products.RemoveRange(Products);
                DocProducts = null;
                ProductSpools = null;
                Products = null;
            }
            foreach (var cutting in Cuttings)
            {
                for (int i = 0; i < cutting.Quantity; i++)
                {
                    UnloadSpools.Add(new PaperBase
                    {
                        CharacteristicID = cutting.CharacteristicID,
                        NomenclatureID = (Guid)NomenclatureID,
                        Nomenclature = NomenclatureName + " " +
                        Characteristics.Where(c => c.CharacteristicID == cutting.CharacteristicID).Select(c => c.CharacteristicName).First(),
                        ProductID = SQLGuidUtil.NewSequentialId()
                    });    
                }
            }
            UnloadSpoolsSaved = false;
            Messenger.Default.Send<ParentSaveMessage>(new ParentSaveMessage());
        }
        private bool UnloadSpoolsSaved { get; set; }
        private ObservableCollection<PaperBase> _unloadSpools = new ObservableCollection<PaperBase>();
        public ObservableCollection<PaperBase> UnloadSpools
        {
            get
            {
                return _unloadSpools;
            }
            set
            {
                _unloadSpools = value;
                RaisePropertyChanged("Spools");
            }
        }
        
        public override void SaveToModel(Guid ItemID)
        {
            base.SaveToModel(ItemID);
            if (UnloadSpoolsSaved) return;
            if (DocProducts == null || DocProducts.Count == 0)
            {
                var docProduction = DB.GammaBase.DocProduction.Find(ItemID);
                var docs = new List<Docs>();
                var docWithdrawals = new List<DocWithdrawal>();
                foreach (var spoolID in SourceSpools)
                {
                    if (!DB.GammaBase.DocProducts.Any(dp => dp.ProductID == spoolID && dp.Docs.DocTypeID == (byte)DocTypes.DocWithdrawal))
                    {
                        var docID = SQLGuidUtil.NewSequentialId();
                        var doc = new Docs()
                            {
                                DocID = docID,
                                Date = DB.CurrentDateTime,
                                DocTypeID = (byte)DocTypes.DocWithdrawal,
                                IsConfirmed = true,
                                PlaceID = WorkSession.PlaceID,
                                UserID = WorkSession.UserID,
                                ShiftID = WorkSession.ShiftID,
                                DocProducts = new ObservableCollection<DocProducts>()
                                {
                                    new DocProducts()
                                    {
                                        DocID = docID,
                                        ProductID = spoolID,
                                        IsOutConfirmed = true
                                    }
                                }
                            };
                        docs.Add(doc);
                        docWithdrawals.Add
                            (
                            new DocWithdrawal()
                            {
                                DocID = doc.DocID,
                                OutPlaceID = WorkSession.PlaceID
                            }
                            );
                    }
                    else
                    {
                        var doc = (from dp in DB.GammaBase.DocProducts
                                   where dp.ProductID == spoolID &&
                                       dp.Docs.DocTypeID == (byte)DocTypes.DocWithdrawal
                                   select dp.Docs).FirstOrDefault();
                        docs.Add(doc);
                        var docWithdrawal = DB.GammaBase.DocWithdrawal.Where(dw => dw.DocID == doc.DocID).FirstOrDefault();
                        docWithdrawals.Add(docWithdrawal);
                    }
                }
                if (docs.Count > 0)
                {
                    DB.GammaBase.Docs.AddRange(docs.Except(DB.GammaBase.Docs));
                    DB.GammaBase.DocWithdrawal.AddRange(docWithdrawals.Except(DB.GammaBase.DocWithdrawal));
                }
                foreach (var docWithdrawal in docWithdrawals)
                {
                    docProduction.DocWithdrawal.Add(docWithdrawal);
                }
                Products = new ObservableCollection<Products>();
                ProductSpools = new ObservableCollection<ProductSpools>();
                DocProducts = new ObservableCollection<DocProducts>();
                foreach (var spool in UnloadSpools)
                {
                    Products.Add(new Products()
                    {
                        ProductID = spool.ProductID,
                        ProductKindID = (byte)ProductKinds.ProductSpool,
                    });
                    ProductSpools.Add(new ProductSpools()
                        {
                            ProductID = spool.ProductID,
                            Diameter = Diameter,
                            BreakNumber = BreakNumber,
                            Weight = (int)spool.Weight,
                            C1CNomenclatureID = spool.NomenclatureID,
                            C1CCharacteristicID = spool.CharacteristicID
                        });
                    DocProducts.Add(new DocProducts()
                        {
                            DocID = ItemID,
                            ProductID = spool.ProductID,
                        });
                }
                DB.GammaBase.Products.AddRange(Products);
                DB.GammaBase.ProductSpools.AddRange(ProductSpools);
                DB.GammaBase.DocProducts.AddRange(DocProducts);
            }
            DB.GammaBase.SaveChanges();
            UnloadSpoolsSaved = true;
            foreach (var product in Products)
            {
                DB.GammaBase.Entry<Products>(product).Reload();
                UnloadSpools.Where(s => s.ProductID == product.ProductID).Select(s => s).First().Number = product.Number;
            }
        }
        private ObservableCollection<DocProducts> DocProducts { get; set; }
        private ObservableCollection<Products> Products { get; set; }
        private ObservableCollection<ProductSpools> ProductSpools { get; set; }
        private Guid? _vmID = Guid.NewGuid();
        public Guid? VMID
        {
            get
            {
                return _vmID;
            }
        }

        private byte? _breakNumber;
        [UIAuth(UIAuthLevel.ReadOnly)]
        public byte? BreakNumber
        {
            get
            {
                return _breakNumber;
            }
            set
            {
                _breakNumber = value;
                RaisePropertyChanged("BreakNumber");
            }
        }
        private int _diameter;
        [UIAuth(UIAuthLevel.ReadOnly)]
        [Range(1,10000,ErrorMessage="Необходимо указать диаметр")]
        public int Diameter
        {
            get
            {
                return _diameter;
            }
            set
            {
                _diameter = value;
                RaisePropertyChanged("Diameter");
            }
        }
        public RelayCommand EditSpoolCommand { get; set; }
        public PaperBase SelectedUnloadSpool 
        {
            get; set;
        }
        private void EditSpool()
        {
            Messenger.Default.Register<ProductChangedMessage>(this,ProductChanged);
            MessageManager.OpenDocProduct(DocProductKinds.DocProductSpool, SelectedUnloadSpool.DocID);
        }
        private void ProductChanged(ProductChangedMessage msg)
        {
            Messenger.Default.Unregister(this);
            UnloadSpools.Where(u => u.ProductID == msg.ProductID).FirstOrDefault().Weight =
                (from ps in DB.GammaBase.ProductSpools where ps.ProductID == msg.ProductID select ps.Weight).FirstOrDefault();
        }
        public ObservableCollection<Cutting> Cuttings { get; set; }
        public class Cutting
        {
            public Guid CharacteristicID { get; set; }
            public int Quantity { get; set; }
        }
        private bool IsConfirmed { get; set; }
        public bool IsReadOnly
        {
            get 
            {
                return IsConfirmed || !DB.HaveWriteAccess("ProductSpools");
            }
        }
    }
}