// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.Interfaces;
using DevExpress.Mvvm;
using System.Collections.ObjectModel;
using System.Windows;
using Gamma.Attributes;
using System.ComponentModel.DataAnnotations;
using Gamma.Models;
using Gamma.Common;
using System.Data.Entity;
using System.Threading;
using Gamma.Entities;

namespace Gamma.ViewModels
{
    class DocProductGroupPackViewModel : SaveImplementedViewModel, ICheckedAccess, IBarImplemented, IProductValidate
    {
        public DocProductGroupPackViewModel()
        {
            IsNewGroupPack = true;
//            GetGrossWeightCommand = new DelegateCommand(GetGrossWeight, () => Scales.IsReady && !IsConfirmed);
            GetWeightCommand = new DelegateCommand(GetWeight, () => Scales.IsReady && !IsReadOnly);
            AddSpoolCommand = new DelegateCommand(AddSpool, () => !IsReadOnly);
            DeleteSpoolCommand = new DelegateCommand(DeleteSpool, () => SelectedSpool != null && !IsReadOnly && IsAllowDelete);
            OpenSpoolCommand = new DelegateCommand(OpenSpool, () => SelectedSpool != null);
            UnpackCommand = new DelegateCommand(Unpack, () => !IsNewGroupPack && ProductId != null && WorkSession.PlaceGroup == PlaceGroup.Wr);
            Spools = new AsyncObservableCollection<PaperBase>();
            Bars.Add(ReportManager.GetReportBar("GroupPacks", VMID));
            Messenger.Default.Register<DocChangedMessage>(this, DocChanged);
        }

        public DocProductGroupPackViewModel(Guid docID, bool docIsReadOnly) : this()
        {
            UpdateViewModel(docID, docIsReadOnly);
        }

        public void UpdateViewModel(Guid docID, bool docIsReadOnly)
        {
            StartInitialization();
            ManualWeightInput = !Scales.IsReady;
            CalculateIsReadOnly(docIsReadOnly);
            DocId = docID;
            var doc = GammaBase.Docs.Include(d => d.DocProduction.DocProductionProducts).FirstOrDefault(d => d.DocID == docID);
            //IsConfirmed = doc?.IsConfirmed ?? false;
            IsNewGroupPack = false;
            if (doc?.DocProduction.DocProductionProducts.Count > 0)
            {
                ProductId = doc.DocProduction.DocProductionProducts.Select(d => d.ProductID).First();
                var productGroupPack = GammaBase.ProductGroupPacks.FirstOrDefault(p => p.ProductID == ProductId);
                Weight = Convert.ToInt32((productGroupPack?.Weight > 10 ? productGroupPack.Weight : productGroupPack?.Weight*1000));
                GrossWeight = Convert.ToInt32((productGroupPack?.GrossWeight > 10 ? productGroupPack.GrossWeight : productGroupPack?.GrossWeight * 1000));
                _manualWeightInput = IsReadOnly ? productGroupPack?.ManualWeightInput ?? false : !Scales.IsReady || (productGroupPack?.ManualWeightInput ?? false);
            }
            Spools.Clear();
            var groupPackSpools = GammaBase.GroupPackSpools(docID).ToList();
            if (groupPackSpools.Count > 0)
            {
                BaseCoreWeight = DB.GetSpoolCoreWeight(groupPackSpools[0].ProductID);
                PlaceProductionid = groupPackSpools[0].PlaceID;
                foreach (var groupPackSpool in groupPackSpools)
                {
                    Spools.Add(new PaperBase()
                    {
                        ProductID = groupPackSpool.ProductID,
                        DocID = groupPackSpool.DocID,
                        NomenclatureID = (Guid)groupPackSpool.C1CNomenclatureID,
                        CharacteristicID = (Guid)groupPackSpool.C1CCharacteristicID,
                        Number = $"{groupPackSpool.Number} от {groupPackSpool.Date}",
                        Weight = groupPackSpool.Quantity??0,
                        Nomenclature = groupPackSpool.NomenclatureName,
                    });
                }
                NomenclatureID = Spools[0].NomenclatureID;
                CharacteristicID = Spools[0].CharacteristicID;
                ProductState = Spools[0].ProductState;
                Diameter = DB.GetSpoolDiameter(Spools[0].ProductID);
                CoreWeight = BaseCoreWeight * Spools.Count;
            }
            EndInitialization();
        }

