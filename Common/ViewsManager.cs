// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System;
using Gamma.Views;
using DevExpress.Mvvm;

namespace Gamma.Common
{
    class ViewsManager
    {
        private ViewsManager()
        {
            Messenger.Default.Register<OpenNomenclatureMessage>(this, OpenNomenclature);
            Messenger.Default.Register<OpenDocProductMessage>(this, OpenDocProduct);
            Messenger.Default.Register<OpenProductionTaskBatchMessage>(this, OpenProductionTask);
            Messenger.Default.Register<OpenReportListMessage>(this, OpenReportList);
            Messenger.Default.Register<FindProductMessage>(this, OpenFindProduct);
            Messenger.Default.Register<OpenManageUsersMessage>(this, OpenManageUsers);
            Messenger.Default.Register<OpenDocCloseShiftMessage>(this, OpenDocCloseShift);
            Messenger.Default.Register<OpenDocCloseShiftsMessage>(this, OpenDocCloseShifts);
            Messenger.Default.Register<ConfigureComPortMessage>(this, ConfigureComPort);
            Messenger.Default.Register<PermitEditMessage>(this, PermitEdit);
            Messenger.Default.Register<RoleEditMessage>(this, RoleEdit);
            Messenger.Default.Register<UserEditMessage>(this, UserEdit);
            Messenger.Default.Register<FindProductionTaskBatchMessage>(this, FindProductionTaskBatch);
            Messenger.Default.Register<OpenPlaceGroupsNomenclatureMessage>(this, OpenPlaceGroupsNomenclature);
            Messenger.Default.Register<OpenMaterialTypesNomenclatureMessage>(this, OpenMaterialTypeNomenclature);
            Messenger.Default.Register<OpenDocShipmentOrderMessage>(this, OpenDocShipmentOrder);
            Messenger.Default.Register<OpenWarehousePersonsMessage>(this, OpenWarehousePersons);
            Messenger.Default.Register<OpenImportOldProductsMessage>(this, OpenImportOldProducts);
            Messenger.Default.Register<OpenDocBrokeMessage>(this, OpenDocBroke);
            Messenger.Default.Register<EditRejectionReasonsMessage>(this, EditRejectionReasonsDialog);
            Messenger.Default.Register<NomenclatureEditMessage>(this, NomenclatureEdit);
            //Messenger.Default.Register<EditDocMovementOrderMessage>(this, EditDocMovementOrder);
            Messenger.Default.Register<EditDocMovementMessage>(this, EditDocMovement);
            Messenger.Default.Register<OpenPlaceZonesMessage>(this, OpenPlaceZones);
            Messenger.Default.Register<OpenQualityReportPMMessage>(this, OpenQualityReportPM);
            Messenger.Default.Register<OpenDocInventarisationMessage>(this, OpenDocInventarisation);
            Messenger.Default.Register<OpenDocWithdrawalMessage>(this, OpenDocWithdrawal);
            Messenger.Default.Register<OpenLogEventMessage>(this, OpenLogEvent);
        }

        private void OpenDocWithdrawal(OpenDocWithdrawalMessage msg)
        {
            UIServices.SetBusyState();
            new DocWithdrawalView(msg.DocId).Show();
        }

        private void OpenDocInventarisation(OpenDocInventarisationMessage msg)
        {
            UIServices.SetBusyState();
            new DocInventarisationView(msg.DocId).Show();
        }

        private void OpenQualityReportPM(OpenQualityReportPMMessage msg)
        {
            UIServices.SetBusyState();
            new QualityReportPMView().Show();
        }

        private void OpenPlaceZones(OpenPlaceZonesMessage msg)
        {
            UIServices.SetBusyState();
            new PlaceZonesView().Show();
        }

        private void EditDocMovement(EditDocMovementMessage msg)
        {
            new DocMovementView(msg.DocId).Show();
        }
/*
        private void EditDocMovementOrder(EditDocMovementOrderMessage msg)
        {
            new DocMovementOrderView(msg.DocId).Show();
        }
        */

