﻿// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Collections.Generic;
using DevExpress.Mvvm;
using Gamma.Common;
using Gamma.Entities;
using Gamma.Models;

namespace Gamma
{
    public class ContextMessage
    {
        public ContextMessage(GammaEntities gammaBase = null)
        {
            GammaBase = gammaBase;
        }

        public GammaEntities GammaBase { get; set; }
    }

    public class RefreshMessage { }

    public class RefreshBrokeListMessage { }

    public class BaseReconnectedMessage { }
    public class OpenFilterDateMessage { }

    public class SpoolWithdrawed { }

    public class PrintReportMessage:BarCommandParameter
    {
        public Guid ReportID { get; set; } // id отчета
        public Guid? VMID { get; set; } // id ViewModel, которая должна обработать событие
    }

    public class OpenQualityReportPMMessage { }

    public class DocChangedMessage
    {
        public DocChangedMessage(Guid docId, bool isConfirmed)
        {
            DocId = docId;
            IsConfirmed = isConfirmed;
        }

        public Guid DocId { get; private set; }
        public bool IsConfirmed { get; private set; }

    }

    public class EditDocMovementOrderMessage
    {
        public EditDocMovementOrderMessage(Guid? docId = null)
        {
            DocId = docId;
        }

        public Guid? DocId { get; private set; }
    }

    public class OpenPlaceZonesMessage { }

    public class OpenDocWithdrawalMessage
    {
        public OpenDocWithdrawalMessage(Guid docId)
        {
            DocId = docId;
        }

        public Guid DocId { get; private set; }
    }

    public class OpenDocInventarisationMessage
    {
        public OpenDocInventarisationMessage(Guid docId)
        {
            DocId = docId;
        }

        public Guid DocId { get; private set; }
    }

    public class EditDocMovementMessage
    {
        public EditDocMovementMessage(Guid docId)
        {
            DocId = docId;
        }

        public Guid DocId { get; private set; }
    }

    public class EditDocRepackMessage
    {
        public EditDocRepackMessage(Guid? docId)
        {
            DocId = docId;
        }

        public Guid? DocId { get; private set; }
    }

    public class EditDocComplectationMessage
	{
		public EditDocComplectationMessage(Guid docId)
		{
			DocId = docId;
		}

		public Guid DocId { get; private set; }
	}

    public class NomenclatureEditMessage
    {
        public NomenclatureEditMessage(Guid nomenclatureId)
        {
            NomenclatureId = nomenclatureId;
        }

        public Guid NomenclatureId { get; }
    }

    public class EditRejectionReasonsMessage
    {
        public EditRejectionReasonsMessage(BrokeProduct product)
        {
            BrokeProduct = product;
        }
        
        public BrokeProduct BrokeProduct { get; set; }
    }

    public class OpenDocBrokeMessage
    {
        public OpenDocBrokeMessage(Guid docId, Guid? productId = null, bool isInFuturePeriod = false)
        {
            DocId = docId;
            ProductId = productId;
            IsInFuturePeriod = isInFuturePeriod;
        }

        public Guid DocId { get; private set; }
        public Guid? ProductId { get; private set; }
        public bool IsInFuturePeriod { get; private set; }
    }

    public class CloseMessage { }
 
    public class OpenMainMessage { }

    public class OpenProductionTaskBatchMessage 
    {
        public OpenProductionTaskBatchMessage(BatchKinds batchKind)
        {
            BatchKind = batchKind;
        }

        public OpenProductionTaskBatchMessage(BatchKinds batchKind, Guid productionTaskBatchId, bool isWindow = true)
        {
            BatchKind = batchKind;
            ProductionTaskBatchID = productionTaskBatchId;
            Window = isWindow;
        }

        public BatchKinds BatchKind { get; private set; }
        public Guid? ProductionTaskBatchID { get; private set; }
        public bool Window { get; private set; } = true;
    }
    public class ProductChangedMessage
    {
        public ProductChangedMessage(Guid productId)
        {
            ProductID = productId;
        }

        public Guid ProductID { get; private set; }
    }

    public class OpenDocShipmentOrderMessage
    {
        public OpenDocShipmentOrderMessage(Guid docShipmentOrderId)
        {
            DocShipmentOrderId = docShipmentOrderId;
        }
        public Guid DocShipmentOrderId { get; set; }
    }

