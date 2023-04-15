using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DBClass;
using TestClass;

namespace MainTest
{
    class Program
    {
        /// <summary>
        /// 日志对象
        /// </summary>
        public static ServceLog log = new ServceLog();
        /// <summary>
        /// 
        /// </summary>
        public static SelectData Sd = new SelectData();
        //public static DBCommon Db = new DBCommon();
        //public static TestWriteXml Wr = new TestWriteXml();
        //string xSQL = "", strSQL = ""; int iRet = -1;
        //DataTable m_dt; static string strXml = "";
        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            ServiceReferenceDMP.WebService2019SoapClient s = new ServiceReferenceDMP.WebService2019SoapClient();
            
            bool flag = false;//标记，只有值为true时，程序才继续运行
            
            try
            {
                new Mutex(false, System.Diagnostics.Process.GetCurrentProcess().ProcessName, out flag);
                /*这里的标记名称，如果以Global\\开头，则在当前机器的所有登录用户会话中都保持唯一，
                 * 如果以Local\\开头，则只在当前用户会话中保持唯一。
                 * 不加则默认以Local\\开头
                 */
                new Mutex(false, "Global\\MainTest.exe", out flag);

                if (!flag)//如果flag为false，表示已经有一个程序正在运行
                {
                    log.WriteInLog("程序已经在运行了，请不要重复运行！");
                    Console.WriteLine("程序已经在运行了，请不要重复运行！");
                    Thread.Sleep(3000);
                    return;
                }
            }
            catch (Exception ex)
            {
                log.WriteInLog("MainTest() 发生异常，错误信息：" + ex.Message);
            }

            while (flag)//此处为死循环
            {
                try
                {
                    //Console.Clear();//清屏
                    //Console.WriteLine(s.Print("a"));
                    //先执行索取任务存储过程
                    Console.WriteLine(Sd.ExeProData());
                    //查询任务
                    Sd.SelectTaskData();

                    Console.WriteLine("本轮操作已经结束，主线程开始休眠，休眠时间（" + 1 + "）分钟");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Main() 执行时发生异常，错误信息：" + ex.Message);
                  
                }
                Thread.Sleep(1 * 60 * 1000);
            }
            Console.ReadKey();
        }
        
    }
}
