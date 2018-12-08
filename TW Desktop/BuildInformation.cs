using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TW_Desktop
{
    public static class BuildInformation
    {
        private static readonly string buildDate = "%BuildDate%";
        private static readonly string buildOS = "%BuildOS%";
        private static readonly string buildArchitecture = "%Architecture%";
        public static string BuildDate
        {
            get
            {
                return (buildDate == "%BuildDate%" ? "Unknown" : buildDate);
            }
        }
        public static string BuildOS
        {
            get
            {
                return (buildOS == "%BuildOS%" ? "Unknown" : buildOS);
            }
        }
        public static string BuildArchitecture
        {
            get
            {
                return (buildArchitecture == "%Architecture%" ? "Unknown" : buildArchitecture);
            }
        }
    }
}