    /// <summary>
    /// Сообщение для открытия формы редактирования продукции
    /// </summary>
    public class OpenDocProductMessage
    {
        public OpenDocProductMessage(DocProductKinds docProductKind, bool isNewProduct = false, Guid? Id = null,
            SourceSpoolsCheckResult checkResult = SourceSpoolsCheckResult.Correct)
        {
            DocProductKind = docProductKind;
            ID = Id;
            IsNewProduct = isNewProduct;
            CheckResult = checkResult;
        }
        /// <summary>
        /// Вид продукции
        /// </summary>
        public DocProductKinds DocProductKind { get; private set; }
        /// <summary>
        /// Идентификатор продукции (для Unload - DocID, для остальных ProductId)
        /// </summary>
        public Guid? ID { get; private set; }

        /// <summary>
        /// Создание нового продукта
        /// </summary>
        public bool IsNewProduct { get; private set; }
        public SourceSpoolsCheckResult CheckResult { get; private set; }
    }
    public class OpenNomenclatureMessage  
    {
        public OpenNomenclatureMessage(int? placeGroupID, bool nomenclatureEdit = false)
        {
            ID = placeGroupID;
            IsPlaceGroupFilter = true;
            NomenclatureEdit = nomenclatureEdit;
        }

        public OpenNomenclatureMessage(MaterialType materialType, bool nomenclatureEdit = false)
        {
            ID = (int) materialType;
            NomenclatureEdit = nomenclatureEdit;
        }

        public bool NomenclatureEdit { get; }
        public int? ID { get; private set; }
        public bool IsPlaceGroupFilter { get; private set; }
    }
    public class ProductionTaskRwMessage
    {
        public ProductionTaskRwMessage(Guid productionTaskBatchID, List<Nomenclature> nomenclatures)
        {
            ProductionTaskBatchID = productionTaskBatchID;
            Nomenclatures = nomenclatures;
        }

        public ProductionTaskRwMessage(Guid productionTaskBatchID, DateTime dateBegin)
        {
            ProductionTaskBatchID = productionTaskBatchID;
            DateBegin = dateBegin;
        }

        public Guid ProductionTaskBatchID;
        public List<Nomenclature> Nomenclatures { get; set; }
        
        public DateTime? DateBegin;

    }
    public class Nomenclature1CMessage
    {
        public Guid Nomenclature1CID { get; set; }
    }
    public class OpenReportListMessage { }
    public class FindProductMessage
    {
        public bool ChooseProduct;
        public ProductKind ProductKind;
        public bool AllowChangeProductKind = true;
        public bool AllowChooseOneValueOnly = true;
        public BatchKinds? BatchKind;
        public List<int> CurrentPlaces;
        public bool AllowChangeCurrentPlaces = true;
    }
    public class ConfigureComPortMessage  { }
    public class ChoosenProductMessage
    {
        public Guid ProductID;
        public List<Guid> ProductIDs;
    }
    public class BarcodeMessage
    {
        public string Barcode;
    }
    public class PermitEditMessage:ContextMessage
    {
        public PermitEditMessage(GammaEntities gammaBase): base(gammaBase) { }

        public PermitEditMessage(Guid permitId, GammaEntities gammaBase = null) : base(gammaBase)
        {
            PermitID = permitId;
        }
        public Guid? PermitID { get; private set; }
    }
    public class RoleEditMessage: ContextMessage
    {
        public RoleEditMessage(GammaEntities gammaBase = null): base(gammaBase) { }

        public RoleEditMessage(Guid roleID, GammaEntities gammaBase = null)
        {
            GammaBase = gammaBase;
            RoleID = roleID;
        }

        public Guid? RoleID;
    }
    public class UserEditMessage: ContextMessage
    {
        public UserEditMessage(Guid? userId, GammaEntities gammaBase = null)
        {
            UserID = userId;
            GammaBase = gammaBase;
        }

        public UserEditMessage(GammaEntities gammaBase = null)
        {
            GammaBase = gammaBase;
        }

        public Guid? UserID;
    }
    public class UserChangedMessage  { }
    public class PermitChangedMessage  { }
    public class RoleChangedMessage  { }
    
    public class OpenManageUsersMessage { }
    public class OpenDocCloseShiftMessage
    {
        public Guid? DocID;
        public int? PlaceID;
        public DateTime? CloseDate;
        public byte? ShiftID;
        public Guid? PersonID;
    }

