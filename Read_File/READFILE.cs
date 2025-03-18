using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.AccessControl;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Read_File
{
    public class READFILE : LeSCommon.LeSCommon
    {

        List<string> AuditFiles;

        public string AuditLogPath = "",AuditLog_API="", AUTH_API="";

        public void StartProcess()
        {
            try
            {
                AuditLogPath = ConfigurationManager.AppSettings["AUDITLOG_PATH"];
                AuditLog_API = ConfigurationManager.AppSettings["AUDITLOG_API"];
                AUTH_API = ConfigurationManager.AppSettings["AUTH_API"];

                
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

                Read_Audit_File();

            }
            catch (Exception ex)
            {
                LogText =   ex.Message;
            }
        }



        public void Read_Audit_File()
        {
            AuditFiles = GetQuotationFileList(AuditLogPath);
            if (AuditFiles != null && AuditFiles.Count > 0)
            {
                LogText = "Total file found =" + AuditFiles.Count;
                for (int i = 0; i < AuditFiles.Count; i++)
                {
                    string AuditFile = AuditFiles[i];
                    string result = ProcessAuditFile(AuditFile);

                    switch (result)
                    {
                        case nameof(FileProcessingResult.ServerError):
                            continue;  

                        case nameof(FileProcessingResult.Success):
                            LogText = "Process Completed Successfully for file - " + Path.GetFileName(AuditFile);
                            MoveFiles(AuditFile, AuditLogPath + "\\Success\\" + Path.GetFileName(AuditFile));
                            break;

                        case nameof(FileProcessingResult.Failure):
                            LogText = "Error on Processing File -" + Path.GetFileName(AuditFile);
                            MoveFiles(AuditFile, AuditLogPath + "\\ERROR\\" + Path.GetFileName(AuditFile));
                            break;
                    }



                }
            }
            else
            {
                LogText = "No Audit file found.";
            }
        }


        public List<string> GetQuotationFileList(string _filePath)
        {
            List<string> _lst = new List<string>();
            try
            {
                _lst = Directory.GetFiles(_filePath, "*.txt").ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("Error on GetQuotationFileList : " + ex.Message);
            }
            return _lst;
        }

      

      

        public  string ProcessAuditFile(string path)
        {
            string[] slAudit;
            string result;
            string json = "";
            string response = "Failure";
            AuditLog auditLog = new AuditLog();

            LogText = "Processing Started for file -" + Path.GetFileName(path);

            try
            {
                if (path == null)
                {
                    throw new ArgumentNullException("path");
                }

                using (StreamReader streamReader = new StreamReader(path, Encoding.UTF8))
                {

                    while ((result = streamReader.ReadLine()) != null)
                    {
                        try
                        {

                            if ((convert.ToString(result).Trim().Length > 0))
                            {
                                slAudit = convert.ToString(result).Split('|');

                                if (slAudit.Length > 0 && Convert.ToString(slAudit[0]) != "") auditLog.BuyerCode = Convert.ToString(slAudit[0]);
                                if (slAudit.Length > 1 && Convert.ToString(slAudit[1]) != "") auditLog.SupplierCode = Convert.ToString(slAudit[1]);
                                if (slAudit.Length > 2 && Convert.ToString(slAudit[2]) != "") auditLog.ModuleName = Convert.ToString(slAudit[2]);
                                if (slAudit.Length > 3) auditLog.FileName = Convert.ToString(slAudit[3]);
                                if (slAudit.Length > 4) auditLog.KeyRef2 = Convert.ToString(slAudit[4]);
                                if (slAudit.Length > 5) auditLog.LogType = Convert.ToString(slAudit[5]);
                                if (slAudit.Length > 6) auditLog.AuditValue = Convert.ToString(slAudit[6]);
                                if (slAudit.Length > 9) auditLog.ServerName = Convert.ToString(slAudit[8]);
                                if (slAudit.Length > 10 && Convert.ToString(slAudit[0]) == "") auditLog.BuyerCode = convert.ToString(slAudit[9]);
                                if (slAudit.Length > 11 && Convert.ToString(slAudit[1]) == "") auditLog.SupplierCode = convert.ToString(slAudit[10]);
                                if (slAudit.Length > 12) auditLog.DocType = convert.ToString(slAudit[11]);
                                auditLog.UpdateDate = DateTime.Now;
                                //auditLog.UpdateDateInt =Convert.ToInt32(DateTime.Now.ToString("yyMMddHHmm"));

                                json = JsonConvert.SerializeObject(auditLog, Formatting.Indented);

                                response = PostData(json, path);

                                 

                            }
                        }catch(Exception ex)
                        {
                            throw ex;
                        }
                    }
                }
            }catch(Exception ex)
            {
                LogText  = ex.Message;
            }
            return response;
        }


        public string PostData(string json,string path)
        {
            string result = "Failure";

            try
            {
                _httpWrapper._SetRequestHeaders.Clear();
                string url = AuditLog_API;
                string token = GetJwtTokenAsync(AUTH_API).Result;

                _httpWrapper._SetRequestHeaders.Add(HttpRequestHeader.Authorization, "Bearer "+ token);
                _httpWrapper.AcceptMimeType = "*/*";
                _httpWrapper.RequestMethod = "POST";
                _httpWrapper.ContentType = "application/json";

                bool response = _httpWrapper.PostURL(url, json, "", "", "");
                if (response)
                {
                    LogText = "Data posted Successfully for file -" + Path.GetFileName(path);
                    result = "Success";
                }
            }catch (Exception ex)
            {
                if (IsNetworkIssue(ex.Message))
                {
                    LogText = "File Will not move to Error";
                    result = "ServerError";

                }

            }

            return result;
        }

 public async Task<string> GetJwtTokenAsync(string tokenEndpoint)
    {
        string result = "";

        HttpClient client = new HttpClient();
        HttpResponseMessage response = await client.PostAsync(tokenEndpoint,null);

        if (response.IsSuccessStatusCode)
        {
            string responseBody = await response.Content.ReadAsStringAsync();
            //Console.WriteLine(responseBody);
            result = responseBody; 
        }
        else
        {
            Console.WriteLine($"Error: {response.StatusCode}");
        }

        return result;
    }

        public Boolean IsNetworkIssue(string cExcepetion)
        {
            Boolean ret = false;
            try
            {
                if (cExcepetion.Contains("The remote server returned an error: (403) Forbidden"))
                    ret = true;
                else if (cExcepetion.Contains("The remote server returned an error: (405)"))
                    ret = true;
                else if (cExcepetion.Contains("The remote server returned an error: (404) Not Found"))
                    ret = true;
                else if (cExcepetion.Contains("Server Unavailable"))
                    ret = true;
                else if (cExcepetion.Contains("Could not get any response"))
                    ret = true;
                else if (cExcepetion.Contains("Unable to connect to the remote server"))
                    ret = true;
                else if (cExcepetion.Contains("Exception has been thrown by the target of an invocation"))
                    ret = true;
                else if (cExcepetion.Contains("(500) Internal Server Error"))
                    ret = true;
                else if (cExcepetion.Contains("Encrypted Message Required"))
                    ret = true;
                else if (cExcepetion.Contains("The operation has timed out"))
                    ret = true;
            }
            catch (Exception ex)
            {
                LogText = "Error in checking network issue - " + ex.Message;
            }
            return ret;
        }


        enum FileProcessingResult
        {
            Success,
            Failure,
            ServerError
        }

    }
}
