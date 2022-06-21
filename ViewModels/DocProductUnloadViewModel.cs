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
using System.Data.Entity.SqlServer;
using Gamma.Controllers;

namespace Gamma.ViewModels
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class DocProductUnloadViewModel : SaveImplementedViewModel, IBarImplemented, ICheckedAccess, IProductValidate
    {
        /// <summary>
        /// Инициализирует новую viewmodel.
        /// Если новый, то id = ProductionTaskID иначе id = DocID
        /// </summary>
        public DocProductUnloadViewModel(Guid docID, bool docIsReadOnly, DocProductViewModel parentDocProductViewModel, Guid? productionTaskID = null, int? docPlaceID = null, bool newProduct = false)
        {
            CreateSpoolsCommand = new DelegateCommand(CreateSpools, CanCreateSpools);
            EditSpoolCommand = new DelegateCommand(EditSpool);
            Messenger.Default.Register<DocChangedMessage>(this, DocChanged);
            Bars.Add(ReportManager.GetReportBar("Unload", VMID));
            UpdateViewModel(docID, docIsReadOnly, parentDocProductViewModel, productionTaskID, docPlaceID, newProduct);
        }

        public void UpdateViewModel(Guid docID, bool docIsReadOnly, DocProductViewModel parentDocProductViewModel, Guid? productionTaskID = null, int? docPlaceID = null, bool newProduct = false)
        {
            StartInitialization();
            DocID = docID;
            ParentDocProductViewModel = parentDocProductViewModel;
            //var productionTaskID =
            //        GammaBase.DocProduction.Where(d => d.DocID == docID).Select(d => d.ProductionTaskID).FirstOrDefault();
            if (productionTaskID == null)
            {
                Functions.ShowMessageError("Не удалось получить информацию о задании",
                    "Error open DocProductUnloadViewModel: productionTaskID = null", DocID);
            }
            else
            {
                ProductionTaskID = (Guid)productionTaskID;
                GetProductionTaskRwInfo(ProductionTaskID);
            }
            CalculateIsReadOnly(docIsReadOnly);
            PlaceID = docPlaceID ?? WorkSession.PlaceID;
            if (newProduct)  // Если новый съем то получаем раскрой из задания DocProduction и инициализруем коллекцию тамбуров съема
            {
                DB.AddLogMessageInformation("Загрузка данных: новый продукт съём",
                        "UpdateViewModel in DocProductUnloadViewModel: newProduct", DocID);
                SetIsChanged(true);
//              GetProductionTaskRWInfo(id);
                UnloadSpools = new ObservableCollection<PaperBaseWithChekWeight>();
                SourceSpools = GammaBase.GetActiveSourceSpools(WorkSession.PlaceID).Select(p => p).OfType<Guid>().ToList();
                if (!SourceSpools.Any())
                {
                    Functions.ShowMessageError("Нет активных раскатов. Окно будет закрыто!",
                        "Error GetActiveSourceSpools in DocProductUnloadViewModel: No active source spools", DocID);
                    CloseWindow();
                }
            }
            else // Получение данных по id документа
            {
                DB.AddLogMessageInformation("Загрузка данных: существующий продукт",
                        "UpdateViewModel in DocProductUnloadViewModel: not newProduct", DocID);
                //IsConfirmed = GammaBase.Docs.Where(d => d.DocID == docID).Select(d => d.IsConfirmed).FirstOrDefault();
                //PlaceID = docPlaceID ?? WorkSession.PlaceID;
                SourceSpools = GammaBase.DocWithdrawalProducts.Where(dp => dp.DocWithdrawal.DocProduction.
                    Any(dprod => dprod.DocID == docID)).Select(dp => dp.ProductID).ToList();             
                DocProductionProducts = new ObservableCollection<DocProductionProducts>
                (GammaBase.DocProductionProducts.Include(d => d.Products).Include(d => d.Products.ProductSpools)
                .Include(d => d.Products.ProductSpools.C1CNomenclature)
                .Include(d => d.Products.ProductSpools.C1CCharacteristics).Where(dp => dp.DocID == docID));
                //Products = new ObservableCollection<Products>(DocProductionProducts.Select(dp => dp.Products));
                UnloadSpools = new ObservableCollection<PaperBaseWithChekWeight>(from dp in DocProductionProducts
                                                                   select new PaperBaseWithChekWeight(this)
                                                                   {
                                                             BreakNumber = dp.Products.ProductSpools.BreakNumber,
                                                             ProductID = dp.Products.ProductSpools.ProductID,
                                                             Number = dp.Products.Number,
                                                             Nomenclature = dp.Products.ProductSpools.C1CNomenclature.Name + " " + dp.Products.ProductSpools.C1CCharacteristics.Name,
                                                             CharacteristicID = (Guid)dp.Products.ProductSpools.C1CCharacteristicID,
                                                             NomenclatureID = dp.Products.ProductSpools.C1CNomenclatureID,
                                                             Weight = (dp.Quantity??0)*1000,
                                                             Diameter = dp.Products.ProductSpools.Diameter,
                                                             Length = dp.Products.ProductSpools.Length??0,
                                                             RealFormat = dp.Products.ProductSpools.RealFormat
                                                             //RealBasisWeight = dp.Products.ProductSpools.RealBasisWeight
                                                         }
                                    );
                //ProductSpools = new ObservableCollection<ProductSpools>(DocProductionProducts.Select(dp => dp.Products.ProductSpools));
                if (UnloadSpools.Count > 0)
                {
                    Diameter = UnloadSpools[0].Diameter;
                    BreakNumber = UnloadSpools[0].BreakNumber;
                    //RealBasisWeight = UnloadSpools[0].RealBasisWeight ?? 0;
                    Length = UnloadSpools[0].Length;
                }
                //SetUnloadSpoolsSaved(true);
            }
            EndInitialization();
        }

        private DocProductViewModel ParentDocProductViewModel { get; set; }

        private void CalculateIsReadOnly(bool isReadOnly)
        {
            IsReadOnly = isReadOnly || !DB.HaveWriteAccess("ProductSpools");
            RaisePropertiesChanged("IsReadOnly");
        }

        private void DocChanged(DocChangedMessage msg)
        {
            if (DocID != msg.DocId) return;
            CalculateIsReadOnly(msg.IsConfirmed);
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
            //RealBasisWeight = Cuttings.FirstOrDefault()?.BasisWeight ?? 0;
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
        private readonly DocumentController documentController = new DocumentController();
        private readonly ProductController productController = new ProductController();

        public DelegateCommand CreateSpoolsCommand { get; private set; }
        
        private void CreateSpools()
        {
            UIServices.SetBusyState();
            DB.AddLogMessageInformation("Нажатие кнопки Создать рулоны в съеме DocID",
                "Start CreateSpools in DocProductUnpackViewModel", DocID);
            using (var gammaBase = DB.GammaDb)
            {
                if (gammaBase.Docs.Any(d => d.DocTypeID == (int) DocTypes.DocProduction &&
                                            d.Date > gammaBase.Docs.FirstOrDefault(dc => dc.DocID == DocID).Date &&
                                            d.PlaceID == gammaBase.Docs.FirstOrDefault(dc => dc.DocID == DocID).PlaceID))
                {
                    Functions.ShowMessageInformation("Есть съем произведенный позднее, редактирование запрещено. Редактируйте каждый рулон отдельно или удалите некорректный съем и создайте новый",
                        "Error CreateSpools in DocProductUnloadViewModel: Is exist later doc", DocID);
                    return;
                }
                SetIsChanged(true);
                UnloadSpools.Clear();
                /*UnloadSpools =
                    new ObservableCollection<PaperBase>(from us in gammaBase.CreateUnloadSpools(DocID, ProductionTaskID, Diameter, BreakNumber, Length)
                        select new PaperBase()
                        {
                            DocID = (Guid)us.DocID,
                            ProductID = (Guid)us.ProductID,
                            Number = us.Number,
                            Nomenclature = us.NomenclatureName
                        });
                */
                //var docProduction = documentController.ConstructDoc(newId, DocTypes.DocProduction, true, PlaceID);
                if (DocID != null && DocID != Guid.Empty)
                {
                    var docProduction = ParentDocProductViewModel.GetDoc;
                    if (docProduction != null)
                    {
                        //var docProduction = GammaBase.Docs.FirstOrDefault(p => p.DocID == DocID);
                        foreach (var cut in Cuttings)
                        {
                            var spool = new PaperBaseWithChekWeight(this)
                            {
                                BreakNumber = BreakNumber,
                                ProductID = SqlGuidUtil.NewSequentialid(),
                                Number = "",
                                NomenclatureID = (Guid)cut.NomenclatureID,
                                CharacteristicID = (Guid)cut.CharacteristicID,
                                Nomenclature = cut.NomenclatureName,
                                Weight = 1,
                                Diameter = Diameter,
                                Length = Length,
                                RealFormat = cut.BaseFormat
                                //RealBasisWeight = RealBasisWeight
                            };
                            UnloadSpools.Add(spool);
                        }
                    }
                }
            }
        }

        //private bool UnloadSpoolsSaved { get; set; }
        //public void SetUnloadSpoolsSaved(bool value) => UnloadSpoolsSaved = value;

        private ObservableCollection<PaperBaseWithChekWeight> _unloadSpools = new ObservableCollection<PaperBaseWithChekWeight>();
        public ObservableCollection<PaperBaseWithChekWeight> UnloadSpools
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

        public bool IsValidProduct => ValidateProduct();

        public bool ValidateProduct()
        {
            foreach (var spool in UnloadSpools.Where(s => s.Weight > 1))
            {
                var result = GammaBase.ValidateSpoolBeforeSave(spool.NomenclatureID, spool.CharacteristicID, Diameter, spool.Weight, spool.RealFormat).FirstOrDefault();
                if (!string.IsNullOrEmpty(result))
                {
                    Functions.ShowMessageInformation(result + Environment.NewLine + "Рулон " + spool.Number + " с весом " + spool.Weight , "ValidateSpoolBeforeSave", DocID, spool.ProductID);
                    return false;
                }
            }
            return true;
        }

        public override bool SaveToModel(Guid itemID)
        {
            DB.AddLogMessageInformation("Начало сохранения продукта съёма", "Start SaveToModel in DocProductUnloadViewModel", DocID);
            //if (IsReadOnly) return true;          
            if (!IsChanged)
            {
                DB.AddLogMessageInformation("Успешный выход без сохранения продукта съёма", "Quit successed from SaveToModel in DocProductUnloadViewModel: IsChanged = " + IsChanged.ToString(), DocID);
                return true;
            }
            if (!IsValidProduct)
            {
                DB.AddLogMessageInformation("Ошибочный выход без сохранения продукта съёма", "Quit failed from SaveToModel in DocProductUnloadViewModel: IsValidProduct = " + IsValidProduct.ToString(), DocID);
                return false;
            }
            var doc = GammaBase.DocProduction.Include(d => d.DocWithdrawal).FirstOrDefault(d => d.DocID == itemID);
            foreach (var spool in UnloadSpools)
            {
                var product =
                GammaBase.Products.Include(p => p.ProductSpools).Include(p => p.DocProductionProducts)
                    .FirstOrDefault(p => p.ProductID == spool.ProductID);
                if (product == null)
                {
                    //var id = SqlGuidUtil.NewSequentialid();
                    product = new Products
                    {
                        ProductID = spool.ProductID,
                        ProductKindID = (int)ProductKind.ProductSpool,
                    };
                    GammaBase.Products.Add(product);
                }
                if (product.ProductSpools == null)
                {
                    product.ProductSpools = new ProductSpools
                    {
                        ProductID = product.ProductID
                    };
                }
                if (product.DocProductionProducts == null || product.DocProductionProducts.Count == 0)
                {
                    product.DocProductionProducts = new List<DocProductionProducts>
                    {
                        new DocProductionProducts
                        {
                            ProductID = product.ProductID,
                            DocID = itemID,
                            Quantity = spool.Weight/1000,
                            C1CNomenclatureID = spool.NomenclatureID,
                            C1CCharacteristicID = spool.CharacteristicID
                        }
                    };
                }

                //if (AllowEditProduct)
                {
                    product.ProductSpools.C1CNomenclatureID = spool.NomenclatureID;
                    product.ProductSpools.C1CCharacteristicID = spool.CharacteristicID;
                    product.ProductSpools.DecimalWeight = spool.Weight/1000;
                    var docProductionProduct = product.DocProductionProducts.FirstOrDefault();
                    if (docProductionProduct != null)
                    {
                        docProductionProduct.Quantity = spool.Weight/1000;
                        docProductionProduct.C1CNomenclatureID = spool.NomenclatureID;
                        docProductionProduct.C1CCharacteristicID = spool.CharacteristicID;
                    }
                }
                //product.ProductSpools.RealBasisWeight = RealBasisWeight;
                product.ProductSpools.RealFormat = spool.RealFormat;
                product.ProductSpools.BreakNumber = BreakNumber;
                product.ProductSpools.Diameter = Diameter;
                product.ProductSpools.Length = Length;
                if (DiameterIsChanged) product.ProductSpools.CurrentDiameter = Diameter;
                if (LengthIsChanged) product.ProductSpools.CurrentLength = Length;

                //product.ProductSpools.ToughnessKindID = ToughnessKindID;

            }

            foreach (var spoolId in SourceSpools)
            {
                var docWithdrawal = GammaBase.Docs.FirstOrDefault(d => d.DocTypeID == (byte) DocTypes.DocWithdrawal 
                                                            && d.DocWithdrawal.DocWithdrawalProducts.Any(dp => dp.ProductID == spoolId 
                                                            && ((dp.Quantity == null && (dp.CompleteWithdrawal == null || dp.CompleteWithdrawal == false))
                                                                || (dp.Quantity != null
                                                                    && GammaBase.DocCloseShiftRemainders.Any(r => r.ProductID == spoolId && (r.IsSourceProduct ?? false) && r.DocWithdrawalID == dp.DocID && r.DocCloseShifts.PlaceID == WorkSession.PlaceID && r.DocCloseShifts.ShiftID == WorkSession.ShiftID
                                                                            && r.DocCloseShifts.Date >= SqlFunctions.DateAdd("hh", -1, DB.GetShiftBeginTime((DateTime)SqlFunctions.DateAdd("hh", -1, DB.CurrentDateTime)))
                                                                            && r.DocCloseShifts.Date <= SqlFunctions.DateAdd("hh", 1, DB.GetShiftEndTime((DateTime)SqlFunctions.DateAdd("hh", -1, DB.CurrentDateTime))))))));
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
            SetIsChanged(false);
            DB.AddLogMessageInformation("Успешное окончание сохранения продукта съёма", "End SaveToModel in DocProductUnloadViewModel: success", DocID);
            return true;
        }
        private ObservableCollection<DocProductionProducts> DocProductionProducts { get; set; }
        private Guid ProductionTaskID { get; set; }
        //private ObservableCollection<Products> Products { get; set; }
        //private ObservableCollection<ProductSpools> ProductSpools { get; set; }
        public Guid? VMID { get; } = Guid.NewGuid();

        private byte? _breakNumber;
        [UIAuth(UIAuthLevel.ReadOnly)]
        [Required(ErrorMessage = @"Необходимо указать количество обрывов")]
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
                if (!GetInitialization)
                {
                    SetIsChanged(true);
                    DB.AddLogMessageInformation("Изменено кол-во обрывов " + value.ToString() + " в съеме DocID",
                        "SET BreakNumber in DocProductUnloadViewModel: value = " + value.ToString(), DocID);
                }
            }
        }

        private bool DiameterIsChanged = false;

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
                if (!GetInitialization)
                {
                    DiameterIsChanged = true;
                    SetIsChanged(true);
                    DB.AddLogMessageInformation("Изменен диаметр " + value.ToString() + " в съеме DocID",
                        "SET Diameter in DocProductUnloadViewModel: value = " + value.ToString(), DocID);
                }
            }
        }

        private bool LengthIsChanged = false;

        private decimal _length;

        [UIAuth(UIAuthLevel.ReadOnly)]
        public decimal Length
        {
            get { return _length; }
            set
            {
                _length = value;
                RaisePropertyChanged("Length");
                if (!GetInitialization)
                {
                    LengthIsChanged = true;
                    SetIsChanged(true);
                    DB.AddLogMessageInformation("Изменена длина " + value.ToString() + " в съеме DocID",
                        "SET Length in DocProductUnloadViewModel: value = " + value.ToString(), DocID);
                }
            }
        }

        /*private decimal _realBasisWeight;

        [UIAuth(UIAuthLevel.ReadOnly)]
        public decimal RealBasisWeight
        {
            get { return _realBasisWeight; }
            set
            {
                _realBasisWeight = value;
                RaisePropertyChanged("RealBasisWeight");
                if (!GetInitialization)
                {
                    SetIsChanged(true);
                    DB.AddLogMessageInformation("Изменена Масса фактическая, г/м2 " + value.ToString() + " в съеме DocID",
                        "SET RealBasisWeight in DocProductUnloadViewModel: value = " + value.ToString(), DocID);
                }
            }
        }*/

        private Guid DocID { get; set; }

        public DelegateCommand EditSpoolCommand { get; set; }
        public PaperBaseWithChekWeight SelectedUnloadSpool 
        {
            get; set;
        }
        private void EditSpool()
        {
            DB.AddLogMessageInformation("Нажатие Изменить рулон ProductID в съеме DocID",
                "Start EditSpool in DocProductUnpackViewModel", DocID, SelectedUnloadSpool.ProductID);
            if (IsChanged)
            {
                if (Functions.ShowMessageQuestion("Открытие карточки продукта: " + Environment.NewLine + "Есть несохраненные данные! Нажмите Да, чтобы сохранить и открыть карточку продукта", "QUEST EditSpool in DocProductUnloadViewModel", DocID, SelectedUnloadSpool.ProductID)
                    == MessageBoxResult.Yes)
                {
                    ParentDocProductViewModel.SaveToModel();
                }
                else
                {
                    return;
                }
            }
            Messenger.Default.Register<ProductChangedMessage>(this, ProductChanged);
            MessageManager.OpenDocProduct(DocProductKinds.DocProductSpool, SelectedUnloadSpool.ProductID, false);
        }

        private void ProductChanged(ProductChangedMessage msg)
        {
            DB.AddLogMessageInformation("Обновление информации после зменения продукта ProductID в съеме DocID",
                "Start ProductChanged in DocProductUnpackViewModel", DocID, msg.ProductID);
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
        //private bool IsConfirmed { get; set; }
        private int PlaceID { get; set; }
        //public bool IsReadOnly { get; set; } //=> IsConfirmed || !DB.HaveWriteAccess("ProductSpools");

        public class PaperBaseWithChekWeight : PaperBase // ViewModelBase
        {
            public PaperBaseWithChekWeight(DocProductUnloadViewModel parentViewModel)
            {
                ParentViewModel = parentViewModel;
            }

            private DocProductUnloadViewModel ParentViewModel { get; set; }

            public decimal WeightWithChek
            {
                get { return Weight; }
                set
                {
                    /*if (!ParentViewModel.GetInitialization)
                    {
                        using (var gammaBase = DB.GammaDbWithNoCheckConnection)
                        {
                            var result = gammaBase.ValidateSpoolBeforeSave(this.NomenclatureID, this.CharacteristicID, this.Diameter, value, this.RealFormat).FirstOrDefault();
                            if (!string.IsNullOrEmpty(result))
                            {
                                Functions.ShowMessageInformation(result, "ValidateWeightWithChange", DocID, this.ProductID);
                                return;
                            }
                        }
                        ParentViewModel.SetIsChanged(true);
                    }*/
                    Weight = value;
                    ParentViewModel.SetIsChanged(true);
                }
            }

            public int? RealFormatWithChek
            {
                get { return RealFormat; }
                set
                {
                    /*if (!ParentViewModel.GetInitialization)
                    {
                        using (var gammaBase = DB.GammaDbWithNoCheckConnection)
                        {
                            var result = gammaBase.ValidateSpoolBeforeSave(this.NomenclatureID, this.CharacteristicID, this.Diameter, this.Weight, value).FirstOrDefault();
                            if (!string.IsNullOrEmpty(result))
                            {
                                Functions.ShowMessageInformation(result, "ValidateRealFormatWithChange", DocID, this.ProductID);
                                return;
                            }
                        }
                        ParentViewModel.SetIsChanged(true);
                    }*/
                    RealFormat = value;
                    ParentViewModel.SetIsChanged(true);
                }
            }
        }
    }
}