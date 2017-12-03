using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logger
{
    /// <summary>
    /// Log Level
    /// </summary>
    [Flags]
    public enum LogLevel
    {
        ALL,
        DEBUG,
        INFO,
        WARNING,
        ERROR
    }
}