    public class OpenDocUnwinderRemainderMessage
    {
        public Guid? DocID;
        public int? PlaceID;
        public DateTime? CloseDate;
        public byte? ShiftID;
        public Guid? PersonID;
    }

    public class OpenPlaceGroupsNomenclatureMessage  { }
    public class OpenMaterialTypesNomenclatureMessage { }
    public class OpenWarehousePersonsMessage { }
    public class OpenDowntimeTemplatesMessage { }
    public class OpenImportOldProductsMessage { }
    public class OpenDocCloseShiftsMessage 
    {
        public PlaceGroup? PlaceGroup;
    }

    public class OpenDocUnwinderRemaindersMessage
    {
        public PlaceGroup? PlaceGroup;
    }

    public class FindProductionTaskBatchMessage
    {
        public BatchKinds BatchKind;
    }

    public class OpenLogEventMessage
    {
        public OpenLogEventMessage(Guid eventID, Guid? parentEventID)
        {
            EventID = eventID;
            ParentEventID = parentEventID;
        }
        public Guid EventID { get; private set; }
        public Guid? ParentEventID { get; private set; }
    }

    public class RecalcQuantityEndFromUnwinderReaminderMessage
    {
        public RecalcQuantityEndFromUnwinderReaminderMessage(Guid? productID, Guid? nomenclatureID, Guid? characteristicID, decimal quantity, decimal? delta)
        {
            ProductID = productID;
            NomenclatureID = nomenclatureID;
            CharacteristicID = characteristicID;
            Quantity = quantity;
            Delta = delta;
        }
        public Guid? ProductID { get; private set; }
        public Guid? NomenclatureID { get; private set; }
        public Guid? CharacteristicID { get; private set; }
        public decimal Quantity { get; private set; }
        public decimal? Delta { get; private set; }
    }

    public class RecalcQuantityFromTankReaminderMessage
    {
        public RecalcQuantityFromTankReaminderMessage(int docMaterialTankGroupID, int? docMaterialProductionTypeID, decimal quantity, List<Guid> nomenclatureID, List<Guid> exceptNomenclatureID)
        {
            DocMaterialTankGroupID = docMaterialTankGroupID;
            DocMaterialProductionTypeID = docMaterialProductionTypeID;
            Quantity = quantity;
            NomenclatureID = nomenclatureID;
            ExceptNomenclatureID = exceptNomenclatureID;
        }
        public int? DocMaterialProductionTypeID { get; private set; }
        public int DocMaterialTankGroupID { get; private set; }
        public decimal Quantity { get; private set; }
        public List<Guid> NomenclatureID { get; private set; }
        public List<Guid> ExceptNomenclatureID { get; private set; }
    }

    public class RecalcMaterialProductionQuantityEndFromTankReaminderMessage
    {
        public RecalcMaterialProductionQuantityEndFromTankReaminderMessage(Guid nomenclatureID, decimal quantity )
        {
            Quantity = quantity;
            NomenclatureID = nomenclatureID;
        }
        public decimal Quantity { get; private set; }
        public Guid NomenclatureID { get; private set; }
        
    }

    public class RecalcQuantityFromTankGroupReaminderMessage
    {
        public RecalcQuantityFromTankGroupReaminderMessage(int docMaterialTankGroupID)
        {
            DocMaterialTankGroupID = docMaterialTankGroupID;
        }
        public int DocMaterialTankGroupID { get; private set; }
    }
        
    public class DeleteNomenclatureInCompositionFromTankGroupMessage
    {
        public DeleteNomenclatureInCompositionFromTankGroupMessage(Guid nomenclatureID)
        {
            NomenclatureID = nomenclatureID;
        }
        public Guid NomenclatureID { get; private set; }
    }

    public class OpenDocMaterialProductionMessage
    {
        public Guid? DocID;
    /*    public int? PlaceID;
        public DateTime? CloseDate;
        public byte? ShiftID;
        public Guid? PersonID;*/
    }

    public static class MessageManager
    {
        public static void NomenclatureEdit(Guid nomenclatureId)
        {
            Messenger.Default.Send(new NomenclatureEditMessage(nomenclatureId));
        }

        public static void OpenDocWithdrawal(Guid docId)
        {
            Messenger.Default.Send(new OpenDocWithdrawalMessage(docId));
        }

