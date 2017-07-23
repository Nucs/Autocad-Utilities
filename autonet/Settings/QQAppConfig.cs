using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using autonet.Common.Settings;
using autonet.Settings;
using Common;
using MoreLinq;
using Newtonsoft.Json;

namespace autonet {
    public class QQAppConfig : SettingsBag {

        private List<string> _lastFiles { get; set; } = new List<string>();

        [JsonIgnore]
        public override string FileName { get; } = Paths.ConfigFile("qqappconfig").FullName;

        /// <summary>
        ///     The sources inwhich it will look for qqc files.
        /// </summary>
        public List<string> Sources { get; set; } = new List<string>();

        /// <summary>
        ///     Path to the selected qqc file.
        /// </summary>
        public string Selected { get; set; }

        /// <summary>
        ///     Returns all qqc config files from sources.
        /// </summary>
        [JsonIgnore]
        public List<QQCFile> Configurations {
            get {
                var fs = Sources
                    .Where(Path.HasExtension)
                    .Select(f => new FileInfo(f))
                    .Where(f => File.Exists(f.FullName))
                    .Select(f => new QQCFile(f))
                    .Concat(
                        Sources.Where(p=>!Path.HasExtension(p) && Directory.Exists(p))
                        .SelectMany(d => Directory.GetFiles(d, "*.qqc", SearchOption.TopDirectoryOnly))
                        .Select(f => new FileInfo(f))
                        .Select(f => new QQCFile(f))
                    )
                    .DistinctBy(f=>f.File.FullName, new Paths.FilePathEqualityComparer())
                    .ToList();
                _lastFiles.Clear();
                _lastFiles.AddRange(fs.Select(f=>f.File.FullName));
                return fs;
            }
        }

        [JsonIgnore]
        public string SelectedPath {
            get {
                if (Selected == null)
                    return null;
                return LastFiles.ToArray().SingleOrDefault(s => Paths.CompareTo(s, Selected));
            }
        } 

        [JsonIgnore]
        public List<string> LastFiles {
            get {
                if (_lastFiles.Count == 0) {
                    var _ = this.Configurations;
                }
                return _lastFiles;
            }
            set => _lastFiles = value;
        }
    }

    public class QQCFile {
        private QQConfigurationFile _config;
        public FileInfo File { get; }

        public QQConfigurationFile Config {
            get {
                if (_config != null)
                    return _config;
                lock (this) 
                    return _config ?? (_config = JsonConfiguration.Load<QQConfigurationFile>(File.FullName));
            }
        }

        public string Name => Config?.Name;
        public QQCFile(FileInfo file, QQConfigurationFile config) {
            File = file;
            _config = config;
        }
        public QQCFile(FileInfo file) {
            File = file;
        }

        public bool HidePath { get; set; } = false;
        public override string ToString() {
            var name = (string.IsNullOrEmpty(Name) ? Path.GetFileNameWithoutExtension(File.FullName) : Name);
            if (HidePath) {
                return name;
            } else {
                return $"{name} - {File.FullName}";
            }
        }

        /// <summary>
        ///     Reload the QQConfiguration file
        /// </summary>
        public QQConfigurationFile Reload() {
            lock(this)
                return _config = JsonConfiguration.Load<QQConfigurationFile>(File.FullName);
        }

    }
}