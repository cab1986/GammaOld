// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using Gamma.Entities;
using Gamma.Models;
using System.ComponentModel.DataAnnotations;
using Gamma.Common;

namespace Gamma
{
    public static class DB
    {
        /// <summary>
        /// Многократноиспользуемый контекст БД
        /// </summary>
        //        private static GammaEntities GammaBase { get; set; }

        //        private static EntityConnection Connection { get; set; }

        /// <summary>
        /// Новый экземпляр контекста БД
        /// </summary>
        public static GammaEntities GammaDb
        {
            get
            {
                try
                {
                    //var context = GammaDbWithNoCheckConnection;
                    var context = new GammaEntities(GammaSettings.ConnectionString);
                    context.Database.CommandTimeout = 300;
                    context.CheckConnection();
                    return context;
                }
                catch (Exception e)
                {
                    MessageBox.Show($"Message:{e.Message} InnerMessage:{e.InnerException.Message}");
                    return null as GammaEntities;
                }
            }
        }

        public static GammaEntities GammaDbWithNoCheckConnection
        {
            get
            {
                try
                {
                    var context = new GammaEntities(GammaSettings.ConnectionString);
                    context.Database.CommandTimeout = 300;
                    return context;
                }
                catch (Exception e)
                {
                    //MessageBox.Show($"Message:{e.Message} InnerMessage:{e.InnerException.Message}");
                    return null as GammaEntities;
                }
            }
        }
        
        public static bool Initialize()
        {
//            GammaBase = new GammaEntities(GammaSettings.ConnectionString);
//            GammaBase.Database.CommandTimeout = 300;      
            try
            {
                //                GammaBase.Database.Connection.Open();
                if (DB.TimerForUploadLogToServer == null)
                {
                    Functions.ShowMessageError(@"Внимание! Не запущена автоматическая выгрузка логов на сервер.", "Error start TimerForUploadLogToServer");
                }
                var currentUser = CurrentUserID;
                if (currentUser != Guid.Empty)
                {
                    if (!WorkSession.SetUser(currentUser))
                    {
                        Functions.ShowMessageError("Не удалось получить информацию о пользователе","Error SetUser in DB.Initialize");
                        return false;
                    }
                    return true;
                }
                else
                    return false;
            }
            catch(Exception e)
            {
                var ee = e.GetType();
                MessageBox.Show($"Message:{e.Message} InnerMessage:{e.InnerException}");
                return false;
            }
        }

        public static CheckCurrentVersion_Result CheckCurrentVersion()
        {
            
            try
            {
                return GammaDb == null ? null : GammaDb.CheckCurrentVersion().FirstOrDefault();
            }
            catch (Exception e)
            {
                //var ee = e.GetType();
                //MessageBox.Show($"Message:{e.Message} InnerMessage:{e.InnerException}");
                return null;
            }
        }

        /*
                public static void RollBack()
                {
                    if (!HaveChanges()) return;
                    GammaBase.Database.Connection.Close();
                    GammaBase.Dispose();
                    GammaBase = new GammaEntities(GammaSettings.ConnectionString);
                    GammaBase.Database.Connection.Open();
                    Messenger.Default.Send(new BaseReconnectedMessage());
                }
        */
        /*
                public static bool HaveChanges()
                {
                    if (GammaBase == null) return false;
                    return GammaBase.
                           ChangeTracker.Entries().Any(e => e.State == EntityState.Added
                                                      || e.State == EntityState.Modified
                                                      || e.State == EntityState.Deleted);
                }
        */
        #region Log

        
        private static System.Threading.Timer _timerForUploadLogToServer { get; set; }
        public static System.Threading.Timer TimerForUploadLogToServer
        {
            get
            {
                if (_timerForUploadLogToServer == null)
                {
                    int num = 0;
                    System.Threading.TimerCallback tm = new System.Threading.TimerCallback(UploadLogToServerFromTimer);
                    // создаем таймер
                    _timerForUploadLogToServer = new System.Threading.Timer(tm, num, 0, WorkSession.TimerPeriodForUploadLogToServer);
                }
                return _timerForUploadLogToServer;
            }

        }

