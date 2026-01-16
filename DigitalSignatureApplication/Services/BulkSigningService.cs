using DigitalSignatureApplication.Config;
using DigitalSignatureApplication.Models;
using DigitalSignatureApplication.Repository;
using Microsoft.Extensions.Logging;
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
        private LegacyPayload _legacyPayload;
        private static readonly ApiConfig _apiConfig;
        private static readonly bool _enableExLog, _enableFileLog, _enableStepLog;
        private GeneratedReportDetails _generatedReport;
        private DbRepository _repository;
        private readonly HttpClient _httpClient;

        static BulkSigningService()
        {
            _apiConfig = ConfigStore.ApiConfig;
            _enableExLog = ConfigStore.CommonServices.TurnOnExceptionLog;
            _enableFileLog = ConfigStore.CommonServices.WriteAtEverySuccessOrFail;
            _enableStepLog = ConfigStore.CommonServices.StepByStepEvaluation;
        }
        public BulkSigningService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("signingAPI");
            _legacyPayload = ConfigStore.LegacyPayload;
            _repository = new DbRepository();
        }
        public void SetGeneratedReport(GeneratedReportDetails generatedReport)
        {
            _generatedReport = generatedReport;
            _legacyPayload = ConfigStore.LegacyPayload;
            _repository = new DbRepository();
        }
        public async Task SeperateThreadDSC()
        {
            string FileName, OnlyFileName, OutputFolderName, DownloadFilePath;
            string convertedFile;
            Byte[] bytes_returned;
            try
            {
                _legacyPayload.AuthorizedSignatory = _generatedReport.AuthCompany;
                FileName = _generatedReport.FileName;
                OnlyFileName = Path.GetFileNameWithoutExtension(_generatedReport.ReportPath);
                convertedFile = Convert.ToBase64String(_generatedReport.PDFInBytes);
                OutputFolderName = string.Concat(_generatedReport.DatabaseName, "#", _generatedReport.AuthCompany, "#", _generatedReport.SignerName);

                try
                {
                    if (string.IsNullOrEmpty(_generatedReport.SignerName))
                    {
                        var GetSigner = await _repository.GetSignerList(_generatedReport.DocEntry, _generatedReport.DocType);
                        var GetSignerList = GetSigner.ToList();
                        if (GetSignerList.Count == 0)
                            throw new NullReferenceException("Signer list was not found for " + FileName);
                        foreach (var signer in GetSignerList)
                        {
                            try
                            {
                                if (String.IsNullOrEmpty(signer.SignerName))
                                    continue;
                                _legacyPayload.SignerName = signer.SignerName;
                                _legacyPayload.FindAuth = signer.SignerTextSearch;
                                _legacyPayload.TopLeft = signer.TopLeft;
                                _legacyPayload.pdfByte1 = convertedFile;
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
                        _legacyPayload.SignerName = _generatedReport.SignerName;
                        _legacyPayload.pdfByte1 = convertedFile;
                        convertedFile = await SignDocument(FileName);
                        if (String.Equals(convertedFile, "BreakCase"))
                            return;
                    }
                    string downloadDir = _apiConfig.DSCOutLocation;

                    var outPath = await _repository.GetOutPath(_generatedReport.DocNum, _generatedReport.DocType, _generatedReport.DatabaseName);

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
                    if (_enableFileLog)
                    {
                        WriteSuccessfulFileGeneration(finalPath, FileName);
                    }
                    WriteEachStep("File successfully signed", _enableStepLog);
                    Log.Information("File successfully signed");

                    var UpdateView = await _repository.UpdateView(_generatedReport.DatabaseName, _generatedReport.DocNum,
                                _generatedReport.DocType, DownloadFilePath);
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
            string uploadDir = _apiConfig.DSCInLocation, convertedFile;
            string[] pdfFileEntries, folderEntries, SplitFolderName;
            Byte[] bytes;
            try
            {
                WriteEachStep("Scanning for files in directories", _enableStepLog);
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
                            WriteEachStep("PDF Files detected, preparing to send them to API", _enableStepLog);
                            try
                            {
                                SplitFolderName = folderName.Split('#');
                                DatabaseName = new DirectoryInfo(SplitFolderName[0]).Name;
                                _legacyPayload.AuthorizedSignatory = SplitFolderName[1];
                                _legacyPayload.SignerName = SplitFolderName[2];
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
                            WriteEachStep("Converted File to Base64", _enableStepLog);
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
                if (_enableExLog)
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
                _legacyPayload.pdfByte1 = convertedFile;
                PDFinBase64 = await SignDocument(FileName);
                if (String.Equals(PDFinBase64, "BreakCase"))
                    return;
                string downloadDir = _apiConfig.DSCOutLocation;
                string finalPath = Path.Combine(downloadDir, OutputFolderName);
                if (!Directory.Exists(finalPath))
                    Directory.CreateDirectory(finalPath);
                DownloadFilePath = Path.Combine(finalPath, FileName);
                bytes_returned = Convert.FromBase64String(PDFinBase64);
                System.IO.File.WriteAllBytes(DownloadFilePath, bytes_returned);
                System.IO.File.Delete(pdfFileName);
                if (_enableFileLog)
                {
                    Log.Information($"File generated succesfully {finalPath} {FileName}");
                    WriteSuccessfulFileGeneration(finalPath, FileName);
                }
                Log.Information("File successfully signed - Manual");
                WriteEachStep("File successfully signed - Manual", _enableStepLog);
            }
            catch (Exception ex)
            {
                if (_enableExLog)
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
            if (_apiConfig.UsePayloadV3)
            {
                var payloadV3 = new PayloadV3();
                payloadV3.b64Pdf = _legacyPayload.pdfByte1;
                payloadV3.authorityName = _legacyPayload.SignerName;
                payloadV3.orgCode = _legacyPayload.AuthorizedSignatory;
                payloadV3.pages = new int[] { 0 };
                payloadV3.placeholderText = _legacyPayload.FindAuth;
                payloadV3.signedQrText = _generatedReport.SignedQrText;
                payloadV3.url = false;
                payloadV3.xAxisOffset = 0;
                payloadV3.yAxisOffset = 0;
                var payloadV3String = JsonConvert.SerializeObject(payloadV3);
                var contentV3 = new StringContent(payloadV3String, Encoding.UTF8, "application/json");
                var responseV3 = await _httpClient.PostAsync("api/digi-sign/v3/do-digi-signing", contentV3);
                responseV3.EnsureSuccessStatusCode();
                DigiSignResponse digiSignResponse = new DigiSignResponse();
                var responseV3Content = await responseV3.Content.ReadAsStringAsync();
                try
                {
                    digiSignResponse = JsonConvert.DeserializeObject<DigiSignResponse>(responseV3Content);
                    Log.Information($"Response from API {digiSignResponse}");
                    if (_enableStepLog)
                    {
                        WriteEachStep("Response from API deserialized", _enableStepLog);
                    }
                }
                catch (Exception ex)
                {
                    if (_enableExLog)
                    {
                        ExceptionGeneration(ex);
                        WritePayload(responseV3Content, FileName);
                    }
                }
                if (string.IsNullOrEmpty(digiSignResponse.b64Signed))
                {
                    return "BreakCase";
                }
                else
                {
                    return digiSignResponse.b64Signed;
                }
            }
            var legacyPayloadString = JsonConvert.SerializeObject(_legacyPayload);
            var content = new StringContent(legacyPayloadString, Encoding.UTF8, "application/json");
            Log.Information("JSON Object serialized and is prepared to be sent");
            Log.Information(legacyPayloadString);
            WriteEachStep("JSON Object serialized and is prepared to be sent", _enableStepLog);
            try
            {
                var response = await _httpClient.PostAsync("api/digi-sign/v1/do-digi-signing-legacy", content);
                response.EnsureSuccessStatusCode();
                DeserializeData DecodedData = new DeserializeData();
                var responseContent = await response.Content.ReadAsStringAsync();
                try
                {
                    DecodedData = JsonConvert.DeserializeObject<DeserializeData>(responseContent);
                    Log.Information($"Response from API {DecodedData}");
                    if (_enableStepLog)
                    {
                        WriteEachStep("Response from API deserialized", _enableStepLog);
                    }
                }
                catch (Exception ex)
                {
                    DecodedData.error = "exception";
                    DecodedData.file = "exception";
                    DecodedData.status = "exception";
                    if (_enableExLog)
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
                    if (_enableFileLog)
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
                if (_enableExLog)
                {
                    ExceptionGeneration(ex);
                }
                if (_enableFileLog)
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