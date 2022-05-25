﻿// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.EntityClient;
using System.Data.SqlClient;
using System.IO;
using System.IO.Ports;
using System.Net;
using System.Xml.Serialization;

namespace Gamma
{
    [Serializable]
    public class GammaSettings
    {
        private static string FileName { get; } = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Gamma\\AppSettings.xml";

        // Параметры com-порта сканера
        public ComPort ScannerComPort = new ComPort();

        public ComPort ScalesComPort = new ComPort();

        //Параметры подключения к бд
        public List<string> Hosts = new List<string>();
        public string HostName = "gamma";
        public string DbName = "";
        public string User = "";

        public bool UseScanner = false;
        private static string _connectionstring;
        
        public static string ConnectionString
        {

            get { return _connectionstring;}
            private set { _connectionstring = value; }
        }

        private GammaSettings()
        { 
            
        }

        private static GammaSettings _gammaSettings;
        public static GammaSettings Get()
        {
            if (_gammaSettings == null)
            {
                if (File.Exists(FileName))
                {
                    var strSetting = new FileStream(FileName, FileMode.Open);
                    var serializer = new XmlSerializer(typeof(GammaSettings));
                    try
                    {
                        _gammaSettings = (GammaSettings)serializer.Deserialize(strSetting);
                    }
                    catch
                    {
                        _gammaSettings = new GammaSettings();
                    }
                    
                }
                else
                {
                    _gammaSettings = new GammaSettings();
                }
            }
            if (!string.IsNullOrEmpty(_gammaSettings.HostName) &&
                !_gammaSettings.Hosts.Contains(_gammaSettings.HostName))
            {
                _gammaSettings.Hosts.Add(_gammaSettings.HostName);
            }
            return _gammaSettings;
        }

        private static string _appID { get; set; }
        public static string AppID
        {
            get
            {
                if (_appID == null)
                {
                    _appID = "(" + DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString() + DateTime.Now.Millisecond.ToString() + ")";
                }
                return _appID;
            }
        }

        private static string _version { get; set; }
        public static string Version
        {
            get
            {
                if (_version == null)
                {
                    try
                    {
                        _version = System.Deployment.Application.ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString() + " " + AppID;
                    }
                    catch
                    {
                        _version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString() + " " + AppID;
                    }
                }
                return _version;
            }
        }

        private static string _localHostName { get; set; }
        public static string LocalHostName
        {
            get
            {
                if (_localHostName == null)
                    _localHostName = Dns.GetHostName();
                return _localHostName;
            }
        }

        public static void SetConnectionString(string dataSource, string dbName, string user = "", string password = "")
        {
            var appSettings = Get();
            appSettings.HostName = dataSource;
            appSettings.DbName = dbName;
            appSettings.User = user;

            SqlConnectionStringBuilder sqlBuilder = new SqlConnectionStringBuilder();

            // Set the properties for the data source.
            sqlBuilder.DataSource = dataSource;
            sqlBuilder.InitialCatalog = dbName;
            sqlBuilder.UserID = user;
            sqlBuilder.Password = password;

            sqlBuilder.ApplicationName = "Gamma v" + Version;
            sqlBuilder.PersistSecurityInfo = true;
            sqlBuilder.MultipleActiveResultSets = true;
            sqlBuilder.ConnectTimeout = 30;

            var entityConnectionStringBuilder = new EntityConnectionStringBuilder();
            entityConnectionStringBuilder.Provider = "System.Data.SqlClient";
            entityConnectionStringBuilder.ProviderConnectionString = sqlBuilder.ToString();
            SqlConnectionString = entityConnectionStringBuilder.ProviderConnectionString;
            entityConnectionStringBuilder.Metadata = "res://*/Entities.GammaModel.csdl|res://*/Entities.GammaModel.ssdl|res://*/Entities.GammaModel.msl";

            ConnectionString = entityConnectionStringBuilder.ToString();
            IsConnectionStringSetted = true;
        }

        public static bool IsConnectionStringSetted { get; private set; }

        public static void Serialize()
        {
            var appSettings = Get();
            if (!appSettings.Hosts.Contains(appSettings.HostName))
            {
                appSettings.Hosts.Add(appSettings.HostName);
            }

            var folderPath = Path.GetDirectoryName(FileName);
            if (folderPath == null) return;
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            using (var appFile = new FileStream(FileName, FileMode.Create))
            {
                var serializer = new XmlSerializer(typeof(GammaSettings));
                serializer.Serialize(appFile, appSettings);
            }
        }
        public static string SqlConnectionString { get; private set; }

        [Serializable]
        public class ComPort
        {
            public string ComPortNumber = "";
            public int BaudRate = 9600;
            public Parity Parity = Parity.None;
            public StopBits StopBits = StopBits.One;
            public int DataBits = 8;
            public Handshake HandShake = Handshake.None;
            public string Prefix = "";
            public string Postfix = "";
            public DateReadFromComPortType TypeDateReadFromComPort = DateReadFromComPortType.Byte;
        }
    }
   
}