        private void CalculateIsReadOnly(bool isReadOnly)
        {
            IsReadOnly = isReadOnly || !DB.HaveWriteAccess("ProductGroupPacks");
            RaisePropertiesChanged("IsReadOnly");
        }

        private void DocChanged(DocChangedMessage msg)
        {
            if (DocId != msg.DocId) return;
            CalculateIsReadOnly(msg.IsConfirmed);
        }

        private void Unpack()
        {
            if (ProductId == null) return;
            if (!GammaBase.Rests.Any(r => r.ProductID == @ProductId))
            {
                MessageBox.Show("Данная упаковка не числится на остатках");
                return;
            }
            if (
                MessageBox.Show("Вы уверены, что хотите распаковать данную упаковку?", "Распаковка",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                UIServices.SetBusyState();
                GammaBase.UnpackGroupPack(ProductId, WorkSession.PrintName);
                MessageBox.Show("Упаковка уничтожена, рулоны возвращены на остатки");
                IsUnpacked = true;
            }
        }
        
        /*
                private int Format { get; set; }
        */
        private bool IsNewGroupPack { get; set; }

        private void GetWeight()
        {
            DB.AddLogMessageInformation("Нажатие кнопки Получить вес в продукте ProductID",
                "Start GetWeight in DocProductGroupPackViewModel", DocId, ProductId);
            if (!Scales.IsReady)
            {
                Functions.ShowMessageError("Не удалось соединиться с весами",
                    "Error GetWeight in DocProductGroupPackViewModel: Failed to connect to scales", DocId, ProductId);
                ManualWeightInput = true;
                return;
            }
            UIServices.SetBusyState();
            var startTime = DateTime.Now;
            do
            {
                GrossWeight = (int)Scales.Weight;
                Thread.Sleep(500);
                if (!((DateTime.Now - startTime).TotalSeconds > 15)) continue;
                Functions.ShowMessageError("Вес не стабилизировался в течении 15 секунд",
                    "Error GetWeight in DocProductGroupPackViewModel: Weight did not stabilize within 15 seconds", DocId, ProductId);
                ManualWeightInput = true;
                break;
            } while (!Scales.IsStable);
//            var weight = Scales.Weight;
/*
            if (weight == null && Scales.ComPortError)
            {
                MessageBox.Show("Не удалось соедениться с весами", "Ошибка весов", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                ManualWeightInput = true;
                return;
            }
*/
        }

        public Guid? ProductId { get; set; }

        private Guid? DocId { get; set; }

        public bool IsAllowDelete = true;

        /*public bool IsConfirmed
        {
            get { return _isConfirmed; }
            set
            {
                _isConfirmed = value;
                RaisePropertiesChanged("IsReadOnly", "WeightIsReadOnly"); // Нужно для реакции интерфейса
            }
        }*/

        [UIAuth(UIAuthLevel.ReadOnly)]
        public bool ManualWeightInput
        {
            get { return _manualWeightInput; }
            set
            {
                if (!value || _manualWeightInput) return;
                _manualWeightInput = true;
                RaisePropertiesChanged("ManualWeightInput", "WeightIsReadOnly");
                if (!GetInitialization)
                {
                    SetIsChanged(true);
                    DB.AddLogMessageInformation("Изменен ручной ввод веса " + value.ToString() + " в ProductID",
                        "SET ManualWeightInput in DocProductGroupPackViewModel: value = " + value.ToString(), DocId, ProductId);
                }
            }
        }

        //public bool IsReadOnly { get; set; } //=> ((!DB.HaveWriteAccess("ProductGroupPacks") || IsConfirmed) && IsValid);

        private readonly Guid? _vmid = Guid.NewGuid();
        public Guid? VMID
        {
            get
            {
                return _vmid;
            }
        }

        private int _weight;
        private int _grossWeight;

        public bool WeightIsReadOnly => IsReadOnly || !ManualWeightInput;

        [Range(1,1000000,ErrorMessage=@"Значение должно быть больше 0")]
        public int Weight
        {
            get
            {
                return _weight;
            }
            set
            {
            	_weight = value;
                RaisePropertyChanged("Weight");
                if (!GetInitialization)
                {
                    SetIsChanged(true);
                    DB.AddLogMessageInformation("Изменен вес нетто " + value.ToString() + " в ProductID",
                        "SET Weight in DocProductGroupPackViewModel: value = " + value.ToString(), DocId, ProductId);
                }
            }
        }

        [Range(1, 1000000, ErrorMessage = @"Значение должно быть больше 0")]
        public int GrossWeight
        {
            get
            {
                return _grossWeight;
            }
            set
            {
            	_grossWeight = value;
                if (!GetInitialization)
                {
                    SetIsChanged(true);
                    DB.AddLogMessageInformation("Изменен вес брутто " + value.ToString() + " в ProductID",
                    "SET GrossWeight in DocProductGroupPackViewModel: value = " + value.ToString(), DocId, ProductId);
                }
                if (!IsReadOnly) Weight = GrossWeight - (int)Math.Ceiling(CoreWeight);
                RaisePropertyChanged("GrossWeight");
            }
        }
        private short _diameter;

//        [UIAuth(UIAuthLevel.ReadOnly)]
        public short Diameter
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
                    SetIsChanged(true);
                    DB.AddLogMessageInformation("Изменен диаметр " + value.ToString() + " в ProductID",
                    "SET Diameter in DocProductGroupPackViewModel: value = " + value.ToString(), DocId, ProductId);
                }
            }
        }
        private decimal _coreWeight;
        public decimal CoreWeight
        {
            get
            {
                return _coreWeight;
            }
            set
            {
            	_coreWeight = value;
                if (!GetInitialization)
                {
                    SetIsChanged(true);
                    DB.AddLogMessageInformation("Изменен вес гильзы " + value.ToString() + " в ProductID",
                    "SET CoreWeight in DocProductGroupPackViewModel: value = " + value.ToString(), DocId, ProductId);
                }
                if (!IsReadOnly) Weight = GrossWeight - (int) Math.Ceiling(CoreWeight);
                RaisePropertyChanged("CoreWeight");
            }
        }
        
        public DelegateCommand GetWeightCommand { get; private set; }
