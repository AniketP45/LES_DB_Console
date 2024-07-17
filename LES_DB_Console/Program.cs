using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Read_File;

namespace LES_DB_Console
{
    internal class Program
    {
        static string processor_name = Convert.ToString(ConfigurationManager.AppSettings["PROCESSOR_NAME"]) + " : " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        static void Main(string[] args)
        {
            READFILE read = new READFILE();
            read.LogText = "====================================";
            read.LogText =  " Process Started...";
            SetCulture(read);
            read.StartProcess();
            read.LogText = " Process Completed...";
            read.LogText = "====================================";
            Environment.Exit(0);
        }
        public static void SetCulture(READFILE _Routine)
        {
            System.Globalization.CultureInfo _defaultCulture = System.Threading.Thread.CurrentThread.CurrentCulture;
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            _Routine.LogText = "Default regional setting - " + _defaultCulture.DisplayName;
            _Routine.LogText = "Current regional setting - " + System.Threading.Thread.CurrentThread.CurrentCulture.DisplayName;
        }
    }
}
