// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using FastReport;
using FastReport.Data;
using FastReport.Design;
using Gamma.Common;
using Gamma.Dialogs;
using Gamma.Entities;
using System.Collections.Generic;
using Gamma.Models;

namespace Gamma
{
	internal static class ReportManager
	{
		static ReportManager()
		{
			GammaBase = DB.GammaDb;
			_reportSettings.CustomSaveReport += SaveReport;
			_reportSettings.PreviewSettings.Buttons = PreviewButtons.Print | PreviewButtons.Save;
			_reportSettings.PreviewSettings.ShowInTaskbar = true;
		}

		private static GammaEntities GammaBase { get; }

        public static BarViewModel GetReportBar(string reportObject, Guid? vmid = null)
		{
			if (!GammaBase.Reports.Any(rep => rep.Name == reportObject) && DB.HaveWriteAccess("Reports"))
			{
				GammaBase.Reports.Add(new Reports
				{
					ReportID = SqlGuidUtil.NewSequentialid(),
					IsReport = false,
					Name = reportObject
				});
				GammaBase.SaveChanges();
			}
			var reports = from rep in GammaBase.Reports
				join parrep in GammaBase.Reports on rep.ParentID equals parrep.ReportID
				where parrep.Name == reportObject
				orderby rep.Name
				select rep;
			var bar = new BarViewModel();
			foreach (var report in reports)
			{
				var command = new BarCommand<object>(msg => MessageManager.PrintReport((PrintReportMessage) msg))
				{
					Caption = report.Name,
					CommandParameter = new PrintReportMessage
					{
						ReportID = report.ReportID,
						VMID = vmid
					}
				};
				bar.Commands.Add(command);
			}
			return bar;
		}

        public static void PrintReport(Guid reportid, ReportParameters reportParams, bool showPreview = true, int numCopies = 1)
        {
            UIServices.SetBusyState();
            DB.AddLogMessageInformation("PrintReport: " + reportid.ToString() + " ReportParams: " + reportParams.ToString());
            using (var report = new Report())
            {
                var reportTemplate = (from rep in GammaBase.Templates where rep.ReportID == reportid select rep.Template)
                    .FirstOrDefault();
                if (reportTemplate == null) return;
                var stream = new MemoryStream(reportTemplate);
                report.Load(stream);
                report.ConvertNulls = false;
                if (reportParams.BeginDate == null)
                    report.SetParameterValue("ParamBeginDate", System.DBNull.Value);
                else
                    report.SetParameterValue("ParamBeginDate", reportParams.BeginDate);
                if (reportParams.EndDate == null)
                    report.SetParameterValue("ParamEndDate", System.DBNull.Value);
                else
                    report.SetParameterValue("ParamEndDate", reportParams.EndDate);
                if (reportParams.PlaceID == null)
                    report.SetParameterValue("ParamPlaceID", System.DBNull.Value);
                else
                    report.SetParameterValue("ParamPlaceID", reportParams.PlaceID);
                if (reportParams.PlaceZoneID == null)
                    report.SetParameterValue("ParamPlaceZoneID", System.DBNull.Value);
                else
                    report.SetParameterValue("ParamPlaceZoneID", reportParams.PlaceZoneID);
                if (reportParams.ProductKindID == null)
                    report.SetParameterValue("ParamProductKindID", System.DBNull.Value);
                else
                    report.SetParameterValue("ParamProductKindID", reportParams.ProductKindID);
                if (reportParams.StateID == null)
                    report.SetParameterValue("ParamStateID", System.DBNull.Value);
                else
                    report.SetParameterValue("ParamStateID", reportParams.StateID);
                if (reportParams.NomenclatureID == null)
                    report.SetParameterValue("ParamNomenclatureID", System.DBNull.Value);
                else
                    report.SetParameterValue("ParamNomenclatureID", reportParams.NomenclatureID);
                if (reportParams.CharacteristicID == null)
                    report.SetParameterValue("ParamCharacteristicID", System.DBNull.Value);
                else
                    report.SetParameterValue("ParamCharacteristicID", reportParams.CharacteristicID);
                //if (reportParams.IsVisibleDetailBand == null)
                //    report.SetParameterValue("ParamIsVisibleDetailBand", System.DBNull.Value);
                //else
                    report.SetParameterValue("ParamIsVisibleDetailBand", reportParams.IsVisibleDetailBand ?? false);

                if (reportParams.ID == null)
                    report.SetParameterValue("ParamID", System.DBNull.Value);
                else
                    report.SetParameterValue("ParamID", reportParams.ID);
                if (string.IsNullOrEmpty(report.Dictionary.Connections[0].ConnectionString))
                {
                    report.Dictionary.Connections[0].ConnectionString = GammaSettings.SqlConnectionString;
                }
                else
                {
                    var reportConnectionBuilder = new SqlConnectionStringBuilder(report.Dictionary.Connections[0].ConnectionString);
                    var settingsConnectionBuilder = new SqlConnectionStringBuilder(GammaSettings.SqlConnectionString);
                    reportConnectionBuilder.UserID = settingsConnectionBuilder.UserID;
                    reportConnectionBuilder.Password = settingsConnectionBuilder.Password;
                    reportConnectionBuilder.DataSource = settingsConnectionBuilder.DataSource;
                    reportConnectionBuilder.InitialCatalog = settingsConnectionBuilder.InitialCatalog;
                    reportConnectionBuilder.ApplicationName = settingsConnectionBuilder.ApplicationName.Replace("Gamma", "Gamma (report)");
                    report.Dictionary.Connections[0].ConnectionString = reportConnectionBuilder.ToString();
                }

                if (showPreview)
                {
                    report.Show();
                }
                else
                {
                    report.PrintSettings.ShowDialog = false;
                    report.PrintSettings.Copies = numCopies;
                    report.Print();
                }
            }
        }

