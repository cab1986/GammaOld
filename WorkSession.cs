using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Gamma
{
    public static class WorkSession
    {
        public static Guid ParamID { get; set; }
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
                var userInfo = (from u in DB.GammaDb.Users
                                 where
                                     u.UserID == _userid
                                 select new
                                 {
                                     u.Places.FirstOrDefault().PlaceID,
                                     placeGroupID = u.Places.FirstOrDefault().PlaceGroupID, u.ShiftID, u.DBAdmin,
                                     programAdmin = u.ProgramAdmin,
                                     u.Places.FirstOrDefault().BranchID,
                                     u.Places.FirstOrDefault().IsProductionPlace,
                                     u.Places
                                 }).FirstOrDefault();
                if (userInfo == null)
                {
                    MessageBox.Show("Не удалосьь получить информацию о пользователе");
                    return;
                }
                DBAdmin = userInfo.DBAdmin;
                ProgramAdmin = userInfo.programAdmin ?? false;
                PlaceID = userInfo.PlaceID;
                BranchID = userInfo.BranchID;
                ShiftID = userInfo.ShiftID;
                PlaceGroup = (PlaceGroups)userInfo.placeGroupID;
                IsProductionPlace = userInfo.IsProductionPlace ?? false;
                PlaceIds = userInfo.Places.Select(p => p.PlaceID).ToList();
                BranchIds = userInfo.Places.Select(p => p.BranchID).Distinct().ToList();
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

        public static int BranchID { get; private set; }
        public static bool ProgramAdmin
        {
            get; private set;
        }
        public static byte ShiftID
        {
            get; private set;
        }

        public static List<int> PlaceIds { get; private set; }
        public static List<int> BranchIds { get; private set; }

        public static bool IsProductionPlace { get; private set; }
        public static PlaceGroups PlaceGroup { get; private set; }
        public static string PrintName { get; set; }
    }
}
