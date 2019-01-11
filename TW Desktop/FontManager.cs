using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace TW_Desktop
{
    public class FontManager
    {
        string[] resources;
        PrivateFontCollection pfc;
        public PrivateFontCollection Fonts
        {
            get
            {
                return pfc;
            }
        }

        public FontManager()
        {
            resources = ResourceManager.GetFileList();
            pfc = new PrivateFontCollection();
        }

        public FontFamily GetLoadedFont(string name)
        {
            foreach (FontFamily font in pfc.Families)
                if (font.Name == name)
                    return font;
            return null;
        }

        public string[] GetLoadedFonts()
        {
            List<string> fontNames = new List<string>();
            foreach (FontFamily ff in pfc.Families)
                fontNames.Add(ff.Name);
            return fontNames.ToArray();
        }

        public void LoadFont(string name)
        {
            Stream stream = ResourceManager.GetResourceFile(name, true);
            stream.Position = 0;
            byte[] font = new byte[stream.Length];
            stream.Read(font, 0, font.Length);
            stream.Close();
            GCHandle hObject = GCHandle.Alloc(font, GCHandleType.Pinned);
            IntPtr ptr = hObject.AddrOfPinnedObject();
            pfc.AddMemoryFont(ptr, font.Length);
            Logger.Info(pfc.Families.Last().Name + "(" + font.Length + " byte) loaded");
        }

        public void LoadFont(byte[] font)
        {
            GCHandle hObject = GCHandle.Alloc(font, GCHandleType.Pinned);
            IntPtr ptr = hObject.AddrOfPinnedObject();
            pfc.AddMemoryFont(ptr, font.Length);
            Logger.Info(pfc.Families.Last().Name + "(" + font.Length + " byte) loaded");
        }

        public void LoadFonts()
        {
            LoadFonts(".ttc");
        }

        public void LoadFonts(string endWith)
        {
            Logger.Info("Loading fonts with " + endWith + " suffix");
            foreach (string file in resources)
                if (file.EndsWith(endWith))
                    LoadFont(file);
        }

        public void LoadFonts(byte[][] fonts)
        {
            foreach (byte[] font in fonts)
                LoadFont(font);
        }
    }
}
