// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using DevExpress.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using Gamma.Entities;

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
        public ReportListViewModel(GammaEntities gammaBase = null): base(gammaBase)
        {
            GammaBase.Reports.Load();
            Reports = GammaBase.Reports.Local;
            NewReportFolderCommand = new DelegateCommand(NewReportFolder);
            NewReportCommand = new DelegateCommand(NewReport, () => !SelectedReport?.IsReport ?? false);
            EditReportCommand = new DelegateCommand(EditReport, () => SelectedReport?.IsReport ?? false);
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
//            GammaBase.Reports.Add(report);
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
                ReportID = SqlGuidUtil.NewSequentialid(),
                Name = ""
            };
            Reports.Add(report);
//            GammaBase.SaveChanges();
            SelectedReport = report;
        }
        private void EditReport()
        {
            GammaBase.SaveChanges();
            ReportManager.DesignReport(SelectedReport.ReportID);
        }
        private void DeleteReport()
        {
            var msgResult = MessageBox.Show("Вы уверены, что хотите удалить данный рапорт?", "Удаление рапорта", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (msgResult != MessageBoxResult.Yes) return;
            if (!SelectedReport.IsReport)
            {
                GammaBase.Templates.RemoveRange(GammaBase.Templates.Where(tmpl => tmpl.Reports.ParentID == SelectedReport.ReportID).Select(tmpl => tmpl));
                var reportsToRemove = GammaBase.Reports.Where(rep => rep.ParentID == SelectedReport.ReportID).Select(rep => rep);
                foreach (var rep in reportsToRemove)
                {
                    Reports.Remove(rep);
                }
            }
            else
            {
                GammaBase.Templates.Remove(GammaBase.Templates.FirstOrDefault(tmpl => tmpl.ReportID == SelectedReport.ReportID));
            }
            GammaBase.Reports.Remove(SelectedReport);
            GammaBase.SaveChanges();
        }
    }
}