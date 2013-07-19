using System.IO;
using System.Text;
using System;
using System.IO.IsolatedStorage;

namespace AppSense
{
    public class LogFormat
    {
        private string sLogFormat;
        private string sErrorTime;
        
     //   IsolatedStorageFile sensorData = IsolatedStorageFile.GetUserStoreForApplication();
        public LogFormat()
        {
            //sLogFormat used to create log files format :
            // dd/mm/yyyy hh:mm:ss AM/PM ==> Log Message
            sLogFormat = DateTime.Now.ToShortDateString().ToString() + " " + DateTime.Now.ToLongTimeString().ToString() + " ==> ";

            //this variable used to create log filename format "
            //for example filename : ErrorLogYYYYMMDD
            string sYear = DateTime.Now.Year.ToString();
            string sMonth = DateTime.Now.Month.ToString();
            string sDay = DateTime.Now.Day.ToString();
            sErrorTime = sYear + sMonth + sDay;
        }

        public void WriteLog(string sErrMsg)
        {
            try
            {
            //    StreamWriter sw = new StreamWriter(new IsolatedStorageFileStream("Logfile.txt", FileMode.Append, FileAccess.Write, sensorData));
                StreamWriter sw = new StreamWriter("C:\\LogFile.txt");
                sw.WriteLine(sErrorTime + "=>" + sErrMsg);
                sw.Flush();
                sw.Close();
            }
            catch
            {
            }
        }
    }

}
        

