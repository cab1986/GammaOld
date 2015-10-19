using GalaSoft.MvvmLight;
using Gamma.Models;
using System.Collections.ObjectModel;
using System.Data.Entity;
using GalaSoft.MvvmLight.Messaging;

namespace Gamma.ViewModels
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class ManageUsersViewModel : ViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the ManageUsersViewModel class.
        /// </summary>
        public ManageUsersViewModel()
        {
            DB.GammaBase.Users.Load();
            DB.GammaBase.Places.Load();
            DB.GammaBase.Roles.Load();
            DB.GammaBase.RolePermits.Load();
            DB.GammaBase.Permits.Load();
            Users = DB.GammaBase.Users.Local;
            Places = DB.GammaBase.Places.Local;
            Roles = DB.GammaBase.Roles.Local;
            Permits = DB.GammaBase.Permits.Local;
            RolePermits = DB.GammaBase.RolePermits.Local;
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
    }
}