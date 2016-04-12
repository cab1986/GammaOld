using System;
using DevExpress.Mvvm;

namespace Gamma
{
    public class BaseReconnectedMessage { }
    public class OpenFilterDateMessage { }
 
    public class PrintReportMessage
    {
        public Guid ReportID; // ID отчета
        public Guid? VMID; // ID ViewModel, которая должна обработать событие
    }
 
    public class OpenMainMessage { }
    public class OpenProductionTaskBatchMessage 
    {
        public BatchKinds BatchKind;
        public Guid? ProductionTaskBatchID;
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
        public int PlaceGroupID;
    }
    public class ProductionTaskRwMessage
    {
        public ProductionTaskRwMessage(Guid productionTaskBatchId, Guid nomenclatureId)
        {
            ProductionTaskBatchId = productionTaskBatchId;
            NomenclatureId = nomenclatureId;
        }

        public ProductionTaskRwMessage(Guid productionTaskBatchId, DateTime dateBegin)
        {
            ProductionTaskBatchId = productionTaskBatchId;
            DateBegin = dateBegin;
        }

        public Guid ProductionTaskBatchId;
        public Guid? NomenclatureId;
        public Guid? CharacteristicId;
        public DateTime? DateBegin;
    }
    public class Nomenclature1CMessage
    {
        public Guid Nomenclature1CID;
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
    public class PermitEditMessage
    {
        public Guid? PermitID;
    }
    public class RoleEditMessage
    {
        public Guid? RoleID;
    }
    public class UserEditMessage
    {
        public Guid? UserID;
    }
    public class UserChangedMessage  { }
    public class PermitChangedMessage  { }
    public class RoleChangedMessage  { }
    
    public class ParentSaveMessage { }
    public class OpenManageUsersMessage { }
    public class OpenDocCloseShiftMessage
    {
        public Guid? DocID;
        public int? PlaceID;
        public DateTime? CloseDate;
        public byte? ShiftID;
    }
    public class OpenPlaceGroupsNomenclatureMessage  { }
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
        public static void EditUser()
        {
            Messenger.Default.Send(new UserEditMessage());
        }
        public static void EditUser(Guid userID)
        {
            Messenger.Default.Send(new UserEditMessage { UserID = userID });
        }
        public static void EditPermit()
        {
            Messenger.Default.Send(new PermitEditMessage());
        }
        public static void EditPermit(Guid permitID)
        {
            Messenger.Default.Send(new PermitEditMessage { PermitID = permitID });
        }
        public static void EditRole()
        {
            Messenger.Default.Send(new RoleEditMessage());
        }
        public static void EditRole(Guid roleID)
        {
            Messenger.Default.Send(new RoleEditMessage { RoleID = roleID });
        }
        public static void ProductionTaskRwNomenclatureChanged(Guid productionTaskBatchId, Guid nomenclatureId)
        {
            Messenger.Default.Send(new ProductionTaskRwMessage(productionTaskBatchId, nomenclatureId));
        }

        public static void ProductionTaskRwDateBeginChanged(Guid productionTaskBatchId, DateTime dateBegin)
        {
            Messenger.Default.Send(new ProductionTaskRwMessage(productionTaskBatchId, dateBegin));
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
            Messenger.Default.Send(new OpenNomenclatureMessage {PlaceGroupID = placeGroupID });
        }

        public static void OpenProductionTask(OpenProductionTaskBatchMessage msg)
        {
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
