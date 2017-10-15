/*using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Media;
using System.Windows.Forms;
using autonet.Common.Settings;
using autonet.Extensions;
using autonet.Settings;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.GraphicsSystem;
using Common;
using Microsoft.WindowsAPICodePack.Dialogs;
using Color = System.Drawing.Color;
using ColorDialog = Autodesk.AutoCAD.Windows.ColorDialog;

namespace autonet.Forms {
    public partial class QQConfigForm : SettingsForm<QQConfigurationFile> {
        public bool Saved { get; private set; } = false;
        public bool ShouldSaveToSources => tsSaveToSources.SelectedIndex == 0;
        public QQConfigForm(QQConfigurationFile cfg) {
            Settings = cfg;
            InitializeComponent();

            lblSpecificColor.BackColor = Color.Transparent;
            lblSpecificColor.Text = "";

            txtThickness.AcceptDoublesOnly();
            txtWidth.AcceptDoublesOnly();
            tsSaveToSources.SelectedIndex = 0;

            //controls
            Bind(txtConfigName, qqc => qqc.Name);
            Bind(cmbLayers, qqc => qqc.Layer);
            Bind(txtWidth, qqc => qqc.Width);
            Bind(txtThickness, qqc => qqc.Thickness);
            Bind(cmbLinetypes, qqc => qqc.Linetype);
            Bind(chkConvertToPolylines, qqc => qqc.ConvertAllToPolyline);
            Bind(chkJoinPolylines, qqc => qqc.JoinPolylines);
            Bind(txtPath, qqc => qqc.Path);
            Bind(clrCustom, qqc => qqc.Color);
            Bind(lblSpecificColor, qqc => qqc.ColorLabel);

            //enablers
            Bind(chkLayer, qqc => qqc.EnabledLayer);
            Bind(chkColor, qqc => qqc.EnabledColor);
            Bind(chkWidth, qqc => qqc.EnabledWidth);
            Bind(chkThickness, qqc => qqc.EnabledThickness);
            Bind(chkLinetype, qqc => qqc.EnabledLinetype);
        }

        public override QQConfigurationFile Settings { get; }

        private void QQForm_Load(object sender, EventArgs e) {
            //cmbLayers
            cmbLayers.DisplayMember = "Name";
            cmbLayers.ValueMember = "Name";
            var data = Quick.Layers;
            cmbLayers.DataSource = data;
            var s = new AutoCompleteStringCollection();
            s.AddRange(data.Select(l => l?.Name).ToArray());
            cmbLayers.SelectedIndexChanged += (_, args) => {
                var layer = cmbLayers.SelectedItem as LayerTableRecord;
                if (layer == null)
                    return;
                var lc = layer.Color;
                clrCustom.Color = Color.FromArgb(lc.Red, lc.Green, lc.Blue);
                lblSpecificColor.Text = lc.ColorNameForDisplay;
            };

            //cmbLinetypes
            cmbLinetypes.DisplayMember = "Name";
            cmbLinetypes.ValueMember = "Name";
            var ddata = Quick.Linetypes;
            cmbLinetypes.DataSource = ddata;
            s = new AutoCompleteStringCollection();
            s.AddRange(ddata.Select(l => l?.Name).ToArray());

            LoadSettings();
        }

        private void cmbLayers_TextChanged(object sender, EventArgs e) { }

        private void label1_Click(object sender, EventArgs e) { }

        private void clrCustom_Click(object sender, EventArgs e) {
            var dlg = new ColorDialog() {IncludeByBlockByLayer = true, Color = Autodesk.AutoCAD.Colors.Color.FromRgb(clrCustom.Color.R, clrCustom.Color.G, clrCustom.Color.B)};
            if (dlg.ShowDialog() == DialogResult.OK) {
                clrChange(dlg.Color);
            }
        }

        private bool isbylayer => clrCustom.Tag.Equals("BYLAYER");
        private bool isbyblock => clrCustom.Tag.Equals("BYBLOCK");

        private void clrChange(Autodesk.AutoCAD.Colors.Color clr) {
            if (!clr.IsByAci) {
                if (clr.IsByLayer) {
                    clrCustom.Color = Color.Transparent;
                    clrCustom.Tag = "BYLAYER";
                    lblSpecificColor.Text = "By Layer";
                } else if (clr.IsByBlock) {
                    clrCustom.Color = Color.Transparent;
                    lblSpecificColor.Text = "By Block";
                    clrCustom.Tag = "BYBLOCK";
                } else {
                    var lc = clr;
                    clrCustom.Color = Color.FromArgb(lc.Red, lc.Green, lc.Blue);
                    clrCustom.Tag = null;
                    lblSpecificColor.Text = "";
                }
            } else {
                var colIndex = clr.ColorIndex;
                var byt = Convert.ToByte(colIndex);

                var rgb = EntityColor.LookUpRgb(byt);
                var b = rgb & 0xffL;
                var g = (rgb & 0xff00L) >> 8;
                long r = rgb >> 16;

                clrCustom.Color = Color.FromArgb((int) r, (int) g, (int) b);
                clrCustom.Tag = null;
                lblSpecificColor.Text = "";
            }
        }


        private void chkLayer_CheckedChanged(object sender, EventArgs e) {
            if (chkLayer.Checked == false)
                cmbLayers.SelectedIndex = -1;
            else if (cmbLayers.Items.Count > 0) {
                cmbLayers.SelectedIndex = 0;
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e) {
            if (chkLinetype.Checked == false)
                cmbLinetypes.SelectedIndex = -1;
            else if (cmbLinetypes.Items.Count > 0) {
                cmbLinetypes.SelectedIndex = 0;
            }
        }

        #region ToolStrip

        private void revertToolStripMenuItem_Click(object sender, EventArgs e) { }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e) { }

        //Load FromFile
        private void fromFileToolStripMenuItem_Click(object sender, EventArgs e) { }


        //Load FromObject
        private void fromObjectToolStripMenuItem_Click(object sender, EventArgs e) {
            Quick.ClearSelected();
            var q = Quick.SelectSingle();
            if (q == null)
                return;
        }

        //Save As
        private void saveAsToolStripMenuItem1_Click(object sender, EventArgs e) {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog {
                RestoreDirectory = true,
                EnsurePathExists = true,
                Title = "Select location to save the config file:",
                ShowPlacesList = true,
                Multiselect = false,
                ShowHiddenItems = true,

                Filters = {new CommonFileDialogFilter("*.qqc", "*.qqc"), new CommonFileDialogFilter("All Files", "*")},
                NavigateToShortcut = true
            };
            if (dialog.ShowDialog() != CommonFileDialogResult.Ok)
                return;

            Save(dialog.FileName);
        }

        //Revert Changes
        private void revertChangesToolStripMenuItem_Click(object sender, EventArgs e) {
            RevertChanges();
        }

        
        private void saveToolStripMenuItem1_Click(object sender, EventArgs e) {
        }

        #endregion

        //Save and Send
        private void saveAndSendToolStripMenuItem_Click(object sender, EventArgs e) {
            //Save As First
            var path = Settings.Path;
            if (string.IsNullOrEmpty(path?.Trim()))
            {
                MessageBox.Show("This config is new and was never saved before.");
                return;
            }
            Save(path);
            var file = Path.Combine(Path.GetTempPath(), "{Name}.qqp");
            try {
                File.Copy(path, file);
            } catch {
                return;
            }
            try {
                //prepare email
                Microsoft.Office.Interop.Outlook.Application oApp = new Microsoft.Office.Interop.Outlook.Application();
                Microsoft.Office.Interop.Outlook.MailItem oMsg = (Microsoft.Office.Interop.Outlook.MailItem) oApp.CreateItem(Microsoft.Office.Interop.Outlook.OlItemType.olMailItem);

                oMsg.Subject = "QQPaste - {Name}";
                Paths.MarkForDeletion(file);
                oMsg.Attachments.Add(file, Microsoft.Office.Interop.Outlook.OlAttachmentType.olByValue, Type.Missing, Type.Missing);
                oMsg.Display(false); //In order to display it in modal inspector change the argument to true
            } catch (System.Exception ee) {
                Console.WriteLine(ee);
                Debug.WriteLine(ee);
                Quick.Write(" " + ee);
            }
        }

        //close without saving
        private void closeToolStripMenuItem_Click(object sender, EventArgs e) {
            if (MessageBox.Show("Are you sure you want to exit without saving?", "", MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk) == DialogResult.Yes)
                Close();
        }

        private void closeToolStripMenuItem1_Click(object sender, EventArgs e) {
            var path = txtPath.Text;
            if (string.IsNullOrEmpty(path?.Trim())) {
                MessageBox.Show("This config is new and was never saved before.");
                return;
            }
            Save(path);
            Close();
        }

        //Save to CurrentLocation
        private void currentFileToolStripMenuItem_Click(object sender, EventArgs e) {
            var path = txtPath.Text;
            if (string.IsNullOrEmpty(path?.Trim())) {
                MessageBox.Show("This config is new and was never saved before.");
                return;
            }
            Save(path);
        }

        //Save other location
        private void otherLocationToolStripMenuItem_Click(object sender, EventArgs e) {
            var file = FileDialogs.SaveFile(QQConfigurationFile.Filter);
            if (file == null)
                return;
            try { if (string.IsNullOrEmpty(Settings.Path) == false && File.Exists(Settings.Path)) File.Delete(Settings.Path);} catch {}
            file.Directory.EnsureCreated();
            txtPath.Text = Settings.Path = file.FullName;
            Save(file.FullName);
        }

        private void btnBrowse_Click(object sender, EventArgs e) {
            var f = FileDialogs.GetFile(QQConfigurationFile.Filter, false, initialdirectory: txtPath.Text);
            if (f == null)
                return;
            txtPath.Text = f.FullName;
        }

        private void tsSaveToSources_Click(object sender, EventArgs e)
        {

        }
    }
}*/