        private void NomenclatureEdit(NomenclatureEditMessage msg)
        {
            new NomenclatureEditView(msg.NomenclatureId).Show();
        }

        private void EditRejectionReasonsDialog(EditRejectionReasonsMessage msg)
        {
            new EditRejectionReasonsView(msg.BrokeProduct).ShowDialog();
        }

        private void OpenDocBroke(OpenDocBrokeMessage msg)
        {
            new DocBrokeView(msg.DocId, msg.ProductId, msg.IsInFuturePeriod).Show();
        }

        private void OpenImportOldProducts(OpenImportOldProductsMessage obj)
        {
            new ImportOldProductsView().Show();
        }

        private void OpenWarehousePersons(OpenWarehousePersonsMessage msg)
        {
            new WarehousePersonsView().Show();
        }

        private void OpenDocShipmentOrder(OpenDocShipmentOrderMessage msg)
        {
            new DocShipmentOrderView(msg.DocShipmentOrderId).Show();
        }

        private void OpenMaterialTypeNomenclature(OpenMaterialTypesNomenclatureMessage msg)
        {
            new MaterialTypesNomenclatureView().Show();
        }

        private void OpenPlaceGroupsNomenclature(OpenPlaceGroupsNomenclatureMessage obj)
        {
            new PlaceGroupsNomenclatureView().Show();
        }
        private static ViewsManager _viewsManager;
        public static void Initialize()
        {
            if (_viewsManager == null) _viewsManager = new ViewsManager();
        }
        private static void FindProductionTaskBatch(FindProductionTaskBatchMessage msg)
        {
            new FindProductionTaskBatchView(msg.BatchKind).Show();
        }
        private static void OpenNomenclature(OpenNomenclatureMessage msg)
        {
            if (msg.IsPlaceGroupFilter)
                new NomenclatureFindView(msg.ID, msg.NomenclatureEdit).Show();
            else
                new NomenclatureFindView((MaterialType)msg.ID).Show();
        }
        private void OpenDocProduct(OpenDocProductMessage msg)
        {
            var view = new DocProductView(msg);
            view.ShowDialog();

        }
        private void ConfigureComPort(ConfigureComPortMessage obj)
        {
            var view = new ComPortSettingsView();
            view.Show();
        }
        private void OpenDocCloseShifts(OpenDocCloseShiftsMessage msg)
        {
            var view = msg.PlaceGroup == null ? new DocCloseShiftsView() : new DocCloseShiftsView((PlaceGroup)msg.PlaceGroup);
            view.Show();
        }

        private void OpenDocCloseShift(OpenDocCloseShiftMessage msg)
        {
            var view = new DocCloseShiftView(msg);
            if (view != null)
            {
                view.WindowState = System.Windows.WindowState.Maximized;
                view.ResizeMode = System.Windows.ResizeMode.NoResize;
                view.ShowDialog();
            }
        }
        private void OpenManageUsers(OpenManageUsersMessage obj)
        {
            var view = new ManageUsersView();
            view.Show();
        }
        private void OpenProductionTask(OpenProductionTaskBatchMessage msg)
        {
            //var view = new ProductionTaskBatchView(msg);
            if (!msg.Window) return;
            var view = new ProductionTaskBatchWindowView(msg);
            view.Show();
        }
        private void OpenReportList(OpenReportListMessage msg)
        {
            var view = new ReportListView();
            view.Show();
        }
        private void OpenFindProduct(FindProductMessage msg)
        {
            var view = new FindProductView(msg);
            view.Show();
        }
        private void PermitEdit(PermitEditMessage msg)
        {
            var view = new PermitEditView(msg);
            view.Show();
        }
        private void UserEdit(UserEditMessage msg)
        {
            var view = new UserEditView(msg);
            view.Show();
        }
        private void RoleEdit(RoleEditMessage msg)
        {
            var view = new RoleEditView(msg);
            view.Show();
        }
        private void OpenLogEvent(OpenLogEventMessage msg)
        {
            var view = new LogEventView(msg.EventID, msg.ParentEventID);
            view.Show();
        }

    }
    
}