        private static bool _uploadLogToServerRunning;
        private static bool _stopUploadLogToServerRun;
        public static object lockerForUploadLogToServer = new object();
        private static bool _uploadLogToServerFromTimerRunning;

        public static void UploadLogToServerFromTimer(object obj)
        {
#if (DEBUG)
            var n = DateTime.Now.ToString();
            System.Diagnostics.Debug.Write(n + " !!!!!UploadLogToServerFromTimer(" + _uploadLogToServerFromTimerRunning.ToString() + ")!" + Environment.NewLine);
#endif
            if (_uploadLogToServerFromTimerRunning || CriticalLogList?.Count == 0) return;
            lock (lockerForUploadLogToServer)
            {
                _uploadLogToServerFromTimerRunning = true;
#if (DEBUG)
                System.Diagnostics.Debug.Write(n + " !!!!!DB.UploadLogToServer(" + _uploadLogToServerFromTimerRunning.ToString() + ")!" + Environment.NewLine);
#endif
                DB.UploadLogToServer(false);
            }
            _uploadLogToServerFromTimerRunning = false;
        }

        public static bool UploadLogToServer(bool isFirst)
        {
            bool ret = true;
            if (!_uploadLogToServerRunning && !_stopUploadLogToServerRun)
            {
                _uploadLogToServerRunning = true;
                using (var gammaDb = GammaDbWithNoCheckConnection)
                {
                    var addedLogIds = new List<Guid>();
                    var criticalLogList = new List<CriticalLog>(CriticalLogList);
#if (DEBUG)
                    System.Diagnostics.Debug.Write(DateTime.Now.ToString() + " !!!!!UploadLogToServer(date)!" + Environment.NewLine);
#endif
                    foreach (var log in criticalLogList)
                    {
                        if (_stopUploadLogToServerRun)
                            break;
                        var addCriticalLog = new CriticalLogs()
                        {
                            LogID = log.LogID,
                            LogDate = log.LogDate,
                            LogUserID = log.LogUserID,
                            HostName = log.HostName,
                            LogTypeID = log.LogTypeID,
                            Log = log.Log,
                            Image = log.Image,
                            DocID = log.DocID,
                            ProductID = log.ProductID,
                            TechnicalLog = log.TechnicalLog
                        };
                        try
                        {
                            gammaDb.CriticalLogs.Add(addCriticalLog);
#if (DEBUG)
                            System.Diagnostics.Debug.Write(DateTime.Now.ToString() + " !!!!!UploadLogToServer: LogID = " + log.LogID + Environment.NewLine);
#endif
                            gammaDb.SaveChanges();
                            addedLogIds.Add(log.LogID);
                        }
                        catch (Exception e)
                        {
                            if (e.InnerException?.InnerException is SqlException)
                            {
                                SqlException sex = e.InnerException?.InnerException as SqlException;
                                if (sex?.Number == 2627)
                                {
                                    gammaDb.CriticalLogs.Remove(addCriticalLog);
                                    addedLogIds.Add(log.LogID);
                                }
                                else
                                    ret = false;
                            }
                            else
                                ret = false;
                        }

                    }
                    CriticalLogList.RemoveAll(r => addedLogIds.Contains(r.LogID));
                    if (addedLogIds?.Count > 0)
                    try
                    {
                        using (var gammaLogDb = LogDbContext)
                        {
                            var criticalLogs = gammaLogDb.CriticalLogs.Where(r => addedLogIds.Contains(r.LogID));
                            if (criticalLogs != null)
                            //foreach (var logDel in criticalLogs)
                            {
                                gammaLogDb.CriticalLogs.RemoveRange(criticalLogs);
                                //gammaLogDb.CriticalLogs.Remove(logDel);
                                gammaLogDb.SaveChanges();
                            }
                        }
                    }
                    catch (Exception e) { }
                }
                _uploadLogToServerRunning = false;
            }
            return ret;
        }
                
        public class CompactDBContext : DbContext
        {
            public CompactDBContext()
                : base(GetConnectionString())
            {   
                Database.SetInitializer(new DropCreateDatabaseIfModelChanges<CompactDBContext>());
                //Database.SetInitializer(new DropCreateDatabaseAlways<LocalDBContext>());
            }
            public DbSet<CriticalLog> CriticalLogs { get; set; }

