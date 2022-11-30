using System;

namespace TabFusionRMS.WebCS
{

    public class DatabaseEngine
    {

        private string msDBName;
        private int msDBType;
        private string msDBConnectionText;
        private int mlDBConnectionTimeout;
        private string msDBDatabase;
        private string msDBPassword;
        private string msDBProvider;
        private string msDBServer;
        private bool mbDBUseDBEngineUIDPWD;
        private string msDBUserId;

        public string DBName
        {
            get
            {
                msDBName = "DB_Engine";
                return msDBName;
            }
        }

        public int DBType
        {
            get
            {
                msDBType = default;
                return msDBType;
            }
        }

        public string DBConnectionText
        {
            get
            {
                var http = new HttpContextAccessor().HttpContext;
                msDBConnectionText =Keys.get_DefaultDBConText("DBConnectionText", http);
                return msDBConnectionText;
            }
        }

        public int DBConnectionTimeout
        {
            get
            {
                var http = new HttpContextAccessor().HttpContext;
                mlDBConnectionTimeout = Convert.ToInt32(Keys.get_DefaultDBConText("DBConnectionTimeout", http));
                return mlDBConnectionTimeout;
            }
        }
        public string DBDatabase
        {
            get
            {
                var http = new HttpContextAccessor().HttpContext;
                msDBDatabase = Keys.get_DefaultDBConText("DBDatabase", http);
                return msDBDatabase;
            }
        }
        public string DBPassword
        {
            get
            {
                var http = new HttpContextAccessor().HttpContext;
                msDBPassword = Keys.get_DefaultDBConText("DBPassword", http);
                return msDBPassword;
            }
        }
        public string DBProvider
        {
            get
            {
                var http = new HttpContextAccessor().HttpContext;
                msDBProvider = Keys.get_DefaultDBConText("DBProvider", http);
                return msDBProvider;
            }
        }
        public string DBServer
        {
            get
            {
                var http = new HttpContextAccessor().HttpContext;
                msDBServer = Keys.get_DefaultDBConText("DBServer", http);
                return msDBServer;
            }
        }

        public bool DBUseDBEngineUIDPWD
        {
            get
            {
                return mbDBUseDBEngineUIDPWD == false;
            }
        }

        public string DBUserId
        {
            get
            {
                var http = new HttpContextAccessor().HttpContext;
                msDBUserId = Keys.get_DefaultDBConText("DBUserId", http);
                return msDBUserId;
            }
        }

    }
}