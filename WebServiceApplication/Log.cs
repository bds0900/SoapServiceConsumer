using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace WebServiceApplication.Pages
{
    public class Logging
    {
        private static readonly object mutex = new object();
        private static Logging logger = null;
        public StreamWriter writer = null;


        public static Logging GetLogger()
        {
            lock (mutex)
            {
                if (logger == null)
                {
                    // create logging instance
                    logger = new Logging();
                }
            }
            return logger;
        }

        public void WriteLog(String clsName, String methodName, String msg)
        {
            WriteLog($"[{clsName}.{methodName}] {msg}");
        }


        public void WriteLog(String msg)
        {
            DateTime dateTime = DateTime.Now;
            String strDate = dateTime.ToString("yyyy-MM-dd HH:mm:ss");

            // need mutex
            OpenLogFile();
            this.writer.WriteLine($"{strDate} {msg}");
            this.writer.Flush();
            CloseLogFile();
        }


        public void OpenLogFile()
        {
            DateTime dateTime = DateTime.Today;
            String strDate = dateTime.ToString("yyyy-MM-dd");
            //String logFileName = Environment.CurrentDirectory + "/DB/"+$"soa.{strDate}.log";
            //String logFileName = $"C:\\Users\\dbeak9336/soa.client.{strDate}.log";
            String logFileName = $"C:\\inetpub\\wwwroot/soa.client.{strDate}.log";

            if (this.writer == null)
            {
                this.writer = new StreamWriter(logFileName, true);
                this.writer.AutoFlush = true;
            }
        }

        public void CloseLogFile()
        {
            if (this.writer != null)
            {
                this.writer.Close();
                this.writer = null;
            }
        }
    }
}
