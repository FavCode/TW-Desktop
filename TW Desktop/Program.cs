using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace TW_Desktop
{
    static class Program
    {
        static Logger logger;

        public static bool debugMode = false;
        public static bool useTray = true;

        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length != 0)
                for (int i = 0;i<args.Length;i++)
                {
                    if (args[i] == "-debug")
                        debugMode = true;
                    if (args[i] == "-disable-tray")
                        useTray = false;
                }
            if (debugMode)
            {
                logger = new Logger();
                Logger.Info("Log started");
                Windows.AllocConsole();
                Console.Title = "Desktop Debug Window";
            }
            Logger.Info("Set up handle for unhandled exception");
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += new ThreadExceptionEventHandler((object sender, ThreadExceptionEventArgs e) =>
            {
                UnhandledException(e.Exception);
            });
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler((object sender, UnhandledExceptionEventArgs e) =>
            {
                UnhandledException((Exception)e.ExceptionObject);
            });
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Logger.Info("Show desktop");
            Application.Run(new Form1());
            Logger.Info("Program quit");
            logger.CloseStream();
        }

        static void UnhandledException(Exception ex)
        {
            Logger.Error("Unhandled exception detected");
            if (Form.ActiveForm is Form1)
            {
                Logger.Info("Close desktop");
                Form.ActiveForm.Close();
            }
            //TODO: Add processor for unhandled exception
        }
    }
}
