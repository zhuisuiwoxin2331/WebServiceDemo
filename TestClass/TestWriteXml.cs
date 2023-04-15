using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace TestClass
{
    public class TestWriteXml
    {
        ServceLog log = new ServceLog();
        /// <summary>
        /// DataTable 转 XML 字符串
        /// </summary>
        /// <param name="m_dt"></param>
        /// <returns></returns>
        public string DataTableToXml(DataTable m_dt)
        {
            try
            {
                string strXml = "";
                strXml = "<dmp>" + "\r\n";
                strXml = strXml + "<head TagName=\"head\" Version=\"3.0.0\" sysnode=\"L0354\" sno=\"WMS\" rno=\"WCS\" messagecode=\"TNEWLOGG\"/>" + "\r\n"; ;
                strXml = strXml + "<body TagName=\"body\">" + "\r\n";

                for (int i = 0; i < m_dt.Rows.Count; i++)
                {
                    strXml = strXml + "<data>" + "\r\n"; ;
                    strXml = strXml + "<!--任务'" + i + "'信息-->" + "\r\n";
                    DataRow dataRow = m_dt.Rows[i];

                    strXml = strXml + "<job_no>" + dataRow["job_no"].ToString() + "</job_no>" + "\r\n";
                    strXml = strXml + "<job_type>" + dataRow["job_type"].ToString() + "</job_type>" + "\r\n";                   
                    strXml = strXml + "<palette_no>" + dataRow["palette_no"].ToString() + "</palette_no>" + "\r\n";
                    strXml = strXml + "<pal_type>" + dataRow["pal_type"].ToString() + "</pal_type>" + "\r\n";
                    strXml = strXml + "<from_ware>" + dataRow["from_ware"].ToString() + "</from_ware>" + "\r\n";
                    strXml = strXml + "<from_address>" + dataRow["from_address"].ToString() + "</from_address>" + "\r\n";
                    strXml = strXml + "<to_ware>" + dataRow["to_ware"].ToString() + "</to_ware>" + "\r\n";
                    strXml = strXml + "<PRIORITY>" + dataRow["PRIORITY"].ToString() + "</PRIORITY>" + "\r\n";
                    strXml = strXml + "<JOBSEQ>" + dataRow["JOBSEQ"].ToString() + "</JOBSEQ>" + "\r\n";
                   
                    strXml = strXml + "</data>" + "\r\n";
                }
                strXml = strXml + "</body>" + "\r\n";
                strXml = strXml + "</dmp>";

                return strXml;
            }
            catch (Exception ex)
            {
                log.WriteInLog("DataTable 转 XML 字符串失败，请检查！" + ex.ToString());
                return "N" +"DataTable 转 XML 字符串失败，请检查！"+ ex.ToString();

            }
        
        }


        /// <summary>
        ///  DataTable 转 xml字符
        /// </summary>
        /// <param name="xmlDS"></param>
        /// <returns></returns>
        public  string DataSetToXml(DataTable xmlDS)
        {
            MemoryStream stream = null;
            XmlTextWriter writer = null;

            try
            {
                stream = new MemoryStream();
                //从stream装载到XmlTextReader
                //writer = new XmlTextWriter(stream, Encoding.Unicode);//Unicode有点问题，可能是字符集不一致
                writer = new XmlTextWriter(stream, Encoding.Default);

                //用WriteXml方法写入文件.
                xmlDS.WriteXml(writer);
                int count = (int)stream.Length;
                byte[] arr = new byte[count];
                stream.Seek(0, SeekOrigin.Begin);
                stream.Read(arr, 0, count);

                //UnicodeEncoding utf = new UnicodeEncoding();
                //return utf.GetString(arr).Trim();
                return Encoding.Default.GetString(arr).Trim();
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (writer != null) writer.Close();
            }
        }

        /// <summary>
        /// xml 字符转DataSet
        /// </summary>
        /// <param name="xmlString"></param>
        /// <returns></returns>
        public  DataSet XmlToDataSet(string xmlString)
        {
            XmlDocument xmldoc = new XmlDocument();
            xmldoc.LoadXml(xmlString);
            StringReader stream = null;
            XmlTextReader reader = null;
            try
            {
                DataSet xmlDS = new DataSet();
                stream = new StringReader(xmldoc.InnerXml);
                reader = new XmlTextReader(stream);
                xmlDS.ReadXml(reader);
                reader.Close();
                return xmlDS;
            }
            catch (System.Exception ex)
            {
                reader.Close();
                throw ex;
            }
        }


        /// <summary>
        /// DataTable 转化XML 文件
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public int DataWr(DataTable dt, string strPathfile)
        {

            try
            {
                //如果文件DataTable.xml存在则直接删除
                if (File.Exists(strPathfile))
                {
                    File.Delete(strPathfile);
                }
                XmlTextWriter writer =
                 new XmlTextWriter(strPathfile, Encoding.GetEncoding("utf-8"));
                writer.Formatting = Formatting.Indented;
                //XML文档创建开始
                writer.WriteStartDocument();
                writer.WriteComment("DataTable: " + dt.TableName);//注释
                writer.WriteStartElement("DataTable"); //DataTable开始
                writer.WriteAttributeString("TableName", dt.TableName);
                writer.WriteAttributeString("CountOfRows", dt.Rows.Count.ToString());
                writer.WriteAttributeString("CountOfColumns", dt.Columns.Count.ToString());
                writer.WriteStartElement("ClomunName", ""); //ColumnName开始
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    writer.WriteAttributeString(
                     "Column" + i.ToString(), dt.Columns[i].ColumnName);
                }
                writer.WriteEndElement(); //ColumnName结束
                //按行各行
                for (int j = 0; j < dt.Rows.Count; j++)
                {
                    writer.WriteStartElement("Row" + j.ToString(), "");
                    //打印各列
                    for (int k = 0; k < dt.Columns.Count; k++)
                    {
                        writer.WriteAttributeString(
                         dt.Columns[k].ColumnName, dt.Rows[j][k].ToString());
                    }
                    writer.WriteEndElement();
                }
                writer.WriteEndElement(); //DataTable结束
                writer.WriteEndDocument();
                writer.Close();
                //XML文档创建结束
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return -1;
            }
            return 0;
        }
        /// <summary>
        /// 读取xml 文件 到dataset
        /// </summary>
        /// <param name="path"></param>
        /// <param name="filename"></param>
        /// <returns></returns>
        public DataSet LoadXml(string path, string filename)
        {
            XmlDataDocument d = new XmlDataDocument();
            d.DataSet.ReadXml(path + @"/"+filename+".xml");
            return d.DataSet;
            
        
        }
        
    }
}
