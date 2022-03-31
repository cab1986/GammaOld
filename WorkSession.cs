// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Gamma
{
    public static class WorkSession
    {
        public static Guid ParamID { get; set; }
        private static DateTime lastSuccesRecivedUserInfo { get; set; }
        private static Guid _userid;
        public static bool SetUser(Guid userid)
        {
            _userid = userid;
            return GetUserInfo();
        }
        public static Guid UserID
        {
            get
            {
                RefreshUserInfoAndExistNewVersion();
                return _userid;
            }
            private set
            {
                _userid = value;
            }
        }
        public static bool DBAdmin
        {
            get; private set;
        }

        private static int _placeID { get; set; }
        public static int PlaceID
        {
            get
            {
                RefreshUserInfoAndExistNewVersion();
                return _placeID;
            }
            private set
            {
                _placeID = value;
            }
        }

        public static int? DepartmentID
        {
            get; private set;
        }

        public static int BranchID { get; private set; }
        public static bool ProgramAdmin
        {
            get; private set;
        }

        private static byte _shiftID { get; set; }
        public static byte ShiftID
        {
            get
            {
                RefreshUserInfoAndExistNewVersion();
                return _shiftID;
            }
            private set
            {
                _shiftID = value;
            }
        }

        /// <summary>
        /// Признак печати транспортной этикетки на удаленном принтере
        /// </summary>
        public static bool IsRemotePrinting { get; private set; }
        /// <summary>
        /// Признак автоматической печати и наклейки групповой этикетки 
        /// </summary>
        public static bool UseApplicator { get; private set; }

        public static List<int> PlaceIds { get; private set; }
        public static List<int> BranchIds { get; private set; }

        public static bool IsProductionPlace { get; private set; }
        public static bool IsMaterialProductionPlace { get; private set; }
        public static bool IsShipmentWarehouse { get; private set; }
        public static bool IsTransitWarehouse { get; private set; }
        public static PlaceGroup PlaceGroup { get; private set; }
        public static string PrintName { get; set; }
        public static string UserName { get; private set; } = "";
        public static Guid? PersonID { get; set; }
        public static string RoleName { get; private set; }

        public static string EndpointConfigurationName = "BasicHttpBinding_IPrinterService";

        /// <summary>
        /// Адрес сервиса отправки электронных писем MailService
        /// </summary>

        public static string EndpointAddressOnMailService { get; private set; }
        /// <summary>
        /// Адрес сервиса печати групповых этикеток GammaService
        /// </summary>
        public static string EndpointAddressOnGroupPackService { get; private set; }

        /// Адрес сервиса печати транспортных этикеток GammaService
        /// </summary>
        public static string EndpointAddressOnTransportPackService { get; private set; }

        /// <summary>
        /// Путь до этикеток (групповых, транспортных)
        /// </summary>
        public static string LabelPath { get; private set; }

        /// <summary>
        /// Кол-во раскатов на переделе
        /// </summary>
        public static int UnwindersCount { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public static bool IsUploadDocBrokeTo1CWhenSave { get; private set; }

        public static bool IsUsedInOneDocMaterialDirectCalcAndComposition { get; private set; }

        private static int RefreshUserInfoPeriod { get; set; } = 30;
        private static bool _isExistNewVersionOfProgram { get; set; } = false;
        public static bool CheckExistNewVersionOfProgram()
        {
            if (_isExistNewVersionOfProgram)
                MessageBox.Show("Внимание! Обнаружена новая версия программы!" + Environment.NewLine + "Требуется перезапустить и обновить программу!");
            return _isExistNewVersionOfProgram;
        }

        private static void RefreshUserInfoAndExistNewVersion()
        {
            if (lastSuccesRecivedUserInfo == null || (DateTime.Now - lastSuccesRecivedUserInfo).Minutes > RefreshUserInfoPeriod)
            {
                GetUserInfo();

                var checkResult = DB.CheckCurrentVersion();
                var resultMessage = checkResult?.ResultMessage;
                if (checkResult != null && !(string.IsNullOrWhiteSpace(resultMessage) && !checkResult.BlockCreation))
                {
                    _isExistNewVersionOfProgram = true;
                }
            }
        }

        private static bool GetUserInfo()
        {
            using (var gammaBase = DB.GammaDbWithNoCheckConnection)
            {
                var userInfo = (from u in gammaBase.Users
                                where
                                    u.UserID == _userid
                                select new
                                {
                                    u.Places1.PlaceID,
                                    placeGroupID = u.Places1.PlaceGroupID,
                                    u.ShiftID,
                                    u.DBAdmin,
                                    programAdmin = u.ProgramAdmin,
                                    u.Places1.BranchID,
                                    u.Places1.IsProductionPlace,
                                    u.Places1.IsMaterialProductionPlace,
                                    u.Places,
                                    u.DepartmentID,
                                    u.Name,
                                    u.Places1.IsShipmentWarehouse,
                                    u.Places1.IsTransitWarehouse,
                                    RoleName = u.Roles.Name,
                                    u.Places1.UnwindersCount,
                                    u.Places1.IsRemotePrinting,
                                    u.Places1.UseApplicator
                                }).FirstOrDefault();
                if (userInfo == null)
                {
                    //MessageBox.Show("Не удалось получить информацию о пользователе");
                    return false;
                }
                UserName = userInfo.Name;
                DBAdmin = userInfo.DBAdmin;
                PlaceID = userInfo.PlaceID;
                DepartmentID = userInfo.DepartmentID; /*(from u in DB.GammaDb.Places
                                    where u.PlaceID == PlaceID
                                    select u.DepartmentID
                                ).FirstOrDefault();*/
                BranchID = userInfo.BranchID;
                ShiftID = userInfo.ShiftID;
                PlaceGroup = (PlaceGroup)userInfo.placeGroupID;
                RoleName = userInfo.RoleName;
                UnwindersCount = userInfo.UnwindersCount ?? 0;
                IsProductionPlace = userInfo.IsProductionPlace ?? false;
                IsMaterialProductionPlace = userInfo.IsMaterialProductionPlace ?? false;
                IsShipmentWarehouse = userInfo.IsShipmentWarehouse ?? false;
                IsTransitWarehouse = userInfo.IsTransitWarehouse ?? false;
                PlaceIds = userInfo.Places.Select(p => p.PlaceID).ToList();
                BranchIds = userInfo.Places.Select(p => p.BranchID).Distinct().ToList();
                IsRemotePrinting = userInfo.IsRemotePrinting ?? false;
                UseApplicator = userInfo.UseApplicator ?? false;
                var localSettings = (from u in gammaBase.LocalSettings
                                    select new { u.MailServiceAddress, u.LabelPath, u.IsUploadDocBrokeTo1CWhenSave, u.IsUsedInOneDocMaterialDirectCalcAndComposition }).FirstOrDefault();
                if (localSettings != null)
                {
                    EndpointAddressOnMailService = localSettings.MailServiceAddress;
                    LabelPath = localSettings.LabelPath;
                    IsUploadDocBrokeTo1CWhenSave = localSettings.IsUploadDocBrokeTo1CWhenSave ?? false;
                    IsUsedInOneDocMaterialDirectCalcAndComposition = localSettings.IsUsedInOneDocMaterialDirectCalcAndComposition ?? true;
                }
                EndpointAddressOnGroupPackService = (from u in gammaBase.PlaceRemotePrinters
                                                     where u.PlaceID == _placeID && u.IsEnabled == true && (u.RemotePrinters.RemotePrinterLabelID == 2 || u.RemotePrinters.RemotePrinterLabelID == 3)
                                                     select u.ModbusDevices.ServiceAddress).FirstOrDefault();
                EndpointAddressOnTransportPackService = (from u in gammaBase.PlaceRemotePrinters
                                                         where u.PlaceID == _placeID && u.IsEnabled == true && !(u.RemotePrinters.RemotePrinterLabelID == 2 || u.RemotePrinters.RemotePrinterLabelID == 3)
                                                         select u.ModbusDevices.ServiceAddress).FirstOrDefault();
                //#if (DEBUG)
                //                EndpointAddressOnMailService = "http://localhost:8735/PrinterService";
                //                EndpointAddressOnTransportPackService = "http://localhost:8735/PrinterService";
                //#endif
                lastSuccesRecivedUserInfo = DateTime.Now;
            }

            return UserName != "";
        }
    }
}
