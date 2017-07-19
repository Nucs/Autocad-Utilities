using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using autonet.Common.Settings;
using Common;

namespace autonet.Settings {
    public class SettingsBag : JsonConfiguration {
        public object this[string key] {
            get {
                lock (this) return Data[key];
            }
            set {
                lock (this) Data[key] = value;
            }
        }

        public SafeDictionary<string, object> Data = new SafeDictionary<string, object>();

        public T Get<T>(string key, T @default = default(T)) {
            lock (this) {
                var ret = this[key];
                if (ret==null || ret.Equals(default(T)))
                    return @default;

                return (T) ret;
            }
        }

        public void Set(string key, object value) {
            lock (this)
                this[key] = value;
        }

        public bool Remove(string key) {
            lock (this)
                return Data.Remove(key);
        }

        public int Remove(Func<KeyValuePair<string, object>, bool> comprarer) {
            lock (this) {
                int ret = 0;
                foreach (var kv in Data.ToArray()) {
                    if (comprarer(kv))
                        if (Remove(kv.Key)) {
                            ret += 1;
                        }
                }
                return ret;
            }
        }

        public override string FileName { get; }

        public SettingsBag(string fileName) {
            FileName = fileName;
        }
    }
}