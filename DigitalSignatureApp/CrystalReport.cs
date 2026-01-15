using CrystalDecisions.Shared;
using CrystalDecisions.CrystalReports.Engine;
using System.IO;
using DigitalSignatureApp.Models;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using DigitalSignatureApp.Loadout;
using DigitalSignatureApp.Loadout.DSCViewModel;
using Dapper;
using System.Text;
using DigitalSignatureApp.DSCRepository;
using System.Security.Principal;
using Microsoft.Extensions.DependencyInjection;

namespace DigitalSignatureApp
{
    public class CrystalReport : CommonServices
    {
        #region Initialization
        private static readonly ConnectionAndReportDetails DataSourceCredentials = new ConnectionAndReportDetails();
        private static readonly bool TurnOnLog, EachStepLog, GenerateSeparatePDF;
        private readonly IDSCRepository _IDSCRepository;
        static CrystalReport()
        {
            DataSourceCredentials = ConfigStore.ConnectionAndReportDetails;
            TurnOnLog = ConfigStore.CommonServices.TurnOnExceptionLog;
            EachStepLog = ConfigStore.CommonServices.StepByStepEvaluation;
            GenerateSeparatePDF = ConfigStore.CommonServices.ReportGenerationSettings.DSCService;
        }
        private readonly IServiceProvider serviceProvider;
        public CrystalReport(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
            _IDSCRepository = new DigitalSignatureRepository();
        }
        #endregion
        public async Task<IEnumerable<DSCViewModel>> PopulateView()
        {
            IEnumerable<DSCViewModel> VM = new List<DSCViewModel>();
            try
            {
                VM = await _IDSCRepository.PopulateView();
                return VM;
            }
            catch (Exception ex)
            {
                if (TurnOnLog)
                    ExceptionGeneration(ex);
                return VM;
            }
        }
        async Task<HashSet<GeneratedReportDetails>> GenerateCrystalReport(IEnumerable<DSCViewModel> ViewModel)
        {
            int threadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
            string ReportExportLocation;
            String DatabaseName, DocNum, Type, FileName, FileNameWithTimeStamp;
            HashSet<GeneratedReportDetails> ReportList = new HashSet<GeneratedReportDetails>();
            List<Task> TaskList = new List<Task>();
            #region PrepareCrystalReportAndExport
            try
            {
                foreach (var record in ViewModel)
                {
                    using (ReportDocument crystalReport = new ReportDocument())
                    {
                        try
                        {
                            
                            //GeneratedPayloadFromVM<DSCViewModel>(record);
                            //GeneratedPayloadFromVM(record);
                            WriteEachStep("Data exists in data table, generating reports", EachStepLog);
                            String Export_Location = record.CREXPORT;
                            if (!Directory.Exists(Export_Location))
                            {
                                Directory.CreateDirectory(Export_Location);
                            }
                            string DocNumWithSplCharacters = record.DocNum;
                            record.DocNum = RemoveSpecialCharacters(record.DocNum);
                            FileName = record.Type + "_" + record.DocNum + ".pdf";
                            //FileName = String.Concat(record.Type, "_", record.DocNum, ".pdf");
                            FileNameWithTimeStamp = AppendTimeStamp(FileName);
                            ReportExportLocation = Export_Location + "\\" + FileNameWithTimeStamp;
                            DocNum = record.DocNum;
                            Type = record.Type;
                            String crystalReportLocation = record.CRPath;
                            crystalReport.Load(crystalReportLocation);
                            ExportOptions exportOptions = new ExportOptions();
                            DiskFileDestinationOptions diskFileDestinationOptions = new DiskFileDestinationOptions();
                            PdfRtfWordFormatOptions pdfRtfWordFormatOptions = new PdfRtfWordFormatOptions();
                            DatabaseName = record.Database;
                            ConnectionInfo(crystalReport);
                            crystalReport.SetParameterValue(0, record.DocEntry);
                            diskFileDestinationOptions.DiskFileName = ReportExportLocation;
                            exportOptions = crystalReport.ExportOptions;
                            exportOptions.ExportDestinationType = ExportDestinationType.DiskFile;
                            exportOptions.ExportFormatType = ExportFormatType.PortableDocFormat;
                            exportOptions.ExportDestinationOptions = diskFileDestinationOptions;
                            exportOptions.ExportFormatOptions = pdfRtfWordFormatOptions;
                            string TableName = record.TBName;
                            string SignerName = record.Auth_Signatory;
                            var PDFFileStream = crystalReport.ExportToStream(ExportFormatType.PortableDocFormat);
                            var PDFInbytes = ReadFully(PDFFileStream);
                            GeneratedReportDetails reportDetails = new GeneratedReportDetails(PDFInbytes, FileNameWithTimeStamp, record.TBName, 
                                record.Type, record.AuthorizedSignatory, record.Auth_Signatory, record.DocNum, record.Database, record.DocEntry,
                                record.DSCShow);
                            //BulkDSC _SignProcess = new BulkDSC(reportDetails);
                            var bulkDSC = serviceProvider.GetRequiredService<BulkDSC>();
                            bulkDSC.SetReportDetails(reportDetails);
                            var task = Task.Run(() => bulkDSC.SeperateThreadDSC());
                            TaskList.Add(task);
                            if (GenerateSeparatePDF)
                                crystalReport.Export();
                        }
                        catch (Exception ex)
                        {
                            if (TurnOnLog)
                                ExceptionGeneration(ex);
                        }
                        finally
                        {
                            crystalReport.Close();
                            crystalReport.Dispose();
                        }
                        if (EachStepLog)
                            SubEachStep("Report Generation Done", EachStepLog, threadId);
                    }
                }
            }
            catch (Exception ex)
            {
                if (TurnOnLog)
                    ExceptionGeneration(ex);
            }
            finally
            {
                foreach(var tasks in TaskList)
                {
                    await tasks;
                }
            }
            return ReportList;
            #endregion
        }
        private static byte[] ReadFully(Stream input)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                input.CopyTo(ms);
                return ms.ToArray();
            }
        }
        private ReportDocument ConnectionInfo(ReportDocument rpt)
        {
            ReportDocument crSubreportDocument;
            Database oCRDb = rpt.Database;

            Tables oCRTables = oCRDb.Tables;

            CrystalDecisions.CrystalReports.Engine.Table oCRTable = default(CrystalDecisions.CrystalReports.Engine.Table);

            TableLogOnInfo oCRTableLogonInfo = default(CrystalDecisions.Shared.TableLogOnInfo);

            ConnectionInfo oCRConnectionInfo = new CrystalDecisions.Shared.ConnectionInfo();

            oCRConnectionInfo.ServerName = DataSourceCredentials.loginDetails.ServerName;
            oCRConnectionInfo.Password = DataSourceCredentials.loginDetails.password;
            oCRConnectionInfo.UserID = DataSourceCredentials.loginDetails.userId;
            //oCRConnectionInfo.DatabaseName = "";

            for (int i = 0; i < oCRTables.Count; i++)
            {
                oCRTable = oCRTables[i];
                oCRTableLogonInfo = oCRTable.LogOnInfo;
                oCRTableLogonInfo.ConnectionInfo = oCRConnectionInfo;
                oCRTable.ApplyLogOnInfo(oCRTableLogonInfo);
                //if (oCRTable.TestConnectivity())
                //    //' If there is a "." in the location then remove the
                //    // ' beginning of the fully qualified location.
                //    //' Example "dbo.northwind.customers" would become
                //    //' "customers".
                //    oCRTable.Location = oCRTable.Location.Substring(oCRTable.Location.LastIndexOf(".") + 1);
            }

            for (int i = 0; i < rpt.Subreports.Count; i++)
            {
                {
                    //  crSubreportObject = (SubreportObject);
                    crSubreportDocument = rpt.OpenSubreport(rpt.Subreports[i].Name);
                    oCRDb = crSubreportDocument.Database;
                    oCRTables = oCRDb.Tables;
                    foreach (CrystalDecisions.CrystalReports.Engine.Table aTable in oCRTables)
                    {
                        oCRTableLogonInfo = aTable.LogOnInfo;
                        oCRTableLogonInfo.ConnectionInfo = oCRConnectionInfo;
                        aTable.ApplyLogOnInfo(oCRTableLogonInfo);
                        //if (aTable.TestConnectivity())
                        //    //' If there is a "." in the location then remove the
                        //    // ' beginning of the fully qualified location.
                        //    //' Example "dbo.northwind.customers" would become
                        //    //' "customers".
                        //    aTable.Location = aTable.Location.Substring(aTable.Location.LastIndexOf(".") + 1);

                    }
                }
            }
            //  }

            rpt.Refresh();
            return rpt;
        }
        public static string RemoveSpecialCharacters(string str)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in str)
            {
                if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == '_')
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }
        public async Task<HashSet<GeneratedReportDetails>> Report()
        {

            WriteEachStep("Report Generation Started", EachStepLog);
            IEnumerable<DSCViewModel> Model = await PopulateView();
            HashSet<GeneratedReportDetails> ListOfReport = await GenerateCrystalReport(Model);
            return ListOfReport;
        }

        
    }
}
