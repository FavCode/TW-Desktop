using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace TWDesktopPluginSdk.Plugin
{
    interface IPlugin
    {
        string PluginName { get; }
        string PluginDescription { get; }
        void Initialize();
        void Draw(Graphics g);
    }
}
