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
        public bool IsMonitoring => !Manager.IsPaused;
        public LspManager Manager { get; } = new LspManager();
        public static AppConfig Config { get; } = JsonConfiguration.Load<AppConfig>();
        public TrayIcon TIcon { get; private set; }
        

        public MainForm() {
            InitializeComponent();
            InitlstWatchedMenu();
            Manager.Error += ManagerOnError;
            Manager.FileChanged += ManagerOnFileChanged;
            Manager.FileRenamed += ManagerOnFileRenamed;
            Manager.FileDeleted += ManagerOnFileDeleted;
            Manager.BeganWatchingFile += ManagerOnBeganWatchingFile;
            Manager.StoppedWatchingFile += ManagerOnStoppedWatchingFile;

            this.AllowDrop = true;
            this.DragEnter += new DragEventHandler(DragAndDropEnter);
            this.DragDrop += new DragEventHandler(DragAndDrop);

            Main._signal.Set();
        }
        void DragAndDropEnter(object sender, DragEventArgs e) {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
        }

        void DragAndDrop(object sender, DragEventArgs e) {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string file in files.Where(File.Exists))
                Manager?.Watch(file);
        }
        private void ManagerOnStoppedWatchingFile(LspFile lspFile, DateTime dateTime) {
            GUI.RemoveListeningTo(lspFile.FileName);
            lock (Config) {
                LspFile[] tmp;
                lock (Manager.LockRoot) 
                    tmp = Manager.Files.ToArray();
                
                Config.Files = tmp.Select(ls => ls.FullPath).ToArray();
                Config.Save();
            }
        }

        private void ManagerOnBeganWatchingFile(LspFile lspFile, DateTime dateTime) {
            GUI.AddListeningTo(lspFile.FileName);
            lock (Config) {
                LspFile[] tmp;
                lock (Manager.LockRoot) 
                    tmp = Manager.Files.ToArray();
                
                Config.Files = tmp.Select(ls => ls.FullPath).ToArray();
                Config.Save();
            }
        }

        private void ManagerOnFileDeleted(LspFile file, DateTime dateTime) {
            GUI.RemoveListeningTo(file.FileName);
        }

        private void ManagerOnFileRenamed(string oldFilename, LspFile file, DateTime _) {
            GUI.FileRenamed(oldFilename, file);
        }

        private void ManagerOnFileChanged(LspFile file, DateTime dateTime) {
            GUI.HasReloaded(file.FileName);
            file.Reload(true);
        }

        private void ManagerOnError(Exception exception, DateTime dateTime) {
            GUI.LogException(exception);
        }

        private void InitlstWatchedMenu() {
            lstWatching.ContextMenu = new ContextMenu(new[] {
                new MenuItem("Reload Selected", (sender, args) => {
                    var t = lstWatching.SelectedItems
                        .Cast<object>()
                        .Select(o => Manager.FindLspFile(o.ToString())).Where(l=>l!=null)
                        .ToArray();
                    foreach (var file in t) {
                        file.Reload();
                    }
                }),
                new MenuItem("Remove Selected", (sender, args) => {
                    var t = lstWatching.SelectedItems
                        .Cast<object>()
                        .Select(o => Manager.FindLspFile(o.ToString())).Where(l=>l!=null)
                        .ToArray();
                    foreach (var file in t) {
                        file.Remove();
                    }
                }),
            });
        }

        private void MainForm_Load(object sender, EventArgs e) {
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

            foreach (var file in Config.Files ??new string[0] ) {
                Manager.Watch(file);
            }

            if (Config.LoadAllOnStartup)
                ReloadAll();
        }

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
                Manager.Pause();
                GUI.SetStatus("Paused Successfully.");
                return;

                _on:
                //turn on:
                GUI.StateTurnOn();
                Manager.Resume();
                GUI.SetStatus("Resumed Successfully.");
                if (Config.LoadAllOnTurnOn)
                    ReloadAll();
                return;
            }
        }

        private void _bindWatcher(LspFileWatcher w) {
            w.FileAdded += (filename, when) => { GUI.AddListeningTo(filename); };
            w.FileDeleted += (filename, when) => { GUI.RemoveListeningTo(filename); };

            w.FileChanged += (filename, when) => {
                LspLoader.LoadFile(filename);
                GUI.HasReloaded(filename);
            };


            SystemSounds.Asterisk.Play();
        }

        private void btnSelectFolder_Click(object sender, EventArgs e) {
            if (Control.ModifierKeys == Keys.Shift) {
                var tmp = Path.ChangeExtension(Paths.MarkForDeletion(Path.GetTempFileName()), ".txt");
                var txt = "";
                lock (Manager.LockRoot) 
                    txt = string.Join(Environment.NewLine, Manager.Files.ToArray().Select(f => f.FullPath));
                
                File.WriteAllText(tmp, txt);
                Process.Start(tmp);
                return;
            }
            lock (this) {
                _reselect:
                CommonOpenFileDialog dialog = new CommonOpenFileDialog {
                    RestoreDirectory = true,
                    EnsurePathExists = true,
                    Title = "Select .lsp files from or a single file.",
                    ShowPlacesList = true,
                    Multiselect = true,
                    ShowHiddenItems = true,
                    Filters = { new CommonFileDialogFilter("*.lsp", "*.lsp"), new CommonFileDialogFilter("All Files", "*") },
                    NavigateToShortcut = true
                };
                if (dialog.ShowDialog() != CommonFileDialogResult.Ok)
                    return;

                foreach (var fn in dialog.FileNames)
                    Manager.Watch(fn);

            }
        }

        private void btnSettings_Click(object sender, EventArgs e) {
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
            LspLoader.LoadFile(path);
            GUI.HasReloaded(path);
        }

        public void ReloadAll() {
            Manager.ReloadAll(true);
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