            public static string GetConnectionString()
            {
                var conStr = System.Configuration.ConfigurationManager.ConnectionStrings["CompactDBContext"].ConnectionString;
                return conStr.Replace("|APPDATA|",
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
            }
        }

        public partial class CriticalLog
        {
            [Key]
            public System.Guid LogID { get; set; }
            public string Log { get; set; }
            public Nullable<System.DateTime> LogDate { get; set; }
            public string LogUserID { get; set; }
            public string HostName { get; set; }
            public Nullable<int> LogTypeID { get; set; }
            public byte[] Image { get; set; }
            public Nullable<System.Guid> DocID { get; set; }
            public Nullable<System.Guid> ProductID { get; set; }
            public string TechnicalLog { get; set; }
        }

        


        private static CompactDBContext _logDbContext { get; set; }
        public static CompactDBContext LogDbContext
        {
            get
            {
                try
                {
                    //if (_logDbContext == null)
                    {
                        _logDbContext = new CompactDBContext();
                        //_logDbContext.Database.CommandTimeout = 300;
                        //_logDbContext.Database.ExecuteSqlCommand(@"DROP TABLE IF EXISTS CriticalLogs");
                        //_logDbContext.Database.ExecuteSqlCommand(
                        //@"CREATE TABLE IF NOT EXISTS CriticalLogs(LogID TEXT PRIMARY KEY,Log TEXT NULL,LogDate TEXT NULL,LogUserID TEXT NULL,HostName TEXT NULL,LogTypeID INTEGER NULL,Image BLOB NULL,DocID TEXT NULL,ProductID TEXT NULL,TechnicalLog TEXT NULL)"
                        /*  @"
                      CREATE TABLE [CriticalLogs](
                          [LogID] [uniqueidentifier] NOT NULL,
                          [Log] [varchar](2000) NULL,
                          [LogDate] [datetime] NULL,
                          [LogUserID] [varchar](150) NULL,
                          [HostName] [varchar](150) NULL,
                          [LogTypeID] [int] NULL,
                          [Image] [varbinary](max) NULL,
                          [DocID] [uniqueidentifier] NULL,
                          [ProductID] [uniqueidentifier] NULL,
                          [TechnicalLog] [varchar](1000) NULL
                          )"
                      );*/
                    }
                    return _logDbContext;
                }
                catch (Exception e)
                {
                    //MessageBox.Show($"Message:{e.Message} InnerMessage:{e.InnerException.Message}");
                    return null as CompactDBContext;
                }
            }
        }

        private static List<CriticalLog> _criticalLogList { get; set; }
        private static List<CriticalLog> CriticalLogList
        {
            get
            {
                if (_criticalLogList == null)
                {
                    using (var gammaLogDb = LogDbContext)
                    {
                        _criticalLogList = new List<CriticalLog>();
                        if (gammaLogDb.CriticalLogs.Count() > 0)
                            try
                            {
                                foreach(var log in gammaLogDb.CriticalLogs)
                                {
                                    _criticalLogList.Add(new CriticalLog()
                                    {
                                        LogID = log.LogID,
                                        LogDate = log.LogDate,
                                        LogUserID = log.LogUserID,
                                        HostName = log.HostName,
                                        LogTypeID = log.LogTypeID,
                                        Log = log.Log,
                                        Image = log.Image,
                                        DocID = log.DocID,
                                        ProductID = log.ProductID,
                                        TechnicalLog = log.TechnicalLog
                                    });
                                }
                                AddLogMessageStartProgramInformation("Невыгруженные записи в лог предыдущего сеанса (кол-во = "+ gammaLogDb.CriticalLogs.Count() + ")", "Download logs from LocalLogDb with starting program (count = " + gammaLogDb.CriticalLogs.Count() + ")");
                            }
                            catch { }
                    }
                }
                return _criticalLogList;
            }
        }

