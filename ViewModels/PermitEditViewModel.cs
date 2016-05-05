using System;
using Gamma.Models;
using System.Collections.ObjectModel;
using System.Linq;
using DevExpress.Mvvm;

namespace Gamma.ViewModels
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class PermitEditViewModel : SaveImplementedViewModel
    {
        /// <summary>
        /// Initializes a new instance of the NewPermitViewModel class.
        /// </summary>

        public PermitEditViewModel(GammaEntities gammaBase = null): base(gammaBase)
        {
            Permit = new Permits {PermitID = SqlGuidUtil.NewSequentialid()};
            PermitTables = new ObservableCollection<PermitTables>();
            _isNewPermit = true;
            InitializeFields();
        }
        public PermitEditViewModel(Guid permitId, GammaEntities gammaBase = null): base(gammaBase)
        {
            PermitID = permitId;
            Permit = GammaBase.Permits.Include("PermitTables").FirstOrDefault(p => p.PermitID == permitId);
            PermitTables = new ObservableCollection<PermitTables>(Permit.PermitTables.ToArray());
            InitializeFields();
        }
        private void InitializeFields()
        {
//            GammaBase.PermitTables.Where(pt => pt.PermitID == PermitID).Load();
//            PermitTables = GammaBase.PermitTables.Local;
            AddTableCommand = new DelegateCommand(AddTable);
            DeleteTableCommand = new DelegateCommand(DeleteTable,() => SelectedPermitTable != null);
        }
        private bool _isNewPermit;
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
        public DelegateCommand AddTableCommand { get; set; }
        private void AddTable()
        {
            var permitTable = new PermitTables() { PermitTableID = SqlGuidUtil.NewSequentialid(), PermitID = Permit.PermitID };
            PermitTables.Add(permitTable);
        }
        public DelegateCommand DeleteTableCommand { get; set; }
        private void DeleteTable()
        {
            PermitTables.Remove(SelectedPermitTable);
        }
        public PermitTables SelectedPermitTable { get; set; }

        protected override void SaveToModel(GammaEntities gammaBase = null)
        {
            gammaBase = gammaBase ?? DB.GammaDb;
            base.SaveToModel(gammaBase);
            if (_isNewPermit)
            {
                gammaBase.Permits.Add(Permit);
            }
            else
            {
                if (!Permit.PermitTables.SequenceEqual(PermitTables))
                {
                    var toadd = PermitTables.Where(p => !Permit.PermitTables.Contains(p));
                    gammaBase.PermitTables.AddRange(toadd);
                    var todel = Permit.PermitTables.Where(p => !PermitTables.Contains(p));
                    gammaBase.PermitTables.RemoveRange(todel);
                }
            }
            Permit.PermitTables = PermitTables;
            gammaBase.SaveChanges();
        }
    }
}