        public static void SpoolWithdrawed()
        {
            Messenger.Default.Send(new SpoolWithdrawed());
        }

        public static void EditUser(GammaEntities gammaBase = null)
        {
            Messenger.Default.Send(new UserEditMessage(gammaBase));
        }
        public static void EditUser(Guid userId, GammaEntities gammaBase = null)
        {
            Messenger.Default.Send(new UserEditMessage (userId, gammaBase));
        }
        public static void EditPermit(GammaEntities gammaBase = null)
        {
            Messenger.Default.Send(new PermitEditMessage(gammaBase));
        }
        public static void EditPermit(Guid permitId, GammaEntities gammaBase = null)
        {
            Messenger.Default.Send(new PermitEditMessage(permitId, gammaBase));
        }
        public static void EditRole(GammaEntities gammaBase = null)
        {
            Messenger.Default.Send(new RoleEditMessage(gammaBase));
        }
        public static void EditRole(Guid roleID, GammaEntities gammaBase = null)
        {
            Messenger.Default.Send(new RoleEditMessage(roleID, gammaBase));
        }

        public static void OpenDocShipmentOrder(Guid docShipmentOrderId)
        {
            UIServices.SetBusyState();
            Messenger.Default.Send(new OpenDocShipmentOrderMessage(docShipmentOrderId));
        }

        public static void ProductChanged(Guid productId)
        {
            Messenger.Default.Send(new ProductChangedMessage(productId));
        }

        public static void ProductionTaskRwNomenclatureChanged(Guid productionTaskBatchID, List<Nomenclature> nomenclatureIds)
        {
            Messenger.Default.Send(new ProductionTaskRwMessage(productionTaskBatchID, nomenclatureIds));
        }

        public static void DocChanged(Guid docId, bool isConfirmed)
        {
            Messenger.Default.Send(new DocChangedMessage(docId, isConfirmed));
        }

        public static void OpenQualityReportPM()
        {
            Messenger.Default.Send(new OpenQualityReportPMMessage());
        }

        public static void EditRejectionReasons(BrokeProduct product)
        {
            Messenger.Default.Send(new EditRejectionReasonsMessage(product));
        }

        public static void ProductionTaskRwDateBeginChanged(Guid productionTaskBatchID, DateTime dateBegin)
        {
            Messenger.Default.Send(new ProductionTaskRwMessage(productionTaskBatchID, dateBegin));
        }
        public static void OpenManageUsers()
        {
            Messenger.Default.Send(new OpenManageUsersMessage());
        }
        public static void OpenMain()
        {
            Messenger.Default.Send(new OpenMainMessage());
        }

        public static void FindNomenclature(int? placeGroupID, bool nomenclatureEdit = false)
        {
            Messenger.Default.Send(new OpenNomenclatureMessage(placeGroupID, nomenclatureEdit));
        }

        public static void FindNomenclature(MaterialType materialType, bool nomenclatureEdit = false)
        {
            Messenger.Default.Send(new OpenNomenclatureMessage(materialType, nomenclatureEdit));
        }

        public static void OpenProductionTask(BatchKinds batchKind, Guid productionTaskBatchId, bool isWindow = true)
        {
            UIServices.SetBusyState();
            Messenger.Default.Send(new OpenProductionTaskBatchMessage(batchKind, productionTaskBatchId, isWindow));
        }

        public static void NewProductionTask(BatchKinds batchKind)
        {
            UIServices.SetBusyState();
            Messenger.Default.Send(new OpenProductionTaskBatchMessage(batchKind));
        }

        public static void OpenDocProduct(ProductKind productKind, Guid productId)
        {
            switch (productKind)
            {
                case ProductKind.ProductSpool:
                    OpenDocProduct(DocProductKinds.DocProductSpool, productId);
                    break;
                case ProductKind.ProductGroupPack:
                    OpenDocProduct(DocProductKinds.DocProductGroupPack, productId);
                    break;
                case ProductKind.ProductPallet:
                    OpenDocProduct(DocProductKinds.DocProductPallet, productId);
                    break;
                case ProductKind.ProductPalletR:
                    OpenDocProduct(DocProductKinds.DocProductPallet, productId);
                    break;
            }
        }

