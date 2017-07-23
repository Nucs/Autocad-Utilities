using System.Collections.Generic;
using System.Drawing;
using autonet.Extensions;
using autonet.Settings;
using Newtonsoft.Json;

namespace autonet {
    public class QQConfigurationFile : SettingsBag {

        public static FileFilter Filter { get; } = new FileFilter(new[] {new KeyValuePair<string, IEnumerable<string>>("qqc", new[] {"*.qqc"}), new KeyValuePair<string, IEnumerable<string>>("All Files", new[] {"*.*"})});

        public QQConfigurationFile() : base() { }
        public QQConfigurationFile(string filename) : base(filename) { }

        public string Path { get; set; }

        /// <summary>
        ///     The config file name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Selected Layer
        /// </summary>
        public string Layer { get; set; }

        /// <summary>
        ///     Polylines width.
        /// </summary>
        public double Width { get; set; }

        /// <summary>
        ///     Thickness.
        /// </summary>
        public double Thickness { get; set; }

        public string Linetype { get; set; }

        public bool ConvertAllToPolyline { get; set; }
        public bool JoinPolylines { get; set; }

        [JsonConverter(typeof(DrawingColorConverter))]
        public Color Color { get; set; }

        public bool EnabledLayer { get; set; }
        public bool EnabledColor { get; set; }
        public bool EnabledWidth { get; set; }
        public bool EnabledThickness { get; set; }
        public bool EnabledLinetype { get; set; }
        public string ColorLabel { get; set; }
    }
}