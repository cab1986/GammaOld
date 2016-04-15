using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Data.Entity.Core.EntityClient;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using DevExpress.Mvvm;
using Gamma.Models;

namespace Gamma
{
    public static class DB
    {
        public static GammaEntities GammaBase;

        private static EntityConnection Connection { get; set; }

        public static GammaEntities GammaDb
        {
            get
            {
                if (Connection != null) return new GammaEntities(Connection);
                Connection = new EntityConnection(GammaSettings.ConnectionString);
                Connection.Open();              
                return new GammaEntities(Connection);
            }
        }

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
                MessageBox.Show($"Message:{e.Message} InnerMessage:{e.InnerException}");
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
            Messenger.Default.Send(new BaseReconnectedMessage());
        }
        public static bool HaveChanges()
        {
            if (GammaBase == null) return false;
            return GammaBase.
                   ChangeTracker.Entries().Any(e => e.State == EntityState.Added
                                              || e.State == EntityState.Modified
                                              || e.State == EntityState.Deleted);
        }
        public static ObservableCollection<Characteristic> GetCharacteristics(Guid? nomenclatureid)
        {
            if (nomenclatureid == null) return new ObservableCollection<Characteristic>();
            return new ObservableCollection<Characteristic>
                (
                (
                    from chars in GammaBase.C1CCharacteristics
                    where chars.C1CNomenclatureID == nomenclatureid && chars.IsActive 
                    select new Characteristic { CharacteristicID = chars.C1CCharacteristicID, CharacteristicName = chars.Name }
                ).OrderBy(c => c.CharacteristicName)
                );
        }

        public static ObservableCollection<Place> GetPlaces(PlaceGroups placeGroup)
        {
            return new ObservableCollection<Place>
            (from places in GammaBase.Places
             where places.PlaceGroupID == (short)placeGroup
             select new Place { PlaceID = places.PlaceID, PlaceName = places.Name }
            );
        }
        public static string CheckSourceSpools(int placeID, Guid productionTaskID)
        {
            return GammaBase.Database.SqlQuery<string>($"SELECT dbo.CheckSourceSpools({placeID}, '{productionTaskID}')").AsEnumerable().FirstOrDefault();
        }

        public static DateTime CurrentDateTime
        {
            get 
            {
                return ((IObjectContextAdapter)GammaBase).ObjectContext.CreateQuery<DateTime>("CurrentDateTime()").AsEnumerable().First();
            }
        }
        public static Guid CurrentUserID => GammaBase.Database.SqlQuery<Guid>("SELECT dbo.CurrentUserID()").FirstOrDefault();

        public static int GetLastFormat(int placeID)
        {
            return GammaBase.Database.SqlQuery<int>($"SELECT dbo.GetLastFormat({placeID})").AsEnumerable().First();
        }
        public static decimal GetCoreWeight(Guid CharacteristicID)
        {
            return GammaBase.Database.SqlQuery<decimal>($"SELECT dbo.GetCoreWeight('{CharacteristicID}')").AsEnumerable().First();
        }
        public static decimal GetSpoolCoreWeight(Guid productid)
        {
            return GammaBase.Database.SqlQuery<decimal>($"SELECT dbo.GetSpoolCoreWeight('{productid}')").AsEnumerable().First();
        }
        public static short GetSpoolDiameter(Guid productid)
        {
            return GammaBase.Database.SqlQuery<short>($"SELECT dbo.GetSpoolDiameter('{productid}')").AsEnumerable().First();
        }
        public static ObservableCollection<string> BaseTables
        { 
            get
            {
                return new ObservableCollection<string>(GammaBase.Database.SqlQuery<string>("SELECT TABLE_NAME FROM information_schema.tables ORDER BY TABLE_NAME"));
            }
        }
        public static void ChangeUserPassword(Guid userid,string password)
        {
            var login = GammaBase.Users.Where(u => u.UserID == userid).Select(u => u.Login).FirstOrDefault();
            try
            {
                string sql = $"ALTER LOGIN {login} WITH PASSWORD = '{password}'";
                GammaBase.Database.ExecuteSqlCommand(sql);
            }
            catch (Exception)
            {
                MessageBox.Show("Смена пароля не удалась", "Неудачная смена пароля", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public static void RecreateUserInDb(Guid userid,string password)
        {
            var parameterList = new List<SqlParameter>
            {
                new SqlParameter("UserID", userid),
                new SqlParameter("Password", password)
            };
            var parameters = parameterList.ToArray();
            GammaBase.Database.ExecuteSqlCommand("exec dbo.RecreateUser @UserID, @Password", parameters);
        }
        public static void RecreateRolePermits(Guid roleID)
        {
            var parameterList = new List<SqlParameter> {new SqlParameter("RoleID", roleID)};
            var parameters = parameterList.ToArray();
            GammaBase.Database.ExecuteSqlCommand("exec dbo.mxp_RecreateRolePermits @RoleID", parameters);
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
        public static bool HaveReadAccess(string tableName)
        {
            if (WorkSession.DBAdmin) return true;
            var permit = GetCachedPermission(tableName);
            if (permit != null) return permit > 0;
            permit = GammaBase.UserPermit(tableName).FirstOrDefault();
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
        public static bool HaveWriteAccess(string tableName)
        {
            if (WorkSession.DBAdmin) return true;
            var permit = GetCachedPermission(tableName);
            if (permit != null) return permit == 2;
            permit = GammaBase.UserPermit(tableName).FirstOrDefault();
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
