using System;
using System.IO;
using autonet.Common.Settings;
using Common;

namespace autonet.Settings {
    public class QuickySettings : JsonConfiguration {
        public override string FileName { get; } = Paths.ConfigFile("Quicky").FullName;
        public string Test { get; set; } = "lol";
    }
}