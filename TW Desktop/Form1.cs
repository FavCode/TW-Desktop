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

        BufferedGraphics graphicsBuffer;
        FontManager fontManager = new FontManager();
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
                    Process.Start(Application.StartupPath + @"\DesktopTray.exe", Handle.ToInt32().ToString());
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
        }

        void DrawBackground(Graphics g)
        {
            Brush b = new LinearGradientBrush(ClientRectangle, Color.FromArgb(255, 161, 255, 213), Color.FromArgb(255, 47, 247, 157), LinearGradientMode.BackwardDiagonal);
            g.FillRectangle(b, ClientRectangle);
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
            label2.Text = "Starting tray...";
            Process.Start(Application.StartupPath + @"\DesktopTray.exe", Handle.ToString());
            label2.Text = "Starting tray checker...";
            new Thread(TrayChecker)
            {
                IsBackground = true
            }.Start();
            label2.Text = "Starting timer...";
            new Thread(TimerUpdate)
            {
                IsBackground = true
            }.Start();
            label2.Text = "Loading fonts...";
            fontManager.LoadFonts();
            fontManager.LoadFonts(".ttf");
            foreach (string ff in fontManager.GetLoadedFonts())
                Console.WriteLine(ff);
            label2.Text = "Finished";
            foreach (Control c in Controls.Cast<Control>().ToList())
                Controls.Remove(c);
            Paint += new PaintEventHandler(Form1_Paint);
            TopMost = false;
            WindowState = FormWindowState.Maximized;
            Windows.ShowWindow(Windows.FindWindow("Shell_TrayWnd", null), Windows.SW_HIDE);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Windows.ShowWindow(Windows.FindWindow("Shell_TrayWnd", null), Windows.SW_SHOW);
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
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && !hasMouseEventAction)
            {
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
                showContextMenu = false;
                hasMouseEventAction = true;
            }
        }
    }
}
