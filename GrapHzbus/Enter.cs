using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Text;
using MySql.Data.MySqlClient;
using System.Configuration;
using System.Threading;


namespace GrapNews
{
    class Enter
    {
        const int MAX = 7;

        static LogInfo log = new LogInfo();
        //主程序入口
        static void Main(string[] args)
        {
            log.writeIn("主程序启动!");
            Enter sv = new Enter();
            Thread th = new Thread(new ThreadStart(sv.Service));
            if (th.ThreadState != ThreadState.Running)
            {
                log.writeIn("打开服务中...");
                try
                {
                    th.Start();
                    log.writeIn("服务已打开.");
                }
                catch (System.OutOfMemoryException)
                {
                    log.writeIn("内存不足,服务启动失败!");
                }
            }
            Console.WriteLine("按回车键停止服务!");
            Console.Read();
            th.Abort();
            log.writeIn("服务已关闭.");
            return;
        }
        //服务入口
        public void Service()
        {
            int company = 1;
            while (true)
            {
                log.writeIn("company = " + (company % 1).ToString());
                Console.WriteLine((company % 1).ToString());
                Inherite c;
                #region companySelected
                switch (company % 1)
                {
                    case 0:
                        c = new Hzbus(MAX);
                        break;
                    default:
                        c = new Hzbus(MAX);//?
                        break;
                }
                #endregion
                Loading(c);
                company++;
            }
        }
        //加载数据
        static void Loading(Inherite c)
        {
            MySqlConnection conn = new MySqlConnection(ConfigurationManager.AppSettings["connectionstring"]);
            //log.writeIn("连接数据库中...");
            try
            {
                conn.Open();
            }
            catch
            {
                log.writeIn("数据库打开失败!");
                return;
            }

            MySqlCommand setformat = new MySqlCommand("set names gbk", conn);
            setformat.ExecuteNonQuery();
            setformat.Dispose();
            string sql = "select website,ca_id from hdcnews.website where co_id = " + c.getCo_id();
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            MySqlDataAdapter msda = new MySqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            msda.Fill(ds, "table");
            foreach (DataRow row in ds.Tables["table"].Rows)
            {
                c.SetWebsite(row["website"].ToString());
                string flag = "";
                flag = c.ReadData(conn);
                if (flag == "1") { }
                else
                {
                    log.writeIn("读取" + flag + "失败");
                    Console.WriteLine("读取" + flag + "失败");
                    //cmd = new MySqlCommand();
                    //cmd.CommandText = "INSERT into hdcnews.exception (co_id,ca_id,website) VALUEs (" +c.getCo_id()+ "," + row["ca_id"].ToString() + ",'" + flag + "')";
                    //cmd.Connection = conn;
                    //cmd.ExecuteNonQuery();
                }
                Compare(c, row["ca_id"].ToString());
                InsertData(c, row["ca_id"].ToString());
                for (int i=0; i < MAX; i++)
                {
                    c.clear(i);
                }
            }
            conn.Close();
            Thread.Sleep(Convert.ToInt32(ConfigurationManager.AppSettings["graptimeout"]));
        }
        //对比消息是否已存在
        static bool Compare(Inherite c, string ca_id)
        {
            string co_id = c.getCo_id();
            string connstr = ConfigurationManager.AppSettings["connectionstring"];
            MySqlConnection conn = new MySqlConnection(connstr);
            conn.Open();
            MySqlCommand cmd = new MySqlCommand();
            for (int i = 0; i < MAX; i++)
            {
                string title = c.rtnTitle(i);
                if (title == null || title == "") 
                {
                    continue;
                }
                MySqlCommand setformat = new MySqlCommand("set names gbk", conn);
                setformat.ExecuteNonQuery();
                setformat.Dispose();
                cmd.CommandText = "select title,ca_id from hdcnews.message where title='" + title + "'";
                cmd.Connection = conn;
                MySqlDataReader reader = cmd.ExecuteReader();
                reader.Read();
                if (reader.HasRows)
                {
                    Console.WriteLine("重复：《" + title+"》");
                    c.clear(i);
                }
                else
                {
                    
                }
                reader.Dispose();
                cmd.Dispose();
            }
            conn.Close();
            return true;
        }
        //插入数据到数据库
        static void InsertData(Inherite c, string ca_id)
        {
            string[] title = new string[MAX];
            string[] time = new string[MAX];
            string[] text = new string[MAX];
            c.rtnInfo(ref title, ref time, ref text);
            //MySqlConnection conn = c.connopen();
            string connstr = ConfigurationManager.AppSettings["connectionstring"];
            MySqlConnection conn = new MySqlConnection(connstr);
            conn.Open();
            MySqlCommand cmd = new MySqlCommand();
            for (int i = MAX - 1; i >= 0; i--)
            {
                //c.setformat();new MySqlConnection(ConfigurationManager.AppSettings["connectionstring"]
                if (text[i] == null || text[i] == "") 
                { continue; }
                MySqlCommand setformat = new MySqlCommand("set names gbk", conn);
                setformat.ExecuteNonQuery();setformat.Dispose();
                cmd.CommandText = "insert into hdcnews.message (title,date,text,inserttime,co_id,ca_id) select '" + title[i] + "','" + time[i] + "','" + text[i] + "','" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'," + c.getCo_id() + "," + ca_id + " from dual where not exists (select * from hdcnews.message where title = '" + title[i] + "' and ca_id = " + ca_id + ")";//后半句为重复则不插入
                cmd.Connection = conn;
                try
                {
                    if (title[i] == ""||title[i]==null)
                    {
                        break;
                    }
                    cmd.ExecuteNonQuery();
                    log.writeIn("已插入 " + title[i]);
                    Console.WriteLine("已插入 " + title[i]);
                }
                catch (Exception er)
                {
                    log.writeIn("插入失败" + er.Message);//
                }
                Thread.Sleep(Convert.ToInt32(ConfigurationManager.AppSettings["inserttimeout"]));//等待时间
            }
            conn.Close();
            cmd.Dispose();
        }
    }
}
