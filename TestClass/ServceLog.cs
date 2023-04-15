using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.IO;
using System.Web;
using System.Threading.Tasks;

namespace TestClass
{
    public class ServceLog
    {

        private  static  string logFileName;
        private string logPath = ConfigurationSettings.AppSettings["LogPath"].ToString();

        /// <summary>
        /// 写DB日志 
        /// </summary>
        /// <param name="msg"></param>
        public void WriteDbLog(string msg)
        {
            try
            { 

            }
            catch (Exception ex)
            {
                ex.ToString();
            }
        }

        /// <summary>
        /// 写日志文件
        /// </summary>
        /// <param name="msg"></param>
        public void WriteInLog(string msg)
        {
            try
            {
                //string logPath = AppDomain.CurrentDomain.BaseDirectory;//程序目录下
                //创建文件夹
                if (!System.IO.Directory.Exists(logPath))
                {
                    System.IO.Directory.CreateDirectory(logPath);
                }
                //创建文件名
                logFileName = DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString("00") + DateTime.Now.Day.ToString("00")  +".log";
                logFileName = logPath + logFileName;
                FileInfo fileinfo = new FileInfo(logFileName);
                //写日志
                using (FileStream fs = fileinfo.OpenWrite())
                {
                    StreamWriter sw = new StreamWriter(fs);
                    sw.BaseStream.Seek(0, SeekOrigin.End);

                    sw.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff") + "  ==> " + msg);

                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
        }
    }
        
}
