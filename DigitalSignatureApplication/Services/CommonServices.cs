using System;
using System.IO;
using System.Xml.Serialization;

namespace DigitalSignatureApplication
{
    public class CommonServices
    {
        protected static void ExceptionGeneration(Exception ex)
        {
            try
            {
                DateTime date = DateTime.Now.Date;
                string strExeFilePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string strWorkPath = System.IO.Path.GetDirectoryName(strExeFilePath);
                string logFolderPath = Path.Combine(strWorkPath, "ExceptionLog");
                if (!Directory.Exists(logFolderPath))
                {
                    Directory.CreateDirectory(logFolderPath);
                }
                string FileName = "Exception.txt";
                FileName = AppendDateStamp(FileName);
                string filePath = Path.Combine(logFolderPath, FileName);
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

            }
            catch (System.IO.IOException)
            {

            }
        }

        protected static void GeneratedPayloadFromVM<T>(T model) where T : class
        {
            try
            {
                DateTime date = DateTime.Now.Date;
                Console.WriteLine(date.ToString());
                string strExeFilePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string strWorkPath = System.IO.Path.GetDirectoryName(strExeFilePath);
                string logFolderPath = Path.Combine(strWorkPath, "VMLog");
                if (!Directory.Exists(logFolderPath))
                {
                    Directory.CreateDirectory(logFolderPath);
                }
                string FileName = "PayloadFromVM.txt";
                FileName = AppendDateStamp(FileName);
                string filePath = Path.Combine(logFolderPath, FileName);
                using (StreamWriter writer = new StreamWriter(filePath, true))
                {
                    writer.WriteLine("-----------------------------------------------------------------------------");
                    writer.WriteLine("Date : " + DateTime.Now.ToString());
                    writer.WriteLine();
                    writer.WriteLine(ToXml(model));
                }

            }
            catch (System.IO.IOException)
            {

            }
        }

