using Logger;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestCreateFile()
        {
            ErrorLog l = new ErrorLog();
            bool isloggered = false;
            try
            {
                int j = 0;
                int i = 6 / j;
            }
            catch (System.Exception ex)
            {
                isloggered = l.Error(false, ex);
            }

            bool fileCreated = File.Exists("C:\\MyLogs2\\ErrorLogFile.log");

            Assert.AreEqual(true, fileCreated);
        }

        [TestMethod]
        public void TestContentAvailability()
        {
            ErrorLog l = new ErrorLog();
            bool isloggered = false;
            try
            {
                int j = 0;
                int i = 6 / j;
            }
            catch (System.Exception ex)
            {
                isloggered = l.Error(false, ex);
            }
            
            StreamReader sr = new StreamReader("C:\\MyLogs2\\ErrorLogFile.log");
            bool isContent = string.IsNullOrEmpty(sr.ReadToEnd());
            Assert.AreEqual(false, isContent);
        }
    }
}