//        public DelegateCommand GetGrossWeightCommand { get; private set; }
        public DelegateCommand AddSpoolCommand { get; private set; }
        public DelegateCommand DeleteSpoolCommand { get; private set; }
        public DelegateCommand UnpackCommand { get; private set; }

        private bool IsUnpacked { get; set; } = false;

        private void AddSpool()
        {
            DB.AddLogMessageInformation("Выбор меню: Добавить тамбур в ГУ ProductID", "Add product in DocProductGroupPackViewModel", DocId, ProductId);
            MessageManager.OpenFindProduct(ProductKind.ProductSpool, true);
            Messenger.Default.Register<ChoosenProductMessage>(this, AddChoosenSpool);
        }
        private void DeleteSpool()
        {
            SetIsChanged(true);
            DB.AddLogMessageInformation("Выбор меню: Удалить продукт в ГУ ProductID", "Delete product in DocProductGroupPackViewModel: SelectedSpool?.DocID = " + SelectedSpool?.DocID + ", SelectedSpool?.ProductID = " + SelectedSpool?.ProductID, DocId, ProductId);
            Spools.Remove(SelectedSpool);
            CoreWeight = BaseCoreWeight * Spools.Count;
        }
        private void AddChoosenSpool(ChoosenProductMessage msg)
        {
            Messenger.Default.Unregister(this);
            if (!CheckAddedSpool(msg.ProductID)) return;
            var spool = (from p in GammaBase.vProductsInfo where p.ProductID == msg.ProductID
                             select p).ToList().Select( p => new PaperBase()
                             {
                                 PlaceProductionid = p.PlaceID,
                                 DocID = p.DocID,
                                 CharacteristicID = (Guid)p.C1CCharacteristicID,
                                 NomenclatureID = (Guid)p.C1CNomenclatureID,
                                 ProductState = (ProductState)p.StateID,
                                 Nomenclature = p.NomenclatureName,
                                 ProductID = p.ProductID,
                                 Weight = (int)(1000 * Decimal.Round(p.Quantity ?? 0, 3)),
                                 Number = p.Number + " от " + p.Date
                             }).FirstOrDefault();
            AddSpoolIfCorrect(spool);
        }

        /// <summary>
        /// Добавление рулона по штрих-коду
        /// </summary>
        /// <param name="barcode">штрих-код</param>
        public void AddSpool(string barcode)
        {
            DB.AddLogMessageInformation("Добавление тамбура через сканирование ШК в ГУ ProductID", "Add product with scan barcode in DocProductGroupPackViewModel: barcode = '"+barcode+"'", DocId, ProductId);
            var p = (from pinf in GammaBase.vProductsInfo
                         where pinf.BarCode == barcode
                         select pinf).FirstOrDefault();
            if (p == null) return;
            if (!CheckAddedSpool(p.ProductID)) return;
            var Quantity = p.Quantity;
            if (p.IsWrittenOff ?? false)
            {
                if (GammaBase.vGroupPackSpools.Where(gs => gs.ProductID == p.ProductID)
                .Join(GammaBase.vProductsInfo,
                    gs => gs.ProductGroupPackID,
                    pi => pi.ProductID,
                    (gs, pi) => new { IsWrittenOff = pi.IsWrittenOff ?? false }
                ).Any(j => !j.IsWrittenOff) ) 
                {
                    if (!UnpackGroupPack(p.ProductID)) return;
                    Quantity = (from pinf in GammaBase.vProductsInfo
                                          where pinf.BarCode == barcode
                                          select pinf.Quantity).FirstOrDefault();
                }
                else
                {
                    Functions.ShowMessageError("Вы пытаететесь добавить списанный рулон", 
                        "Error add product with scan barcode in DocProductGroupPackViewModel: spool is worked", DocId, ProductId);

                    return;
                }
            }
            var spool = new PaperBase()
                             {
                                 PlaceProductionid = p.PlaceID,
                                 DocID = p.DocID,
                                 CharacteristicID = (Guid)p.C1CCharacteristicID,
                                 NomenclatureID = (Guid)p.C1CNomenclatureID,
                                 ProductState = (ProductState)p.StateID,
                                 Nomenclature = p.NomenclatureName,
                                 ProductID = p.ProductID,
                                 Weight = (int)(1000 * Decimal.Round((Quantity ?? p.Quantity) ?? 0, 3)),
                                 Number = $"{p.Number} от {p.Date}"
                             };
            AddSpoolIfCorrect(spool);
        }

        /// <summary>
        /// Распаковка упаковки, в которой находится рулон
        /// </summary>
        /// <param name="productId">ИД рулона</param>
        /// <returns></returns>
        private bool UnpackGroupPack(Guid productId)
        {
            DB.AddLogMessageInformation("Распаковка упаковки (если найденный рулон находится в ГУ)", "UnpackGroupPack in DocProductGroupPackViewModel: productID = " + productId, DocId, ProductId);
            using (var gammaBase = DB.GammaDb)
            {
                var groupPack = gammaBase.vGroupPackSpools.Where(gs => gs.ProductID == productId).Join(gammaBase.Products,
                            gs => gs.ProductGroupPackID,
                            p => p.ProductID,
                            (gs, p) => new { ProductId = p.ProductID, Number = p.Number }).Join(gammaBase.Rests,
                            p => p.ProductId,
                            r => r.ProductID,
                            (p, r) => new { ProductId = r.ProductID, Number = p.Number }).FirstOrDefault();
                if (groupPack == null)
                {
                    Functions.ShowMessageError("Выбранный рулон в упаковке, но при поиске упаковки произошел сбой",
                        "Error UnpackGroupPack in DocProductGroupPackViewModel: spool in group pack, but the search for the package failed - productId = " + productId, DocId, ProductId);
                    return false;
                }
                var dlgResult =
                Functions.ShowMessageQuestion($"Выбранный тамбур находится в упаковке {groupPack.Number}. Данная упаковка будет распакована. Вы согласны?",
                        "QUEST UnpackGroupPack in DocProductGroupPackViewModel: productId = " + groupPack.ProductId, DocId, ProductId);
                if (dlgResult != MessageBoxResult.Yes) return false;
                gammaBase.UnpackGroupPack(groupPack.ProductId, WorkSession.PrintName);
                MessageBox.Show($"Упаковка {groupPack.Number} уничтожена");
                return true;
            }
        }

        private bool CheckAddedSpool(Guid productId)
        {
            if (Spools.Count <= 0) return true;
            var result = GammaBase.CheckAddedGroupPackSpool(Spools.First().ProductID, productId).FirstOrDefault();
            if (string.IsNullOrEmpty(result)) return true;
            Functions.ShowMessageError(result, "Error CheckAddedSpool in DocProductGroupPackViewModel: productId = " + productId, DocId, ProductId);
            return false;
        }

        private decimal BaseCoreWeight { get; set; }

        private void AddSpoolIfCorrect(PaperBase spool)
        {
            SetIsChanged(true);
            if (Spools.Count == 0)
            {
                PlaceProductionid = spool.PlaceProductionid;
                NomenclatureID = spool.NomenclatureID;
                CharacteristicID = spool.CharacteristicID;
                ProductState = spool.ProductState;
                BaseCoreWeight = DB.GetSpoolCoreWeight(spool.ProductID);
                Diameter = DB.GetSpoolDiameter(spool.ProductID);
                Spools.Add(spool);
                CoreWeight = BaseCoreWeight * Spools.Count;
                GrossWeight = (int)Math.Ceiling(CoreWeight) + (int)Spools.Sum(sp => sp.Weight);
            }
            else
            {
                if (spool.PlaceProductionid != PlaceProductionid)
                {
                    Functions.ShowMessageError("Нельзя упаковывать рулоны с разных переделов",
                        "Error AddSpoolIfCorrect in DocProductGroupPackViewModel: other place - productId = " + spool.ProductID, DocId, ProductId);
                }
                else if (spool.NomenclatureID != NomenclatureID || spool.CharacteristicID != CharacteristicID)
                {
                    Functions.ShowMessageError("Номенклатура рулона не совпадает с номенклатурой ГУ",
                        "Error AddSpoolIfCorrect in DocProductGroupPackViewModel: Spool nomenclature != GroupPack nomenclature - productId = " + spool.ProductID, DocId, ProductId);
                }
                else if (Spools.Select(s => s.ProductID).Contains(spool.ProductID))
                {
                    Functions.ShowMessageError("Данный рулон уже добавлен",
                        "Error AddSpoolIfCorrect in DocProductGroupPackViewModel: this place has already been added - productId = " + spool.ProductID, DocId, ProductId);
                }
                else
                {
                    Spools.Add(spool);
                    CoreWeight = BaseCoreWeight * Spools.Count;
                    GrossWeight = (int)Math.Ceiling(CoreWeight) + (int)Spools.Sum(sp => sp.Weight);
                }
            }
        }
        private int? PlaceProductionid { get; set; }
        private Guid NomenclatureID {get; set; }
        private Guid CharacteristicID { get; set; }
        private ProductState ProductState { get; set; }
        private ObservableCollection<PaperBase> _spools;
        public ObservableCollection<PaperBase> Spools
        {
            get
            {
                return _spools;
            }
            set
            {
            	_spools = value;
                RaisePropertyChanged("Spools");
            }
        }

        public bool IsValidProduct => ValidateProduct();

        public bool ValidateProduct()
        {
            var result = GammaBase.ValidateGroupPackBeforeSave(NomenclatureID, CharacteristicID, Diameter, Weight, Spools.Count, Spools.FirstOrDefault().ProductID).FirstOrDefault();
            if (!string.IsNullOrEmpty(result))
                {
                    Functions.ShowMessageInformation(result, "ValidateGroupPackBeforeSave", DocId);
                    return false;
                }
            return true;
        }

        /// <summary>
        /// Сохранение в базу
        /// </summary>
        /// <param name="itemID">ID документа</param>
        /// <param name="gammaBase">Контекст БД</param>
        public override bool SaveToModel(Guid itemID)
        {
            DB.AddLogMessageInformation("Начало сохранения продукта групповая упаковка", "Start SaveToModel in DocProductGroupPackViewModel", DocId, ProductId);
            //if (!IsValid || IsUnpacked || IsReadOnly) return true;
            if (!IsChanged)
            {
                DB.AddLogMessageInformation("Успешный выход без сохранения продукта групповая упаковка", "Quit successed from SaveToModel in DocProductGroupPackViewModel: IsChanged = " + IsChanged.ToString(), DocId);
                return true;
            }
            if (!IsValidProduct)
            {
                DB.AddLogMessageInformation("Ошибочный выход без сохранения продукта групповая упаковка", "Quit failed from SaveToModel in DocProductGroupPackViewModel: IsValidProduct = " + IsValidProduct.ToString(), DocId);
                return false;
            }
            DocId = itemID;
            if (Weight != Spools.Sum(s => s.Weight))
            {
                Functions.ShowMessageInformation("Внимание! Вес групповой упаковки не совпадает с общим весом катушек внутри!", "Sum Spools Weight != GroupPack Weight", DocId, ProductId);
                return false;
            }
            var doc = GammaBase.Docs.Include(d => d.DocProduction).Include(d => d.DocProduction.DocWithdrawal).Include(d => d.DocProduction.DocProductionProducts)
                .Where(d => d.DocID == itemID).Select(d => d).First();
            var product = GammaBase.Products.Include(p => p.ProductGroupPacks).Include(p => p.DocProductionProducts)
                .FirstOrDefault(p => p.DocProductionProducts.FirstOrDefault().DocID == itemID);
            if (doc.DocProduction == null)
                doc.DocProduction = new DocProduction()
                {
                    DocID = doc.DocID,
                    InPlaceID = doc.PlaceID,
                    DocProductionProducts = new List<DocProductionProducts>()
                };
            if (product == null)
            {
                var productId = SqlGuidUtil.NewSequentialid();
                product = new Products()
                {
                    ProductID = productId,
                    ProductKindID = (byte)ProductKind.ProductGroupPack,
                    ProductGroupPacks = new ProductGroupPacks(),
                    DocProductionProducts = new List<DocProductionProducts>()
                    {
                        new DocProductionProducts()
                        {
                            DocID = doc.DocID,
                            ProductID = productId,
                        }
                    }
                };
            }
            if (product.ProductGroupPacks == null) product.ProductGroupPacks = new ProductGroupPacks() { ProductID = product.ProductID };
            if (product.DocProductionProducts == null)
            {
                product.DocProductionProducts = new List<DocProductionProducts>
                {
                    new DocProductionProducts
                    {
                        ProductID = product.ProductID,
                        DocID = itemID,
                    }
                };
            }
            var docProductionProduct = product.DocProductionProducts.FirstOrDefault();
            if (docProductionProduct != null)
            {
                docProductionProduct.Quantity = (decimal) Weight/1000;
                docProductionProduct.C1CNomenclatureID = NomenclatureID;
                docProductionProduct.C1CCharacteristicID = CharacteristicID;
            }
            product.StateID = (byte) ProductState;
            product.ProductGroupPacks.C1CNomenclatureID = NomenclatureID;
            product.ProductGroupPacks.C1CCharacteristicID = CharacteristicID;
            product.ProductGroupPacks.Weight = (decimal)Weight/1000;
            product.ProductGroupPacks.GrossWeight = (decimal)GrossWeight/1000;
            product.ProductGroupPacks.Diameter = Diameter;
            product.ProductGroupPacks.ManualWeightInput = ManualWeightInput;
            var docWithdrawal = new Docs();
            if (doc.DocProduction.DocWithdrawal.Count > 0)
            {
                var docWithdrawalId = doc.DocProduction.DocWithdrawal.FirstOrDefault().DocID;
                docWithdrawal = GammaBase.Docs.Include(d => d.DocWithdrawal)
                    .Include(d => d.DocWithdrawal.DocWithdrawalProducts).First(d => d.DocID == docWithdrawalId);
            }
            else 
            {
                docWithdrawal.DocID = SqlGuidUtil.NewSequentialid();
                docWithdrawal.PlaceID = WorkSession.PlaceID;
                docWithdrawal.Date = DB.CurrentDateTime;
                docWithdrawal.UserID = WorkSession.UserID;
                docWithdrawal.ShiftID = WorkSession.ShiftID;
                docWithdrawal.PrintName = WorkSession.PrintName;
                docWithdrawal.DocTypeID = (byte)DocTypes.DocWithdrawal;
                docWithdrawal.IsConfirmed = doc.IsConfirmed;
                var docProductions = new ObservableCollection<DocProduction> {doc.DocProduction};
                docWithdrawal.DocWithdrawal = new DocWithdrawal
                {
                    DocID = docWithdrawal.DocID,
                    OutPlaceID = docWithdrawal.PlaceID,
                    DocProduction = docProductions,
                    DocWithdrawalProducts = new ObservableCollection<DocWithdrawalProducts>()
                };
                GammaBase.Docs.Add(docWithdrawal);
            }
            docWithdrawal.DocWithdrawal.DocWithdrawalProducts.Clear();
            var countUnweightedSpools = Spools?.Where(s => s.Weight == 1).Count();
            if (countUnweightedSpools != null && countUnweightedSpools > 0)
            {
                var quantityWeightedSpools = Spools.Where(s => s.Weight != 1).Sum(s => s.Weight);
                var quantityUnweightedSpool = (int)((Weight - quantityWeightedSpools) / countUnweightedSpools);
                var quantityLastUnweightedSpool = (int)(quantityUnweightedSpool + (Weight - quantityWeightedSpools - (quantityUnweightedSpool * countUnweightedSpools)));
                var lastUnweightedSpool = Spools.Where(s => s.Weight == 1).Last();
                var resultValidateUnweightedSpool = GammaBase.ValidateSpoolBeforeSaveInGroupPack(quantityLastUnweightedSpool, lastUnweightedSpool.ProductID).FirstOrDefault();
                if (!string.IsNullOrEmpty(resultValidateUnweightedSpool))
                {
                    Functions.ShowMessageInformation(resultValidateUnweightedSpool, "ValidateSpoolBeforeSaveInGroupPack", DocId, ProductId);
                    return false;
                }
                lastUnweightedSpool.Weight = (decimal)quantityLastUnweightedSpool;
                if (countUnweightedSpools > 1)
                {
                    resultValidateUnweightedSpool = GammaBase.ValidateSpoolBeforeSaveInGroupPack(quantityUnweightedSpool, Spools.Where(s => s.Weight == 1).FirstOrDefault().ProductID).FirstOrDefault();
                    if (!string.IsNullOrEmpty(resultValidateUnweightedSpool))
                    {
                        Functions.ShowMessageInformation(resultValidateUnweightedSpool, "ValidateSpoolBeforeSaveInGroupPack", DocId, ProductId);
                        return false;
                    }
                    foreach (var spool in Spools.Where(s => s.Weight == 1))
                        spool.Weight = quantityUnweightedSpool;
                }
            }
            foreach (var spool in Spools)
            {
                docWithdrawal.DocWithdrawal.DocWithdrawalProducts.Add(new DocWithdrawalProducts()
                {
                    DocID = docWithdrawal.DocID,
                    ProductID = spool.ProductID,
                    Quantity = spool.Weight / 1000,// product.ProductGroupPacks.Weight/Spools.Count,
                    CompleteWithdrawal = true
                });
            }
            GammaBase.SaveChanges();
            SetIsChanged(false);
            DB.AddLogMessageInformation("Успешное окончание сохранения групповая упаковка", "End SaveToModel in DocProductGroupPackViewModel: success", DocId, ProductId);
            return true;
        }
        // ReSharper disable once MemberCanBePrivate.Global
        public PaperBase SelectedSpool { get; set; }

        private ObservableCollection<BarViewModel> _bars = new ObservableCollection<BarViewModel>();
        //private bool _isConfirmed;
        private bool _manualWeightInput;

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
        public DelegateCommand OpenSpoolCommand { get; private set; }
        private void OpenSpool()
        {
            if (SelectedSpool == null) return;
            MessageManager.OpenDocProduct(DocProductKinds.DocProductSpool, SelectedSpool.ProductID);  
        }
        public override bool IsValid => base.IsValid && Spools.Count > 0;
    }
}
