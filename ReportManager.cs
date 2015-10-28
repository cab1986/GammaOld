using System;
using System.Collections.Generic;
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
        }
        
        public static BarViewModel GetReportBar(string ReportObject)
        {
            var reports = from rep in DB.GammaBase.Reports
                          join parrep in DB.GammaBase.Reports on rep.ParentID equals parrep.ReportID where parrep.Name == ReportObject
                          select rep;
            var bar = new BarViewModel();
            foreach (Reports report in reports)
            {
                var command = new BarCommand<Object>(ID => PrintReport((Guid)ID)) { Caption = report.Name,CommandParameter = report.ReportID };
                bar.Commands.Add(command);
            }
            return bar;
        }
        private void EventPrintReport(Guid reportID)
        {
            var msg = new PrintReportMessage() { ReportID = reportID };
            MessageManager.PrintReport(msg);
        }
        public static void PrintReport(Guid reportID, Guid? ParamID = null)
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
                if (ParamID != null) report.SetParameterValue("ParamID", ParamID);
                report.Dictionary.Connections[0].ConnectionString = GammaSettings.SQLConnectionString;
                report.Show();
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
                reportTemplate = new Models.Templates();
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
