using DigitalSignatureApplication.Config;
using DigitalSignatureApplication.Models;
using DigitalSignatureApplication.Repository;
using Newtonsoft.Json;
using Serilog;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DigitalSignatureApplication
{

    public class BulkSigningService : CommonServices
    {
        private LegacyPayload legacyPayload;
        private static readonly ApiConfig ApiConfiguration;
        private static readonly bool TurnOnLog, LogFileWriteStatus, EachStepLog;
        private GeneratedReportDetails SingleReport;
        private DbRepository dbRepository;
        static BulkSigningService()
        {
            ApiConfiguration = ConfigStore.ApiConfig;
            TurnOnLog = ConfigStore.CommonServices.TurnOnExceptionLog;
            LogFileWriteStatus = ConfigStore.CommonServices.WriteAtEverySuccessOrFail;
            EachStepLog = ConfigStore.CommonServices.StepByStepEvaluation;
        }
        private readonly HttpClient client;
        public BulkSigningService(IHttpClientFactory httpClientFactory)
        {
            client = httpClientFactory.CreateClient("signingAPI");
            legacyPayload = ConfigStore.LegacyPayload;
            dbRepository = new DbRepository();
        }
        public void SetReportDetails(GeneratedReportDetails reportDetails)
        {
            SingleReport = reportDetails;
            legacyPayload = ConfigStore.LegacyPayload;
            dbRepository = new DbRepository();
        }
        public async Task SeperateThreadDSC()
        {
            string FileName, OnlyFileName, OutputFolderName, DownloadFilePath;
            string convertedFile;
            Byte[] bytes_returned;
            try
            {
                legacyPayload.AuthorizedSignatory = SingleReport.AuthCompany;
                FileName = SingleReport.FileName;
                OnlyFileName = Path.GetFileNameWithoutExtension(SingleReport.ReportPath);
                convertedFile = Convert.ToBase64String(SingleReport.PDFInBytes);
                OutputFolderName = string.Concat(SingleReport.DatabaseName, "#", SingleReport.AuthCompany, "#", SingleReport.SignerName);

                try
                {
                    if (string.IsNullOrEmpty(SingleReport.SignerName))
                    {
                        var GetSigner = await dbRepository.GetSignerList(SingleReport.DocEntry, SingleReport.DocType);
                        var GetSignerList = GetSigner.ToList();
                        if (GetSignerList.Count == 0)
                            throw new NullReferenceException("Signer list was not found for " + FileName);
                        foreach (var signer in GetSignerList)
                        {
                            try
                            {
                                if (String.IsNullOrEmpty(signer.SignerName))
                                    continue;
                                legacyPayload.SignerName = signer.SignerName;
                                legacyPayload.FindAuth = signer.SignerTextSearch;
                                legacyPayload.TopLeft = signer.TopLeft;
                                legacyPayload.pdfByte1 = convertedFile;
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
                        legacyPayload.SignerName = SingleReport.SignerName;
                        legacyPayload.pdfByte1 = convertedFile;
                        convertedFile = await SignDocument(FileName);
                        if (String.Equals(convertedFile, "BreakCase"))
                            return;
                    }
                    string downloadDir = ApiConfiguration.DSCOutLocation;

                    var outPath = await dbRepository.GetOutPath(SingleReport.DocNum, SingleReport.DocType, SingleReport.DatabaseName);

                    try
                    {
                        if (!string.IsNullOrEmpty(outPath.FirstOrDefault().OutPath))
                        {
                            Log.Information($"OutPath from DB: {outPath.FirstOrDefault().OutPath}");
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
                    Log.Information("File successfully signed");

                    var UpdateView = await dbRepository.UpdateView(SingleReport.DatabaseName, SingleReport.DocNum,
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
                Log.CloseAndFlush();
                await Task.CompletedTask;
            }
        }
        public async Task ManualDSC()
        {
            string FileName, OnlyFileName, OutputFolderName, DatabaseName;
            string uploadDir = ApiConfiguration.DSCInLocation, convertedFile;
            string[] pdfFileEntries, folderEntries, SplitFolderName;
            Byte[] bytes;
            try
            {
                WriteEachStep("Scanning for files in directories", EachStepLog);
                Log.Information("Scanning for files in directories");
                folderEntries = Directory.GetDirectories(uploadDir);
                foreach (string folderName in folderEntries)
                {
                    pdfFileEntries = Directory.GetFiles(folderName, "*.pdf", SearchOption.TopDirectoryOnly);
                    foreach (string pdfFileName in pdfFileEntries)
                    {
                        try
                        {
                            Log.Information("PDF Files detected, preparing to send them to API");
                            WriteEachStep("PDF Files detected, preparing to send them to API", EachStepLog);
                            try
                            {
                                SplitFolderName = folderName.Split('#');
                                DatabaseName = new DirectoryInfo(SplitFolderName[0]).Name;
                                legacyPayload.AuthorizedSignatory = SplitFolderName[1];
                                legacyPayload.SignerName = SplitFolderName[2];
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
                            Log.Information("Converted File to Base64");
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
                if (TurnOnLog)
                {
                    ExceptionGeneration(ex);
                }
            } 
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private async Task ManualSignOperation(string pdfFileName, string FileName, string OutputFolderName)
        {
            try
            {
                byte[] bytes, bytes_returned;
                string convertedFile, PDFinBase64, DownloadFilePath;
                bytes = System.IO.File.ReadAllBytes(pdfFileName);
                convertedFile = Convert.ToBase64String(bytes);
                legacyPayload.pdfByte1 = convertedFile;
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
                    Log.Information($"File generated succesfully {finalPath} {FileName}");
                    WriteSuccessfulFileGeneration(finalPath, FileName);
                }
                Log.Information("File successfully signed - Manual");
                WriteEachStep("File successfully signed - Manual", EachStepLog);
            }
            catch (Exception ex)
            {
                if (TurnOnLog)
                {
                    ExceptionGeneration(ex);
                }
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private async Task<string> SignDocument(string FileName)
        {
            HttpRequestMessage request;
            request = new HttpRequestMessage(HttpMethod.Post, "");
            var stringPayload = JsonConvert.SerializeObject(legacyPayload);
            var content = new StringContent(stringPayload, Encoding.UTF8, "application/json");
            request.Content = content;
            Log.Information("JSON Object serialized and is prepared to be sent");
            Log.Information(stringPayload);
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
                    Log.Information($"Response from API {DecodedData}");
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
            finally
            {
                Log.CloseAndFlush();
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
                return true;
            }
            return false;
        }
    }
}