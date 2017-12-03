using System;
using System.Collections.Generic;
using System.Text;

namespace Logger
{
    class ErrorLogForJSON
    {
        public string Source { get; set; }

        public string Method { get; set; }

        public string Date { get; set; }

        public string Time { get; set; }

        public string Computer { get; set; }

        public string Error { get; set; }

        public string StackTrace { get; set; }
    }
}
