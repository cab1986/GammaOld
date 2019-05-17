// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System.Collections.ObjectModel;
using Gamma.Interfaces;
using System;
using DevExpress.Mvvm;
using System.Linq;
using Gamma.Attributes;
using Gamma.Models;
using System.Collections.Generic;
using System.Windows;
using System.Data.Entity;
using Gamma.Common;
using System.ComponentModel.DataAnnotations;
using Gamma.Entities;

namespace Gamma.ViewModels
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class DocProductUnloadViewModel : SaveImplementedViewModel, IBarImplemented, ICheckedAccess
    {
        /// <summary>
        /// Инициализирует новую viewmodel.
        /// Если новый, то id = ProductionTaskID иначе id = DocID
        /// </summary>
        public DocProductUnloadViewModel(Guid docID, bool newProduct)
        {
            DocID = docID;
            var productionTaskID =
                    GammaBase.DocProduction.Where(d => d.DocID == docID).Select(d => d.ProductionTaskID).FirstOrDefault();
            if (productionTaskID == null)
            {
                MessageBox.Show("Не удалось получить информацию о задании");
            }
            else
            {
                ProductionTaskID = (Guid)productionTaskID;
                GetProductionTaskRwInfo(ProductionTaskID);
            }
            CreateSpoolsCommand = new DelegateCommand(CreateSpools, CanCreateSpools);
            EditSpoolCommand = new DelegateCommand(EditSpool);
            Bars.Add(ReportManager.GetReportBar("Unload", VMID));
            if (newProduct)  // Если новый съем то получаем раскрой из задания DocProduction и инициализруем коллекцию тамбуров съема
            {
//              GetProductionTaskRWInfo(id);
                UnloadSpools = new ObservableCollection<PaperBase>();
                SourceSpools = GammaBase.GetActiveSourceSpools(WorkSession.PlaceID).Select(p => p).OfType<Guid>().ToList();
                if (!SourceSpools.Any())
                {
                    MessageBox.Show("Нет активных раскатов. Окно будет закрыто!", "Нет активных раскатов", MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    CloseWindow();
                }
            }
            else // Получение данных по id документа
            {
                IsConfirmed = GammaBase.Docs.Where(d => d.DocID == docID).Select(d => d.IsConfirmed).FirstOrDefault();
                SourceSpools = GammaBase.DocWithdrawalProducts.Where(dp => dp.DocWithdrawal.DocProduction.
                    Any(dprod => dprod.DocID == docID)).Select(dp => dp.ProductID).ToList();             
                DocProductionProducts = new ObservableCollection<DocProductionProducts>
                (GammaBase.DocProductionProducts.Include(d => d.Products).Include(d => d.Products.ProductSpools)
                .Include(d => d.Products.ProductSpools.C1CNomenclature)
                .Include(d => d.Products.ProductSpools.C1CCharacteristics).Where(dp => dp.DocID == docID));
                Products = new ObservableCollection<Products>(DocProductionProducts.Select(dp => dp.Products));
                UnloadSpools = new ObservableCollection<PaperBase>(from dp in DocProductionProducts
                                                                   select new PaperBase
                                                         {
                                                             BreakNumber = dp.Products.ProductSpools.BreakNumber,
                                                             ProductID = dp.Products.ProductSpools.ProductID,
                                                             Number = dp.Products.Number,
                                                             Nomenclature = dp.Products.ProductSpools.C1CNomenclature.Name + " " + dp.Products.ProductSpools.C1CCharacteristics.Name,
                                                             CharacteristicID = (Guid)dp.Products.ProductSpools.C1CCharacteristicID,
                                                             NomenclatureID = dp.Products.ProductSpools.C1CNomenclatureID,
                                                             Weight = dp.Quantity??0*1000,
                                                             Diameter = dp.Products.ProductSpools.Diameter,
                                                             Length = dp.Products.ProductSpools.Length??0
                                                         }
                                    );
                ProductSpools = new ObservableCollection<ProductSpools>
                (DocProductionProducts.Select(dp => dp.Products.ProductSpools));
                if (UnloadSpools.Count > 0)
                {
                    Diameter = UnloadSpools[0].Diameter;
                    BreakNumber = UnloadSpools[0].BreakNumber;
                }

            }
        }

        private bool CanCreateSpools()
        {
            return !IsReadOnly && IsValid && UnloadSpools?.Count == 0;
        }

        private List<Guid> SourceSpools { get; set; }
        private void GetProductionTaskRwInfo(Guid productionTaskID)
        {
            Diameter = GammaBase.ProductionTaskSGB.Where(p => p.ProductionTaskID == productionTaskID).Select(p => p.Diameter).FirstOrDefault() ?? 0;
            Cuttings = new ObservableCollection<Cutting>
            (
                GammaBase.ProductionTaskRWCutting.Where(
                    p => p.ProductionTaskID == productionTaskID).GroupBy(p => new { p.C1CNomenclatureID, p.C1CCharacteristicID })
                    .Select(g => new Cutting()
                    {
                        NomenclatureID = g.Key.C1CNomenclatureID,
                        CharacteristicID = g.Key.C1CCharacteristicID,
                        Quantity = g.Count()
                    })
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

        public DelegateCommand CreateSpoolsCommand { get; private set; }
        
        private void CreateSpools()
        {
            UIServices.SetBusyState();
            using (var gammaBase = DB.GammaDb)
            {
                if (gammaBase.Docs.Any(d => d.DocTypeID == (int) DocTypes.DocProduction &&
                                            d.Date > gammaBase.Docs.FirstOrDefault(dc => dc.DocID == DocID).Date &&
                                            d.PlaceID == gammaBase.Docs.FirstOrDefault(dc => dc.DocID == DocID).PlaceID))
                {
                    MessageBox.Show(
                        "Есть съем произведенный позднее, редактирование запрещено. Редактируйте каждый рулон отдельно или удалите некорректный съем и создайте новый",
                        "Ошибка создания рулонов", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                    return;
                }
                UnloadSpools.Clear();
                UnloadSpools =
                    new ObservableCollection<PaperBase>(from us in gammaBase.CreateUnloadSpools(DocID, ProductionTaskID, Diameter, BreakNumber, Length)
                        select new PaperBase()
                        {
                            DocID = (Guid)us.DocID,
                            ProductID = (Guid)us.ProductID,
                            Number = us.Number,
                            Nomenclature = us.NomenclatureName
                        });
            }
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
                RaisePropertyChanged("UnloadSpools");
            }
        }
        public override bool SaveToModel(Guid itemID)
        {
            if (IsReadOnly) return true;          
            if (UnloadSpoolsSaved) return true;
            var doc = GammaBase.DocProduction.Include(d => d.DocWithdrawal).FirstOrDefault(d => d.DocID == itemID);
            foreach (var spoolId in SourceSpools)
            {
                var docWithdrawal = GammaBase.Docs.FirstOrDefault(d => d.DocTypeID == (byte) DocTypes.DocWithdrawal 
                                                            && d.DocWithdrawal.DocWithdrawalProducts.Any(dp => dp.ProductID == spoolId && dp.Quantity == null &&
                                                            (dp.CompleteWithdrawal == null || dp.CompleteWithdrawal == false)));
                //&& d.DocWithdrawal.DocWithdrawalProducts.Select(dp => dp.ProductID)
                //                                                     .Contains(spoolId));
                if (docWithdrawal == null)
                {
                    var docID = SqlGuidUtil.NewSequentialid();
                    docWithdrawal = new Docs()
                    {
                        DocID = docID,
                        PlaceID = WorkSession.PlaceID,
                        ShiftID = WorkSession.ShiftID,
                        Date = DB.CurrentDateTime,
                        DocTypeID = (byte)DocTypes.DocWithdrawal,
                        IsConfirmed = true,
                        UserID = WorkSession.UserID,
                        DocWithdrawal = new DocWithdrawal()
                        {
                            DocID = docID,
                            OutPlaceID = WorkSession.PlaceID,
                            DocWithdrawalProducts = new List<DocWithdrawalProducts>
                            {
                                new DocWithdrawalProducts
                                {
                                    DocID = docID,
                                    ProductID = spoolId
                                }
                            }
                        }
                    };
                    GammaBase.Docs.Add(docWithdrawal);
                }
                if (!doc.DocWithdrawal.Contains(docWithdrawal.DocWithdrawal)) doc.DocWithdrawal.Add(docWithdrawal.DocWithdrawal);
            }
            GammaBase.SaveChanges();
            UnloadSpoolsSaved = true;
            return true;
        }
        private ObservableCollection<DocProductionProducts> DocProductionProducts { get; set; }
        private Guid ProductionTaskID { get; set; }
        private ObservableCollection<Products> Products { get; set; }
        private ObservableCollection<ProductSpools> ProductSpools { get; set; }
        public Guid? VMID { get; } = Guid.NewGuid();

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
        [Range(1,10000,ErrorMessage=@"Необходимо указать диаметр")]
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

        private decimal _length;

        [UIAuth(UIAuthLevel.ReadOnly)]
        public decimal Length
        {
            get { return _length; }
            set
            {
                _length = value;
                RaisePropertyChanged("Length");
            }
        }

        private Guid DocID { get; set; }

        public DelegateCommand EditSpoolCommand { get; set; }
        public PaperBase SelectedUnloadSpool 
        {
            get; set;
        }
        private void EditSpool()
        {
            Messenger.Default.Register<ProductChangedMessage>(this,ProductChanged);
            MessageManager.OpenDocProduct(DocProductKinds.DocProductSpool, SelectedUnloadSpool.ProductID);
        }

        private void ProductChanged(ProductChangedMessage msg)
        {
            Messenger.Default.Unregister(this);
            var unloadSpool = UnloadSpools.FirstOrDefault(u => u.ProductID == msg.ProductID);
            if (unloadSpool != null)
                unloadSpool.Weight =
                    (from ps in GammaBase.ProductSpools where ps.ProductID == msg.ProductID select ps.DecimalWeight*1000).FirstOrDefault();
        }

        public ObservableCollection<Cutting> Cuttings { get; set; }
        /*
        public class Cutting
        {
            public Guid CharacteristicID { get; set; }
            public int Quantity { get; set; }
        }
        */
        private bool IsConfirmed { get; set; }
        public bool IsReadOnly => IsConfirmed || !DB.HaveWriteAccess("ProductSpools");
    }
}