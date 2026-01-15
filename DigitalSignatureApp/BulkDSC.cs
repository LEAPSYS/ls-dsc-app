using System;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net.Http;
using Newtonsoft.Json;
using DigitalSignatureApp.Models;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using DigitalSignatureApp.Loadout;
using Dapper;
using DigitalSignatureApp.Models.DSCViewModel;
using DigitalSignatureApp.DSCRepository;
using System.Threading;
using System.Security.Permissions;
using System.Security.Principal;
using System.Diagnostics;
using System.Security.AccessControl;
using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;
using Newtonsoft.Json.Linq;

namespace DigitalSignatureApp
{

    public class BulkDSC : CommonServices
    {
        #region Initialization
        private DSCInfoAndAPI ConfigSettings;
        private static readonly ApiConfig ApiConfiguration;
        private static readonly bool TurnOnLog, LogFileWriteStatus, EachStepLog;
        private GeneratedReportDetails SingleReport;
        private IDSCRepository _IDSCRepository;
        static BulkDSC()
        {
            ApiConfiguration = ConfigStore.ApiConfig;
            TurnOnLog = ConfigStore.CommonServices.TurnOnExceptionLog;
            LogFileWriteStatus = ConfigStore.CommonServices.WriteAtEverySuccessOrFail;
            EachStepLog = ConfigStore.CommonServices.StepByStepEvaluation;
        }
        private readonly HttpClient client;
        public BulkDSC(IHttpClientFactory httpClientFactory)
        {
            client = httpClientFactory.CreateClient("signingAPI");
            ConfigSettings = ConfigStore.DSCInfoAndAPI;
            _IDSCRepository = new DigitalSignatureRepository();
        }
        public void SetReportDetails(GeneratedReportDetails reportDetails)
        {
            SingleReport = reportDetails;
            ConfigSettings = ConfigStore.DSCInfoAndAPI;
            _IDSCRepository = new DigitalSignatureRepository();
        }
        #endregion
        public async Task SeperateThreadDSC()
        {
            //Thread.Sleep(60000);
            string FileName, OnlyFileName, OutputFolderName, DownloadFilePath;
            string convertedFile;
            Byte[] bytes_returned;
            try
            {
                ConfigSettings.AuthorizedSignatory = SingleReport.AuthCompany;
                FileName = SingleReport.FileName;
                OnlyFileName = Path.GetFileNameWithoutExtension(SingleReport.ReportPath);
                convertedFile = Convert.ToBase64String(SingleReport.PDFInBytes);
                OutputFolderName = string.Concat(SingleReport.DatabaseName, "#", SingleReport.AuthCompany, "#", SingleReport.SignerName);

                try
                {
                    if (string.IsNullOrEmpty(SingleReport.SignerName))
                    {
                        var GetSigner = await _IDSCRepository.GetSignerList(SingleReport.DocEntry, SingleReport.DocType);
                        var GetSignerList = GetSigner.ToList();
                        if (GetSignerList.Count == 0)
                            throw new NullReferenceException("Signer list was not found for " + FileName);
                        foreach (var signer in GetSignerList)
                        {
                            try
                            {
                                if (String.IsNullOrEmpty(signer.SignerName))
                                    continue;
                                ConfigSettings.SignerName = signer.SignerName;
                                ConfigSettings.FindAuth = signer.SignerTextSearch;
                                ConfigSettings.TopLeft = signer.TopLeft;
                                ConfigSettings.pdfByte1 = convertedFile;
                                convertedFile = await SignDocument(FileName);
                                if (String.Equals(convertedFile, "BreakCase"))
                                    break;
                            }
                            catch (Exception ex)
                            {
                                ExceptionGeneration(ex);
                            }
                        }
                        if (String.Equals(convertedFile, "BreakCase"))
                            return;
                    }
                    else
                    {
                        ConfigSettings.SignerName = SingleReport.SignerName;
                        ConfigSettings.pdfByte1 = convertedFile;
                        convertedFile = await SignDocument(FileName);
                        if (String.Equals(convertedFile, "BreakCase"))
                            return;
                    }
                    string downloadDir = ApiConfiguration.DSCOutLocation;

                    var outPath = await _IDSCRepository.GetOutPath(SingleReport.DocNum, SingleReport.DocType, SingleReport.DatabaseName);

                    try
                    {
                        if (!string.IsNullOrEmpty(outPath.FirstOrDefault().OutPath))
                        {
                            Console.WriteLine($"OutPath from DB: {outPath.FirstOrDefault().OutPath}");
                            downloadDir = outPath.FirstOrDefault().OutPath;
                            OutputFolderName = string.Empty;
                        }
                    }
                    catch (Exception)
                    {
                    }

                    string finalPath = Path.Combine(downloadDir, OutputFolderName);
                    if (!Directory.Exists(finalPath))
                        Directory.CreateDirectory(finalPath);
                    DownloadFilePath = Path.Combine(finalPath, FileName);
                    bytes_returned = Convert.FromBase64String(convertedFile);
                    System.IO.File.WriteAllBytes(DownloadFilePath, bytes_returned);
                    if (LogFileWriteStatus)
                    {
                        WriteSuccessfulFileGeneration(finalPath, FileName);
                    }
                    WriteEachStep("File successfully signed", EachStepLog);

                    var UpdateView = await _IDSCRepository.UpdateView(SingleReport.DatabaseName, SingleReport.DocNum,
                                SingleReport.DocType, DownloadFilePath);
                }
                catch (Exception ex)
                {
                    ExceptionGeneration(ex);
                }
            }
            catch (Exception ex)
            {
                ExceptionGeneration(ex);
            }
            finally
            {
                await Task.CompletedTask;
            }
        }
        public async Task ManualDSC()
        {
            #region UnusedCodeSavedForFutureReference
            //string uploadPath = "\\\\192.168.21.144";
            //string subPath = Path.Combine(uploadPath, "Attachment");
            //string uploadDir = Path.Combine(subPath, "DSCIN");
            //string[] fileEntries;
            //string FileName = String.Empty;
            //string uploadPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            //string uploadDir = Path.Combine(uploadPath, "PDFUploaded");
            #endregion
            string FileName, OnlyFileName, OutputFolderName, DatabaseName;
            string uploadDir = ApiConfiguration.DSCInLocation, convertedFile;
            string[] pdfFileEntries, folderEntries, SplitFolderName;
            Byte[] bytes;
            try
            {
                WriteEachStep("Scanning for files in directories", EachStepLog);
                folderEntries = Directory.GetDirectories(uploadDir);
                foreach (string folderName in folderEntries)
                {
                    pdfFileEntries = Directory.GetFiles(folderName, "*.pdf", SearchOption.TopDirectoryOnly);
                    foreach (string pdfFileName in pdfFileEntries)
                    {
                        try
                        {
                            WriteEachStep("PDF Files detected, preparing to send them to API", EachStepLog);
                            try
                            {
                                SplitFolderName = folderName.Split('#');
                                DatabaseName = new DirectoryInfo(SplitFolderName[0]).Name;
                                ConfigSettings.AuthorizedSignatory = SplitFolderName[1];
                                ConfigSettings.SignerName = SplitFolderName[2];
                            }
                            catch (Exception ex)
                            {
                                System.IO.File.Delete(pdfFileName);
                                ExceptionGeneration(ex);
                                continue;
                            }
                            FileName = Path.GetFileName(pdfFileName);
                            OnlyFileName = Path.GetFileNameWithoutExtension(pdfFileName);
                            OutputFolderName = new DirectoryInfo(folderName).Name;
                            bytes = System.IO.File.ReadAllBytes(pdfFileName);
                            convertedFile = Convert.ToBase64String(bytes);
                            WriteEachStep("Converted File to Base64", EachStepLog);
                            await ManualSignOperation(pdfFileName, FileName, OutputFolderName);
                        }
                        catch (Exception ex)
                        {
                            ExceptionGeneration(ex);
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                // exception possibly related to establishing folder location on network or converting pdf file
                if (TurnOnLog)
                {
                    ExceptionGeneration(ex);
                }
            }
        }

        private async Task ManualSignOperation(string pdfFileName,
            string FileName, string OutputFolderName)
        {
            try
            {
                byte[] bytes, bytes_returned;
                string convertedFile, PDFinBase64, DownloadFilePath;
                bytes = System.IO.File.ReadAllBytes(pdfFileName);
                convertedFile = Convert.ToBase64String(bytes);
                ConfigSettings.pdfByte1 = convertedFile;
                PDFinBase64 = await SignDocument(FileName);
                if (String.Equals(PDFinBase64, "BreakCase"))
                    return;
                string downloadDir = ApiConfiguration.DSCOutLocation;
                string finalPath = Path.Combine(downloadDir, OutputFolderName);
                if (!Directory.Exists(finalPath))
                    Directory.CreateDirectory(finalPath);
                DownloadFilePath = Path.Combine(finalPath, FileName);
                bytes_returned = Convert.FromBase64String(PDFinBase64);
                System.IO.File.WriteAllBytes(DownloadFilePath, bytes_returned);
                System.IO.File.Delete(pdfFileName);
                if (LogFileWriteStatus)
                {
                    WriteSuccessfulFileGeneration(finalPath, FileName);
                }
                WriteEachStep("File successfully signed - Manual", EachStepLog);
            }
            catch (Exception ex)
            {
                if (TurnOnLog)
                {
                    ExceptionGeneration(ex);
                }
            }
        }

        private async Task<string> SignDocument(string FileName)
        {

            HttpRequestMessage request;
            request = new HttpRequestMessage(HttpMethod.Post, "");
            var stringPayload = JsonConvert.SerializeObject(ConfigSettings);
            //WriteSerializedPayload(stringPayload, FileName);
            var content = new StringContent(stringPayload, Encoding.UTF8, "application/json");
            request.Content = content;
            WriteEachStep("JSON Object serialized and is prepared to be sent", EachStepLog);
            try
            {
                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                DeserializeData DecodedData = new DeserializeData();
                var responseContent = await response.Content.ReadAsStringAsync();
                try
                {
                    DecodedData = JsonConvert.DeserializeObject<DeserializeData>(responseContent);
                    if (EachStepLog)
                    {
                        WriteEachStep("Response from API deserialized", EachStepLog);
                    }
                }
                catch (Exception ex)
                {
                    DecodedData.error = "exception";
                    DecodedData.file = "exception";
                    DecodedData.status = "exception";
                    if (TurnOnLog)
                    {
                        ExceptionGeneration(ex);
                        WritePayload(responseContent, FileName);
                    }
                }

                if (String.Equals(DecodedData.status.ToLower(), "success"))
                {
                    return DecodedData.file;
                }
                else
                {
                    if (LogFileWriteStatus)
                    {
                        WriteFailedFile(FileName, new InvalidDataException(DecodedData.error));
                    }
                    WriteFailedFile(FileName, new InvalidDataException(DecodedData.error));
                    ExceptionGeneration(new InvalidDataException(DecodedData.error));
                    return "BreakCase";
                }
            }
            catch (Exception ex)
            {
                // exception related to establishing connection with api
                if (TurnOnLog)
                {
                    ExceptionGeneration(ex);
                }
                if (LogFileWriteStatus)
                {
                    WriteFailedFile(FileName, ex);
                }
                return "BreakCase";
            }
        }
        protected virtual bool IsFileLocked(FileInfo file)
        {
            try
            {
                if (!file.Exists)
                {
                    return false;
                }
                using (FileStream stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    stream.Close();
                }
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }

            //file is not locked
            return false;
        }
    }
}