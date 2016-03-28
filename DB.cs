using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gamma.Models;
using System.Collections.ObjectModel;
using System.Data.Entity.Infrastructure;
using System.Windows;
using System.Data.Entity;
using GalaSoft.MvvmLight.Messaging;
using System.Data.SqlClient;
using System.Net;
using System.Data.Entity.Core.Objects.DataClasses;

namespace Gamma
{
    public static class DB
    {
        public static GammaEntities GammaBase;

        public static bool Initialize()
        {
            GammaBase = new GammaEntities(GammaSettings.ConnectionString);       
            try
            {
                GammaBase.Database.Connection.Open();
                WorkSession.UserID = CurrentUserID;
                return true;
            }
            catch(Exception e)
            {
                MessageBox.Show(String.Format("Message:{0} InnerMessage:{1}", e.Message, e.InnerException));
                return false;
            }
        }
        public static void RollBack()
        {
            if (!HaveChanges()) return;
            GammaBase.Database.Connection.Close();
            GammaBase.Dispose();
            GammaBase = new GammaEntities(GammaSettings.ConnectionString);
            GammaBase.Database.Connection.Open();
            Messenger.Default.Send<BaseReconnectedMessage>(new BaseReconnectedMessage());
        }
        public static bool HaveChanges()
        {
            if (GammaBase == null) return false;
            return GammaBase.
                   ChangeTracker.Entries().Any(e => e.State == EntityState.Added
                                              || e.State == EntityState.Modified
                                              || e.State == EntityState.Deleted);
        }
        public static ObservableCollection<Characteristic> GetCharacteristics(Guid? nomenclatureID)
        {
            if (nomenclatureID == null) return new ObservableCollection<Characteristic>();
            return new ObservableCollection<Characteristic>
                (
                (
                    from chars in DB.GammaBase.C1CCharacteristics
                    where chars.C1CNomenclatureID == nomenclatureID && chars.IsActive 
                    select new Characteristic { CharacteristicID = chars.C1CCharacteristicID, CharacteristicName = chars.Name }
                ).OrderBy(c => c.CharacteristicName)
                );
        }

        public static ObservableCollection<Place> GetPlaces(PlaceGroups placeGroup)
        {
            return new ObservableCollection<Place>
            (from places in DB.GammaBase.Places
             where places.PlaceGroupID == (short)placeGroup
             select new Place { PlaceID = places.PlaceID, PlaceName = places.Name }
            );
        }
        public static string CheckSourceSpools(int placeID, Guid productionTaskID)
        {
            return GammaBase.Database.SqlQuery<string>(string.Format("SELECT dbo.CheckSourceSpools({0}, '{1}')", placeID,
                productionTaskID)).AsEnumerable().FirstOrDefault();
        }

