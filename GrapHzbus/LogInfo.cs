using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace GrapNews
{
    class LogInfo
    {
        string filePath;
        string fileName;
        public LogInfo()
        {
            filePath = Environment.CurrentDirectory + "\\log\\";
            fileName = "record-" + DateTime.Now.ToString("MM-dd") + ".log";
            if (!Directory.Exists(filePath))
                Directory.CreateDirectory(filePath);
            if (!File.Exists(filePath + fileName))
                File.Create(filePath + fileName).Close();
        }

        public bool writeIn(string text)
        {
            try
            {
                FileStream fs = new FileStream(filePath + fileName, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                StreamWriter sw = new StreamWriter(fs);
                string time = DateTime.Now.ToString("[yyyy-MM-dd HH:mm:ss]");
                sw.WriteLine(time + text);
                sw.Close();
                fs.Close();
                return true;
            }
            catch (System.ArgumentNullException)
            {
                Console.WriteLine("请先打开文件.");
                return false;
            }
        }
    }
}
