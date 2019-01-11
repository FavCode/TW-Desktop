using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace TW_Desktop
{
    public partial class Form1 : Form
    {
        class BackgroundObject
        {
            public static string[] BackgroundChars = new string[]
            {
                "□",
                "⭕",
                "■",
                "●",
                "▲"
            };

            public float x;
            public float y;
            public float rotation;
            public float rotationDir;
            public float speed;
            public string text;
            public BackgroundObject(float x,float y,float rotation,float speed)
            {
                this.x = x;
                this.y = y;
                this.rotation = rotation;
                this.speed = speed;
                rotationDir = new Random().Next(-1, 1);
                text = BackgroundChars[new Random().Next(0, BackgroundChars.Length - 1)];
            }
        }

        string handle = "";

        string debugMessage = "";

        bool drawing = false;
        bool rendering = false;

        bool hasMouseEventAction = false;

        bool showContextMenu = false;
        int cMenuWidth = 0;
        int cMenuTargetWidth = 0;
        Point cMenuPos;
        int cMenuAnimM = 20;
        string[] cMenuItems = new string[]
        {
            "This is first item~",
            "This is second context menu item~",
            "I am third~",
            "这里是第四个菜单项哦~"
        };
        string lastUpdateTime = "";
        delegate void Draw();

        List<BackgroundObject> bo = new List<BackgroundObject>();

        BufferedGraphics graphicsBuffer;
        FontManager fontManager = new FontManager();
        PluginManager pluginManager = new PluginManager();
        Bitmap bm;
        Graphics graphics;

        public Form1()
        {
            InitializeComponent();
            graphicsBuffer = (new BufferedGraphicsContext()).Allocate(CreateGraphics(), DisplayRectangle);
            bm = new Bitmap(DisplayRectangle.Width, DisplayRectangle.Height);
            graphics = Graphics.FromImage(bm);
        }

        void DrawAll()
        {
            if (drawing)
                return;
            drawing = true;
            DrawBackground(graphics);
            DrawTime(graphics);
            DrawIcons(graphics);
            DrawContextMenu(graphics);
            if (Program.debugMode)
                DrawDebug(graphics);
            drawing = false;
        }

        void Render()
        {
            if (rendering)
                return;
            rendering = true;
            Graphics displayGraphics = graphicsBuffer.Graphics;
            displayGraphics.Clear(Color.White);
            displayGraphics.DrawImage(bm, 0, 0);
            graphicsBuffer.Render();
            rendering = false;
        }

        void TrayChecker()
        {
            while (true)
            {
                int trayCount = Process.GetProcessesByName("DesktopTray").Length;
                if (trayCount == 0)
                {
                    Logger.Info("Tray has been ended, restarting...");
                    Process.Start(Application.StartupPath + @"\DesktopTray.exe", handle);
                }
                else
                    for (int i = 0; i == trayCount; i++)
                        Process.GetProcessesByName("DesktopTray")[i].Kill();
                Thread.Sleep(5000);
            }
        }

        void TimerUpdate()
        {
            Draw d = new Draw(DrawAll);
            Draw r = new Draw(Render);
            while (true)
            {
                Invoke(d);
                Invoke(r);
                Thread.Sleep(10);
            }
        }

        void FixContextMenuAnim()
        {
            if (Math.Abs(cMenuTargetWidth - cMenuWidth) / cMenuAnimM == 0 && cMenuAnimM != 2)
                cMenuAnimM--;
        }

        void DrawDebug(Graphics g)
        {
            Brush red = new SolidBrush(Color.Red);
            Font f = new Font(fontManager.GetLoadedFont("Consolas"), 12);
            Font iF = new Font(fontManager.GetLoadedFont("Consolas"), 12, FontStyle.Italic);
            g.DrawString("Debug Mode", f, red, new PointF(0, 0));
            g.DrawString($"v{Assembly.GetExecutingAssembly().GetName().Version} built by {BuildInformation.BuildOS} ({BuildInformation.BuildArchitecture}) at {BuildInformation.BuildDate}", iF, red, new PointF(0, Height - g.MeasureString("Height Test", iF).Height));
            g.DrawString(debugMessage, f, red, new PointF(Width - g.MeasureString(debugMessage, f).Width, 0));
        }

        void DrawBackground(Graphics g)
        {
            Font f = new Font(fontManager.GetLoadedFont("Microsoft YaHei UI Light"), 128);
            Brush b = new LinearGradientBrush(ClientRectangle, Color.FromArgb(255, 161, 255, 213), Color.FromArgb(255, 47, 247, 157), LinearGradientMode.BackwardDiagonal);
            Brush bb = new SolidBrush(Color.DarkGray);
            g.FillRectangle(b, ClientRectangle);
            /*foreach (BackgroundObject i in bo)
            {
                if (i.speed == 0)
                    bo.Remove(i);
                i.y -= i.speed;
                i.rotation += i.rotationDir * i.speed;
                g.Transform.Rotate(i.rotation);
                g.DrawString(i.text, f, bb, new PointF(i.x, i.y));
                i.speed -= 1;
                g.Transform.Rotate(0);
            }*/
        }

        void DrawTime(Graphics g)
        {
            Font f = new Font(fontManager.GetLoadedFont("Microsoft YaHei UI Light"), 128);
            Brush b = new SolidBrush(Color.FromArgb(255, 54, 154, 104));
            string time = DateTime.Now.ToShortTimeString();
            float xOffset = -(PointToClient(Cursor.Position).X - Size.Width / 2) / 20;
            float yOffset = -(PointToClient(Cursor.Position).Y - Size.Height / 2) / 20;
            lastUpdateTime = time;
            if (g == null)
            {
                g = graphics;
                Brush lb = new LinearGradientBrush(ClientRectangle, Color.FromArgb(255, 161, 255, 213), Color.FromArgb(255, 47, 247, 157), LinearGradientMode.BackwardDiagonal);
                g.FillRectangle(lb, ClientRectangle);
                g.DrawString(time, f, b, new PointF(Width - g.MeasureString(time, f).Width / 1.2f + xOffset, Height - g.MeasureString(time, f).Height / 1.5f + yOffset));
                Render();
            } else
                g.DrawString(time, f, b, new PointF(Width - g.MeasureString(time, f).Width / 1.2f + xOffset, Height - g.MeasureString(time, f).Height / 1.5f + yOffset));
        }

        void DrawIcons(Graphics g)
        {

        }

        void DrawContextMenu(Graphics g)
        {
            Region oriClip = g.Clip;
            Brush b = new SolidBrush(Color.FromArgb(100, 255, 255, 255));
            Brush fb = new SolidBrush(Color.White);
            Brush bb = new SolidBrush(Color.FromArgb(50, 0, 0, 0));
            Font f = new Font(fontManager.GetLoadedFont("Microsoft YaHei"), 12);
            if (!showContextMenu)
            {
                cMenuTargetWidth = 0;
            }
            if (!showContextMenu && cMenuWidth == 0)
                return;
            if (cMenuTargetWidth > cMenuWidth)
            {
                FixContextMenuAnim();
                if (cMenuAnimM == 2)
                {
                    cMenuWidth = cMenuTargetWidth;
                    cMenuAnimM = 20;
                }
                else
                    cMenuWidth += Math.Abs(cMenuTargetWidth - cMenuWidth) / cMenuAnimM;
            }
            else if (cMenuTargetWidth < cMenuWidth)
            {
                FixContextMenuAnim();
                if (cMenuAnimM == 2)
                {
                    cMenuWidth = cMenuTargetWidth;
                    cMenuAnimM = 20;
                }
                else
                    cMenuWidth -= Math.Abs(cMenuTargetWidth - cMenuWidth) / cMenuAnimM;
            }
            float fh = g.MeasureString("Height Test", f).Height + 4;
            Rectangle r = new Rectangle(cMenuPos, new Size(cMenuWidth, cMenuItems.Length * (int)(fh + 2)));
            g.FillRectangle(b, r);
            g.SetClip(r);
            for (int i = 0;i<cMenuItems.Length;i++)
            {
                string item = cMenuItems[i];
                Rectangle itemBg = new Rectangle(new Point(cMenuPos.X, cMenuPos.Y + (int)(fh + 2) * i), new Size(cMenuWidth, (int)fh + 2));
                if (itemBg.Contains(Cursor.Position))
                    g.FillRectangle(bb, itemBg);
                g.DrawString(item, f, fb, new PointF(cMenuPos.X + 34, cMenuPos.Y + (fh + 2) * i + 4));
            }
            g.Clip = oriClip;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            handle = Handle.ToString();
            if (Program.useTray)
            {
                label2.Text = "Starting tray...";
                Process.Start(Application.StartupPath + @"\DesktopTray.exe", Handle.ToString());
                Logger.Info("Start tray with handle " + Handle.ToString());
                label2.Text = "Starting tray checker...";
                Logger.Info("Start tray protecter");
                new Thread(TrayChecker)
                {
                    IsBackground = true
                }.Start();
            }
            label2.Text = "Starting timer...";
            Logger.Info("Start update timer");
            new Thread(TimerUpdate)
            {
                IsBackground = true
            }.Start();
            label2.Text = "Loading fonts...";
            Logger.Info("Load fonts");
            fontManager.LoadFonts();
            fontManager.LoadFonts(".ttf");
            Logger.Info(fontManager.Fonts.Families.Length + " font(s) loaded");
            label2.Text = "Loading plugins...";
            Logger.Info("Load plugins from plugins folder");
            pluginManager.LoadPlugins();
            Logger.Info(pluginManager.Plugins.Length + " plugin(s) loaded");
            label2.Text = "Finished";
            Logger.Info("Load finished");
            Logger.Info("Remove all windows controls");
            foreach (Control c in Controls.Cast<Control>().ToList())
                Controls.Remove(c);
            for (int i = 0; i < Width / 20; i++)
                bo.Add(new BackgroundObject(new Random().Next(10, Width - 20), Height + 50, new Random().Next(0, 360), new Random().Next((int)(Height * 0.05)) + new Random().Next(-3, 3)));
            Logger.Info("Bind paint event");
            Paint += new PaintEventHandler(Form1_Paint);
            TopMost = false;
            WindowState = FormWindowState.Maximized;
            Logger.Info("Window style changed");
            Windows.ShowWindow(Windows.FindWindow("Shell_TrayWnd", null), Windows.SW_HIDE);
            Logger.Info("Hide taskbar");
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Windows.ShowWindow(Windows.FindWindow("Shell_TrayWnd", null), Windows.SW_SHOW);
            Logger.Info("Show taskbar");
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            DrawAll();
            Render();
        }

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            graphicsBuffer = (new BufferedGraphicsContext()).Allocate(CreateGraphics(), DisplayRectangle);
            bm = new Bitmap(DisplayRectangle.Width, DisplayRectangle.Height);
            graphics = Graphics.FromImage(bm);
            Invalidate();
            Logger.Info("Window size changed, redrawing window");
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && !hasMouseEventAction)
            {
                Logger.Info("Show Context Menu");
                Font f = new Font(fontManager.GetLoadedFont("Microsoft YaHei"), 12);
                float cw = 0;
                cMenuPos = Cursor.Position;
                foreach (string item in cMenuItems)
                    if (graphics.MeasureString(item, f).Width > cw)
                        cw = graphics.MeasureString(item, f).Width;
                cMenuTargetWidth = (int)cw + 2 * 34;
                if (cMenuPos.X + cMenuTargetWidth > Width)
                    cMenuPos = new Point(cMenuPos.X - (cMenuPos.X + cMenuTargetWidth - Width), cMenuPos.Y);
                int mh = cMenuItems.Length * (int)(graphics.MeasureString("Height Test", f).Height + 4);
                if (cMenuPos.Y + mh > Height)
                    cMenuPos = new Point(cMenuPos.X, cMenuPos.Y - (cMenuPos.Y + mh - Height));
                showContextMenu = true;
                hasMouseEventAction = true;
            }

            if (hasMouseEventAction)
            {
                DrawAll();
                Render();
            }
            hasMouseEventAction = false;
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            Font cf = new Font(fontManager.GetLoadedFont("Microsoft YaHei"), 12);
            if (showContextMenu && !new Rectangle(cMenuPos, new Size(cMenuWidth, cMenuItems.Length * (int)(graphics.MeasureString("Height Test", cf).Height + 6))).Contains(Cursor.Position))
            {
                Logger.Info("Hide Context Menu");
                showContextMenu = false;
                hasMouseEventAction = true;
            }
        }
    }
}
