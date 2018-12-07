using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace TW_Desktop
{
    static class Program
    {
        public static bool debugMode = false;

        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length != 0 && args[0] == "-debug")
                debugMode = true;
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
