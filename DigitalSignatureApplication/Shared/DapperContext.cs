using DigitalSignatureApplication.Config;
using Oracle.ManagedDataAccess.Client;
using Sap.Data.Hana;
using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;

namespace DigitalSignatureApplication.Shared
{
    public class DapperContext
    {
        private readonly string ConnectionString;
        private readonly string DateType;
        private readonly ConnectionType Type;
        public DapperContext()
        {
            ConnectionString = ConfigStore.ConnectionAndReportDetails.ConnectionDetails.Connection_String;
            DateType = ConfigStore.CommonServices.DataSource;
            if (String.Equals(DateType.ToUpper(), "SQL"))
                Type = ConnectionType.Sql;
            else if (String.Equals(DateType.ToUpper(), "HANA"))
                Type = ConnectionType.Hana;
            else if (String.Equals(DateType.ToUpper(), "ORACLE"))
                Type = ConnectionType.Oracle;
            else
            {
                Exception ex = new Exception("Incorrect Data Connection Type, please check your config file");
                string strExeFilePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string strWorkPath = System.IO.Path.GetDirectoryName(strExeFilePath);
                string logFolderPath = Path.Combine(strWorkPath, "ExceptionLog");
                if (!Directory.Exists(logFolderPath))
                {
                    Directory.CreateDirectory(logFolderPath);
                }
                string filePath = Path.Combine(logFolderPath, "Exception.txt");
                using (StreamWriter writer = new StreamWriter(filePath, true))
                {
                    writer.WriteLine("-----------------------------------------------------------------------------");
                    writer.WriteLine("Date : " + DateTime.Now.ToString());
                    writer.WriteLine();
                    while (ex != null)
                    {
                        writer.WriteLine(ex.GetType().FullName);
                        writer.WriteLine("Message : " + ex.Message);
                        writer.WriteLine("StackTrace : " + ex.StackTrace);
                        ex = ex.InnerException;
                    }
                }
                throw ex;
            }
        }
        public enum ConnectionType
        {
            Sql,
            Hana,
            Oracle
        };
        public IDbConnection CreateConnection()
        {
            if (Type == ConnectionType.Sql)
                return new SqlConnection(ConnectionString);
            else if (Type == ConnectionType.Hana)
                return new HanaConnection(ConnectionString);
            else if (Type == ConnectionType.Oracle)
                return new OracleConnection(ConnectionString);
            else throw new Exception("Invalid Data Connection Type");
        }
    }
}
