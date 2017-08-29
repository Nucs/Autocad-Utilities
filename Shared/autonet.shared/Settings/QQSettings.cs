using autonet.Common.Settings;
using Common;

namespace autonet.Settings {
    public class QQSettings : JsonConfiguration {
        public override string FileName { get; } = Paths.ConfigFile("QQQuicky").FullName;

    }
}