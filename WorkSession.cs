using System;
using System.Linq;
using System.Windows;

namespace Gamma
{
    public static class WorkSession
    {
        public static Guid Paramid { get; set; }
        private static Guid _userid;
        public static Guid UserID
        {
            get
            {
                return _userid;
            }
            set
            {
            	_userid = value;
                var userInfo = (from u in DB.GammaBase.Users
                                 where
                                     u.UserID == _userid
                                 select new
                                 {
                                     u.Places.FirstOrDefault().PlaceID,
                                     placeGroupID = u.Places.FirstOrDefault().PlaceGroupID, u.ShiftID, u.DBAdmin,
                                     programAdmin = u.ProgramAdmin
                                 }).FirstOrDefault();
                if (userInfo == null)
                {
                    MessageBox.Show("Не удалосьь получить информацию о пользователе");
                    return;
                }
                DBAdmin = userInfo.DBAdmin;
                ProgramAdmin = userInfo.programAdmin ?? false;
                PlaceID = userInfo.PlaceID;
                ShiftID = userInfo.ShiftID;
                PlaceGroup = (PlaceGroups)userInfo.placeGroupID;
            }
        }
        public static bool DBAdmin
        {
            get; private set;
        }

        public static int PlaceID
        {
            get; private set;
        }
        public static bool ProgramAdmin
        {
            get; private set;
        }
        public static byte ShiftID
        {
            get; private set;
        }
        public static PlaceGroups PlaceGroup { get; private set; }
        public static string PrintName { get; set; }
    }
}
