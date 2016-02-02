using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gamma
{
    public static class WorkSession
    {
        public static Guid ParamID { get; set; }
        private static Guid? _userID;
        public static Guid? UserID
        {
            get
            {
                return _userID;
            }
            set
            {
            	_userID = value;
                var userInfo = (from u in DB.GammaBase.Users
                                 where
                                     u.UserID == _userID
                                 select new
                                 {
                                     placeID = u.PlaceID,
                                     placeGroupID = u.Places.PlaceGroupID,
                                     ShiftID = u.ShiftID,
                                     dbAdmin = u.DBAdmin
                                 }).FirstOrDefault();
                DBAdmin = userInfo.dbAdmin;
                PlaceID = userInfo.placeID;
                ShiftID = userInfo.ShiftID;
                PlaceGroup = (PlaceGroups)userInfo.placeGroupID;
            }
        }
        public static bool DBAdmin
        {
            get;
            set;
        }

        public static int PlaceID
        {
            get;
            set;
        }
        public static byte ShiftID
        {
            get;
            set;
        }
        public static PlaceGroups PlaceGroup { get; private set; }
        public static string PrintName { get; set; }
    }
}
