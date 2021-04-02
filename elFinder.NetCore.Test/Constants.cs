using System;
using System.Collections.Generic;
using System.Text;

namespace elFinder.NetCore.Test
{
    public static class Config
    {
        public const string AppSettingFile = "appsettings.json";
        public const string HostKey = "Host";
        public const string DriverDirectoryKey = "DriverDirectory";
        public const string DriverKey = "Driver";
    }

    public static class Driver
    {
        public const string Chrome = nameof(Chrome);
    }
}
