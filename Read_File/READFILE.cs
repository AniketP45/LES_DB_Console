using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace Read_File
{
    public class READFILE : LeSCommon.LeSCommon
    {

        List<string> AuditFiles;

        public string AuditLogPath = "",AuditLog_API="";

        public void StartProcess()
        {
            try
            {
                AuditLogPath = ConfigurationManager.AppSettings["AUDITLOG_PATH"];
                AuditLog_API = ConfigurationManager.AppSettings["AUDITLOG_API"];
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
                    if (ProcessAuditFile(AuditFile))
                    {
                        LogText = Path.GetFileName(AuditFile) + " Processed Successfully";
                        MoveFiles(AuditFile, AuditLogPath + "\\Success\\" + Path.GetFileName(AuditFile));

                    }
                    else
                    {
                        LogText = "Error on Processing File -"+ Path.GetFileName(AuditFile);

                        MoveFiles(AuditFile, AuditLogPath + "\\ERROR\\" + Path.GetFileName(AuditFile));

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

      

      

        public  bool ProcessAuditFile(string path)
        {
            string[] slAudit;
            string result;
            string json = "";
            bool response = false;
            AuditLog auditLog = new AuditLog();
            try
            {
                if (path == null)
                {
                    throw new ArgumentNullException("path");
                }

                using (StreamReader streamReader = new StreamReader(path, Encoding.UTF8))
                {
                    //result = streamReader.ReadToEnd();

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


        public bool PostData(string json,string path)
        {

            string url = AuditLog_API;

            _httpWrapper.AcceptMimeType = "*/*";
            _httpWrapper.RequestMethod = "POST";
            _httpWrapper.ContentType = "application/json";

            bool result = _httpWrapper.PostURL(url, json, "", "", "");
            if (result)
            {
                LogText = "Data posted Successfully for file -" + Path.GetFileName(path);
            }


            return result;
        }

    }
}
