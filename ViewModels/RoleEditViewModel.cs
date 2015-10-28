using System;
using Gamma.Models;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using Gamma;
using System.Linq;

namespace Gamma.ViewModels
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class RoleEditViewModel : DataBaseEditViewModel
    {
        /// <summary>
        /// Initializes a new instance of the RoleEditViewModel class.
        /// </summary>
        public RoleEditViewModel()
        {
            IsNewRole = true;
            Role = new Roles() { RoleID = SQLGuidUtil.NewSequentialId() };
            RolePermits = new ObservableCollection<RolePermits>();
        }
        public RoleEditViewModel(Guid roleID)
        {
            Marks = new PermissionMark().ToDictionary();
            Role = DB.GammaBase.Roles.Include("RolePermits").Where(r => r.RoleID == roleID).FirstOrDefault();
            RolePermits = new ObservableCollection<RolePermits>(Role.RolePermits);
        }
        private bool IsNewRole = false;
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
        public override void SaveToModel()
        {
            base.SaveToModel();
            if (IsNewRole)
            {
                DB.GammaBase.Roles.Add(Role);
                DB.GammaBase.RolePermits.AddRange(RolePermits);
            }
            else
            {
                if (!Role.RolePermits.SequenceEqual(RolePermits))
                {
                    var toadd = RolePermits.Where(p => !Role.RolePermits.Contains(p));
                    DB.GammaBase.RolePermits.AddRange(toadd);
                    var todel = Role.RolePermits.Where(p => !RolePermits.Contains(p));
                    DB.GammaBase.RolePermits.RemoveRange(todel);
                }
            }
            DB.GammaBase.SaveChanges();
            if (!IsNewRole) DB.RecreateRolePermits(Role.RoleID);
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