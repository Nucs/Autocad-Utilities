/*using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using autonet.Common.Settings;
using autonet.Forms;
using Common;

namespace autonet {
    public partial class QQSelectForm : Form {
        public QQAppConfig Config { get; } = JsonConfiguration.Load<QQAppConfig>();
        public QQCFile Selected { get; private set; }

        public QQSelectForm() {
            InitializeComponent();
            ClearDetails();
            RefreshList();
            ApplyHideChkbox();
        }

        private void chkHidePath_CheckedChanged(object sender, EventArgs e) {
            ApplyHideChkbox();
        }

        public List<QQCFile> Configs = new List<QQCFile>();

        public void RefreshList() {
            Configs = Config.Configurations;
            foreach (var c in Configs) {
                c.HidePath = chkHidePath.Checked;
            }
            lstSources.DataSource = null;
            lstSources.Refresh();
            lstSources.DataSource = Configs;
        }

        public void RefreshNames() {
            var db = lstSources.DataSource;
            lstSources.DataSource = null;
            lstSources.Refresh();
            lstSources.DataSource = db;
        }

        public void ApplyHideChkbox() {
            foreach (var c in Configs) {
                c.HidePath = chkHidePath.Checked;
            }
            RefreshNames();
        }

        public void UpdateDetails() { }

        /// <summary>
        /// Add top category
        /// </summary>
        private void addToolStripMenuItem_Click(object sender, EventArgs e) { }


        public void CreateNewConfig() {
            var cfg = new QQConfigurationFile();
            var form = StartEditor(cfg);
            if (form.ShouldSaveToSources && string.IsNullOrEmpty(cfg.Path?.Trim()) == false && File.Exists(cfg.Path)) {
                if (this.Config.Sources.Contains(cfg.Path, new Paths.FilePathEqualityComparer()) || this.Config.Sources.Contains(Path.GetDirectoryName(cfg.Path), new Paths.FilePathEqualityComparer()))
                    return;
                this.Config.Sources.Add(cfg.Path);
                RefreshList();
            }
        }

        public QQConfigForm StartEditor(QQCFile cfg) {
            if (cfg == null) throw new ArgumentNullException(nameof(cfg));
            return StartEditor(cfg.Config);
        }

        public QQConfigForm StartEditor(QQConfigurationFile cfg) {
            if (cfg == null) throw new ArgumentNullException(nameof(cfg));
            var f = new QQConfigForm(cfg);
            f.ShowDialog(this);
            return f;
        }

        /// <summary>
        ///     Add new file source
        /// </summary>
        private void fileToolStripMenuItem1_Click(object sender, EventArgs e) {
            var file = FileDialogs.GetFile(QQConfigurationFile.Filter, MultiSelect: false);
            if (file == null)
                return;
            if (this.Config.Sources.Contains(file.FullName, new Paths.FilePathEqualityComparer())) {
                MessageBox.Show("This file is already in the source collection!");
                return;
            }

            this.Config.Sources.Add(file.FullName);
            this.Config.Save();
            RefreshList();
        }

        /// <summary>
        ///     Add new directory source.
        /// </summary>
        private void directoryToolStripMenuItem_Click(object sender, EventArgs e) {
            var dir = FileDialogs.GetDirectory();
            if (dir == null || Directory.Exists(dir.FullName) == false)
                return;
            int count = 0;
            foreach (var f in dir.GetFiles("*.qqc")) {
                if (this.Config.Sources.Contains(f.FullName, new Paths.FilePathEqualityComparer()))
                    continue;

                count++;
                this.Config.Sources.Add(f.FullName);
            }
            MessageBox.Show($"{count} files were added.", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.Config.Save();
            RefreshList();
        }

        /// <summary>
        ///     Adds a new static directory for search inside.
        /// </summary>
        private void staticDirectoryToolStripMenuItem_Click(object sender, EventArgs e) {
            var dir = FileDialogs.GetDirectory();
            if (dir == null || Directory.Exists(dir.FullName) == false)
                return;

            Config.Sources.Add(dir.FullName);
            this.Config.Save();
            RefreshList();
        }

        private void lstSources_SelectedIndexChanged(object sender, EventArgs e) {
            var current = lstSources.SelectedItem as QQCFile;
            if (current == null) {
                ClearDetails();
                return;
            }
            current.Reload();
            LoadDetails(current);
        }

        private void LoadDetails(QQCFile f) {
            if (f == null) throw new ArgumentNullException(nameof(f));
            LoadDetails(f.Config);
        }

        private void LoadDetails(QQConfigurationFile f) {
            if (f == null) throw new ArgumentNullException(nameof(f));
            txtConfigName.Text = f.Name;
            txtPath.Text = f.Path;
            txtThickness.Text = f.Thickness.ToString();
            txtWidth.Text = f.Width.ToString();
            cmbLayers.Text = f.Layer;
            cmbLinetypes.Text = f.Linetype;
            clrCustom.Color = f.Color;
            lblSpecificColor.Text = f.ColorLabel;
            chkColor.Checked = f.EnabledColor;
            chkLinetype.Checked = f.EnabledLinetype;
            chkLayer.Checked = f.EnabledLayer;
            chkThickness.Checked = f.EnabledThickness;
            chkWidth.Checked = f.EnabledWidth;
            chkJoinPolylines.Checked = f.JoinPolylines;
            chkConvertAllToPolylines.Checked = f.ConvertAllToPolyline;
        }

        private void ClearDetails() {
            if (lstSources.SelectedIndex != -1) {
                lstSources.SelectedIndex = -1;
                return; //todo check might be recrusive event call.
            }

            txtConfigName.Text = "";
            txtPath.Text = "";
            txtThickness.Text = "";
            txtWidth.Text = "";
            cmbLayers.Text = "";
            cmbLinetypes.Text = "";
            clrCustom.Color = Color.Transparent;
            lblSpecificColor.Text = "";

            chkColor.Checked = false;
            chkLinetype.Checked = false;
            chkLayer.Checked = false;
            chkThickness.Checked = false;
            chkWidth.Checked = false;
            chkJoinPolylines.Checked = false;
            chkConvertAllToPolylines.Checked = false;
        }

        private void groupBox1_Enter(object sender, EventArgs e) { }

        private void QQForm_Load(object sender, EventArgs e) {
        }

        private void deleteSelectedToolStripMenuItem_Click(object sender, EventArgs e) {
            Config.Sources = new SourcesEditorForm(Config.Sources.ToList()).RetShowDialog(this)?.Distinct(new Paths.FilePathEqualityComparer()).ToList();

            Config.Save();
            RefreshList();
        }

        private void lstSources_DoubleClick(object sender, EventArgs e) { }

        private void lstSources_MouseDoubleClick(object sender, MouseEventArgs e) {
            int index = lstSources.IndexFromPoint(e.Location);
            if (index != System.Windows.Forms.ListBox.NoMatches) {
                Selected = (QQCFile) lstSources.Items[index];
                Close();
            }
        }
    }
}*/