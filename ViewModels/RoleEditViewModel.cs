// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using Gamma.Common;
using Gamma.Entities;

namespace Gamma.ViewModels
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
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
            Permits = new ObservableCollection<Permits>(GammaBase.Permits.Select(p => p));
        }
        public RoleEditViewModel(Guid roleID)
        {
            Marks = new PermissionMark().ToDictionary();
            Role = GammaBase.Roles.Include("RolePermits").FirstOrDefault(r => r.RoleID == roleID);
            RolePermits = new ObservableCollection<RolePermits>(Role.RolePermits);
            Permits = new ObservableCollection<Permits>(GammaBase.Permits.Select(p => p));
        }
        private readonly bool _isNewRole;
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

        public override bool SaveToModel()
        {
            if (_isNewRole)
            {
                GammaBase.Roles.Add(Role);
                GammaBase.RolePermits.AddRange(RolePermits);
            }
            else
            {
                if (!Role.RolePermits.SequenceEqual(RolePermits))
                {
                    var toadd = RolePermits.Where(p => !Role.RolePermits.Contains(p));
                    GammaBase.RolePermits.AddRange(toadd);
                    var todel = Role.RolePermits.Where(p => !RolePermits.Contains(p));
                    GammaBase.RolePermits.RemoveRange(todel);
                }
            }
            GammaBase.SaveChanges();
            if (!_isNewRole) DB.RecreateRolePermits(Role.RoleID);
            return true;
        }
        //private Dictionary<int, string> _marks = 
        public Dictionary<byte,string> Marks { get; set; }
        public ObservableCollection<Permits> Permits { get; set; } 
    }
}