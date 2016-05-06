﻿using System;
using System.Linq;
using System.Text;
using FastReport;
using FastReport.Data;
using System.IO;
using Gamma.Dialogs;
using Gamma.Models;
using FastReport.Design;
using System.Security.Cryptography;

namespace Gamma
{
    class ReportManager
    {          
        static ReportManager()
        {
            GammaBase = DB.GammaDb;
            _reportSettings.CustomSaveReport += SaveReport;
            _reportSettings.PreviewSettings.Buttons = (PreviewButtons.Print | PreviewButtons.Save);
            _reportSettings.PreviewSettings.ShowInTaskbar = true;
        }
        private static GammaEntities GammaBase { get; set; }
        public static BarViewModel GetReportBar(string reportObject, Guid? vmid = null)
        {
            if (!GammaBase.Reports.Any(rep => rep.Name == reportObject) && DB.HaveWriteAccess("Reports"))
            {
                GammaBase.Reports.Add(new Reports()
                {
                    ReportID = SqlGuidUtil.NewSequentialid(),
                    IsReport = false,
                    Name = reportObject
                });
                GammaBase.SaveChanges();
            }
            var reports = from rep in GammaBase.Reports
                          join parrep in GammaBase.Reports on rep.ParentID equals parrep.ReportID where parrep.Name == reportObject
                          select rep;
            var bar = new BarViewModel();
            foreach (Reports report in reports)
            {
                var command = new BarCommand<PrintReportMessage>(msg => MessageManager.PrintReport(msg))
                {
                    Caption = report.Name,
                    CommandParameter = new PrintReportMessage()
                    {
                        ReportID = report.ReportID,
                        VMID = vmid
                    }
                };
                bar.Commands.Add(command);
            }
            return bar;
        }
        public static void PrintReport(Guid reportid, Guid? paramId = null, bool showPreview = true)
        {
            using (var report = new Report())
            {
                var reportTemplate = (from rep in GammaBase.Templates where rep.ReportID == reportid select rep.Template).FirstOrDefault();
                if (reportTemplate == null) return;
                var stream = new MemoryStream(reportTemplate);                
                report.Load(stream);
                var paramBeginDate = report.Parameters.FindByName("ParamBeginDate"); // new Parameter("ParamBeginDate");
                var paramEndDate = report.Parameters.FindByName("ParamEndDate");
                if (paramBeginDate != null && paramEndDate != null)
                {
                    var dialog = new FilterDateReportDialog();
                    dialog.ShowDialog();
                    if (dialog.DialogResult == true)
                    {
                        paramBeginDate.Value = dialog.DateBegin;
                        paramEndDate.Value = dialog.DateEnd;
                    }
                    else return;
                }
                if (paramId != null) report.SetParameterValue("ParamID", paramId);
                report.Dictionary.Connections[0].ConnectionString = GammaSettings.SqlConnectionString;
                if (showPreview)
                    report.Show();
                else
                {
                    report.PrintSettings.ShowDialog = false;
                    report.Print();
                }
            }
        }
        public static void PrintReport(string reportName, string reportFolder = null, Guid? ParamID = null, bool showPreview = true)
        {
            var parentid = GammaBase.Reports.Where(r => r.Name == reportFolder).Select(r => r.ReportID).FirstOrDefault();
            var reports = GammaBase.Reports.Where(r => r.Name == reportName && (parentid == null || r.ParentID == parentid)).
                Select(r => r.ReportID).ToList();
            if (reports.Count == 1)
            {
                PrintReport(reports[0], ParamID, showPreview);
            }
        }
        public static void DesignReport(Guid reportid)
        {
            CurrentReportID = reportid;
            var repTemplate = (from rep in GammaBase.Templates where rep.ReportID == reportid select rep.Template).FirstOrDefault();
            using (var report = new Report())
            {
                if (repTemplate != null)
                {
                    report.Load(new MemoryStream(repTemplate));
                    report.FileName = (from rep in GammaBase.Reports where rep.ReportID == reportid select rep.Name).FirstOrDefault();
                    report.Dictionary.Connections[0].ConnectionString = GammaSettings.SqlConnectionString;
                }
                else
                {
                    report.FileName = (from rep in GammaBase.Reports where rep.ReportID == reportid select rep.Name).FirstOrDefault();
                    var conn = new MsSqlDataConnection
                    {
                        Name = "GammaConnection",
                        ConnectionString = GammaSettings.SqlConnectionString
                    };
                    report.Dictionary.Connections.Add(conn);
                    var ParamID = new Parameter("Paramid") {DataType = typeof (Guid)};
                    report.Parameters.Add(ParamID);
                }
                report.Design();
            }
        }

        private static EnvironmentSettings _reportSettings = new EnvironmentSettings();
        private static void SaveReport(object sender, OpenSaveReportEventArgs e)
        {
            var reportTemplate = (from template in GammaBase.Templates
                                  where template.ReportID == CurrentReportID
                                  select template).FirstOrDefault();
            if (reportTemplate == null)
            {
                reportTemplate = new Templates();
                reportTemplate.TemplateID = SqlGuidUtil.NewSequentialid();
                reportTemplate.Name = e.FileName;
                reportTemplate.ReportID = CurrentReportID;
                GammaBase.Templates.Add(reportTemplate);
            }
            using (var stream = new MemoryStream())
                {
                    e.Report.Save(stream);
                    reportTemplate.Template = stream.ToArray();
                    using (var md5 = MD5.Create())
                    {
                        reportTemplate.Version = Encoding.Default.GetString(md5.ComputeHash(stream));
                    }
                }
            GammaBase.SaveChanges();
        }
        private static Guid CurrentReportID { get; set; }
        
    }
}
