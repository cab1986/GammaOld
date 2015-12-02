﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gamma.Views;
using GalaSoft.MvvmLight.Messaging;

namespace Gamma.Common
{
    class ViewsManager
    {
        private ViewsManager()
        {
            Messenger.Default.Register<OpenNomenclatureMessage>(this, OpenNomenclature);
            Messenger.Default.Register<OpenDocProductMessage>(this, OpenDocProduct);
            Messenger.Default.Register<OpenProductionTaskMessage>(this, OpenProductionTask);
            Messenger.Default.Register<OpenReportListMessage>(this, OpenReportList);
            Messenger.Default.Register<FindProductMessage>(this, OpenFindProduct);
            Messenger.Default.Register<OpenManageUsersMessage>(this, OpenManageUsers);
            Messenger.Default.Register<OpenDocCloseShiftMessage>(this, OpenDocCloseShift);
            Messenger.Default.Register<OpenDocCloseShiftsMessage>(this, OpenDocCloseShifts);
            Messenger.Default.Register<ConfigureComPortMessage>(this, ConfigureComPort);
            Messenger.Default.Register<PermitEditMessage>(this, PermitEdit);
            Messenger.Default.Register<RoleEditMessage>(this, RoleEdit);
            Messenger.Default.Register<UserEditMessage>(this, UserEdit);
            Messenger.Default.Register<FindProductionTaskMessage>(this, FindProductionTask);
        }
        private static ViewsManager _viewsManager;
        public static void Initialize()
        {
            if (_viewsManager == null) _viewsManager = new ViewsManager();
        }
        private static void FindProductionTask(FindProductionTaskMessage msg)
        {
            new FindProductionTaskView(msg.PlaceGroup).Show();
        }
        private static void OpenNomenclature(OpenNomenclatureMessage msg)
        {
            new NomenclatureView().Show();
        }
        private void OpenDocProduct(OpenDocProductMessage msg)
        {
            new DocProductView(msg).Show();
        }
        private void ConfigureComPort(ConfigureComPortMessage obj)
        {
            var view = new ComPortSettingsView();
            view.Show();
        }
        private void OpenDocCloseShifts(OpenDocCloseShiftsMessage msg)
        {
            DocCloseShiftsView view;
            if (msg.PlaceGroup == null)
                view = new DocCloseShiftsView();
            else
                view = new DocCloseShiftsView((PlaceGroups)msg.PlaceGroup);
            view.Show();
        }

        private void OpenDocCloseShift(OpenDocCloseShiftMessage msg)
        {
            var view = new DocCloseShiftView(msg);
            view.Show();
        }
        private void OpenManageUsers(OpenManageUsersMessage obj)
        {
            var View = new ManageUsersView();
            View.Show();
        }
        private void OpenProductionTask(OpenProductionTaskMessage msg)
        {
            var View = new ProductionTaskView(msg);
            View.Show();
        }
        private void OpenReportList(OpenReportListMessage msg)
        {
            var View = new ReportListView();
            View.Show();
        }
        private void OpenFindProduct(FindProductMessage msg)
        {
            var View = new FindProductView(msg);
            View.Show();
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

    }
    
}