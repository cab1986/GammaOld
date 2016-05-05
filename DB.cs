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
        private static GammaEntities GammaBase { get; set; }

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
        public static ObservableCollection<Characteristic> GetCharacteristics(Guid? nomenclatureid, GammaEntities gammaBase = null)
        {
            gammaBase = gammaBase ?? GammaDb;
            if (nomenclatureid == null) return new ObservableCollection<Characteristic>();
            return new ObservableCollection<Characteristic>
                (
                (
                    from chars in gammaBase.C1CCharacteristics
                    where chars.C1CNomenclatureID == nomenclatureid && chars.IsActive 
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
        public static string CheckSourceSpools(int placeID, Guid productionTaskID, GammaEntities gammaBase = null)
        {
            return (gammaBase ?? GammaDb).Database.SqlQuery<string>($"SELECT dbo.CheckSourceSpools({placeID}, '{productionTaskID}')").AsEnumerable().FirstOrDefault();
        }

        public static DateTime CurrentDateTime => ((IObjectContextAdapter)GammaDb).ObjectContext.CreateQuery<DateTime>("CurrentDateTime()").AsEnumerable().First();

        public static Guid CurrentUserID => GammaDb.Database.SqlQuery<Guid>("SELECT dbo.CurrentUserID()").FirstOrDefault();

        public static int GetLastFormat(int placeID, GammaEntities gammaBase = null)
        {
            return (gammaBase??GammaDb).Database.SqlQuery<int>($"SELECT dbo.GetLastFormat({placeID})").AsEnumerable().First();
        }

        public static int GetUnwindersCount(int placeID, GammaEntities gammaBase = null)
        {
            return (gammaBase ?? GammaDb).Database.SqlQuery<int>($"SELECT dbo.GetUnwindersCount({placeID})").AsEnumerable().First();
        }
        public static decimal GetCoreWeight(Guid characteristicID, GammaEntities gammaBase = null)
        {
            return (gammaBase?? GammaDb).Database.SqlQuery<decimal>($"SELECT dbo.GetCoreWeight('{characteristicID}')").AsEnumerable().First();
        }
        public static decimal GetSpoolCoreWeight(Guid productid, GammaEntities gammaBase = null)
        {
            return (gammaBase?? GammaDb).Database.SqlQuery<decimal>($"SELECT dbo.GetSpoolCoreWeight('{productid}')").AsEnumerable().First();
        }
        public static short GetSpoolDiameter(Guid productid, GammaEntities gammaBase = null)
        {
            return (gammaBase ?? GammaDb).Database.SqlQuery<short>($"SELECT dbo.GetSpoolDiameter('{productid}')").AsEnumerable().First();
        }
        public static ObservableCollection<string> BaseTables
        { 
            get
            {
                return new ObservableCollection<string>(GammaDb.Database.SqlQuery<string>("SELECT TABLE_NAME FROM information_schema.tables ORDER BY TABLE_NAME"));
            }
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
            gammaBase = gammaBase??GammaDb;
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

    }

    public class Characteristic
    {
        public Guid CharacteristicID { get; set; }
        public string CharacteristicName { get; set; }
    }
}
