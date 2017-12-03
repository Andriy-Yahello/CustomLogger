using System;
using System.Diagnostics;
using System.Net;
using System.IO;

using System.Configuration;
using Newtonsoft.Json;
using System.Text;
using System.Xml;

namespace Logger
{
    /// <summary>
    /// Logger is used for creating a customized error log files or an error can be registered as
    /// a log entry in the Windows Event Log on the administrator's machine.
    /// </summary>
    public class ErrorLog : ILog
    {
        protected static string strLogFilePath = ConfigurationManager.AppSettings["filePath"];
        private static StreamWriter sw = null;

        /// <summary>
        /// Setting LogFile path.
        /// application directory.
        /// </summary>
        public static string LogFilePath
        {
            set
            {
                strLogFilePath = value;
            }
            get
            {
                return strLogFilePath;
            }
        }

        /// <summary>
        /// Empty Constructor
        /// </summary>
        public ErrorLog() {
            CheckDirectory(strLogFilePath);
        }

        /// <summary>
        /// Write error log entry for window event if the bLogType is true. Otherwise, write the log entry to
        /// customized text-based text file
        /// </summary>
        /// <param name="bLogType"></param>
        /// <param name="objException"></param>
        /// <returns>false if the problem persists</returns>
        public static bool ErrorRoutine(bool bLogType, Exception objException)
        {
            try
            {
                //Write to Windows event log
                if (bLogType)
                {
                    string EventLogName = "ErrorSample";

                    if (!EventLog.SourceExists(EventLogName))
                        EventLog.CreateEventSource(objException.Message, EventLogName);

                    // Inserting into event log
                    EventLog Log = new EventLog();
                    Log.Source = EventLogName;
                    Log.WriteEntry(objException.Message, EventLogEntryType.Error);
                }
                //Custom text-based event log
                else
                {
                    if (true != CustomErrorRoutine(objException))
                        return false;
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }


        #region Fields
        /// <summary>
        /// Log level
        /// </summary>
        /// <remarks>
        /// Log all entries with <see cref="LogLevel"/> set here and above. 
        /// </remarks>
        private static LogLevel _loglevel = (LogLevel)Enum.Parse(typeof(LogLevel), ConfigurationManager.AppSettings["loglevel"]);
        #endregion

        #region Properties
        public static LogLevel MyLogLevel
        {
            get
            {
                return _loglevel;
            }
            set
            {
                _loglevel = value;
            }
        }
        #endregion

        #region public Methods
        /// <summary>
        /// Write info string to log
        /// </summary>
        /// <param name="logString">The string to write to the log</param>
        public Exception Info(string logString)
        {
            return Log(logString);
        }

        /// <summary>
        /// Write warning message to log
        /// </summary>
        /// <param name="logString">The message to write to the log</param>
        /// <returns>Null on success or the <see cref="Exception"/> that occurred when processing the message.</returns>
        public Exception Warning(string logString)
        {
            return Log(logString, LogLevel.WARNING);
        }

        public bool Error(bool logString, Exception ee)
        {
            return ErrorRoutine(false, ee);
        }

        public string Debug(string logString, string type)
        {
            return Log(logString, LogLevel.DEBUG, type);
        }

        public string Debug(string logString, Type type)
        {
            return Log(logString, LogLevel.DEBUG, type);
        }

        public string Log(string logString, LogLevel logLevel, Type type)
        {
            Log(new System.Xml.Linq.XElement("LogString", string.Concat(logString, " in ", type.FullName)), logLevel);
            return null;
        }
        public string Log(string logString, LogLevel logLevel, string type)
        {
            Log(new System.Xml.Linq.XElement("LogString", string.Concat(logString, " in ", type)), logLevel);
            return null;
        }
        public Exception Log(string logString, LogLevel logLevel = LogLevel.INFO)
        {
            return string.IsNullOrEmpty(logString) ? null : Log(new System.Xml.Linq.XElement("LogString", logString), logLevel);
        }

        public static Exception Log(System.Xml.Linq.XElement xElement, LogLevel logLevel)
        {
            // Filter entries below log level
            if (xElement == null || logLevel < MyLogLevel)
                return null;

            try
            {
                var logEntry = new System.Xml.Linq.XElement("LogEntry");
                logEntry.Add(new System.Xml.Linq.XAttribute("Date", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
                logEntry.Add(new System.Xml.Linq.XAttribute("LogLevel", logLevel));
                logEntry.Add(xElement);

                return WriteLogEntryToFile(logEntry);
            }
            catch (Exception ex)
            {

                return ex;
            }
        }


        #endregion

        #region private Methods

        /// <summary>
        /// If the LogFile path is empty then, it will write the log entry to LogFile.txt under application directory.
        /// If the LogFile.txt is not availble it will create it
        /// If the Log File path is not empty but the file is not availble it will create it.
        /// </summary>
        /// <param name="objException"></param>
        /// <returns>false if the problem persists</returns>
        private static bool CustomErrorRoutine(Exception objException)
        {
            string strPathName = string.Empty;
            if (strLogFilePath.Equals(string.Empty))
            {
                //Get Default log file path "LogFile.txt"
                strPathName = GetLogFilePath();
            }
            else
            {
                //If the log file path is not empty but the file is not available it will create it
                if (false == File.Exists(strLogFilePath))
                {
                    if (false == CheckDirectory(strLogFilePath))
                        return false;

                    FileStream fs = new FileStream(strLogFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                    fs.Close();
                }
                strPathName = strLogFilePath;

            }

            bool bReturn = true;

            //read format type from App.config file
            string format = ConfigurationManager.AppSettings["format"];


            // write the error log to that text file
            if (true != WriteErrorLog(strPathName, objException, format))
            {
                bReturn = false;
            }
            return bReturn;
        }

        /// <summary>
        /// Write Source,method,date,time,computer,error and stack trace information to the text file
        /// </summary>
        /// <param name="strPathName"></param>
        /// <param name="objException"></param>
        /// <returns>false if the problem persists</returns>
        private static bool WriteErrorLog(string strPathName, Exception objException, string format)
        {
            bool bReturn = false;
            string strException = string.Empty;

            switch (format)
            {
                case "plain":
                    try
                    {
                        var sw1 = new FileWrapper(strPathName);
                        sw1.WriteFile("Source		: " + objException.Source.ToString().Trim());
                        sw1.WriteFile("Method		: " + objException.TargetSite.Name.ToString());
                        sw1.WriteFile("Date		: " + DateTime.Now.ToLongTimeString());
                        sw1.WriteFile("Time		: " + DateTime.Now.ToShortDateString());
                        sw1.WriteFile("Computer	: " + Dns.GetHostName().ToString());
                        sw1.WriteFile("Error		: " + objException.Message.ToString().Trim());
                        sw1.WriteFile("Stack Trace	: " + objException.StackTrace.ToString().Trim());
                        sw1.WriteFile("^^-------------------------------------------------------------------^^");
                        sw1.Dispose();
                        bReturn = true;
                    }
                    catch (Exception)
                    {
                        bReturn = false;
                    }
                    break;
                case "xml":
                    try
                    {
                        System.Xml.Linq.XElement xElement = new System.Xml.Linq.XElement("Error", objException.Message.ToString().Trim());
                        var logEntry = new System.Xml.Linq.XElement("LogEntry");
                        logEntry.Add(new System.Xml.Linq.XElement("Date", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
                        logEntry.Add(new System.Xml.Linq.XElement("Source", objException.Source.ToString().Trim()));
                        logEntry.Add(new System.Xml.Linq.XElement("Method", objException.TargetSite.Name.ToString()));
                        logEntry.Add(new System.Xml.Linq.XElement("Time", DateTime.Now.ToShortDateString()));
                        logEntry.Add(new System.Xml.Linq.XElement("Computer", Dns.GetHostName().ToString()));
                        logEntry.Add(new System.Xml.Linq.XElement("StackTrace", objException.StackTrace.ToString().Trim()));
                        logEntry.Add(xElement);
                        WriteLogEntryToFile(logEntry);
                    }
                    catch (Exception e)
                    {
                        var a = e.Message;
                        bReturn = false;
                    }
                    break;

                case "json":
                    try
                    {
                        ErrorLogForJSON mylog = new ErrorLogForJSON()
                        {
                            Source = objException.Source.ToString().Trim(),
                            Method = objException.TargetSite.Name.ToString(),
                            Date = DateTime.Now.ToLongTimeString(),
                            Time = DateTime.Now.ToShortDateString(),
                            Computer = objException.Message.ToString().Trim(),
                            Error = objException.Message.ToString().Trim(),
                            StackTrace = objException.StackTrace.ToString().Trim()
                        };

                        using (var fs = new FileStream(strLogFilePath, FileMode.Append, FileAccess.Write, FileShare.None))
                        {
                            using (sw = new StreamWriter(fs))
                            {
                                JsonSerializer serializer = new JsonSerializer();
                                serializer.Serialize(sw, mylog);
                                sw.Close();
                            }
                        }
                        bReturn = true;
                    }
                    catch (Exception)
                    {
                        bReturn = false;
                    }
                    break;
            }

            return bReturn;
        }

        /// <summary>
        /// Check the log file in applcation directory. If it is not available, creae it
        /// </summary>
        /// <returns>Log file path</returns>
        private static string GetLogFilePath()
        {
            try
            {
                // get the base directory
                string baseDir = AppDomain.CurrentDomain.BaseDirectory + AppDomain.CurrentDomain.RelativeSearchPath;

                // search the file below the current directory
                string retFilePath = baseDir + "//" + "LogFile.txt";

                // if exists, return the path
                if (File.Exists(retFilePath) == true)
                    return retFilePath;
                //create a text file
                else
                {
                    if (false == CheckDirectory(retFilePath))
                        return string.Empty;

                    FileStream fs = new FileStream(retFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                    fs.Close();
                }

                return retFilePath;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Create a directory if not exists
        /// </summary>
        /// <param name="strLogPath"></param>
        /// <returns></returns>
        private static bool CheckDirectory(string strLogPath)
        {
            try
            {
                int nFindSlashPos = strLogPath.Trim().LastIndexOf("\\");
                string strDirectoryname = strLogPath.Trim().Substring(0, nFindSlashPos);

                if (!Directory.Exists(strDirectoryname))
                    Directory.CreateDirectory(strDirectoryname);

                return true;
            }
            catch (Exception)
            {
                return false;

            }
        }

        private void WriteLine(string text, bool append = true)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(strLogFilePath, append, Encoding.UTF8))
                    if (text != "") writer.WriteLine(text);
            }
            catch (Exception)
            {

                throw;
            }
        }

        private static Exception WriteLogEntryToFile(System.Xml.Linq.XElement xmlEntry)
        {
            var format = ConfigurationManager.AppSettings["format"];
            if (xmlEntry == null)
                return null;

            //If the log file path is not empty but the file is not available it will create it
            if (false == File.Exists(strLogFilePath))
            {
                if (false == CheckDirectory(strLogFilePath))
                    return null;

                FileStream fs = new FileStream(strLogFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                fs.Close();
            }
            try
            {
                using (var fs = new FileStream(strLogFilePath, FileMode.Append, FileAccess.Write, FileShare.None))
                {
                    using (sw = new StreamWriter(fs))
                    {
                        switch (format)
                        {
                            case "xml":
                                sw.WriteLine(xmlEntry);
                                break;
                            case "json":
                                string json = JsonConvert.SerializeObject(xmlEntry);
                                sw.WriteLine(json);
                                break;
                            case "plain":
                                StringBuilder sb = new StringBuilder();
                                sb.Append(xmlEntry.Name);
                                sb.Append(' ');
                                sb.AppendLine(xmlEntry.Value);
                                foreach (System.Xml.Linq.XAttribute item in xmlEntry.Attributes())
                                {
                                    sb.Append(item.Name);
                                    sb.Append(' ');
                                    sb.AppendLine(item.Value);
                                }
                                sw.WriteLine(sb);
                                break;
                        }


                    }
                }
                return null;
            }
            catch (Exception ex)
            {

                return ex;
            }
        }
        #endregion
    }


}