        public static DateTime CurrentDateTime
        {
            get 
            {
                return ((IObjectContextAdapter)DB.GammaBase).ObjectContext.CreateQuery<DateTime>("CurrentDateTime()").AsEnumerable().First();
            }
        }
        public static Guid CurrentUserID
        {
            get
            {
                return GammaBase.Database.SqlQuery<Guid>("SELECT dbo.CurrentUserID()").FirstOrDefault();
            }
        }
        public static int GetLastFormat(int placeID)
        {
            return GammaBase.Database.SqlQuery<int>(string.Format("SELECT dbo.GetLastFormat({0})",placeID)).AsEnumerable().First();
        }
        public static decimal GetCoreWeight(Guid characteristicID)
        {
            return GammaBase.Database.SqlQuery<decimal>(string.Format("SELECT dbo.GetCoreWeight('{0}')", characteristicID)).AsEnumerable().First();
        }
        public static decimal GetSpoolCoreWeight(Guid productID)
        {
            return GammaBase.Database.SqlQuery<decimal>(string.Format("SELECT dbo.GetSpoolCoreWeight('{0}')", productID)).AsEnumerable().First();
        }
        public static short GetSpoolDiameter(Guid productID)
        {
            return GammaBase.Database.SqlQuery<short>(string.Format("SELECT dbo.GetSpoolDiameter('{0}')", productID)).AsEnumerable().First();
        }
        public static ObservableCollection<string> BaseTables
        { 
            get
            {
                return new ObservableCollection<string>(GammaBase.Database.SqlQuery<string>("SELECT TABLE_NAME FROM information_schema.tables ORDER BY TABLE_NAME"));
            }
        }
        public static void ChangeUserPassword(Guid UserID,string password)
        {
            var login = DB.GammaBase.Users.Where(u => u.UserID == UserID).Select(u => u.Login).FirstOrDefault();
            try
            {
                string sql = String.Format("ALTER LOGIN {0} WITH PASSWORD = '{1}'", login, password);
                GammaBase.Database.ExecuteSqlCommand(sql);
            }
            catch (Exception)
            {
                MessageBox.Show("Смена пароля не удалась", "Неудачная смена пароля", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public static void RecreateUserInDB(Guid userID,string password)
        {
            List<SqlParameter> parameterList = new List<SqlParameter>();
            parameterList.Add(new SqlParameter("UserID",userID));
            parameterList.Add(new SqlParameter("Password",password));
            SqlParameter[] parameters = parameterList.ToArray();
            DB.GammaBase.Database.ExecuteSqlCommand("exec dbo.RecreateUser @UserID, @Password", parameters);
        }
        public static void RecreateRolePermits(Guid roleID)
        {
            List<SqlParameter> parameterList = new List<SqlParameter>();
            parameterList.Add(new SqlParameter("RoleID",roleID));
            SqlParameter[] parameters = parameterList.ToArray();
            DB.GammaBase.Database.ExecuteSqlCommand("exec dbo.mxp_RecreateRolePermits @RoleID", parameters);
        }
        private class TablePermission
        {
            public string TableName { get; set; }
            public byte Permit { get; set; }
        }
        private static List<TablePermission> tablePermissions = new List<TablePermission>();
        private static byte? GetCachedPermission(string tableName)
        {
            var tablePermission = tablePermissions.Where(tp => tp.TableName == tableName).ToList();
            if (tablePermission.Count == 0)
                return null;
            else
                return tablePermission[0].Permit;
        }
        public static bool HaveReadAccess(string tableName)
        {
            if (WorkSession.DBAdmin) return true;
            var permit = GetCachedPermission(tableName);
            if (permit != null) return permit > 0;
            permit = DB.GammaBase.UserPermit(tableName).FirstOrDefault();
            if (permit == null)
            {
                tablePermissions.Add(new TablePermission()
                    {
                        TableName = tableName,
                        Permit = 0
                    });
                return false;
            }
            else
            {
                tablePermissions.Add(new TablePermission()
                {
                    TableName = tableName,
                    Permit = (byte)permit
                });
                return permit > 0;
            }
        }
        public static bool HaveWriteAccess(string tableName)
        {
            if (WorkSession.DBAdmin) return true;
            var permit = GetCachedPermission(tableName);
            if (permit != null) return permit == 2;
            permit = DB.GammaBase.UserPermit(tableName).FirstOrDefault();
            if (permit == null)
            {
                tablePermissions.Add(new TablePermission()
                {
                    TableName = tableName,
                    Permit = 0
                });
                return false;
            }
            else
            {
                tablePermissions.Add(new TablePermission()
                {
                    TableName = tableName,
                    Permit = (byte)permit
                });
                return permit == 2;
            }
        }
        [DbFunction("GammaModel.Store","GetShiftBeginTime")]
        public static DateTime? GetShiftBeginTime(DateTime date)
        {
            throw new NotSupportedException("Direct calls are not supported");
        }
        [DbFunction("GammaModel.Store","GetShiftEndTime")]
        public static DateTime? GetShiftEndTime(DateTime date)
        {
            throw new NotSupportedException("Direct calls are not supported");
        }

    }

    public class Characteristic
    {
        public Guid CharacteristicID { get; set; }
        public string CharacteristicName { get; set; }
    }
}
