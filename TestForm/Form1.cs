using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DBClass;
using TestClass;


namespace TestForm
{
    public partial class Form1 : Form
    {
        int tRet = 0; DataTable m_dt; DataSet m_d;
        string strXml = "";
        TestWriteXml Wr = new TestWriteXml();
        ServceLog log = new ServceLog();
        DBCommon Db = new DBCommon();
        string xSQL = "", strSQL = ""; int iRet = -1;
        DateTime currentTime = new DateTime();
        string m_XmlPath = ""; DataRow m_St;
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {

            

            //////取配置文件xml文件位置
            ////m_XmlPath=ConfigurationSettings.AppSettings["XmlPath"].ToString();
            //////取出系统时间
            ////currentTime = DateTime.Now;

            ////m_XmlPath = m_XmlPath + currentTime.Year.ToString() + currentTime.Month.ToString("00") + currentTime.Day.ToString("00")+"/";
            //////创建文件夹
            ////if (!Directory.Exists(m_XmlPath))
            ////{
            ////    Directory.CreateDirectory(m_XmlPath);
            ////}
            //////拼写文件名
            ////m_XmlPath = m_XmlPath + currentTime.Hour.ToString("00") + currentTime.Minute.ToString("00") + currentTime.Second.ToString("00") + ".xml";
          
            string xSQL = "";
            xSQL = " select artno,barcode,name,packbigunit from defart  where artno in ('263411','686510') ";

            m_dt = Db.Select(xSQL).Tables[0];
            
            strXml = Wr.DataTableToXml(m_dt);//datatable  转 xml
            log.WriteInLog(strXml);

            m_d = Wr.XmlToDataSet(strXml);// xml 转 datatable
            Db.BeginTrans();//开启事务 执行DATASET 插入数据库当中
            try
            {
                foreach (DataRow mDr in m_d.Tables[0].Rows)
                {

                    xSQL = "";
                    xSQL = xSQL + " insert into defarttmp2019(ARTNO	,BARCODE	,NAME	,PACKBIGUNIT) values ";
                    xSQL = xSQL + " ('" + mDr["ARTNO"].ToString() + "','" + mDr["BARCODE"].ToString() + "','" + mDr["NAME"].ToString() + "','" + mDr["PACKBIGUNIT"].ToString() + "')";
                    Db.dbExecute(xSQL);

                    
                }
                Db.CommitTrans();//提交事务
                log.WriteInLog("插入defarttmp2019表数据成功！");
            }
            catch (Exception ex)
            {
                Db.RollbackTrans();//回滚事务
                log.WriteInLog("插入defarttmp2019表数据失败！"  + ex.ToString());
            }

            
        }

        private void button2_Click(object sender, EventArgs e)
        {
           //m_d= Wr.LoadXml("D:\\InterFaceFile\\20190328\\" ,"173843");

            string xSQL = "";
            xSQL = " select artno,barcode,name,packbigunit from defart  where artno in ('263411','686510') ";

            m_dt = Db.Select(xSQL).Tables[0];

            string strXml = "";
            strXml = "<dmp>" + "\r\n";
            strXml = strXml + "<head TagName=\"head\" Version=\"3.0.0\" sysnode=\"L0354\" sno=\"WMS\" rno=\"WCS\" messagecode=\"TNEWLOGG\"/>" + "\r\n";;
            strXml = strXml + "<body TagName=\"body\">" + "\r\n";

            for (int i = 0; i < m_dt.Rows.Count;i++ )
            {
                strXml = strXml + "<data>" + "\r\n";;
                strXml = strXml + "<!--任务'" + i + "'信息-->" + "\r\n";
                DataRow dataRow = m_dt.Rows[i];
                strXml = strXml + "<ARTNO>" + dataRow["ARTNO"].ToString() + "</ARTNO>" + "\r\n";
                strXml = strXml + "<NAME>" + dataRow["NAME"].ToString() + "</NAME>" + "\r\n";
                strXml = strXml + "<BARCODE>" + dataRow["BARCODE"].ToString() + "</BARCODE>" + "\r\n";
                strXml = strXml + "<PACKBIGUNIT>" + dataRow["PACKBIGUNIT"].ToString() + "</PACKBIGUNIT>" + "\r\n";
                strXml = strXml + "</data>" + "\r\n";
            }
            strXml = strXml + "</body>" + "\r\n";
            strXml = strXml + "</dmp>" ;

            m_d = Wr.XmlToDataSet(strXml);// xml 转 datatable
            foreach (DataRow mDr in m_d.Tables["data"].Rows)
            {

                xSQL = "";
                xSQL = xSQL + " insert into defarttmp2019(ARTNO	,BARCODE	,NAME	,PACKBIGUNIT) values ";
                xSQL = xSQL + " ('" + mDr["ARTNO"].ToString() + "','" + mDr["BARCODE"].ToString() + "','" + mDr["NAME"].ToString() + "','" + mDr["PACKBIGUNIT"].ToString() + "')";
                Db.Execute(xSQL);


            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Db.BeginTrans();
            if (EfTaskData() > 0)
            {
                tRet = SelectTask(ref m_St);
                if (m_dt.Rows.Count > 0 && tRet==0)
                {
                    strXml = Wr.DataTableToXml(m_dt);
                }
            }
            else
            {
                log.WriteInLog("目前没有任务下发！");
                Db.CommitTrans();
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
    }
}
