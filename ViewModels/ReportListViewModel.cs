using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Gamma.Models;
using System;
using System.Collections.ObjectModel;
using System.Data.Entity;

namespace Gamma.ViewModels
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class ReportListViewModel : ViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the ReportListViewModel class.
        /// </summary>
        public ReportListViewModel()
        {
            DB.GammaBase.Reports.Load();
            Reports = DB.GammaBase.Reports.Local;
            NewReportFolderCommand = new RelayCommand(NewReportFolder);
            NewReportCommand = new RelayCommand(NewReport, () => SelectedReport == null ? false : !SelectedReport.IsReport);
            EditReportCommand = new RelayCommand(EditReport, () => SelectedReport == null ? false : SelectedReport.IsReport);
        }

        private ObservableCollection<Reports> _reports;
        public ObservableCollection<Reports> Reports
        {
            get
            {
                return _reports;
            }
            set
            {
                _reports = value;
                RaisePropertyChanged("Reports");
            }
        }
        public RelayCommand NewReportFolderCommand { get; private set; }
        public RelayCommand NewReportCommand { get; private set; }
        public RelayCommand EditReportCommand { get; private set; }
        public RelayCommand DeleteReportCommand { get; private set; }

        private void NewReportFolder()
        {
            var report = new Reports();
            report.IsReport = false;
            report.ReportID = Guid.NewGuid();
            Reports.Add(report);
            DB.GammaBase.Reports.Add(report);
            SelectedReport = report;
        }
        private Reports _selectedReport;
        public Reports SelectedReport
        {
            get
            {
                return _selectedReport;
            }
            set
            {
                _selectedReport = value;
                RaisePropertyChanged("SelectedReport");
            }
        }
        private void NewReport()
        {
            var report = new Reports();
            report.IsReport = true;
            report.ParentID = SelectedReport.ReportID;
            report.ReportID = Guid.NewGuid();
            Reports.Add(report);
            SelectedReport = report;
        }
        private void EditReport()
        {
            DB.GammaBase.SaveChanges();
            ReportManager.DesignReport(SelectedReport.ReportID);
        }
    }
}