using System;
using System.Collections.Generic;
using DevExpress.Mvvm;
using Gamma.Common;
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

    public class BaseReconnectedMessage { }
    public class OpenFilterDateMessage { }

    public class PrintReportMessage
    {
        public Guid ReportID { get; set; } // id отчета
        public Guid? VMID { get; set; } // id ViewModel, которая должна обработать событие
    }

    public class EditDocMovementOrderMessage
    {
        public EditDocMovementOrderMessage(Guid? docId = null)
        {
            DocId = docId;
        }

        public Guid? DocId { get; private set; }
    }

    public class EditDocMovementMessage
    {
        public EditDocMovementMessage(Guid docId)
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
        public OpenDocBrokeMessage(Guid docId, Guid? productId = null)
        {
            DocId = docId;
            ProductId = productId;
        }

        public Guid DocId { get; private set; }
        public Guid? ProductId { get; private set; }
    }

    public class CloseMessage { }
 
    public class OpenMainMessage { }
    public class OpenProductionTaskBatchMessage 
    {
        public BatchKinds BatchKind { get; set; }
        public Guid? ProductionTaskBatchID { get; set; }
        public bool Window { get; set; } = true;
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

        public OpenNomenclatureMessage(MaterialTypes materialType, bool nomenclatureEdit = false)
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
    }
    public class ConfigureComPortMessage  { }
    public class ChoosenProductMessage
    {
        public Guid ProductID;
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
    }


    public class OpenPlaceGroupsNomenclatureMessage  { }
    public class OpenMaterialTypesNomenclatureMessage { }
    public class OpenWarehousePersonsMessage { }
    public class OpenImportOldProductsMessage { }
    public class OpenDocCloseShiftsMessage 
    {
        public PlaceGroups? PlaceGroup;
    }
    public class FindProductionTaskBatchMessage
    {
        public BatchKinds BatchKind;
    }
    public static class MessageManager
    {
        public static void NomenclatureEdit(Guid nomenclatureId)
        {
            Messenger.Default.Send(new NomenclatureEditMessage(nomenclatureId));
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

        public static void FindNomenclature(MaterialTypes materialType, bool nomenclatureEdit = false)
        {
            Messenger.Default.Send(new OpenNomenclatureMessage(materialType, nomenclatureEdit));
        }

        public static void OpenProductionTask(OpenProductionTaskBatchMessage msg)
        {
            UIServices.SetBusyState();
            Messenger.Default.Send(msg);
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

        public static void OpenDocBroke(Guid docId, Guid? productId = null)
        {
            Messenger.Default.Send(new OpenDocBrokeMessage(docId, productId));
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
        
        public static void OpenFindProduct(ProductKind productKind, bool chooseProduct = false, bool allowChangeProductKind = false)
        {
            Messenger.Default.Send(new FindProductMessage
            {
                    ProductKind = productKind,
                    ChooseProduct = chooseProduct,
                    AllowChangeProductKind = allowChangeProductKind
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
        public static void OpenDocCloseShifts(PlaceGroups placeGroup)
        {
            UIServices.SetBusyState();
            Messenger.Default.Send(new OpenDocCloseShiftsMessage { PlaceGroup = placeGroup });
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

    }
}