        public static void AddLogMessageInformation(string log, string technicalLog = null, Guid? docID = null, Guid? productID = null) => AddLogMessage(log, CriticalLogTypes.Information, technicalLog, docID, productID);
        public static void AddLogMessageQuestion(string log, string technicalLog = null, Guid? docID = null, Guid? productID = null) => AddLogMessage(log, CriticalLogTypes.Question, technicalLog, docID, productID);
        public static void AddLogMessageError(string log, string technicalLog = null, Guid? docID = null, Guid? productID = null) => AddLogMessage(log, CriticalLogTypes.Error, technicalLog, docID, productID);
        public static void AddLogMessageStartProgramInformation(string log, string technicalLog = null, Guid? docID = null, Guid? productID = null) => AddLogMessage(log, CriticalLogTypes.Information, technicalLog, docID, productID);
        public static void AddLogMessageInformationWithImage(string log, System.IO.MemoryStream image, string technicalLog = null, Guid? docID = null, Guid? productID = null) => AddLogMessage(log, CriticalLogTypes.Information, image, technicalLog, docID, productID);

        public static void AddLogMessage(string log, CriticalLogTypes logTypeID, string technicalLog = null, Guid? docID = null, Guid? productID = null)
        {
            CriticalLogList.Add(new CriticalLog
             {
                 LogID = SqlGuidUtil.NewSequentialid(),
                 LogDate = DateTime.Now,
                 LogUserID = WorkSession.UserName + (WorkSession.PrintName != String.Empty ? " (" + WorkSession.PrintName + ")" : ""),
                 HostName = GammaSettings.LocalHostName,
                 LogTypeID = (int)logTypeID,
                 Log = (log?.Length > 2000 ? log?.Substring(0, 2000) : log),
                 DocID = docID,
                 ProductID = productID,
                 TechnicalLog = (technicalLog?.Length > 1000 ? technicalLog?.Substring(0, 1000) : technicalLog)
             });
        }
        
        public static void AddLogMessage(string log, CriticalLogTypes logTypeID, System.IO.MemoryStream image, string technicalLog = null, Guid? docID = null, Guid? productID = null)
        {
            CriticalLogList.Add(new CriticalLog
            {
                LogID = SqlGuidUtil.NewSequentialid(),
                LogDate = DateTime.Now,
                LogUserID = WorkSession.UserName + (WorkSession.PrintName != String.Empty ? " (" + WorkSession.PrintName + ")" : ""),
                HostName = GammaSettings.LocalHostName,
                LogTypeID = (int)logTypeID,
                Log = (log?.Length > 2000 ? log?.Substring(0, 2000) : log),
                Image = image.ToArray(),
                DocID = docID,
                ProductID = productID,
                TechnicalLog = (technicalLog?.Length > 1000 ? technicalLog?.Substring(0, 1000) : technicalLog)
            });
        }

        public static void SaveLogToLocalServer()
        {
            if (CriticalLogList?.Count > 0)
            {
                if (UploadLogToServer(false))
                {
                    int countCriticalLogList = CriticalLogList?.Count ?? 0;
                    if (countCriticalLogList > 0)
                    {
                        byte countSecondSleep = 10;
                        byte maxSecondWaiting = 60;
                        System.Threading.Thread.Sleep(countSecondSleep * 1000);
                        if (DB.UploadLogToServer(false))
                        {
                            var countUploadedLogToServer = (countCriticalLogList - (CriticalLogList?.Count ?? 0));
                            if (countUploadedLogToServer != 0 && ((CriticalLogList?.Count ?? 0) * (countSecondSleep / countUploadedLogToServer) <= maxSecondWaiting))
                            {
                                byte t = 0;
                                while (CriticalLogList?.Count > 0 && t < maxSecondWaiting / countSecondSleep)
                                {
                                    if (CriticalLogList?.Count > 0)
                                        System.Threading.Thread.Sleep(countSecondSleep);
                                    UploadLogToServerFromTimer(false);
                                    t++;
                                }
                            }
                        }
                    }
                }
                if (CriticalLogList?.Count > 0)
                {
                    _stopUploadLogToServerRun = true;
                    _uploadLogToServerFromTimerRunning = true;
                    _uploadLogToServerRunning = true;
                    try
                    {
                        using (var logDB = LogDbContext)
                        {
                            var logDBCriticalLogList = logDB.CriticalLogs.Select(l => l.LogID).ToList();
                            foreach (var log in CriticalLogList.Where(l => !logDBCriticalLogList.Contains(l.LogID)))
                            {
                                logDB.CriticalLogs.Add(new CriticalLog
                                {
                                    LogID = log.LogID,
                                    LogDate = log.LogDate,
                                    LogUserID = log.LogUserID,
                                    HostName = log.HostName,
                                    LogTypeID = log.LogTypeID,
                                    Log = log.Log,
                                    DocID = log.DocID,
                                    ProductID = log.ProductID,
                                    TechnicalLog = log.TechnicalLog,
                                    Image = log.Image
                                });
                            }
                            logDB.SaveChanges();
                        }
                    }
                    catch (Exception e)
                    {

                    }
                    _stopUploadLogToServerRun = false;
                    _uploadLogToServerFromTimerRunning = false;
                    _uploadLogToServerRunning = false;
                }
            }
        }
        #endregion

