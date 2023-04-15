using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.OracleClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBClass
{
    public class DBCommon
    {
        
        public static string ConnectionString = ConfigurationSettings.AppSettings["ConnectString"].ToString();
        OracleConnection conn = new OracleConnection(ConnectionString);//建立数据库连接

        public OracleTransaction dbTrans;
        /// <summary>
        /// 查询SQL
        /// </summary>
        /// <param name="strsql"></param>
        /// <returns></returns>
        public DataSet Select(string strsql)
        {
            //conn.Open();
            ConnAsOpen();
            OracleDataAdapter DA = new OracleDataAdapter(strsql, conn);
            DataSet DS = new DataSet();
            DA.Fill(DS);
            conn.Close();
            return DS;
        }
        /// <summary>
        /// 事务中查sql
        /// </summary>
        /// <param name="strsql"></param>
        /// <returns></returns>

        public DataSet dbSelect(string strsql)
        {
            //conn.Open();
            OracleCommand cmd = new OracleCommand();
            cmd.Connection = conn;
            cmd.Transaction = dbTrans;
            //OracleDataAdapter DA = new OracleDataAdapter(strsql, conn);
            OracleDataAdapter DA = new OracleDataAdapter();
            cmd.CommandText = strsql;
            DA.SelectCommand = cmd;
            DataSet DS = new DataSet();
            DA.Fill(DS);
            //conn.Close();
            return DS;
        }

        /// <summary>
        /// 执行SQL
        /// </summary>
        /// <param name="strsql"></param>
        /// <returns></returns>
        public int Execute(string strsql)
        {
            try
            {
                //conn.Open();
                ConnAsOpen();
                OracleCommand oc = new OracleCommand(strsql, conn);
                int i = oc.ExecuteNonQuery();
                conn.Close();
                return i;
            }
            catch
            {
                return 0;
            }

        }
        /// 执行指定的存储过程
        /// </summary>
        /// <param name="text">存储过程名称</param>
        /// <param name="paramAll">参数数组</param>
        /// <returns>受影响的行数</returns>
        public int ExecProc(string text, OracleParameter[] paramAll)
        {
            ConnAsOpen();
            OracleCommand cmd = new OracleCommand();
            try
            {
                cmd.Connection = conn;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Transaction = dbTrans;

                cmd.CommandText = text;
                if (paramAll != null && paramAll.Length > 0)
                {
                    foreach (OracleParameter param in paramAll)
                    {
                        cmd.Parameters.Add(param);
                    }
                }
                int i = cmd.ExecuteNonQuery();
                //conn.Close();
                return i;
            }
            catch (Exception ex)
            {
                return -1;
                throw new Exception("WMS.Interface.DBAccess.Common  --> ExecProc(string text, OracleParameter[] paramAll)。当前执行存储过程时失败，SQL语句（" + text + "）。错误信息(" + ex.Message + ")，请检查。");
            }
        }




        /// <summary>
        /// 事务内执行SQL
        /// </summary>
        /// <param name="strsql"></param>
        /// <returns></returns>
        public int dbExecute(string strsql)
        {
            try
            {
                //conn.Open();

                OracleCommand oc = new OracleCommand(strsql, conn);
                oc.Transaction = dbTrans;
                int i = oc.ExecuteNonQuery();
                //conn.Close();
                return i;
            }
            catch
            {
                return 0;
            }

        }
        /// <summary>
        /// 开启数据库事务
        /// </summary>
        public void BeginTrans()
        {
            try
            {
                ConnAsOpen();

                dbTrans = conn.BeginTransaction();

            }
            catch (Exception ex)
            {

            }
        }
        /// <summary>
        /// 提交事务
        /// </summary>
        public void CommitTrans()
        {
            try
            {
                ConnAsOpen();
                dbTrans.Commit();
                //dbInTrans = false;
            }
            catch (Exception ex)
            {
                //throw new Exception("WMS.Interface.DBAccess.Common  --> CommitTrans()。当前提交数据库事务时失败，错误信息(" + ex.Message + ")，请检查。");
            }
        }
        /// <summary>
        /// 回滚事务
        /// </summary>
        public void RollbackTrans()
        {
            try
            {
                ConnAsOpen();
                dbTrans.Rollback();
                //dbInTrans = false;
            }
            catch (Exception ex)
            {
                //throw new Exception("WMS.Interface.DBAccess.Common  --> RollbackTrans()。当前回滚数据库事务时失败，错误信息(" + ex.Message + ")，请检查。");
            }
        }


        /// <summary>
        /// 检查数据库连接是否是打开状态
        /// </summary>
        /// <returns>true--打开  false--其它状态</returns>
        public bool CheckConnIsOpen()
        {
            return conn != null && conn.State == ConnectionState.Open;
        }



        /// <summary>
        /// 判定数据库连接是否是打开的，不是打开状态则执行打开操作，保证数据库打开
        /// </summary>
        public void ConnAsOpen()
        {
            try
            {
                if (!CheckConnIsOpen())
                {
                    ConnOpen();
                }
            }
            catch (Exception ex)
            {
                //throw new Exception("WMS.Interface.DBAccess.Common  --> AsOpen()。当前打开数据库连接时失败，错误信息(" + ex.Message + ")，请检查。");
            }
        }

        /// <summary>
        /// 关闭数据库连接对象
        /// </summary>
        public void ConnClose()
        {
            try
            {
                if (conn.State != ConnectionState.Closed)
                {
                    conn.Close();
                    //conn.Dispose();
                }
                //dbConnIsOpen = false;
            }
            catch (Exception ex)
            {
                //throw new Exception("WMS.Interface.DBAccess.Common  --> ConnClose()。当前关闭数据库连接时失败，错误信息(" + ex.Message + ")，请检查。");
            }
        }

        /// <summary>
        /// 打开数据库连接对象
        /// </summary>
        public void ConnOpen()
        {
            try
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                //dbConnIsOpen = true;
            }
            catch (Exception ex)
            {
                if (conn.State != ConnectionState.Closed)
                {
                    conn.Close();
                }
                //throw new Exception("WMS.Interface.DBAccess.WMS_Common  --> ConnOpen()。当前打开数据库连接时失败，错误信息(" + ex.Message + ")，请检查。");
            }
        }
    }
}
