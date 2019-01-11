using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TW_Desktop
{
    public class Logger
    {
        static FileStream fs;
        static StreamWriter sw;
        public Logger()
        {
            fs = new FileStream(Directory.GetCurrentDirectory() + @"\desktop.log",FileMode.OpenOrCreate,FileAccess.ReadWrite);
            fs.Position = fs.Length;
            sw = new StreamWriter(fs);
        }
        static string GetTime()
        {
            return DateTime.Now.ToLongTimeString();
        }
        public static void Info(string str)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"[{GetTime()}] #INFO# {str}");
            sw.WriteLine($"[{GetTime()}] #INFO# {str}");
            sw.Flush();
        }
        public static void Warn(string str)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"[{GetTime()}] #WARN# {str}");
            sw.WriteLine($"[{GetTime()}] #WARN# {str}");
            sw.Flush();
        }
        public static void Error(string str)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[{GetTime()}] #ERROR# {str}");
            sw.WriteLine($"[{GetTime()}] #ERROR# {str}");
            sw.Flush();
        }
        public void CloseStream()
        {
            fs.Flush();
            fs.Close();
        }
    }
}
