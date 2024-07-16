using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Configuration;
using System.Text;
using System.Threading.Tasks;


namespace Read_Audit_File
{
    public class ReadFile : LeSCommon.LeSCommon
    {
        List<string> AuditFiles;

        public string AuditLogPath = "";

        public void StartProcess()
        {
            try
            {
                AuditLogPath = ConfigurationManager.AppSettings["AUDITLOG_PATH"];
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

                Read_Audit_File();

            }
            catch (Exception ex)
            {
                LogText = "Error occurred while initialization" + ex.Message;
            }
        }



        public void Read_Audit_File()
        {
            AuditFiles = GetQuotationFileList(AuditLogPath);
            if (AuditFiles != null && AuditFiles.Count > 0)
            {

                //LeSDataMain.LeSDM.AddLog("Quotation found " + _QuoteXmlFiles.Count + ".");
                for (int i = 0; i < AuditFiles.Count; i++)
                {
                    string AuditFile = AuditFiles[i];
                    if (ProcessAuditFile(AuditFile))
                    {
                        //LeSDataMain.LeSDM.MoveFiles(_QuoteFile, Path.GetDirectoryName(_QuoteFile) + "\\Backup", "Quotation save successfully.");
                    }
                    else
                    {
                        //LeSDataMain.LeSDM.MoveFiles(_QuoteFile, Path.GetDirectoryName(_QuoteFile) + "\\Error", "Unable to save Quotation.");
                    }
                }
            }
            else
            {
                LogText="No Quotation found.";
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

        public bool ProcessAuditFile(string _QuoteFile)
        {
            return false;
        }

    }
}
