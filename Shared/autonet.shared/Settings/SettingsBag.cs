using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using autonet.Common.Settings;
using Common;
using Newtonsoft.Json;

namespace autonet.Settings {
    public class SettingsBag : JsonConfiguration {
        [JsonIgnore]
        public override string FileName { get; }

        public SettingsBag() { }

        public SettingsBag(string fileName) {
            FileName = fileName;
            foreach (var pi in this.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)) {
                if ((pi.CanRead && pi.CanWrite) == false)
                    continue;
                PropertyData.Add(pi.Name, pi);
            }
        }

        public object this[string key] {
            get {
                lock (this) return Get<object>(key);
            }
            set {
                lock (this) Set(key,value);
            }
        }

        public readonly SafeDictionary<string, object> Data = new SafeDictionary<string, object>();
        private readonly SafeDictionary<string,PropertyInfo> PropertyData = new SafeDictionary<string, PropertyInfo>();

        public T Get<T>(string key, T @default = default(T)) {
            lock (this) {
                if (PropertyData.ContainsKey(key))
                    return (T) PropertyData[key].GetValue(this,null);

                var ret = Data[key];
                if (ret==null || ret.Equals(default(T)))
                    return @default;

                return (T) ret;
            }
        }

        public void Set(string key, object value) {
            lock (this) {
                if (PropertyData.ContainsKey(key))
                    PropertyData[key].SetValue(this,value,null);
                else
                    Data[key] = value;
            }
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


    }
}