        public static ObservableCollection<Characteristic> GetCharacteristics(Guid? nomenclatureid, GammaEntities gammaBase = null)
        {
            gammaBase = gammaBase ?? GammaDb;
            if (nomenclatureid == null) return new ObservableCollection<Characteristic>();
            return new ObservableCollection<Characteristic>
                (
                (
                    from chars in gammaBase.C1CCharacteristics
                    where chars.C1CNomenclatureID == nomenclatureid && chars.IsActive && !(chars.C1CDeleted ?? false)
                    select new Characteristic { CharacteristicID = chars.C1CCharacteristicID, CharacteristicName = chars.Name }
                ).OrderBy(c => c.CharacteristicName)
                );
        }

        /*
                public static ObservableCollection<Place> GetPlaces(PlaceGroups placeGroup)
                {
                    return new ObservableCollection<Place>
                    (from places in GammaBase.Places
                     where places.PlaceGroupID == (short)placeGroup
                     select new Place { PlaceID = places.PlaceID, PlaceName = places.Name }
                    );
                }
        */

/*
        public static string GetNextDocNumber(DocTypes docType, int placeId, int shiftId, GammaEntities gammaBase = null)
        {
            return
                (gammaBase ?? GammaDb).Database.SqlQuery<string>(
                    $"SELECT dbo.GetNextDocNumber('{(int) docType}', '{placeId}', '{shiftId}')")
                    .AsEnumerable()
                    .FirstOrDefault();
        }
*/

        public static string GetProductNomenclatureNameBeforeDate(Guid productId, DateTime date, GammaEntities gammaBase = null)
        {
            return (gammaBase ?? GammaDbWithNoCheckConnection).Database.SqlQuery<string>($"SELECT dbo.GetProductNomenclatureNameBeforeDate('{productId}', '{date.ToString("yyyy-MM-dd HH:mm:ss.fff")}')").AsEnumerable().FirstOrDefault();
        }

        public static decimal CalculateSpoolWeightBeforeDate(Guid productId, DateTime date, GammaEntities gammaBase = null)
        {
            return (gammaBase ?? GammaDbWithNoCheckConnection).Database.SqlQuery<decimal>($"SELECT dbo.CalculateSpoolWeightBeforeDate('{productId}', '{date.ToString("yyyy-MM-dd HH:mm:ss.fff")}')").AsEnumerable().FirstOrDefault();
        }

        public static string CheckSourceSpools(int placeID, Guid productionTaskID, GammaEntities gammaBase = null)
        {
            return (gammaBase ?? GammaDbWithNoCheckConnection).Database.SqlQuery<string>($"SELECT dbo.CheckSourceSpools({placeID}, '{productionTaskID}')").AsEnumerable().FirstOrDefault();
        }

        public static DateTime CurrentDateTime => ((IObjectContextAdapter)GammaDbWithNoCheckConnection).ObjectContext.CreateQuery<DateTime>("CurrentDateTime()").AsEnumerable().First();

        public static Guid CurrentUserID => GammaDbWithNoCheckConnection == null ? Guid.Empty : GammaDbWithNoCheckConnection.Database.SqlQuery<Guid>("SELECT dbo.CurrentUserID()").FirstOrDefault();

        public static int GetLastFormat(int placeID, GammaEntities gammaBase = null)
        {
            return (gammaBase?? GammaDbWithNoCheckConnection).Database.SqlQuery<int>($"SELECT dbo.GetLastFormat({placeID})").AsEnumerable().First();
        }

