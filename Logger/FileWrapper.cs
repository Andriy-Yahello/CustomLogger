using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logger
{
    class FileWrapper : IDisposable
    {
        private string _path { get; set; }

        public FileWrapper(string path)
        {
            _path = path;
        }

        public string FileRead()
        {
            using (StreamReader sr = new StreamReader(_path))
            {
                string str = sr.ReadToEnd();
                sr.Close();
                return str;
            }
        }

        public void WriteFile(string str)
        {
            using (StreamWriter sw = new StreamWriter(_path, true))
            {
                sw.WriteLine(str);
                sw.Flush();
                sw.Close();
            }
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
