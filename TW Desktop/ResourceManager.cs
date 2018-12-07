using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace TW_Desktop
{
    public static class ResourceManager
    {
        public static string[] GetFileList()
        {
            return Assembly.GetExecutingAssembly().GetManifestResourceNames();
        }

        public static Stream GetResourceFile(string name)
        {
            return GetResourceFile(name, false);
        }

        public static Stream GetResourceFile(string name, bool isNamespaceDefined)
        {
            Type type = MethodBase.GetCurrentMethod().DeclaringType;
            string start = "";
            if (!isNamespaceDefined)
                start = type.Namespace + ".";
            return Assembly.GetExecutingAssembly().GetManifestResourceStream(start + name);
        }
    }
}
