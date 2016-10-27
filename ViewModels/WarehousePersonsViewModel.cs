using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using DevExpress.Mvvm;
using Gamma.Models;

namespace Gamma.ViewModels
{
    /// <summary>
    /// Класс для отображения работников склада и управления ими
    /// </summary>
    public class WarehousePersonsViewModel : RootViewModel
    {
        public WarehousePersonsViewModel(GammaEntities gammaBase = null) : base(gammaBase)
        {
            AddPersonCommand = new DelegateCommand(AddPerson, () => !string.IsNullOrWhiteSpace(NewPersonName));
            DeletePersonCommand = new DelegateCommand(DeletePerson, () => SelectedPerson != null);
            RefreshPersons();
            Bars.Add(ReportManager.GetReportBar("PersonsList", VMID));
            Bars.Add(new BarViewModel
            {
                Name = "",
                Commands = new ObservableCollection<BarCommand<object>>
                {
                    new BarCommand<object>(p => DeletePerson())
                    {
                        Caption = "Удалить"
                    }
                }
            });
            Messenger.Default.Register<PrintReportMessage>(this, PrintReport);
        }

        private void PrintReport(PrintReportMessage msg)
        {
            if (msg.VMID != VMID) return;
            ReportManager.PrintReport(msg.ReportID);
        }

        private Guid VMID { get; set; } = Guid.NewGuid();

        private void AddPerson()
        {
            if (string.IsNullOrWhiteSpace(NewPersonName))
            {
                MessageBox.Show("Новое имя не заполнено", "Ошибка имени", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                return;
            }
            GammaBase.Persons.Add(new Persons
            {
                Name = NewPersonName,
                PostTypeID = 1
            });
            GammaBase.SaveChanges();
            RefreshPersons();
        }

        private void DeletePerson()
        {
            if (SelectedPerson == null) return;
            if (GammaBase.DocShipmentOrders.Any(d => d.OutActivePersonID == SelectedPerson.PersonId || d.InActivePersonID == SelectedPerson.PersonId) ||
                GammaBase.DocInProducts.Any(d => d.PersonID == SelectedPerson.PersonId) ||
                GammaBase.DocOutProducts.Any(d => d.PersonID == SelectedPerson.PersonId))
            {
                MessageBox.Show("Данный сотрудник уже собирал приказы или ему назначен приказ. Удалить невозможно",
                    "Удаление", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                return;
            }
            GammaBase.Persons.Remove(GammaBase.Persons.FirstOrDefault(p => p.PersonID == SelectedPerson.PersonId));
            GammaBase.SaveChanges();
            RefreshPersons();
        }

        private void RefreshPersons()
        {
            Persons = new ObservableCollection<Person>(GammaBase.Persons.Where(p => p.PostTypeID == 1 && p.BranchID == WorkSession.BranchID).Select(p => new Person
            {
                PersonId = p.PersonID,
                Name = p.Name
            }));
        }

        public Person SelectedPerson { get; set; }

        public string NewPersonName { get; set; }

        public DelegateCommand AddPersonCommand { get; set; }
        public DelegateCommand DeletePersonCommand { get; private set; }

        private ObservableCollection<Person> _persons;

        public ObservableCollection<Person> Persons
        {
            get { return _persons; }
            set
            {
                _persons = value;
                RaisePropertyChanged("Persons");
            }
        }

        private ObservableCollection<BarViewModel> _bars = new ObservableCollection<BarViewModel>();

        public ObservableCollection<BarViewModel> Bars
        {
            get
            {
                return _bars;
            }
            set
            {
                _bars = value;
                RaisePropertyChanged("Bars");
            }
        }

    }
}
