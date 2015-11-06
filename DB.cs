﻿using System;
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
        public static ObservableCollection<Characteristic> GetCharacteristics(Guid nomenclatureID)
        {
            return new ObservableCollection<Characteristic>
                (
                    from chars in DB.GammaBase.C1CCharacteristics
                    where chars.C1CNomenclatureID == nomenclatureID && chars.IsActive 
                    select new Characteristic { CharacteristicID = chars.C1CCharacteristicID, CharacteristicName = chars.Name }
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
        public static bool HaveReadAccess(string tableName)
        {
            if (WorkSession.DBAdmin) return true;
            var permit = DB.GammaBase.UserPermit(tableName).FirstOrDefault();
            if (permit == null) return false;
            else return permit > 0;
        }
        public static bool HaveWriteAccess(string tableName)
        {
            if (WorkSession.DBAdmin) return true;
            var permit = DB.GammaBase.UserPermit(tableName).FirstOrDefault();
            if (permit == null) return false;
            else return permit == 2;
        }

    }

    public class Characteristic
    {
        public Guid CharacteristicID { get; set; }
        public string CharacteristicName { get; set; }
    }

    public class Place
    {
        public Guid PlaceID { get; set; }
        public string PlaceName { get; set; }
    }
}
