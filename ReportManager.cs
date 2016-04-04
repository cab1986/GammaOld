using System;
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
            ReportSettings.CustomSaveReport += SaveReport;
            ReportSettings.PreviewSettings.Buttons = (PreviewButtons.Print | PreviewButtons.Save);
            ReportSettings.PreviewSettings.ShowInTaskbar = true;
        }
        public static BarViewModel GetReportBar(string ReportObject, Guid? vmID = null)
        {
            if (!DB.GammaBase.Reports.Any(rep => rep.Name == ReportObject) && DB.HaveWriteAccess("Reports"))
            {
                DB.GammaBase.Reports.Add(new Reports()
                {
                    ReportID = SQLGuidUtil.NewSequentialId(),
                    IsReport = false,
                    Name = ReportObject
                });
                DB.GammaBase.SaveChanges();
            }
            var reports = from rep in DB.GammaBase.Reports
                          join parrep in DB.GammaBase.Reports on rep.ParentID equals parrep.ReportID where parrep.Name == ReportObject
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
                        VMID = vmID
                    }
                };
                bar.Commands.Add(command);
            }
            return bar;
        }
        public static void PrintReport(Guid reportID, Guid? paramID = null)
        {
            using (var report = new Report())
            {
                var reportTemplate = (from rep in DB.GammaBase.Templates where rep.ReportID == reportID select rep.Template).FirstOrDefault();
                if (reportTemplate == null) return;
                var Stream = new MemoryStream(reportTemplate);                
                report.Load(Stream);
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
                if (paramID != null) report.SetParameterValue("ParamID", paramID);
                report.Dictionary.Connections[0].ConnectionString = GammaSettings.SQLConnectionString;
                report.Show();
            }
        }
        public static void PrintReport(string reportName, string reportFolder = null, Guid? paramID = null)
        {
            var parentID = DB.GammaBase.Reports.Where(r => r.Name == reportFolder).Select(r => r.ReportID).FirstOrDefault();
            var reports = DB.GammaBase.Reports.Where(r => r.Name == reportName && (parentID == null || r.ParentID == parentID)).
                Select(r => r.ReportID).ToList();
            if (reports.Count == 1)
            {
                PrintReport(reports[0], paramID);
            }
        }
        public static void DesignReport(Guid reportID)
        {
            CurrentReportID = reportID;
            var repTemplate = (from rep in DB.GammaBase.Templates where rep.ReportID == reportID select rep.Template).FirstOrDefault();
            using (var report = new Report())
            {
                if (repTemplate != null)
                {
                    report.Load(new MemoryStream(repTemplate));
                    report.FileName = (from rep in DB.GammaBase.Reports where rep.ReportID == reportID select rep.Name).FirstOrDefault();
                    report.Dictionary.Connections[0].ConnectionString = GammaSettings.SQLConnectionString;
                }
                else
                {
                    report.FileName = (from rep in DB.GammaBase.Reports where rep.ReportID == reportID select rep.Name).FirstOrDefault();
                    MsSqlDataConnection conn = new MsSqlDataConnection();
                    conn.Name = "GammaConnection";
                    conn.ConnectionString = GammaSettings.SQLConnectionString;
                    report.Dictionary.Connections.Add(conn);
                    Parameter paramID = new Parameter("ParamID");
                    paramID.DataType = typeof(Guid);
                    report.Parameters.Add(paramID);
                }
                report.Design();
            }
        }

        private static EnvironmentSettings ReportSettings = new EnvironmentSettings();
        private static void SaveReport(object sender, OpenSaveReportEventArgs e)
        {
            var reportTemplate = (from template in DB.GammaBase.Templates
                                  where template.ReportID == CurrentReportID
                                  select template).FirstOrDefault();
            if (reportTemplate == null)
            {
                reportTemplate = new Templates();
                reportTemplate.TemplateID = SQLGuidUtil.NewSequentialId();
                reportTemplate.Name = e.FileName;
                reportTemplate.ReportID = CurrentReportID;
                DB.GammaBase.Templates.Add(reportTemplate);
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
            DB.GammaBase.SaveChanges();
        }
        private static Guid CurrentReportID { get; set; }
        
    }
}
