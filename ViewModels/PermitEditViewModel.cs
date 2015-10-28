using GalaSoft.MvvmLight;
using System;
using Gamma.Models;
using System.Collections.ObjectModel;
using System.Linq;
using System.Data.Entity;
using GalaSoft.MvvmLight.Command;

namespace Gamma.ViewModels
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class PermitEditViewModel : DataBaseEditViewModel
    {
        /// <summary>
        /// Initializes a new instance of the NewPermitViewModel class.
        /// </summary>

        public PermitEditViewModel()
        {
            Permit = new Permits();
            Permit.PermitID = SQLGuidUtil.NewSequentialId();
            PermitTables = new ObservableCollection<PermitTables>();
            IsNewPermit = true;
            InitializeFields();
        }
        public PermitEditViewModel(Guid permitID)
        {
            PermitID = permitID;
            Permit = DB.GammaBase.Permits.Include("PermitTables").Where(p => p.PermitID == permitID).FirstOrDefault();
            PermitTables = new ObservableCollection<PermitTables>(Permit.PermitTables.ToArray());
            InitializeFields();
        }
        private void InitializeFields()
        {
//            DB.GammaBase.PermitTables.Where(pt => pt.PermitID == PermitID).Load();
//            PermitTables = DB.GammaBase.PermitTables.Local;
            AddTableCommand = new RelayCommand(AddTable);
            DeleteTableCommand = new RelayCommand(DeleteTable,() => SelectedPermitTable != null);
        }
        private bool IsNewPermit = false;
        private Guid PermitID { get; set; }
        private string _name;
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
            	_name = value;
                RaisePropertyChanged("Name");
            }
        }
        private ObservableCollection<string> _baseTables = DB.BaseTables;
        public ObservableCollection<string> BaseTables
        {
            get
            {
                return _baseTables;
            }
            set
            {
                _baseTables = value;
            }
        }
        private Permits _permit;
        public Permits Permit
        {
            get
            {
                return _permit;
            }
            set
            {
            	_permit = value;
                RaisePropertyChanged("Permit");
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
        public RelayCommand AddTableCommand { get; set; }
        private void AddTable()
        {
            var permitTable = new PermitTables() { PermitTableID = SQLGuidUtil.NewSequentialId(), PermitID = Permit.PermitID };
            PermitTables.Add(permitTable);
        }
        public RelayCommand DeleteTableCommand { get; set; }
        private void DeleteTable()
        {
            PermitTables.Remove(SelectedPermitTable);
        }
        public PermitTables SelectedPermitTable { get; set; }
        public override void SaveToModel()
        {
            base.SaveToModel();
            if (IsNewPermit)
            {
                DB.GammaBase.Permits.Add(Permit);
            }
            else
            {
                if (!Permit.PermitTables.SequenceEqual(PermitTables))
                {
                    var toadd = PermitTables.Where(p => !Permit.PermitTables.Contains(p));
                    DB.GammaBase.PermitTables.AddRange(toadd);
                    var todel = Permit.PermitTables.Where(p => !PermitTables.Contains(p));
                    DB.GammaBase.PermitTables.RemoveRange(todel);
                }
            }
            Permit.PermitTables = PermitTables;
            DB.GammaBase.SaveChanges();
        }
    }
}