        public static int GetUnwindersCount(int placeID, GammaEntities gammaBase = null)
        {
            return (gammaBase ?? GammaDbWithNoCheckConnection).Database.SqlQuery<int>($"SELECT dbo.GetUnwindersCount({placeID})").AsEnumerable().First();
        }


        public static decimal GetCoreWeight(Guid characteristicID, GammaEntities gammaBase = null)
        {
            return (gammaBase?? GammaDbWithNoCheckConnection).Database.SqlQuery<decimal>($"SELECT dbo.GetCoreWeight('{characteristicID}')").AsEnumerable().First();
        }

        public static string GetCharacteristicNameForProductionTaskSGB(Guid characteristicId)
        {
            return
                GammaDbWithNoCheckConnection.Database.SqlQuery<string>(
                    $"SELECT dbo.GetCharacteristicNameForProductionTaskSGB('{characteristicId}')")
                    .AsEnumerable()
                    .First();
        }

        public static decimal GetSpoolCoreWeight(Guid productid, GammaEntities gammaBase = null)
        {
            return (gammaBase?? GammaDbWithNoCheckConnection).Database.SqlQuery<decimal>($"SELECT dbo.GetSpoolCoreWeight('{productid}')").AsEnumerable().First();
        }

        public static short GetSpoolDiameter(Guid productid, GammaEntities gammaBase = null)
        {
            return (gammaBase ?? GammaDbWithNoCheckConnection).Database.SqlQuery<short>($"SELECT dbo.GetSpoolDiameter('{productid}')").AsEnumerable().First();
        }

        public static bool AllowEditProduct(Guid productId, GammaEntities gammaBase = null)
        {
            return
                (gammaBase ?? GammaDbWithNoCheckConnection).Database.SqlQuery<bool>($"SELECT dbo.AllowEditProduct('{productId}')")
                    .AsEnumerable()
                    .First();
        }

        public static bool AllowEditDoc(Guid docId, GammaEntities gammaBase = null)
        {
            return
                (gammaBase ?? GammaDbWithNoCheckConnection).Database.SqlQuery<bool>($"SELECT dbo.AllowEditDoc('{docId}')")
                    .AsEnumerable()
                    .First();
        }

        public static ObservableCollection<string> BaseTables => new ObservableCollection<string>(GammaDbWithNoCheckConnection.Database.SqlQuery<string>("SELECT TABLE_NAME FROM information_schema.tables ORDER BY TABLE_NAME"));

        public static void UploadDocCloseShiftTo1C(Guid docId, GammaEntities gammaBase = null)
        {
            const string sql = "exec [dbo].[UploadDocCloseShiftReportTo1C] @DocID";
            UploadDocTo1C(sql, docId, gammaBase);
        }

        public static void UploadProductionTaskBatchTo1C(Guid productionTaskBatchID, GammaEntities gammaBase = null)
        {
            const string sql = "exec [dbo].[UploadProductionTaskBatchTo1C] @DocID";
            UploadDocTo1C(sql, productionTaskBatchID, gammaBase);
        }

        public static bool UploadShipmentOrderTo1C(Guid docShipmentOrderID, GammaEntities gammaBase = null)
        {
            const string sql = "exec [dbo].[UploadShipmentOrderTo1C] @DocID";
            return UploadDocTo1C(sql, docShipmentOrderID, gammaBase); 
        }

        public static void UploadDocComplectationTo1C(Guid docComplectationID, GammaEntities gammaBase = null)
        {
            const string sql = "exec [dbo].[UploadDocComplectationTo1C] @DocID";
            UploadDocTo1C(sql, docComplectationID, gammaBase);
        }

        public static void UploadFreeMovementTo1C(Guid docMovementId, GammaEntities gammaBase = null)
        {
            const string sql = "exec [dbo].[UploadFreeMovementTo1C] @DocID";
            UploadDocTo1C(sql, docMovementId, gammaBase);
        }

