using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using autonet;
using autonet.Common.Settings;
using autonet.lsp;
using Common;
using Microsoft.WindowsAPICodePack.Dialogs;
using nucs.WinForms.Tray;

namespace LispDebugAssistant {
    public partial class MainForm : Form {
        public bool IsMonitoring { get; set; } = false;
        public LspFileWatcher CurrentListener { get; private set; }
        public string CurrentFolder => Config.CurrentFolder;
        public static AppConfig Config { get; } = JsonConfiguration.Load<AppConfig>();
        public TrayIcon TIcon { get; private set; }

        public MainForm() {
            InitializeComponent();
            InitlstWatchedMenu();
            Main._signal.Set();
        }

        private void InitlstWatchedMenu() {
            lstWatching.ContextMenu = new ContextMenu(new[] {
                new MenuItem("Reload Selected", (sender, args) => {
                    var t = lstWatching.SelectedItems
                        .Cast<object>()
                        .Select(o => Path.Combine(Paths.ConfigDirectory.FullName, "lsp/" + o.ToString()))
                        .ToArray();
                    foreach (var file in t) {
                        Reload(file);
                    }
                }),
                /*new MenuItem("Remove Selected", (sender, args) => {
                    var t = lstWatching.SelectedItems
                        .Cast<object>()
                        .Select(o => Path.Combine(Paths.ConfigDirectory.FullName, "lsp/" + o.ToString()))
                        .ToArray();
                    foreach (var file in t) {
                        GUI.RemoveListeningTo(file);
                    }
                }),*/
            });
        }

        private void MainForm_Load(object sender, EventArgs e) {
            GUI.SetCurrentFolder(CurrentFolder);
            GUI.SetStatus("Initial loading...");
            if (MainForm.Config.AutoStart) {
                OnOffSwitch(true);
            }
            TIcon = new TrayIcon(Icon);
            TIcon.AddMenuItem("Show/Hide", () => {
                if (this.Visible)
                    this.Hide();
                else {
                    this.Show();
                }
            });
            TIcon.Show();
            TIcon.ShowBalloonTipFor(5000, "Lisp Debug Assistant", "Letting you know im running!", ToolTipIcon.Info);

            if (Config.LoadAllOnStartup)
                ReloadAll();
        }

        /*protected override void SetVisibleCore(bool value) {
            base.SetVisibleCore(!MainForm.Config.StartMinimized && value);
        }*/

        private void btnOnOff_Click(object sender, EventArgs e) {
            OnOffSwitch();
        }

        /// <param name="setto">True=on, false=off, null=toggle</param>
        private void OnOffSwitch(bool? setto = null) {
            lock (this) {
                if (setto == true)
                    goto _on;
                else if (setto == false)
                    goto _off;
                else if (IsMonitoring) {
                    goto _off;
                } else {
                    goto _on;
                }

                _off:
                //turn off:
                GUI.StateTurnOff();
                CurrentListener?.Dispose();
                CurrentListener = null;
                IsMonitoring = false;
                GUI.SetStatus("Turned Off Successfully.");
                return;

                _on:
                //turn on:
                GUI.StateTurnOn();
                if (CurrentFolder == null || Directory.Exists(CurrentFolder) == false) {
                    GUI.AddLog("ERROR", $"Could not find selected directory! {(string.IsNullOrEmpty(CurrentFolder) ? "" : $"({CurrentFolder})")}");
                    GUI.StateTurnOff();
                    GUI.SetStatus("Failed loading...");
                    return;
                }
                try {
                    CurrentListener = new LspFileWatcher(CurrentFolder);
                    _bindWatcher(CurrentListener);
                    GUI.SetListeningTo(Directory.GetFiles(CurrentFolder, "*.lsp"));
                } catch (Exception ex) {
                    GUI.LogException(ex);
                    if (Debugger.IsAttached)
                        Debug.WriteLine(ex);
                    Console.WriteLine(ex);
                    GUI.StateTurnOff();
                    GUI.SetStatus("Failed Starting...");
                    return;
                }
                IsMonitoring = true;
                GUI.SetStatus("Turned On Successfully.");
                if (Config.LoadAllOnTurnOn)
                    ReloadAll();
                return;
            }
        }

        private void _bindWatcher(LspFileWatcher w) {
            w.FileAdded += (filename, when) => { GUI.AddListeningTo(filename); };
            w.FileDeleted += (filename, when) => { GUI.RemoveListeningTo(filename); };

            w.FileChanged += (filename, when) => {
                LspManager.LoadFile(filename);
                GUI.HasReloaded(filename);
            };


            SystemSounds.Asterisk.Play();
        }

        private void btnSelectFolder_Click(object sender, EventArgs e) {
            if (Control.ModifierKeys == Keys.Shift) {
                if (string.IsNullOrEmpty(CurrentFolder) == false) {
                    Process.Start("explorer.exe", CurrentFolder);
                }
                return;
            }
            lock (this) {
                _reselect:
                CommonOpenFileDialog dialog = new CommonOpenFileDialog {
                    InitialDirectory = CurrentFolder,
                    RestoreDirectory = true,
                    EnsurePathExists = true,
                    Title = "Select directory to read all .lsp files from.",
                    IsFolderPicker = true,
                    Multiselect = false,
                    ShowHiddenItems = true,
                    NavigateToShortcut = true
                };
                if (dialog.ShowDialog() != CommonFileDialogResult.Ok)
                    return;
                var d = dialog.FileName;

                Config.CurrentFolder = d;
                Config.Save();

                GUI.SetCurrentFolder(CurrentFolder);

                if (IsMonitoring) {
                    this.btnOnOff_Click(null, null);
                    this.btnOnOff_Click(null, null);
                }
            }
        }

        private void btnSettings_Click(object sender, EventArgs e) {
            //todo should lock *this*?
            new SettingsForm(Config).ShowDialog(this);
        }

        private void lblStatus_Click(object sender, EventArgs e) { }

        private void btnClearLog_Click(object sender, EventArgs e) {
            lock (lstLog.Items) {
                lstLog.Items.Clear();
            }
        }

        public void Reload(string path) {
            path = path.Replace('\\', '/');
            if (File.Exists(path) == false)
                return;
            LspManager.LoadFile(path);
            GUI.HasReloaded(path);
        }

        public void ReloadAll() {
            var t = lstWatching.Items
                .Cast<object>()
                .Select(o => Path.Combine(Paths.ConfigDirectory.FullName, "lsp/" + o.ToString()))
                .ToArray();
            foreach (var file in t) {
                Reload(file);
            }
        }

        private void btnReload_Click(object sender, EventArgs e) {
            if (MessageBox.Show("Are you sure you want to reload all?", "Reloading...", MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk) == DialogResult.Yes)
                ReloadAll();
        }

        private void MainForm_Shown(object sender, EventArgs e) {
            if (Config.StartMinimized)
                this.Hide();
        }
    }
}