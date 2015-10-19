using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gamma
{
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
    public class ChoosenSourceProductMessage : MessageBase
    {
        public Guid ProductID;
    }
    public class BarcodeMessage : MessageBase
    {
        public string Barcode;
    }
    public class ParentSaveMessage : MessageBase { }
    public static class MessageManager
    {
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
    }
}
