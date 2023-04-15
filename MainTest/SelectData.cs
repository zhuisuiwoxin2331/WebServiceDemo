using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OracleClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DBClass;
using TestClass;

namespace MainTest
{
    public class SelectData
    {
        //声明需要调用的对象
         DBCommon Db = new DBCommon();
         TestWriteXml Wr = new TestWriteXml();
         ServceLog log = new ServceLog();
         ServiceReferenceDMP.WebService2019SoapClient s = new ServiceReferenceDMP.WebService2019SoapClient();
        //声明变量
         string xSQL = "", strSQL = ""; string strXml = ""; 
         int iRet = -1, nRet = -1;
         DataTable m_dt;  DataRow m_St;
         bool nRflag = false;//标记是否返回成功
        /// <summary>
        /// //查询任务
        /// </summary>
        public void SelectTaskData()
        {
            try
            {
                Db.BeginTrans();
                if (EfTaskData() > 0)//锁下发数据
                {
                    //log20190530 add FXL 新增分类型分别下发 


                    //
                    iRet = SelectTask(ref m_St);//查询下发数据
                    if (m_dt.Rows.Count > 0 && iRet == 0)
                    {
                        //DataTable 转 XML 字符串
                        strXml = Wr.DataTableToXml(m_dt);
                        if (strXml.Substring(0, 1) == "N")
                        {
                            Db.RollbackTrans();
                            log.WriteInLog("DataTable 转 XML 字符串失败！" + strXml);
                            Console.WriteLine("DataTable 转 XML 字符串失败！"+ strXml);
                            return;
                        }
                        //把XML 数据 下发到WCS下
                        nRet=s.DMPProtocol(strXml);
                        if (nRet == 1)//返回值若为1 说明 接口调用异常 需要重新发送
                        {
                            nRflag = true;
                        }
                        while (nRflag == true)//此处为死循环 直到发送成功为止
                        {
                            nRet = s.DMPProtocol(strXml);
                            if (nRet !=1)//返回值若为1 说明 接口调用异常 需要重新发送
                            {
                                nRflag = false;
                            }
                        }
                        //修改状态为发送（立库任务执行表）
                        if (ElTaskData() > 0)
                        {
                            Db.CommitTrans();
                            Console.WriteLine("任务下发成功！");
                            log.WriteInLog("任务下发成功！");
                            log.WriteInLog(strXml);
                        }
                        else 
                        {
                            Db.CommitTrans();
                            Console.WriteLine("任务下发成功！但是修改状态出错，请检查！");
                        }


                    }
                    else
                    {
                        Db.RollbackTrans();
                        log.WriteInLog("目前没有任务下发......");
                        Console.WriteLine("目前没有任务下发.....");
                       
                    }
                }
                else
                {
                    Db.RollbackTrans();
                    log.WriteInLog("目前没有任务下发......");
                    Console.WriteLine("目前没有任务下发.....");
                     
                }
            }
            catch (Exception ex)
            {
                Db.RollbackTrans();
                log.WriteInLog("SelectTask(查询立库任务执行表)失败！" + ex.ToString());
                Console.WriteLine("SelectTask(查询立库任务执行表)失败！" + ex.ToString());
                
            }
        
        }

        /// <summary>
        /// 查询SW_JobActionList(立库任务执行表)
        /// </summary>
        /// <returns></returns>
        public int SelectTask(ref DataRow m_St)
        {
            try
            {
                strSQL = " select job_no,job_type,status,palette_no, pal_type,from_ware,from_address,to_ware,to_address,";
                strSQL = strSQL + " priority,jobseq,export_time,finish_time,error_msg,create_time from sw_jobactionlist where status=-1 ";
                m_dt = Db.dbSelect(strSQL).Tables[0];
                return 0;
            }
            catch (Exception ex)
            {
                log.WriteInLog("SelectTask(查询立库任务执行表)失败！" + ex.ToString());
                Console.WriteLine("SelectTask(查询立库任务执行表)失败！" + ex.ToString());
                return -1;
            }
        }
        /// <summary>
        /// 锁定任务数据（立库任务执行表）
        /// </summary>
        /// <returns></returns>
        public int EfTaskData()
        {
            try
            {
                xSQL = " update SW_JobActionList set  status=-1  where status=0 ";
                iRet = Db.dbExecute(xSQL);
                return iRet;
            }
            catch (Exception ex)
            {
                log.WriteInLog("EfTaskData(锁定任务数据立库任务执行表)失败！" + ex.ToString());
                return -1;
            }

        }
        /// <summary>
        /// 修改状态为发送（立库任务执行表）
        /// </summary>
        /// <returns></returns>
        public int ElTaskData()
        {
            try
            {
                xSQL = " update SW_JobActionList set  status=1  where status=-1 ";
                iRet = Db.dbExecute(xSQL);
                return iRet;
            }
            catch (Exception ex)
            {
                log.WriteInLog("ElTaskData(修改状态为发送立库任务执行表)失败！" + ex.ToString());
                return -1;
            }

        }
        /// <summary>
        /// 执行P_ASRS_NewTaskGet 索取新任务
        /// </summary>
        /// <returns></returns>
        public string ExeProData()
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
                log.WriteInLog("P_ASRS_NEWTASKGET执行存储过程失败,请检查！"+ex.ToString());
                Console.WriteLine("P_ASRS_NEWTASKGET执行存储过程失败,请检查！" + ex.ToString());
                return "N";
            }

            
        }
    }
}
