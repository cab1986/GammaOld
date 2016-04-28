using System;
using Gamma.Models;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;

namespace Gamma.ViewModels
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class RoleEditViewModel : SaveImplementedViewModel
    {
        /// <summary>
        /// Initializes a new instance of the RoleEditViewModel class.
        /// </summary>
        public RoleEditViewModel()
        {
            _isNewRole = true;
            Role = new Roles() { RoleID = SqlGuidUtil.NewSequentialid() };
            RolePermits = new ObservableCollection<RolePermits>();
        }
        public RoleEditViewModel(Guid roleID)
        {
            Marks = new PermissionMark().ToDictionary();
            Role = DB.GammaBase.Roles.Include("RolePermits").FirstOrDefault(r => r.RoleID == roleID);
            RolePermits = new ObservableCollection<RolePermits>(Role.RolePermits);
        }
        private bool _isNewRole;
        private Roles _role;
        public Roles Role
        {
            get
            {
                return _role;
            }
            set
            {
            	_role = value;
                RaisePropertyChanged("Role");
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

        protected override void SaveToModel(GammaEntities gammaBase = null)
        {
            gammaBase = gammaBase ?? DB.GammaDb;
            base.SaveToModel(gammaBase);
            if (_isNewRole)
            {
                gammaBase.Roles.Add(Role);
                gammaBase.RolePermits.AddRange(RolePermits);
            }
            else
            {
                if (!Role.RolePermits.SequenceEqual(RolePermits))
                {
                    var toadd = RolePermits.Where(p => !Role.RolePermits.Contains(p));
                    gammaBase.RolePermits.AddRange(toadd);
                    var todel = Role.RolePermits.Where(p => !RolePermits.Contains(p));
                    gammaBase.RolePermits.RemoveRange(todel);
                }
            }
            gammaBase.SaveChanges();
            if (!_isNewRole) DB.RecreateRolePermits(Role.RoleID);
        }
        //private Dictionary<int, string> _marks = 
        public Dictionary<byte,string> Marks { get; set; }
        private ObservableCollection<Permits> _permits = new ObservableCollection<Permits>(DB.GammaBase.Permits.Select(p => p));
        public ObservableCollection<Permits> Permits
        {
            get
            {
                return _permits;
            }
            set
            {
            	_permits = value;
            }
        }
    }
}