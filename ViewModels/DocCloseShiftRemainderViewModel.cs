﻿// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using Gamma.Interfaces;
using Gamma.Attributes;
using System.Data.Entity;
using Gamma.Entities;

namespace Gamma.ViewModels
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public sealed class DocCloseShiftRemainderViewModel : DbEditItemWithNomenclatureViewModel, ICheckedAccess
    {
        private PlaceGroup _placeGroup;

        /// <summary>
        /// Initializes a new instance of the DocCloseShiftRemainderViewModel class.
        /// </summary>
        public DocCloseShiftRemainderViewModel(GammaEntities gammaDb = null)
        {
            GammaBase = gammaDb ?? DB.GammaDb;
            using (var gammaBase = DB.GammaDb)
            {
                var lastProductionProduct =
                    gammaBase.Docs.Where(
                        d =>
                            d.PlaceID == WorkSession.PlaceID && d.ShiftID == WorkSession.ShiftID &&
                            d.DocTypeID == (int)DocTypes.DocProduction)
                        .OrderByDescending(d => d.Date)
                        .FirstOrDefault()?.DocProduction.DocProductionProducts.FirstOrDefault();
                if (lastProductionProduct == null) return;
                var pinfo = gammaBase.vProductsInfo.First(p => p.ProductID == lastProductionProduct.ProductID);
                NomenclatureID = pinfo.C1CNomenclatureID;
                CharacteristicID = pinfo.C1CCharacteristicID;
                PlaceGroup = (PlaceGroup)gammaBase.Places.First(p => p.PlaceID == WorkSession.PlaceID).PlaceGroupID;
            }
            Coefficient = GetCoefficient(PlaceGroup, (Guid)CharacteristicID);
        }

        public DocCloseShiftRemainderViewModel(Guid docID, GammaEntities gammaDb = null)
        {
            GammaBase = gammaDb ?? DB.GammaDb;
            using (var gammaBase = DB.GammaDb)
            {
                DocCloseShiftRemainder = gammaBase.DocCloseShiftRemainders.Include(dr => dr.DocCloseShifts).Where(d => d.DocID == docID && (!d.IsSourceProduct ?? true)).
                Select(d => d).FirstOrDefault();
                if (DocCloseShiftRemainder == null)
                {
                    var doc = GammaBase.Docs.Include(d => d.Places).First(d => d.DocID == docID);
                    IsConfirmed = doc.IsConfirmed;
                    PlaceGroup = (PlaceGroup)doc.Places.PlaceGroupID;
                    return;
                }
                IsConfirmed = DocCloseShiftRemainder.DocCloseShifts.IsConfirmed;
                var pinfo = gammaBase.vProductsInfo.FirstOrDefault(p => p.ProductID == DocCloseShiftRemainder.ProductID);
                if (pinfo == null) return;
                NomenclatureID = pinfo.C1CNomenclatureID;
                CharacteristicID = pinfo.C1CCharacteristicID;
                PlaceGroup = (PlaceGroup)gammaBase.Places.First(p => p.PlaceID == DocCloseShiftRemainder.DocCloseShifts.PlaceID).PlaceGroupID;
                Coefficient = GetCoefficient(PlaceGroup, (Guid) CharacteristicID);
                Quantity = DocCloseShiftRemainder.Quantity / Coefficient;
            }
        }

        public string RemainderLabel { get; set; }

        private decimal GetCoefficient(PlaceGroup placeGroup, Guid characteristicId)
        {
            var coefficient = 1M;
            switch (placeGroup)
            {
                case PlaceGroup.PM:
                    coefficient = 1M;
                    break;
                case PlaceGroup.Convertings:
                    using (var gammaBase = DB.GammaDb)
                    {
                        coefficient =
                        gammaBase.C1CCharacteristics.First(c => c.C1CCharacteristicID == characteristicId)
                            .C1CMeasureUnitsPackage.Coefficient ?? 1;
                        break;
                    }
            }
            return coefficient;
        }

        [UIAuth(UIAuthLevel.ReadOnly)]
        public override Guid? CharacteristicID
        {
            get { return base.CharacteristicID; }
            set
            {
                base.CharacteristicID = value;
                Coefficient = GetCoefficient(PlaceGroup, (Guid)CharacteristicID);
            }
        }

        private decimal Coefficient { get; set; }

        private PlaceGroup PlaceGroup
        {
            get { return _placeGroup; }
            set
            {
                _placeGroup = value;
                switch (PlaceGroup)
                {
                    case PlaceGroup.PM:
                        RemainderLabel = "Остаток на накате, кг";
                        break;
                    case PlaceGroup.Convertings:
                        RemainderLabel = "Передано упаковок следующей смене";
                        break;
                }
            }
        }

        private bool IsConfirmed { get; }
        private DocCloseShiftRemainders DocCloseShiftRemainder { get; set; }

        [UIAuth(UIAuthLevel.ReadOnly)]
        public decimal Quantity { get; set; }
        protected override bool CanChooseNomenclature()
        {
            return base.CanChooseNomenclature() && DB.HaveWriteAccess("ProductSpools") && !IsConfirmed;
        }

        public override bool SaveToModel(Guid itemID)
        {
            using (var gammaBase = DB.GammaDb)
            {
                var doc = gammaBase.Docs.First(d => d.DocID == itemID);
                if (DocCloseShiftRemainder == null && Quantity > 0)
                {
                    var productId = SqlGuidUtil.NewSequentialid();
                    switch (PlaceGroup)
                    {
                        case PlaceGroup.PM:
                            var productSpool = new Products()
                            {
                                ProductID = productId,
                                ProductKindID = (byte)ProductKind.ProductSpool,
                                ProductSpools = new ProductSpools()
                                {
                                    C1CCharacteristicID = (Guid)CharacteristicID,
                                    C1CNomenclatureID = (Guid)NomenclatureID,
                                    Diameter = 0,
                                    DecimalWeight = 0,
                                    ProductID = productId
                                }
                            };
                            gammaBase.Products.Add(productSpool);
                            break;
                        case PlaceGroup.Convertings:
                            var productPallet = new Products()
                            {
                                ProductID = productId,
                                ProductKindID = (byte)ProductKind.ProductPallet,
                                ProductPallets = new ProductPallets
                                {
                                    ProductID = productId,
                                    ProductPalletItems = new List<ProductPalletItems>()
                                {
                                    new ProductPalletItems
                                    {
                                        C1CCharacteristicID = (Guid)CharacteristicID,
                                        C1CNomenclatureID = (Guid)NomenclatureID,
                                        ProductID = productId,
                                        ProductPalletItemID = SqlGuidUtil.NewSequentialid()
                                    }
                                }
                                }
                            };
                            gammaBase.Products.Add(productPallet);
                            break;

                    }
                    var docID = SqlGuidUtil.NewSequentialid();
                    var docProductionProducts = new ObservableCollection<DocProductionProducts>
                {
                    new DocProductionProducts()
                    {
                        DocID = docID,
                        ProductID = productId,
                        C1CNomenclatureID = NomenclatureID,
                        C1CCharacteristicID = CharacteristicID
                    }
                };
                    var docProduction = new Docs()
                    {
                        DocID = docID,
                        Date = DB.CurrentDateTime,
                        PlaceID = doc.PlaceID,
                        DocTypeID = (byte)DocTypes.DocProduction,
                        DocProduction = new DocProduction()
                        {
                            DocID = docID,
                            InPlaceID = doc.PlaceID,
                            DocProductionProducts = docProductionProducts
                        }
                    };
                    gammaBase.Docs.Add(docProduction);
                    DocCloseShiftRemainder = new DocCloseShiftRemainders()
                    {
                        DocCloseShiftRemainderID = SqlGuidUtil.NewSequentialid(),
                        DocID = itemID,
                        ProductID = productId,
                        Quantity = Quantity * Coefficient,
                        IsSourceProduct = false
                    };
                    gammaBase.DocCloseShiftRemainders.Add(DocCloseShiftRemainder);
                }
                else if (DocCloseShiftRemainder != null)
                {
                    DocCloseShiftRemainder.Quantity = Quantity * Coefficient;
                    if (
                        gammaBase.DocProductionProducts.Any(
                            d => d.ProductID == DocCloseShiftRemainder.ProductID && d.DocProduction.Docs.ShiftID == null))
                    {
                        var docProductionProduct =
                            gammaBase.DocProductionProducts.Include(d => d.Products.ProductSpools).Include(d => d.Products.ProductPallets.ProductPalletItems)
                                .FirstOrDefault(d => d.ProductID == DocCloseShiftRemainder.ProductID);
                        if (docProductionProduct != null && NomenclatureID != null)
                        {
                            docProductionProduct.C1CNomenclatureID = NomenclatureID;
                            docProductionProduct.C1CCharacteristicID = CharacteristicID;
                            switch (PlaceGroup)
                            {
                                case PlaceGroup.PM:
                                    docProductionProduct.Products.ProductSpools.C1CNomenclatureID = (Guid)NomenclatureID;
                                    docProductionProduct.Products.ProductSpools.C1CCharacteristicID = (Guid)CharacteristicID;
                                    break;
                                case PlaceGroup.Convertings:
                                    var productPalletItem =
                                        docProductionProduct.Products.ProductPallets.ProductPalletItems.FirstOrDefault();
                                    if (productPalletItem != null)
                                    {
                                        productPalletItem.C1CNomenclatureID = (Guid)NomenclatureID;
                                        productPalletItem.C1CCharacteristicID = (Guid)CharacteristicID;
                                    }
                                    break;
                            }

                        }
                    }
                }
                gammaBase.SaveChanges();
            }
            return true;
        }

        public bool IsReadOnly => !DB.HaveWriteAccess("DocCloseShiftRemainders") && !WorkSession.DBAdmin || IsConfirmed;
    }
}