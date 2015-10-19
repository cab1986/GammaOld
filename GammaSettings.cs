using System;
using System.Collections.Generic;
using System.Data.Entity.Core.EntityClient;
using System.Data.SqlClient;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace Gamma
{
    [Serializable]
    public class GammaSettings
    {
        private const string FileName = "AppSettings.xml";

        // Параметры com-порта сканера
        public string ComPort = "";
        public int BaudRate = 9600;
        public Parity Parity = Parity.None;
        public StopBits StopBits = StopBits.One;
        public int DataBits = 8;
        public Handshake HandShake = Handshake.None;

        //Параметры подключения к бд
        public string HostName = "";
        public string DbName = "";
        public string User = "";

        private static string _connectionstring;
        
        public static string ConnectionString
        {

            get { return _connectionstring;}
            private set { _connectionstring = value; }
        }
        private GammaSettings()
        { 
            
        }

        private static GammaSettings _GammaSettings;
        public static GammaSettings Get()
        {
            if (_GammaSettings == null)
            {
                if (File.Exists(FileName))
                {
                    var strSetting = new FileStream(FileName, FileMode.Open);
                    var Serializer = new XmlSerializer(typeof(GammaSettings));
                    _GammaSettings = (GammaSettings)Serializer.Deserialize(strSetting);
                }
                else
                {
                    _GammaSettings = new GammaSettings();
                }
            }

            return _GammaSettings;
        }

        public static void SetConnectionString(string DataSource, string DbName, string User = "", string Password = "")
        {
            var AppSettings = GammaSettings.Get();
            AppSettings.HostName = DataSource;
            AppSettings.DbName = DbName;
            AppSettings.User = User;

            SqlConnectionStringBuilder sqlBuilder = new SqlConnectionStringBuilder();

            // Set the properties for the data source.
            sqlBuilder.DataSource = DataSource;
            sqlBuilder.InitialCatalog = DbName;
            sqlBuilder.UserID = User;
            sqlBuilder.Password = Password;
            sqlBuilder.ApplicationName = "EntityFramework";
            sqlBuilder.PersistSecurityInfo = true;
            sqlBuilder.MultipleActiveResultSets = true;

            var EntityConnectionStringBuilder = new EntityConnectionStringBuilder();
            EntityConnectionStringBuilder.Provider = "System.Data.SqlClient";
            EntityConnectionStringBuilder.ProviderConnectionString = sqlBuilder.ToString();
            SQLConnectionString = EntityConnectionStringBuilder.ProviderConnectionString;
            EntityConnectionStringBuilder.Metadata = "res://*/Models.GammaModel.csdl|res://*/Models.GammaModel.ssdl|res://*/Models.GammaModel.msl";

            ConnectionString = EntityConnectionStringBuilder.ToString();
        }

        public static void Serialize()
        {
            var AppSettings = GammaSettings.Get();
            using (var AppFile = new FileStream(FileName, FileMode.Create))
            {
                var Serializer = new XmlSerializer(typeof(GammaSettings));
                Serializer.Serialize(AppFile, AppSettings);
            }
        }
        public static string SQLConnectionString { get; private set; }
    }
}
