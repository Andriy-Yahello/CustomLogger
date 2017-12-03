using System;
using System.Collections.Generic;
using System.Text;
using static Logger.ErrorLog;

namespace Logger
{
    public interface ILog
    {
        Exception Log(string logString, LogLevel logLevel);
        string Log(string logString, LogLevel logLevel, Type type);
    }

}
