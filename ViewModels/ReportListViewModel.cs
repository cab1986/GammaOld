using DevExpress.Mvvm;
using Gamma.Models;
using System;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Windows;

namespace Gamma.ViewModels
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class ReportListViewModel : SaveImplementedViewModel
    {
        /// <summary>
        /// Initializes a new instance of the ReportListViewModel class.
        /// </summary>
        public ReportListViewModel()
        {
            DB.GammaBase.Reports.Load();
            Reports = DB.GammaBase.Reports.Local;
            NewReportFolderCommand = new DelegateCommand(NewReportFolder);
            NewReportCommand = new DelegateCommand(NewReport, () => SelectedReport == null ? false : !SelectedReport.IsReport);
            EditReportCommand = new DelegateCommand(EditReport, () => SelectedReport == null ? false : SelectedReport.IsReport);
            DeleteReportCommand = new DelegateCommand(DeleteReport, () => SelectedReport != null);
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
        public DelegateCommand NewReportFolderCommand { get; private set; }
        public DelegateCommand NewReportCommand { get; private set; }
        public DelegateCommand EditReportCommand { get; private set; }
        public DelegateCommand DeleteReportCommand { get; private set; }

        private void NewReportFolder()
        {
            var report = new Reports();
            report.IsReport = false;
            report.ReportID = Guid.NewGuid();
            Reports.Add(report);
//            DB.GammaBase.Reports.Add(report);
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
            var report = new Reports() 
            { 
                IsReport = true, 
                ParentID = SelectedReport.ReportID, 
                ReportID = SQLGuidUtil.NewSequentialId(),
                Name = ""
            };
            Reports.Add(report);
//            DB.GammaBase.SaveChanges();
            SelectedReport = report;
        }
        private void EditReport()
        {
            DB.GammaBase.SaveChanges();
            ReportManager.DesignReport(SelectedReport.ReportID);
        }
        private void DeleteReport()
        {
            var msgResult = MessageBox.Show("Вы уверены, что хотите удалить данный рапорт?", "Удаление рапорта", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (msgResult != MessageBoxResult.Yes) return;
            if (!SelectedReport.IsReport)
            {
                DB.GammaBase.Templates.RemoveRange(DB.GammaBase.Templates.Where(tmpl => tmpl.Reports.ParentID == SelectedReport.ReportID).Select(tmpl => tmpl));
                var reportsToRemove = DB.GammaBase.Reports.Where(rep => rep.ParentID == SelectedReport.ReportID).Select(rep => rep);
                foreach (var rep in reportsToRemove)
                {
                    Reports.Remove(rep);
                }
            }
            else
            {
                DB.GammaBase.Templates.Remove(DB.GammaBase.Templates.Where(tmpl => tmpl.ReportID == SelectedReport.ReportID).FirstOrDefault());
            }
            DB.GammaBase.Reports.Remove(SelectedReport);
            DB.GammaBase.SaveChanges();
        }
    }
}