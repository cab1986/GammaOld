using System;
using DevExpress.Mvvm;
using Gamma.Common;
using Gamma.Models;

namespace Gamma
{
    public class ContextMessage
    {
        public ContextMessage(GammaEntities gammaBase = null)
        {
            GammaBase = null;
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
        public Guid ProductID;
    }

    public class OpenDocProductMessage
    {
        public DocProductKinds DocProductKind;
        public Guid? ID;
        public bool IsNewProduct;
    }
    public class OpenNomenclatureMessage  
    {
        public OpenNomenclatureMessage(int placeGroupID)
        {
            ID = placeGroupID;
            IsPlaceGroupFilter = true;
        }

        public OpenNomenclatureMessage(MaterialTypes materialType)
        {
            ID = (int) materialType;
        }

        public int ID { get; private set; }
        public bool IsPlaceGroupFilter { get; private set; }
    }
    public class ProductionTaskRwMessage
    {
        public ProductionTaskRwMessage(Guid productionTaskBatchID, Guid nomenclatureid)
        {
            ProductionTaskBatchID = productionTaskBatchID;
            NomenclatureID = nomenclatureid;
        }

        public ProductionTaskRwMessage(Guid productionTaskBatchID, DateTime dateBegin)
        {
            ProductionTaskBatchID = productionTaskBatchID;
            DateBegin = dateBegin;
        }

        public Guid ProductionTaskBatchID;
        public Guid? NomenclatureID;
        public Guid? CharacteristicID;
        public DateTime? DateBegin;
    }
    public class Nomenclature1CMessage
    {
        public Guid Nomenclature1CID { get; set; }
    }
    public class OpenReportListMessage { }
    public class FindProductMessage
    {
        public bool ChooseSourceProduct;
        public ProductKinds ProductKind;
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
    
//    public class ParentSaveMessage { }
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
        public static void ProductionTaskRwNomenclatureChanged(Guid productionTaskBatchID, Guid nomenclatureid)
        {
            Messenger.Default.Send(new ProductionTaskRwMessage(productionTaskBatchID, nomenclatureid));
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

        public static void OpenNomenclature(int placeGroupID)
        {
            Messenger.Default.Send(new OpenNomenclatureMessage(placeGroupID));
        }

        public static void OpenNomenclature(MaterialTypes materialType)
        {
            Messenger.Default.Send(new OpenNomenclatureMessage(materialType));
        }

        public static void OpenProductionTask(OpenProductionTaskBatchMessage msg)
        {
            UIServices.SetBusyState();
            Messenger.Default.Send(msg);
        }
        public static void OpenDocProduct(DocProductKinds docProductKind, Guid id)
        {
            Messenger.Default.Send(new OpenDocProductMessage
            {
                    DocProductKind = docProductKind,
                    ID = id,
                    IsNewProduct = false
                });
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
                    ChooseSourceProduct = false
            });
        }
        public static void OpenFindProduct(ProductKinds productKind, bool chooseSourceProduct = false)
        {
            Messenger.Default.Send(new FindProductMessage
            {
                    ProductKind = productKind,
                    ChooseSourceProduct = chooseSourceProduct
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
        public static void CreateNewProduct(DocProductKinds docProductKind, Guid? id = null)
        {
            Messenger.Default.Send(new OpenDocProductMessage
            {
                    DocProductKind = docProductKind,
                    ID = id,
                    IsNewProduct = true
                });
        }

    }
}
