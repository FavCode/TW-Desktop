using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using TWDesktopPluginSdk.Plugin;

namespace TW_Desktop
{
    class PluginManager
    {
        List<Plugin> plugins = new List<Plugin>();

        public Plugin[] Plugins
        {
            get
            {
                return plugins.ToArray();
            }
        }

        public class Plugin
        {
            Assembly assembly;
            Type mainClass;
            Object instance;
            string name;
            string version;
            string description;

            public Assembly Assembly
            {
                get
                {
                    return assembly;
                }
            }
            public Type Class
            {
                get
                {
                    return mainClass;
                }
            }
            public Object Instance
            {
                get
                {
                    return instance;
                }
            }
            public string Name
            {
                get
                {
                    return name;
                }
            }
            public string Version
            {
                get
                {
                    return version;
                }
            }
            public string Descrption
            {
                get
                {
                    return description;
                }
            }

            public Plugin(Assembly assembly,Type mainClass,Object instance,string name,string version,string description)
            {
                this.assembly = assembly;
                this.mainClass = mainClass;
                this.instance = instance;
                this.name = name;
                this.version = version;
                this.description = description;
            }
        }

        public void LoadPlugins()
        {
            LoadPlugins(Directory.GetCurrentDirectory() + @"\plugins");
        }

        public void LoadPlugins(string directory)
        {
            if (!Directory.Exists(directory))
                return;
            string[] files = Directory.GetFiles(directory);
            foreach (string file in files)
                LoadPlugin(file);
        }

        public void LoadPlugin(string file)
        {
            Logger.Info("Load plugin " + new FileInfo(file).Name);
            try
            {
                Assembly assembly = Assembly.LoadFrom(file);
                Type type = assembly.GetType("Plugin");
                if (!(type is IPlugin))
                    return;
                Object obj = Activator.CreateInstance(type);
                MethodInfo mi = type.GetMethod("Initialize");
                mi.Invoke(obj, null);
                plugins.Add(new Plugin(assembly, type, obj, type.GetProperty("PluginName").GetValue(obj, null).ToString(), assembly.GetName().Version.ToString(), type.GetProperty("PluginDescription").GetValue(obj, null).ToString()));
                Logger.Info("Plugin " + plugins.Last().Name + " v" + plugins.Last().Version + " loaded");
            } catch(Exception ex)
            {
                Logger.Warn("Can't load plugin " + new FileInfo(file).Name + " (" + ex.Message + ")");
                return;
            }
        }
    }
}