        private static string ToXml<T>(T model) where T : class
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            using (StringWriter sw = new StringWriter())
            {
                serializer.Serialize(sw, model);
                return sw.ToString();
            }
        }
        protected static void WriteSuccessfulFileGeneration(string FilePath, string FileName)
        {
            string strExeFilePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string strWorkPath = System.IO.Path.GetDirectoryName(strExeFilePath);
            string logFolderPath = Path.Combine(strWorkPath, "Signed Reports");
            if (!Directory.Exists(logFolderPath))
            {
                Directory.CreateDirectory(logFolderPath);
            }
            string SignedFileName = "SignedReports.txt";
            SignedFileName = AppendDateStamp(SignedFileName);
            string signedReport = Path.Combine(logFolderPath, SignedFileName);
            using (StreamWriter writer = new StreamWriter(signedReport, true))
            {
                writer.WriteLine("-----------------------------------------------------------------------------");
                writer.WriteLine("Date : " + DateTime.Now.ToString());
                writer.WriteLine();
                writer.Write("File Name: ");
                writer.WriteLine(FileName);
                writer.Write("File Path: ");
                writer.WriteLine(FilePath);
                writer.WriteLine();
            }
        }
        protected static void WriteFailedFile(string FileName, Exception ex)
        {
            try
            {
                string strExeFilePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string strWorkPath = System.IO.Path.GetDirectoryName(strExeFilePath);
                string logFolderPath = Path.Combine(strWorkPath, "Signed Reports");
                if (!Directory.Exists(logFolderPath))
                {
                    Directory.CreateDirectory(logFolderPath);
                }
                string FailedFileName = "FailedReports.txt";
                FailedFileName = AppendDateStamp(FailedFileName);
                string signedReport = Path.Combine(logFolderPath, FailedFileName);
                using (StreamWriter writer = new StreamWriter(signedReport, true))
                {
                    writer.WriteLine("-----------------------------------------------------------------------------");
                    writer.WriteLine("Date : " + DateTime.Now.ToString());
                    writer.WriteLine();
                    writer.Write("File Name: ");
                    writer.WriteLine(FileName);
                    writer.WriteLine();
                    while (ex != null)
                    {
                        writer.WriteLine(ex.GetType().FullName);
                        writer.WriteLine("Message : " + ex.Message);
                        writer.WriteLine("StackTrace : " + ex.StackTrace);
                        ex = ex.InnerException;
                    }
                }
            }
            catch (System.IO.IOException)
            {
            }
        }
        protected static void WritePayload(string payload, string FileName)
        {
            try
            {
                string strExeFilePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string strWorkPath = System.IO.Path.GetDirectoryName(strExeFilePath);
                string logFolderPath = Path.Combine(strWorkPath, "ExceptionLog");
                if (!Directory.Exists(logFolderPath))
                {
                    Directory.CreateDirectory(logFolderPath);
                }
                string signedReport = Path.Combine(logFolderPath, FileName + " Failed Payload.txt");
                using (StreamWriter writer = new StreamWriter(signedReport, false))
                {
                    writer.WriteLine(payload);
                }
            }
            catch (System.IO.IOException)
            {

            }
        }
        protected static void WriteSerializedPayload(string payload, string FileName)
        {
            try
            {
                string strExeFilePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string strWorkPath = System.IO.Path.GetDirectoryName(strExeFilePath);
                string logFolderPath = Path.Combine(strWorkPath, "ExceptionLog");
                if (!Directory.Exists(logFolderPath))
                {
                    Directory.CreateDirectory(logFolderPath);
                }
                string signedReport = Path.Combine(logFolderPath, FileName + "Payload.txt");
                using (StreamWriter writer = new StreamWriter(signedReport, false))
                {
                    writer.WriteLine(payload);
                }
            }
            catch (System.IO.IOException)
            {

            }
        }
        protected static void WriteEachStep(string stepEvaluation, bool switchOn)
        {
            if (switchOn)
            {
                int threadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
                string strExeFilePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string strWorkPath = System.IO.Path.GetDirectoryName(strExeFilePath);
                string logFolderPath = Path.Combine(strWorkPath, "Program Log", "Thread ID " + threadId);
                if (!Directory.Exists(logFolderPath))
                {
                    Directory.CreateDirectory(logFolderPath);
                }
                string signedReport = Path.Combine(logFolderPath, "Thread ID " + threadId + ".txt");
                using (StreamWriter writer = new StreamWriter(signedReport, true))
                {
                    writer.WriteLine("-----------------------------------------------------------------------------");
                    writer.WriteLine("Date : " + DateTime.Now.ToString());
                    writer.WriteLine();
                    writer.Write("Thread: ");
                    writer.WriteLine(threadId);
                    writer.WriteLine();
                    writer.Write("Program Cycle: ");
                    writer.WriteLine(stepEvaluation);
                    writer.WriteLine();
                }


            }
        }
        protected static void SubEachStep(string stepEvaluation, bool switchOn, int mainThreadId)
        {
            if (switchOn)
            {
                int threadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
                string strExeFilePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string strWorkPath = System.IO.Path.GetDirectoryName(strExeFilePath);
                string logFolderPath = Path.Combine(strWorkPath, "Program Log", "Thread ID " + mainThreadId);
                if (!Directory.Exists(logFolderPath))
                {
                    Directory.CreateDirectory(logFolderPath);
                }
                string signedReport = Path.Combine(logFolderPath, "SubThread ID " + threadId + ".txt");
                //signedReport = SpLAppendTimeStamp(signedReport);
                using (StreamWriter writer = new StreamWriter(signedReport, true))
                {
                    writer.WriteLine("-----------------------------------------------------------------------------");
                    writer.WriteLine("Date : " + DateTime.Now.ToString());
                    writer.WriteLine();
                    writer.Write("Sub Thread: ");
                    writer.WriteLine(threadId);
                    writer.WriteLine();
                    writer.Write("Program Cycle: ");
                    writer.WriteLine(stepEvaluation);
                    writer.WriteLine();
                }


            }
        }
        protected static string AppendTimeStamp(string fileName)
        {
            return string.Concat(
                Path.GetFileNameWithoutExtension(fileName),
                " ",
                DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                Path.GetExtension(fileName)
                );
        }
        protected static string AppendDateStamp(string fileName)
        {
            return string.Concat(
                Path.GetFileNameWithoutExtension(fileName),
                " ",
                DateTime.Now.ToString("yyyyMMdd"),
                Path.GetExtension(fileName)
                );
        }
        protected static string ReturnOriginalFileName(string FileName)
        {
            string[] SplitFileName = FileName.Split(' ');
            return string.Concat(
            SplitFileName[0],
            Path.GetExtension(FileName)
            );
        }

    }
}