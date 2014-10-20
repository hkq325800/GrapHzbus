using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using MySql.Data.MySqlClient;

namespace GrapNews
{
    class Hzbus : Inherite
    {
        private static string rootHttp = "http://www.hzbus.com.cn/";

        public Hzbus(int amount) : base(amount) 
        {
            co_id = "1000";
        }

        public override string ReadData(MySqlConnection conn)
        {
            string flag = "";
            string sql = "";
            string[] hrefs = new string[no];
            HttpWebResponse myHttpWebResponse;
            HttpWebRequest myHttpWebRequest;
            Stream streamResponse;
            StreamReader streamRead;
            string outputData;
            bool found = false;
            try
            {
                myHttpWebRequest = (HttpWebRequest)WebRequest.Create(website);// Create a new 'HttpWebRequest' object to the mentioned URL.
                myHttpWebRequest.UserAgent = ".NET Framework Test Client";
                myHttpWebRequest.ServicePoint.ConnectionLimit = int.MaxValue;
                myHttpWebRequest.Timeout = 10000;
                //myHttpWebRequest.KeepAlive = true;
                myHttpWebResponse = (HttpWebResponse)myHttpWebRequest.GetResponse();// Assign the response object of 'HttpWebRequest' to a 'HttpWebResponse' variable.
                streamResponse = myHttpWebResponse.GetResponseStream();// Display the contents of the page to the console.
                streamRead = new StreamReader(streamResponse);
                outputData = streamRead.ReadLine();
                found = false;
                int i = 0;
                while (!streamRead.EndOfStream)
                {
                    //String outputData = new String(readBuff, 0, count);
                    int index = outputData.IndexOf("<td width=\"70\"><span class=\"inter4\">");
                    if (index >= 0)
                        found = true;
                    if (found)
                    {
                        time[i] = outputData.Substring(outputData.IndexOf("<span class=\"inter4\">") + 21, 10);
                        outputData = streamRead.ReadLine();
                        index = outputData.IndexOf("<a href=\"");
                        if (index >= 0)
                        {
                            string href = outputData.Substring(index + 9);
                            string temp = href.Substring(href.IndexOf("newzx1\">") + 8);
                            temp = temp.Replace(" </a></td>", "");
                            //title[i] = temp.ToString();
                            href = href.Substring(0, href.IndexOf('\"'));

                            MySqlCommand cmd = new MySqlCommand();
                            cmd.CommandText = "select * from hdcnews.exception where website='" + rootHttp + href + "' ";
                            cmd.Connection = conn;
                            MySqlDataReader reader = cmd.ExecuteReader();
                            reader.Read();
                            if (reader.HasRows)
                            {
                                hrefs[i] = "";
                            }
                            else
                            {
                                hrefs[i] = rootHttp + href;
                            }
                            cmd.Dispose();
                            reader.Dispose();

                            i++;
                            if (i > no - 1) break;
                            found = false;
                        }
                    }
                    outputData = streamRead.ReadLine();
                }
                // Release the response object resources.
                streamRead.Close();
                streamResponse.Close();
                myHttpWebResponse.Close();//读取基本信息完毕 开始读取文本
                myHttpWebRequest.Abort();
            }
            catch (Exception e)
            {
                return flag="category页";
            }
            try
            {
                for (int j = 0; j < no; j++)
                {
                    flag = hrefs[j];
                    if (flag == "")
                    { continue; }
                    myHttpWebRequest = (HttpWebRequest)WebRequest.Create(hrefs[j]);
                    myHttpWebRequest.UserAgent = ".NET Framework Test Client";
                    myHttpWebRequest.ServicePoint.ConnectionLimit = int.MaxValue;
                    myHttpWebRequest.Timeout = 10000;
                    //myHttpWebRequest.KeepAlive = true;
                    myHttpWebResponse = (HttpWebResponse)myHttpWebRequest.GetResponse();
                    streamResponse = myHttpWebResponse.GetResponseStream();
                    streamRead = new StreamReader(streamResponse);
                    outputData = streamRead.ReadLine();
                    StringBuilder sb = new StringBuilder();
                    found = false;
                    while (!streamRead.EndOfStream)
                    {
                        if (outputData.IndexOf("<span id=\"MsgTitle\">") >= 0)
                        {
                            title[j] = outputData.Substring(outputData.IndexOf("<span id=\"MsgTitle\">"), outputData.IndexOf("</span>") - outputData.IndexOf("<span id=\"MsgTitle\">"));
                            title[j] = title[j].Remove(title[j].IndexOf("<"), title[j].IndexOf(">") - title[j].IndexOf("<") + 1);
                        }
                        if (outputData.IndexOf("<FONT face=宋体>") >= 0)
                        {
                            found = true;
                        }
                        if (found)
                        {
                            while (outputData.IndexOf("</FONT>") < 0)
                                {
                                    while (outputData.IndexOf("<") >= 0 && outputData.IndexOf(">") > 0) 
                                    {
                                        outputData = outputData.Remove(outputData.IndexOf("<"), outputData.IndexOf(">") - outputData.IndexOf("<") + 1);
                                    }
                                    sb.Append(outputData.Trim() + "\n");
                                    outputData = streamRead.ReadLine();
                                }
                            if (outputData.IndexOf("</FONT>") >= 0)
                            {
                                while (outputData.IndexOf("<") >= 0 && outputData.IndexOf(">") > 0)
                                {
                                    outputData = outputData.Remove(outputData.IndexOf("<"), outputData.IndexOf(">") - outputData.IndexOf("<") + 1);
                                }
                                sb.Append(outputData.Trim() + "\n");
                            }
                            found = false;
                            sb = sb.Replace("&nbsp;", " ");
                            text[j] = sb.ToString();
                            break;
                        }
                        outputData = streamRead.ReadLine();
                    }
                    streamRead.Close();
                    streamResponse.Close();
                    myHttpWebResponse.Close();
                    myHttpWebRequest.Abort();
                }
                
                flag = "1";
                return flag;
            }
            catch(Exception ex)
            {
                //log.writeIn("发生错误:" + ex.StackTrace.ToString() +"hehe"+ ex.TargetSite.ToString());
                return flag;
            }
        }
    }
}
