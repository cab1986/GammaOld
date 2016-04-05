using GalaSoft.MvvmLight.Messaging;
using System;

namespace Gamma
{
    public class BaseReconnectedMessage : MessageBase { }
    public class OpenFilterDateMessage : MessageBase { }
 
    public class PrintReportMessage : MessageBase
    {
        public Guid ReportID; // ID отчета
        public Guid? VMID; // ID ViewModel, которая должна обработать событие
    }
 
    public class OpenMainMessage : MessageBase { }
    public class OpenProductionTaskBatchMessage : MessageBase 
    {
        public BatchKinds BatchKind;
        public Guid? ProductionTaskBatchID;
    }
    public class ProductChangedMessage : MessageBase
    {
        public Guid ProductID;
    }

    public class OpenDocProductMessage : MessageBase
    {
        public DocProductKinds DocProductKind;
        public Guid? ID;
        public bool IsNewProduct;
    }
    public class OpenNomenclatureMessage : MessageBase 
    {
        public int PlaceGroupID;
    }
    public class ProductionTaskRWMessage : MessageBase
    {
        public Guid ProductionTaskBatchID;
        public Guid NomenclatureID;
        public Guid CharacteristicID;
    }
    public class Nomenclature1CMessage : MessageBase
    {
        public Guid Nomenclature1CID;
    }
    public class OpenReportListMessage : MessageBase { }
    public class FindProductMessage : MessageBase
    {
        public bool ChooseSourceProduct;
        public ProductKinds ProductKind;
    }
    public class ConfigureComPortMessage : MessageBase { }
    public class ChoosenProductMessage : MessageBase
    {
        public Guid ProductID;
    }
    public class BarcodeMessage : MessageBase
    {
        public string Barcode;
    }
    public class PermitEditMessage : MessageBase 
    {
        public Guid? PermitID;
    }
    public class RoleEditMessage : MessageBase
    {
        public Guid? RoleID;
    }
    public class UserEditMessage : MessageBase
    {
        public Guid? UserID;
    }
    public class UserChangedMessage : MessageBase { }
    public class PermitChangedMessage : MessageBase { }
    public class RoleChangedMessage : MessageBase { }
    
    public class ParentSaveMessage : MessageBase { }
    public class OpenManageUsersMessage : MessageBase { }
    public class OpenDocCloseShiftMessage : MessageBase
    {
        public Guid? DocID;
        public int? PlaceID;
        public DateTime? CloseDate;
        public byte? ShiftID;
    }
    public class OpenPlaceGroupsNomenclatureMessage : MessageBase { }
    public class OpenDocCloseShiftsMessage : MessageBase
    {
        public PlaceGroups? PlaceGroup;
    }
    public class FindProductionTaskBatchMessage : MessageBase
    {
        public BatchKinds BatchKind;
    }
    public static class MessageManager
    {
        public static void EditUser()
        {
            Messenger.Default.Send<UserEditMessage>(new UserEditMessage());
        }
        public static void EditUser(Guid userID)
        {
            Messenger.Default.Send<UserEditMessage>(new UserEditMessage() { UserID = userID });
        }
        public static void EditPermit()
        {
            Messenger.Default.Send<PermitEditMessage>(new PermitEditMessage());
        }
        public static void EditPermit(Guid permitID)
        {
            Messenger.Default.Send<PermitEditMessage>(new PermitEditMessage() { PermitID = permitID });
        }
        public static void EditRole()
        {
            Messenger.Default.Send<RoleEditMessage>(new RoleEditMessage());
        }
        public static void EditRole(Guid roleID)
        {
            Messenger.Default.Send<RoleEditMessage>(new RoleEditMessage() { RoleID = roleID });
        }
        public static void ProductionTaskRWNomenclatureChanged(Guid productionTaskBatchID, Guid nomenclatureID)
        {
            Messenger.Default.Send<ProductionTaskRWMessage>(new ProductionTaskRWMessage()
            {
                ProductionTaskBatchID = productionTaskBatchID,
                NomenclatureID = nomenclatureID
            });
        }
        public static void OpenManageUsers()
        {
            Messenger.Default.Send<OpenManageUsersMessage>(new OpenManageUsersMessage());
        }
        public static void OpenMain()
        {
            Messenger.Default.Send<OpenMainMessage>(new OpenMainMessage());
        }

        public static void OpenNomenclature(int placeGroupID)
        {
            Messenger.Default.Send<OpenNomenclatureMessage>(new OpenNomenclatureMessage() {PlaceGroupID = placeGroupID });
        }

        public static void OpenProductionTask(OpenProductionTaskBatchMessage msg)
        {
            Messenger.Default.Send<OpenProductionTaskBatchMessage>(msg);
        }
        public static void OpenDocProduct(DocProductKinds docProductKind, Guid id)
        {
            Messenger.Default.Send<OpenDocProductMessage>(new OpenDocProductMessage()
                {
                    DocProductKind = docProductKind,
                    ID = id,
                    IsNewProduct = false
                });
        }
        public static void OpenReportList()
        {
            Messenger.Default.Send<OpenReportListMessage>(new OpenReportListMessage());
        }
        
        public static void PrintReport(PrintReportMessage msg)
        {
            Messenger.Default.Send<PrintReportMessage>(msg);
        }

        public static void OpenFindProduct()
        {
            Messenger.Default.Send<FindProductMessage>(new FindProductMessage()
                {
                    ChooseSourceProduct = false
                });
        }
        public static void OpenFindProduct(ProductKinds productKind, bool chooseSourceProduct = false)
        {
            Messenger.Default.Send<FindProductMessage>(new FindProductMessage()
                {
                    ProductKind = productKind,
                    ChooseSourceProduct = chooseSourceProduct
                });
        }
        public static void OpenDocCloseShift(Guid docID)
        {
            Messenger.Default.Send<OpenDocCloseShiftMessage>(new OpenDocCloseShiftMessage() {DocID = docID});
        }
        public static void OpenDocCloseShift(int placeID, DateTime closeDate, byte shiftID)
        {
            Messenger.Default.Send<OpenDocCloseShiftMessage>(new OpenDocCloseShiftMessage() { PlaceID = placeID, CloseDate = closeDate, ShiftID = shiftID });
        }
        public static void OpenDocCloseShifts(PlaceGroups placeGroup)
        {
            Messenger.Default.Send<OpenDocCloseShiftsMessage>(new OpenDocCloseShiftsMessage() { PlaceGroup = placeGroup });
        }
        public static void ConfigureComPort()
        {
            Messenger.Default.Send<ConfigureComPortMessage>(new ConfigureComPortMessage());
        }
        public static void FindProductionTaskBatch(BatchKinds batchKind)
        {
            Messenger.Default.Send<FindProductionTaskBatchMessage>(new FindProductionTaskBatchMessage() { BatchKind =  batchKind});
        }
        public static void OpenPlaceGroupsNomenclature()
        {
            Messenger.Default.Send<OpenPlaceGroupsNomenclatureMessage>(new OpenPlaceGroupsNomenclatureMessage());
        }
        public static void CreateNewProduct(DocProductKinds docProductKind, Guid? id = null)
        {
            Messenger.Default.Send<OpenDocProductMessage>(new OpenDocProductMessage()
                {
                    DocProductKind = docProductKind,
                    ID = id,
                    IsNewProduct = true
                });
        }

    }
}
