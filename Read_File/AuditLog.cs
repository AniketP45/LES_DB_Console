using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Read_File
{
    public class AuditLog
    {
        public int LogId { get; set; }
        public string ModuleName { get; set; }
        public string FileName { get; set; }
        public string AuditValue { get; set; }
        public string KeyRef1 { get; set; }
        public string KeyRef2 { get; set; }
        public string LogType { get; set; }
        public DateTime? UpdateDate { get; set; }
        public int? BuyerId { get; set; }
        public int? SupplierId { get; set; }
        public string ServerName { get; set; }
        public string BuyerCode { get; set; }
        public string SupplierCode { get; set; }
        public string DocType { get; set; }
        public string FileName2 { get; set; }
        public string FileName3 { get; set; }
        public string ProcessorName { get; set; }
        public int? UpdateDateInt { get; set; }
    }

}