        public static void PrintReport(Guid reportid, Guid? paramId = null, bool showPreview = true, int numCopies = 1)
		{
			UIServices.SetBusyState();
			using (var report = new Report())
			{
				var reportTemplate = (from rep in GammaBase.Templates where rep.ReportID == reportid select rep.Template)
					.FirstOrDefault();
				if (reportTemplate == null) return;
				var stream = new MemoryStream(reportTemplate);
				report.Load(stream);
				var paramBeginDate = report.Parameters.FindByName("ParamBeginDate"); // new Parameter("ParamBeginDate");
				var paramEndDate = report.Parameters.FindByName("ParamEndDate");
                var paramPlaceID = report.Parameters.FindByName("ParamPlaceID");
                DateTime? paramBeginDateValue = null;
                DateTime? paramEndDateValue = null;
                int? paramPlaceIDValue = null;
                if (paramBeginDate != null | paramEndDate != null | paramPlaceID != null)
				{
					var dialog = new FilterDateReportDialog();
                    dialog.lcDate.Visibility = (paramBeginDate != null || paramEndDate != null) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
                    dialog.lcPlace.Visibility = (paramPlaceID != null) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed; 
                    dialog.ShowDialog();
					if (dialog.DialogResult == true)
					{
                        if (paramBeginDate!= null) paramBeginDateValue = dialog.DateBegin;
                        if (paramEndDate != null) paramEndDateValue = dialog.DateEnd;
                        if (paramPlaceID != null) paramPlaceIDValue = dialog.PlaceID;
                    }
					else
					{
						return;
					}
				}
                PrintReport(reportid, new ReportParameters() { ID = paramId, BeginDate = paramBeginDateValue, EndDate = paramEndDateValue, PlaceID = paramPlaceIDValue }, showPreview, numCopies);
            }
		}

		public static void PrintReport(string reportName, string reportFolder = null, Guid? paramID = null,
			bool showPreview = true, int numCopies = 1)
		{
			var parentId = GammaBase.Reports.Where(r => r.Name == reportFolder).Select(r => r.ReportID).FirstOrDefault();
			var reports = GammaBase.Reports.Where(r => r.Name == reportName && (parentId == null || r.ParentID == parentId))
				.Select(r => r.ReportID)
				.ToList();
			if (reports.Count == 1)
				PrintReport(reports[0], paramID, showPreview, numCopies);
		}

        public static void PrintReport(string reportName, string reportFolder, ReportParameters reportParameters,
            bool showPreview = true, int numCopies = 1)
        {
            var parentId = reportFolder == null || reportFolder == "" || reportFolder == String.Empty ? (Guid?)null : GammaBase.Reports.Where(r => r.Name == reportFolder).Select(r => r.ReportID).FirstOrDefault();
            var reports = GammaBase.Reports.Where(r => r.Name == reportName && (parentId == null || r.ParentID == parentId))
                .Select(r => r.ReportID)
                .ToList();
            if (reports.Count == 1)
                PrintReport(reports[0], reportParameters, showPreview, numCopies);
        }

        public static void DesignReport(Guid reportid)
		{
			CurrentReportID = reportid;
			var repTemplate =
				(from rep in GammaBase.Templates where rep.ReportID == reportid select rep.Template).FirstOrDefault();
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
					var paramId = new Parameter("ParamID") {DataType = typeof(Guid)};
					report.Parameters.Add(paramId);
				}
				report.Design();
			}
		}

		private static readonly EnvironmentSettings _reportSettings = new EnvironmentSettings();

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