        public static void OpenDocProduct(DocProductKinds docProductKind, Guid id)
        {
            UIServices.SetBusyState();
            Messenger.Default.Send(new OpenDocProductMessage(
                    docProductKind,
                    false,
                    id)
                );
        }

        public static void OpenDoc(DocTypes docType, Guid docId)
        {
            switch (docType)
            {
                case DocTypes.DocBroke:
                    OpenDocBroke(docId);
                    break;
                case DocTypes.DocComplectation:
                    OpenDocComplectation(docId);
                    break;
                case DocTypes.DocRepack:
                    OpenDocRepack(docId);
                    break;
            }
        }

        public static void OpenDocBroke(Guid docId, Guid? productId = null, bool isInFuturePeriod = false)
        {
            Messenger.Default.Send(new OpenDocBrokeMessage(docId, productId, isInFuturePeriod));
        }

        public static void OpenReportList()
        {
            Messenger.Default.Send(new OpenReportListMessage());
        }
        
        public static void PrintReport(PrintReportMessage msg)
        {
            Messenger.Default.Send(msg);
        }

        public static void OpenFindProduct()
        {
            Messenger.Default.Send(new FindProductMessage
            {
                    ChooseProduct = false
            });
        }

        public static void OpenDocMovementOrder(Guid? docId = null)
        {
            Messenger.Default.Send(new EditDocMovementOrderMessage(docId));
        }

        public static void OpenDocMovement(Guid docId)
        {
            Messenger.Default.Send(new EditDocMovementMessage(docId));
        }

        public static void OpenDocRepack(Guid? docId)
        {
            Messenger.Default.Send(new EditDocRepackMessage(docId));
        }

        public static void OpenDocComplectation(Guid docId)
	    {
		    Messenger.Default.Send(new EditDocComplectationMessage(docId));
	    }
        
        public static void OpenFindProduct(ProductKind productKind, bool chooseProduct = false, List<int> currentPlaces = null, bool allowChangeCurrentPlaces = true, bool allowChangeProductKind = false, bool allowChooseOneValueOnly = true )
        {
            Messenger.Default.Send(new FindProductMessage
            {
                    ProductKind = productKind,
                    ChooseProduct = chooseProduct,
                    AllowChangeProductKind = allowChangeProductKind,
                    AllowChooseOneValueOnly = allowChooseOneValueOnly,
                    CurrentPlaces = currentPlaces,
                    AllowChangeCurrentPlaces = allowChangeCurrentPlaces
            });
        }
        public static void OpenFindProduct(BatchKinds batchKind, bool chooseProduct = false, List<int> currentPlaces = null, bool allowChangeCurrentPlaces = true, bool allowChangeProductKind = false, bool allowChooseOneValueOnly = true)
        {
            Messenger.Default.Send(new FindProductMessage
            {
                BatchKind = batchKind,
                ChooseProduct = chooseProduct,
                AllowChangeProductKind = allowChangeProductKind,
                AllowChooseOneValueOnly = allowChooseOneValueOnly,
                CurrentPlaces = currentPlaces,
                AllowChangeCurrentPlaces = allowChangeCurrentPlaces
            });
        }
        public static void OpenDocCloseShift(Guid docID)
        {
            Messenger.Default.Send(new OpenDocCloseShiftMessage {DocID = docID});
        }
        public static void OpenDocCloseShift(int placeID, DateTime closeDate, byte shiftID)
        {
            Messenger.Default.Send(new OpenDocCloseShiftMessage { PlaceID = placeID, CloseDate = closeDate, ShiftID = shiftID });
        }
        public static void OpenDocCloseShift(int placeID, DateTime closeDate, byte shiftID, Guid? personID)
        {
            Messenger.Default.Send(new OpenDocCloseShiftMessage { PlaceID = placeID, CloseDate = closeDate, ShiftID = shiftID, PersonID = personID });
        }

        public static void OpenDocCloseShifts(PlaceGroup placeGroup)
        {
            UIServices.SetBusyState();
            Messenger.Default.Send(new OpenDocCloseShiftsMessage { PlaceGroup = placeGroup });
        }

        public static void OpenDocUnwinderRemainders(PlaceGroup placeGroup)
        {
            UIServices.SetBusyState();
            Messenger.Default.Send(new OpenDocUnwinderRemaindersMessage { PlaceGroup = placeGroup });
        }

