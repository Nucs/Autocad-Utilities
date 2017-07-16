using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Common;

namespace LispDebugAssistant {
    public static class GUI {
        private static MainForm m => Main.MainForm;
        public static void SetStatus(params string[] txts) {
            _invoke(() => m.lblStatus.Text = string.Join("", txts));
        }

        public static void StateTurnOn() {
            _invoke(() => { m.btnOnOff.Text = "Turn Off"; });
            AddLog("Watcher", "Has been turned On.");
        }

        public static void StateTurnOff() {
            _invoke(() => { m.btnOnOff.Text = "Turn On"; });
            AddLog("Watcher", "Has been turned off.");
        }

        public static void StateToggle() {
            if (m.IsMonitoring)
                StateTurnOff();
            else {
                StateTurnOn();
            }
        }

        public static void AddListeningTo(string name) {
            _invoke(() => {
                name = name.Contains("\\") || name.Contains("/") ? Path.GetFileName(name) : name;
                lock (m.lstWatching.Items) {
                    if (m.lstWatching.Items.Contains(name) == false) {
                        m.lstWatching.Items.Add(name);
                        GUI.AddLog("Watcher", $"Started listening to {Path.GetFileName(name)}");
                    }
                }
            });
        }

        public static void RemoveListeningTo(string name) {
            _invoke(() => {
                name = name.Contains("\\") || name.Contains("/") ? Path.GetFileName(name) : name;
                lock (m.lstWatching.Items) {
                    if (m.lstWatching.Items.Contains(name)) {
                        m.lstWatching.Items.Remove(name);
                        GUI.AddLog("Watcher", $"Stopped listening to {Path.GetFileName(name)}");
                    }
                }
            });
        }

        public static void AddLog(string topic, params string[] txts) {
            _invoke(() => m.lstLog.Items.Insert(0, $"[{topic}]" + string.Join("", txts ?? new string[0])));
        }

        public static void SetListeningTo(string[] rows) {
            _invoke(() => {
                lock (m.lstWatching.Items) {
                    foreach (var s in m.lstWatching.Items.Cast<object>().Select(s=>s.ToString()).ToArray()) {
                        RemoveListeningTo(s);
                    }
                    if (rows != null && rows.Length != 0) {
                        foreach (var row in rows) {
                            AddListeningTo(row);
                        }
                    }
                }
            });
        }

        private static void _invoke(Action act) {
            Main.WaitForMainForm();
            if (Main.MainForm.InvokeRequired) {
                Main.MainForm.Invoke(new MethodInvoker(() => _invoke(act)));
                return;
            }
            act();
        }
        private static T _invokeret<T>(Func<T> act) {
            Main.WaitForMainForm();
            if (Main.MainForm.InvokeRequired) {
                object ret = null;
                Main.MainForm.Invoke(new MethodInvoker(() => ret= _invokeret(act)));
                return (T) ret;
            }
            return act();
        }

        public static void LogException(Exception e) {
            var dt = DateTime.Now;
            LogException(e,dt);
        }

        public static void LogException(Exception e, DateTime when) {
            var txts = e.ToString()
                .Replace("\r", "")
                .Replace("\n\n", "\n").Replace("\n\n", "\n")
                .Split('\n')
                .Reverse()
                .ToArray();
            _invoke(() => {
                foreach (var t in txts) {
                    m.lstLog.Items.Insert(0, t);
                }
                m.lstLog.Items.Insert(0, $"[EXCEPTION] has occured at "+ when.ToString("s"));
            });
        }

        public static void HasReloaded(string filename) {
            filename = filename.Contains("\\") || filename.Contains("/") ? Path.GetFileName(filename) : filename;
            AddLog("Watcher", "File Has reloaded: "+filename);
        }

        public static void FileRenamed(string oldFilename, LspFile file) {
            _invoke(() => {
                lock (m.lstWatching) {
                    m.lstWatching.Items.Remove(Path.GetFileName(Paths.NormalizePath(oldFilename)));
                    m.lstWatching.Items.Add(file.FileName);
                }
                AddLog("Watcher",$"File `{Path.GetFileName(oldFilename)}` has renamed to `{file.FileName}`");
            });
        }
    }
}