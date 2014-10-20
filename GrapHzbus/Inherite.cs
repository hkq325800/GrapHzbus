using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using MySql.Data.MySqlClient;
using System.Configuration;
using System.Threading;

namespace GrapNews
{
    public abstract class Inherite
    {//模板页
        protected string[] title;
        protected string[] time;
        protected string[] text;
        protected string co_id;
        protected int no;
        protected string website;

        public string rtnTitle(int i)//
        {
            return title[i];//?
        }

        public void clear(int i)
        {
            text[i] = "";
            time[i] = "";
            title[i] = "";
        }

        public string getCo_id()//
        {
            return co_id;
        }

        protected Inherite(int amount)//
        {
            no = amount;
            time = new string[amount];
            title = new string[amount];
            text = new string[amount];
        }

        public void SetWebsite(string web)//
        {
            website = web;//2000 108 2001 109
        }

        public abstract string ReadData(MySqlConnection conn);//

        public bool rtnInfo(ref string[] t, ref string[] tm, ref string[] txt)//
        {
            t = title;
            tm = time;
            txt = text;
            return true;
        }

        public void setformat()
        {
            try
            {
                MySqlCommand setformat = new MySqlCommand("set names gbk", new MySqlConnection(ConfigurationManager.AppSettings["connectionstring"]));
                setformat.ExecuteNonQuery();
                setformat.Dispose();
            }
            catch
            {
                return;
            }
        }

        public MySqlConnection connopen()
        {
            try
            {
                string connstr = ConfigurationManager.AppSettings["connectionstring"];
                MySqlConnection conn = new MySqlConnection(connstr);
                conn.Open();
                return conn;
            }
            catch
            {
                return null;
            }
        }
    }
}
