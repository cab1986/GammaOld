using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gamma
{
    public class BaseReconnectedMessage : MessageBase { }
    public class OpenFilterDateMessage : MessageBase { }
    public class PrintReportMessage : MessageBase
    {
        public Guid ReportID;
    }
    public class OpenMainMessage : MessageBase { }
    public class OpenProductionTaskMessage : MessageBase 
    {
        public ProductionTaskKinds ProductionTaskKind;
        public Guid? ProductionTaskID;
    }
    public class ProductChangedMessage : MessageBase
    {
        public Guid ProductID;
    }

    public class OpenDocProductMessage : MessageBase
    {
        public DocProductKinds DocProductKind;
        public Guid ID;
        public bool IsNewProduct;
    }
    public class OpenNomenclatureMessage : MessageBase { }
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
    public class ChoosenSourceProductMessage : MessageBase
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
    public class OpenDocCloseShiftsMessage : MessageBase
    {
        public PlaceGroups? PlaceGroup;
    }
    public class FindProductionTaskMessage : MessageBase
    {
        public PlaceGroups PlaceGroup;
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
        public static void OpenManageUsers()
        {
            Messenger.Default.Send<OpenManageUsersMessage>(new OpenManageUsersMessage());
        }
        public static void OpenMain()
        {
            Messenger.Default.Send<OpenMainMessage>(new OpenMainMessage());
        }

        public static void OpenNomenclature()
        {
            Messenger.Default.Send<OpenNomenclatureMessage>(new OpenNomenclatureMessage());
        }

        public static void OpenProductionTask(OpenProductionTaskMessage msg)
        {
            Messenger.Default.Send<OpenProductionTaskMessage>(msg);
        }
        public static void OpenDocProduct(OpenDocProductMessage msg)
        {
            Messenger.Default.Send<OpenDocProductMessage>(msg);
        }
        public static void OpenReportList()
        {
            Messenger.Default.Send<OpenReportListMessage>(new OpenReportListMessage());
        }
        public static void PrintReport(PrintReportMessage msg)
        {
            Messenger.Default.Send<PrintReportMessage>(msg);
        }
        public static void OpenFindProduct(FindProductMessage msg)
        {
            Messenger.Default.Send<FindProductMessage>(msg);
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
        public static void FindProductionTask(PlaceGroups placeGroup)
        {
            Messenger.Default.Send<FindProductionTaskMessage>(new FindProductionTaskMessage() { PlaceGroup = placeGroup });
        }

    }
}