        public static bool UploadDocTo1C(string sql, Guid docId, GammaEntities gammaBase = null)
        {
            gammaBase = gammaBase ?? GammaDb;
            var ret = false;
            try
            {
                var docIdParameter = new SqlParameter("DocID", SqlDbType.UniqueIdentifier)
                {
                    Value = docId
                };
                gammaBase.Database.ExecuteSqlCommand(TransactionalBehavior.DoNotEnsureTransaction, sql, docIdParameter);
                ret = true;
            }
            catch (Exception)
            {
                MessageBox.Show("Не удалось выгрузить документ в 1С");
                AddLogMessageError("Error " + sql+"=" + docId.ToString());
                ret = false;
            }
            return ret;
        }

        public static void UploadDocBrokeTo1C(Guid docBrokeId, GammaEntities gammaBase = null)
        {
            const string sql = "exec [dbo].[UploadDocBrokeTo1C] @DocID";
            UploadDocTo1C(sql, docBrokeId, gammaBase);
        }
        
        public static void ChangeUserPassword(Guid userid,string password, GammaEntities gammaBase = null)
        {
            gammaBase = gammaBase ?? GammaDb;
            var login = gammaBase.Users.Where(u => u.UserID == userid).Select(u => u.Login).FirstOrDefault();
            try
            {
                string sql = $"ALTER LOGIN {login} WITH PASSWORD = '{password}'";
                gammaBase.Database.ExecuteSqlCommand(sql);
            }
            catch (Exception)
            {
                MessageBox.Show("Смена пароля не удалась", "Неудачная смена пароля", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            gammaBase.Dispose();
        }
        public static void RecreateUserInDb(Guid userid,string password, GammaEntities gammaBase = null)
        {
            var parameterList = new List<SqlParameter>
            {
                new SqlParameter("UserID", userid),
                new SqlParameter("Password", password)
            };
            var parameters = parameterList.ToArray();
            // ReSharper disable once CoVariantArrayConversion
            (gammaBase ?? GammaDb).Database.ExecuteSqlCommand("exec dbo.RecreateUser @UserID, @Password", parameters);
        }
        public static void RecreateRolePermits(Guid roleID, GammaEntities gammaBase = null)
        {
            var parameterList = new List<SqlParameter> {new SqlParameter("RoleID", roleID)};
            var parameters = parameterList.ToArray();
            // ReSharper disable once CoVariantArrayConversion
            (gammaBase ?? GammaDb).Database.ExecuteSqlCommand("exec dbo.mxp_RecreateRolePermits @RoleID", parameters);
        }
        private class TablePermission
        {
            public string TableName { get; set; }
            public byte Permit { get; set; }
        }
        private static List<TablePermission> _tablePermissions = new List<TablePermission>();
        private static byte? GetCachedPermission(string tableName)
        {
            var tablePermission = _tablePermissions.Where(tp => tp.TableName == tableName).ToList();
            if (tablePermission.Count == 0)
                return null;
            return tablePermission[0].Permit;
        }
        public static bool HaveReadAccess(string tableName, GammaEntities gammaBase = null)
        {
            if (WorkSession.DBAdmin) return true;
            var permit = GetCachedPermission(tableName);
            if (permit != null) return permit > 0;
            permit = (gammaBase ?? GammaDb).UserPermit(tableName).FirstOrDefault();
            if (permit == null)
            {
                _tablePermissions.Add(new TablePermission
                {
                        TableName = tableName,
                        Permit = 0
                    });
                return false;
            }
            _tablePermissions.Add(new TablePermission
            {
                TableName = tableName,
                Permit = (byte)permit
            });
            return permit > 0;
        }
        public static bool HaveWriteAccess(string tableName, GammaEntities gammaBase = null)
        {
            gammaBase = gammaBase?? GammaDbWithNoCheckConnection;
            if (WorkSession.DBAdmin) return true;
            var permit = GetCachedPermission(tableName);
            if (permit != null) return permit == 2;
            permit = gammaBase.UserPermit(tableName).FirstOrDefault();
            if (permit == null)
            {
                _tablePermissions.Add(new TablePermission
                {
                    TableName = tableName,
                    Permit = 0
                });
                return false;
            }
            _tablePermissions.Add(new TablePermission
            {
                TableName = tableName,
                Permit = (byte)permit
            });
            return permit == 2;
        }
        [DbFunction("GammaDBModel", "GetShiftBeginTime")]
        public static DateTime? GetShiftBeginTime(DateTime date)
        {
            throw new NotSupportedException("Direct calls are not supported");
        }
        [DbFunction("GammaDBModel", "GetShiftEndTime")]
        public static DateTime? GetShiftEndTime(DateTime date)
        {
            throw new NotSupportedException("Direct calls are not supported");
        }

        public static DateTime GetShiftBeginTimeFromDate(DateTime date)
        {
            return GammaDbWithNoCheckConnection.Database.SqlQuery<DateTime>($"SELECT dbo.GetShiftBeginTime('{date.ToString("yyyyMMdd HH:mm:ss")}')").AsEnumerable().First();
        }
        public static DateTime GetShiftEndTimeFromDate(DateTime date)
        {
            return GammaDbWithNoCheckConnection.Database.SqlQuery<DateTime>($"SELECT dbo.GetShiftEndTime('{date.ToString("yyyyMMdd HH:mm:ss")}')").AsEnumerable().First();
        }

        public static Guid? GetDocMaterialInFromDocID(int placeID,int shiftID, DateTime date, string productionCharacteristicIDs)
        {
            return GammaDbWithNoCheckConnection.Database.SqlQuery<Guid?>($"SELECT dbo.GetDocMaterialInFromDocID({placeID},{shiftID},'{date.ToString("yyyyMMdd HH:mm:ss")}','{productionCharacteristicIDs}')").AsEnumerable().First();
        }

        public static Guid? GetDocMaterialInFromDocIDconsideringComposition(int placeID, int shiftID, DateTime date, bool? isCompositionCalculationParameter, string productionCharacteristicIDs)
        {
            string comp = (isCompositionCalculationParameter == null ? "NULL" : (bool)isCompositionCalculationParameter ? "1" : "0");
            return GammaDbWithNoCheckConnection.Database.SqlQuery<Guid?>($"SELECT dbo.GetDocMaterialInFromDocIDconsideringComposition({placeID},{shiftID},'{date.ToString("yyyyMMdd HH:mm:ss")}',{comp},'{productionCharacteristicIDs}')").AsEnumerable().First();
        }

        public static short? GetPaperMachinePlace(int placeid, GammaEntities gammaBase = null)
        {
            return (gammaBase ?? GammaDbWithNoCheckConnection).Database.SqlQuery<short>($"SELECT dbo.GetPaperMachinePlace('{placeid}')").AsEnumerable().First();
        }

        public static int? GetAbilityChangeProductionTaskState(Guid productionTaskBatchID, GammaEntities gammaBase = null)
        {
            return (gammaBase ?? GammaDbWithNoCheckConnection).Database.SqlQuery<int>($"SELECT dbo.GetAbilityChangeProductionTaskState('{productionTaskBatchID}')").AsEnumerable().First();
        }

        public static int? GetAbilityChangeDocShipmentOrder(Guid docShipmentOrderID, GammaEntities gammaBase = null)
        {
            return (gammaBase ?? GammaDbWithNoCheckConnection).Database.SqlQuery<int>($"SELECT dbo.GetAbilityChangeDocShipmentOrder('{docShipmentOrderID}')").AsEnumerable().First();
        }

        public static int? GetAbilityChangeDocComplectation(Guid docCompletationID, GammaEntities gammaBase = null)
        {
            return (gammaBase ?? GammaDbWithNoCheckConnection).Database.SqlQuery<int>($"SELECT dbo.GetAbilityChangeDocComplectation('{docCompletationID}')").AsEnumerable().First();
        }

        public static string GetPlaceProperty(int placeID, string propertyName, GammaEntities gammaBase = null)
        {
            return (gammaBase ?? GammaDbWithNoCheckConnection).Database.SqlQuery<string>($"SELECT dbo.GetPlaceProperty('{placeID}', '{propertyName}')").AsEnumerable().First();
        }

        public static decimal? GetProductionTaskOEE(Guid productionTaskID, GammaEntities gammaBase = null)
        {
            return (gammaBase ?? GammaDbWithNoCheckConnection).Database.SqlQuery<decimal?>($"SELECT dbo.GetProductionTaskOEE('{productionTaskID}')").AsEnumerable().First();
        }

    }
}