        public static void OpenDocUnwinderRemainder(Guid docID)
        {
            Messenger.Default.Send(new OpenDocUnwinderRemainderMessage { DocID = docID });
        }
        public static void OpenDocUnwinderRemainder(int placeID, DateTime closeDate, byte shiftID, Guid? personID)
        {
            Messenger.Default.Send(new OpenDocUnwinderRemainderMessage { PlaceID = placeID, CloseDate = closeDate, ShiftID = shiftID, PersonID = personID });
        }
        
        public static void ConfigureComPort()
        {
            Messenger.Default.Send(new ConfigureComPortMessage());
        }
        public static void FindProductionTaskBatch(BatchKinds batchKind)
        {
            Messenger.Default.Send(new FindProductionTaskBatchMessage { BatchKind =  batchKind});
        }
        public static void OpenPlaceGroupsNomenclature()
        {
            Messenger.Default.Send(new OpenPlaceGroupsNomenclatureMessage());
        }

        public static void OpenMaterialTypesNomenclature()
        {
            Messenger.Default.Send(new OpenMaterialTypesNomenclatureMessage());
        }

        public static void OpenWarehousePersons()
        {
            Messenger.Default.Send(new OpenWarehousePersonsMessage());
        }

        public static void OpenDowntimeTemplates()
        {
            Messenger.Default.Send(new OpenDowntimeTemplatesMessage());
        }

        public static void OpenImportOldProducts()
        {
            Messenger.Default.Send(new OpenImportOldProductsMessage());
        }

        public static void CreateNewProduct(DocProductKinds docProductKind, Guid? id = null, SourceSpoolsCheckResult checkResult = SourceSpoolsCheckResult.Correct)
        {
            Messenger.Default.Send(new OpenDocProductMessage(
                docProductKind,
                true, id,
                checkResult));
        }

        public static void OpenPlaceZones()
        {
            Messenger.Default.Send(new OpenPlaceZonesMessage());
        }

        internal static void OpenDocInventarisation(Guid docId)
        {
            Messenger.Default.Send(new OpenDocInventarisationMessage(docId));
        }

        public static void OpenLogEvent(Guid eventID, Guid? parentEventID)
        {
            Messenger.Default.Send(new OpenLogEventMessage(eventID, parentEventID));
        }

        public static void RecalcQuantityEndFromUnwinderReaminderEvent(Guid? productID, Guid? nomenclatureID, Guid? characteristicID, decimal quantity, decimal? delta)
        {
            Messenger.Default.Send(new RecalcQuantityEndFromUnwinderReaminderMessage (productID, nomenclatureID, characteristicID, quantity, delta));
        }

        public static void RecalcQuantityFromTankReaminderEvent(int docMaterialTankGroupID, int? docMaterialTankProductionTypeID, decimal quantity, List<Guid> nomenclatureID, List<Guid> exceptNomenclatureID)
        {
            Messenger.Default.Send(new RecalcQuantityFromTankReaminderMessage(docMaterialTankGroupID, docMaterialTankProductionTypeID, quantity, nomenclatureID, exceptNomenclatureID));
        }

        public static void RecalcMaterialProductionQuantityEndFromTankReaminderEvent(Guid nomenclatureID, decimal quantity)
        {
            Messenger.Default.Send(new RecalcMaterialProductionQuantityEndFromTankReaminderMessage(nomenclatureID, quantity));
        }

        public static void RecalcQuantityFromTankGroupReaminderEvent(int docMaterialTankGroupID)
        {
            Messenger.Default.Send(new RecalcQuantityFromTankGroupReaminderMessage(docMaterialTankGroupID));
        }
                
        public static void DeleteNomenclatureInCompositionFromTankGroupEvent(Guid nomenclatureID)
        {
            Messenger.Default.Send(new DeleteNomenclatureInCompositionFromTankGroupMessage(nomenclatureID));
        }

        public static void OpenDocMaterialProduction(Guid docID)
        {
            Messenger.Default.Send(new OpenDocMaterialProductionMessage { DocID = docID });
        }
/*        public static void OpenDocMaterialProduction(int placeID, DateTime closeDate, byte shiftID, Guid? personID)
        {
            Messenger.Default.Send(new OpenDocMaterialProductionMessage { PlaceID = placeID, CloseDate = closeDate, ShiftID = shiftID, PersonID = personID });
        }
*/
    }
}
