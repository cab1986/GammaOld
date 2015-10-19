using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gamma.Models;
using System.Collections.ObjectModel;
using System.Data.Entity.Infrastructure;
using System.Windows;
using System.Data.Entity;

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
                MessageBox.Show(e.InnerException.Message);
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
        }
        public static bool HaveChanges()
        {
            return !GammaBase.
                   ChangeTracker.Entries().Any(e => e.State == EntityState.Added
                                              || e.State == EntityState.Modified
                                              || e.State == EntityState.Deleted);
        }
        public static ObservableCollection<Characteristic> GetCharacteristics(Guid nomenclatureID)
        {
            return new ObservableCollection<Characteristic>
                (
                    from chars in DB.GammaBase.C1CCharacteristics
                    where chars.C1CNomenclatureID == nomenclatureID
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
