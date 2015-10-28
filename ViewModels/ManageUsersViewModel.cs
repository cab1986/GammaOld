using GalaSoft.MvvmLight;
using Gamma.Models;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System;
using System.Collections.Generic;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;

namespace Gamma.ViewModels
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class ManageUsersViewModel : RootViewModel
    {
        /// <summary>
        /// Initializes a new instance of the ManageUsersViewModel class.
        /// </summary>
        public ManageUsersViewModel()
        {
            LoadTables();
            EditItemCommand = new RelayCommand(EditItem,SelectedNotNull);
            NewItemCommand = new RelayCommand(NewItem);
            DeleteItemCommand = new RelayCommand(DeleteItem,SelectedNotNull);
            Messenger.Default.Register<BaseReconnectedMessage>(this,LoadTables);
        }
        private bool SelectedNotNull()
        {
            switch (TabIndex)
            {
                case 0:
                    return SelectedUser != null;
                case 1:
                    return SelectedRole != null;
                case 2:
                    return SelectedPermit != null;
                default:
                    return false;
            }
        }
        private void LoadTables(BaseReconnectedMessage msg)
        {
            LoadTables();
        }
        private void LoadTables()
        {
            DB.GammaBase.Users.Include("Places").Load();
            DB.GammaBase.Roles.Load();
            DB.GammaBase.Permits.Load();
            Users = DB.GammaBase.Users.Local;
            Roles = DB.GammaBase.Roles.Local;
            Permits = DB.GammaBase.Permits.Local;
        }

         
        private Users _selectedUser;
        public Users SelectedUser
        {
            get
            {
                return _selectedUser;
            }
            set
            {
                _selectedUser = value;
                RaisePropertyChanged("SelectedUser");
            }
        }
        private ObservableCollection<Users> _users;
        public ObservableCollection<Users> Users
        {
            get
            {
                return _users;
            }
            set
            {
            	_users = value;
                RaisePropertyChanged("Users");
            }
        }
        private ObservableCollection<Places> _places;
        public ObservableCollection<Places> Places
        {
            get
            {
                return _places;
            }
            set
            {
            	_places = value;
                RaisePropertyChanged("Places");
            }
        }
        private ObservableCollection<Permits> _permits;
        public ObservableCollection<Permits> Permits
        {
            get
            {
                return _permits;
            }
            set
            {
            	_permits = value;
                RaisePropertyChanged("Permits");
            }
        }
        private Permits _selectedPermit;
        public Permits SelectedPermit
        {
            get
            {
                return _selectedPermit;
            }
            set
            {
            	_selectedPermit = value;
                RaisePropertyChanged("SelectedPermit");
            }
        }
        private ObservableCollection<PermitTables> _permitTables;
        public ObservableCollection<PermitTables> PermitTables
        {
            get
            {
                return _permitTables;
            }
            set
            {
            	_permitTables = value;
                RaisePropertyChanged("PermitTables");
            }
        }
        private ObservableCollection<Roles> _roles;
        public ObservableCollection<Roles> Roles
        {
            get
            {
                return _roles;
            }
            set
            {
            	_roles = value;
                RaisePropertyChanged("Roles");
            }
        }
        private Roles _selectedRole;
        public Roles SelectedRole
        {
            get
            {
                return _selectedRole;
            }
            set
            {
            	_selectedRole = value;
                RaisePropertyChanged("SelectedRole");
            }
        }
        private ObservableCollection<RolePermits> _rolePermits;
        public ObservableCollection<RolePermits> RolePermits
        {
            get
            {
                return _rolePermits;
            }
            set
            {
            	_rolePermits = value;
                RaisePropertyChanged("RolePermits");
            }
        }
        public List<string> PermissionMarks { get; set; }
        public RelayCommand SavePermitsCommand { get; set; }
        private void SavePermits()
        {
            DB.GammaBase.SaveChanges();
        }
        public int TabIndex { get; set; }
        public RelayCommand NewItemCommand { get; set; }
        private void NewItem()
        {
            switch (TabIndex)
            {
                case 0:
                    MessageManager.EditUser();
                    break;
                case 1:
                    MessageManager.EditRole();
                    break;
                case 2:
                    MessageManager.EditPermit();
                    break;
                default:
                    break;
            }
        }
        public RelayCommand EditItemCommand { get; set; }
        private void EditItem()
        {
            switch (TabIndex)
            {
                case 0:
                    MessageManager.EditUser(SelectedUser.UserID);
                    break;
                case 1:
                    MessageManager.EditRole(SelectedRole.RoleID);
                    break;
                case 2:
                    MessageManager.EditPermit(SelectedPermit.PermitID);
                    break;
                default:
                    break;
            }
        }
        public RelayCommand DeleteItemCommand { get; set; }
        private void DeleteItem()
        {
            switch (TabIndex)
            {
                case 0:
                    Users.Remove(SelectedUser);
                    break;
                case 1:
                    Roles.Remove(SelectedRole);
                    break;
                case 2:
                    Permits.Remove(SelectedPermit);
                    break;
                default:
                    break;
            }
            DB.GammaBase.SaveChanges();
        }
    }
}