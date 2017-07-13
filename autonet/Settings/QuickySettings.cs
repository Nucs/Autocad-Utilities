using System;
using System.IO;
using autonet.Common.Settings;

namespace autonet.Settings {
    public class QuickySettings : JsonConfiguration {
        public override string FileName { get; } = Path.Combine(Environment.ExpandEnvironmentVariables("USERPROFILE"), "autoload/" + Environment.MachineName + ".json");
        public string Test { get; set; } = "lol";
    }
}