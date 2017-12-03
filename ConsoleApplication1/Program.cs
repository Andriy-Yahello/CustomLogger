using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Logger;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            ErrorLog customLog = new ErrorLog();
            Program pro = new Program();
            try
            {
                customLog.Debug("Initializing int i", pro.GetType());
                int i = 1;
                customLog.Debug("Initializing int i", pro.GetType());
                int j = 0;
                customLog.Debug("Trying to divide i by j", (typeof(Program)).FullName);
                int k = i / j;
            }
            catch (Exception ee)
            {
                bool bReturnLog = false;

                customLog.Info("Information");

                customLog.Warning("This is a warning.");

                customLog.Error(false, ee);
                //Console.WriteLine(ErrorLog.strLogFilePath);

                //ErrorLog.LogFilePath = "C:\\MyLogs\\ErrorLogFile.txt"; 
                //false for writing log entry to customized text file
                //bReturnLog = ErrorLog.ErrorRoutine(false, ee);

                //if (false == bReturnLog)
                //    Console.WriteLine("Unable to write a log");
            }
        }
    }
}
