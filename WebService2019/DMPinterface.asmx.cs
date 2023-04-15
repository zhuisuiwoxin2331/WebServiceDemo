using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OracleClient;
using System.Linq;
using System.Web;
using System.Web.Services;
using DBClass;
using TestClass;

namespace WebService2019
{
    /// <summary>
    /// DMPinterface 的摘要说明
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // 若要允许使用 ASP.NET AJAX 从脚本中调用此 Web 服务，请取消注释以下行。 
    // [System.Web.Script.Services.ScriptService]

    public class WebService2019 : System.Web.Services.WebService
    {
        DBCommon Db = new DBCommon();//数据库对象
        ServceLog log = new ServceLog();//日志对象
        
        DataSet m_d; string xSQL = "";
        TestWriteXml Wr = new TestWriteXml();
        /// <summary>
        /// 测试
        /// </summary>
        /// <returns></returns>
        [WebMethod]
        public string HelloWorld()
        {
            return "Hello World";
        }
        /// <summary>
        /// 测试
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        [WebMethod(Description= "判断 输出")]
        public string Print( string str )
        {
            if (str == "a")
            {
                return "Hello F";
            }
            else
            {
                return "Hello BeiJing";
            }           
        }
        /// <summary>
        /// 写入XML数据
        /// </summary>
        /// <param name="xmlData"></param>
        /// <returns></returns>
        [WebMethod(Description = "写入XML数据")]
        public int DMPProtocol(string xmlData)
        {
            log.WriteInLog("WCS回传的XML文件内容："+xmlData);//记录下传时 原来的xml 数据

            m_d = Wr.XmlToDataSet(xmlData);// xml 转 DataSet
            Db.BeginTrans();//开启事务 执行DataSet 插入数据库当中
            try
            {
                foreach (DataRow mDr in m_d.Tables["data"].Rows)
                {

                    xSQL = "";
                    xSQL = xSQL + " insert into sw_jobrequestlist(job_no,request_type,status,palette_no,";
                    xSQL = xSQL + " from_ware,from_address,to_ware,to_address,jobin,jobout,device_no,rec_time) ";
                    xSQL = xSQL + " values ('" + mDr["job_no"].ToString() + "','" + mDr["request_type"].ToString() + "',0,";
                    xSQL = xSQL + " '" + mDr["palette_no"].ToString() + "','" + mDr["from_ware"].ToString() + "',";
                    xSQL = xSQL + " '" + mDr["from_address"].ToString() + "','" + mDr["to_ware"].ToString() + "',";
                    xSQL = xSQL + " '" + mDr["to_address"].ToString() + "','" + mDr["jobin"].ToString() + "',";
                    xSQL = xSQL + " '" + mDr["jobout"].ToString() + "','" + mDr["device_no"].ToString() + "',";
                    xSQL = xSQL + " sysdate)";
                    Db.dbExecute(xSQL);

                    //执行存储过程
                    OracleParameter op1 = new OracleParameter("AS_LOCNO", OracleType.VarChar);
                    OracleParameter op2 = new OracleParameter("AS_JOBNO", OracleType.VarChar);
                    op1.Value = "001";
                    op2.Value = mDr["job_no"].ToString();
                    OracleParameter op_msg = new OracleParameter("AS_MSG", OracleType.VarChar, 300);
                    op_msg.Direction = ParameterDirection.Output;

                    if (Db.ExecProc("P_ASRS_TASKREQUEST", new OracleParameter[] { 
                               op1,op_msg }) <= 0)
                    {
                        Db.RollbackTrans();
                        Db.ConnClose();
                        log.WriteInLog("P_ASRS_TASKREQUEST执行存储过程失败,请检查！");
                        return -1;

                    }
                    if (op_msg.Value.ToString().Substring(0, 1) == "N")
                    {
                        Db.RollbackTrans();
                        Db.ConnClose();
                        log.WriteInLog("P_ASRS_TASKREQUEST执行存储过程失败,请检查！" + op_msg.Value.ToString());
                        return -1;
                    }  

                }
                Db.CommitTrans();//提交事务
                log.WriteInLog("插入sw_jobrequestlist表数据成功！");
                //执行P_ASRS_NewTaskGet 索取新任务
                ExeProDataNewTask();
                return 0;
            }
            catch (Exception ex)
            {
                Db.RollbackTrans();//回滚事务
                log.WriteInLog("插入sw_jobrequestlist表数据失败！" + ex.ToString());
                return -1;
            }
            
        }
        /// <summary>
        /// 测试数据库处理
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        [WebMethod(Description = "TEST数据库")]
        public int DbTest(string str)
        {
            string xSQL = "";int i;
            xSQL = "update defart a set  a.brifname=to_char(sysdate,'yyyy/mm/dd hh24:mi:ss')  where a.artno='703196' ";
            i=Db.Execute(xSQL);
            return i;
        }

        /// <summary>
        /// 执行P_ASRS_NewTaskGet 索取新任务
        /// </summary>
        /// <returns></returns>
        public string ExeProDataNewTask()
        {
            try
            {
                OracleParameter op1 = new OracleParameter("AS_LOCNO", OracleType.VarChar);
                op1.Value = "001";
                OracleParameter op_msg = new OracleParameter("AS_MSG", OracleType.VarChar, 300);
                op_msg.Direction = ParameterDirection.Output;

                if (Db.ExecProc("P_ASRS_NEWTASKGET", new OracleParameter[] { 
                               op1,op_msg }) <= 0)
                {
                    Db.RollbackTrans();
                    Db.ConnClose();
                    log.WriteInLog("P_ASRS_NEWTASKGET执行存储过程失败,请检查！");
                    Console.WriteLine("P_ASRS_NEWTASKGET执行存储过程失败,请检查！");
                    return "N";

                }
                if (op_msg.Value.ToString().Substring(0, 1) == "N")
                {
                    Db.RollbackTrans();
                    Db.ConnClose();
                    log.WriteInLog("P_ASRS_NEWTASKGET执行存储过程失败,请检查！" + op_msg.Value.ToString());
                    Console.WriteLine("P_ASRS_NEWTASKGET执行存储过程失败,请检查！" + op_msg.Value.ToString());
                    return op_msg.Value.ToString();
                }
                return op_msg.Value.ToString();

            }
            catch (Exception ex)
            {
                Db.RollbackTrans();
                Db.ConnClose();
                log.WriteInLog("P_ASRS_NEWTASKGET执行存储过程失败,请检查！" + ex.ToString());
                Console.WriteLine("P_ASRS_NEWTASKGET执行存储过程失败,请检查！" + ex.ToString());
                return "N";
            }


        }
    }
}
