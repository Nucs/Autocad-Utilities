using autonet.Settings;

namespace autonet.Forms
{
    partial class QQConfigForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.clrPaint = new System.Windows.Forms.ColorDialog();
            this.clrCustom = new AdamsLair.WinForms.ColorControls.ColorShowBox();
            this.cmbLayers = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.lblSpecificColor = new System.Windows.Forms.Label();
            this.chkColor = new System.Windows.Forms.CheckBox();
            this.chkLayer = new System.Windows.Forms.CheckBox();
            this.chkWidth = new System.Windows.Forms.CheckBox();
            this.txtWidth = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.chkLinetype = new System.Windows.Forms.CheckBox();
            this.label6 = new System.Windows.Forms.Label();
            this.cmbLinetypes = new System.Windows.Forms.ComboBox();
            this.label7 = new System.Windows.Forms.Label();
            this.txtThickness = new System.Windows.Forms.TextBox();
            this.chkThickness = new System.Windows.Forms.CheckBox();
            this.chkConvertToPolylines = new System.Windows.Forms.CheckBox();
            this.chkJoinPolylines = new System.Windows.Forms.CheckBox();
            this.grpPolylines = new System.Windows.Forms.GroupBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.loadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fromFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fromObjectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.saveToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.currentFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.otherLocationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAndSendToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.revertChangesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.closeToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.closeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.revertToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tsLoad = new System.Windows.Forms.ToolStripMenuItem();
            this.tsLoadFIle = new System.Windows.Forms.ToolStripMenuItem();
            this.tsLoadObject = new System.Windows.Forms.ToolStripMenuItem();
            this.txtConfigName = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtPath = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.tsSaveToSources = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.grpPolylines.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // clrCustom
            // 
            this.clrCustom.Color = System.Drawing.Color.Transparent;
            this.clrCustom.Location = new System.Drawing.Point(30, 179);
            this.clrCustom.LowerColor = System.Drawing.Color.Transparent;
            this.clrCustom.Name = "clrCustom";
            this.clrCustom.Size = new System.Drawing.Size(65, 24);
            this.clrCustom.TabIndex = 8;
            this.clrCustom.Text = "Line/Polygon Color";
            this.clrCustom.UpperColor = System.Drawing.Color.Transparent;
            this.clrCustom.Click += new System.EventHandler(this.clrCustom_Click);
            // 
            // cmbLayers
            // 
            this.cmbLayers.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.cmbLayers.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.cmbLayers.Location = new System.Drawing.Point(30, 130);
            this.cmbLayers.Name = "cmbLayers";
            this.cmbLayers.Size = new System.Drawing.Size(362, 21);
            this.cmbLayers.TabIndex = 10;
            this.cmbLayers.TextChanged += new System.EventHandler(this.cmbLayers_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(30, 113);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(33, 13);
            this.label1.TabIndex = 11;
            this.label1.Text = "Layer";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(30, 160);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(72, 13);
            this.label3.TabIndex = 15;
            this.label3.Text = "Specific Color";
            // 
            // lblSpecificColor
            // 
            this.lblSpecificColor.AutoSize = true;
            this.lblSpecificColor.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.lblSpecificColor.Location = new System.Drawing.Point(100, 184);
            this.lblSpecificColor.Name = "lblSpecificColor";
            this.lblSpecificColor.Size = new System.Drawing.Size(110, 13);
            this.lblSpecificColor.TabIndex = 22;
            this.lblSpecificColor.Text = "BYBLOCK/BYLAYER";
            // 
            // chkColor
            // 
            this.chkColor.AutoSize = true;
            this.chkColor.Location = new System.Drawing.Point(10, 183);
            this.chkColor.Name = "chkColor";
            this.chkColor.Size = new System.Drawing.Size(15, 14);
            this.chkColor.TabIndex = 23;
            this.chkColor.UseVisualStyleBackColor = true;
            // 
            // chkLayer
            // 
            this.chkLayer.AutoSize = true;
            this.chkLayer.Location = new System.Drawing.Point(10, 133);
            this.chkLayer.Name = "chkLayer";
            this.chkLayer.Size = new System.Drawing.Size(15, 14);
            this.chkLayer.TabIndex = 24;
            this.chkLayer.UseVisualStyleBackColor = true;
            this.chkLayer.CheckedChanged += new System.EventHandler(this.chkLayer_CheckedChanged);
            // 
            // chkWidth
            // 
            this.chkWidth.AutoSize = true;
            this.chkWidth.Location = new System.Drawing.Point(9, 230);
            this.chkWidth.Name = "chkWidth";
            this.chkWidth.Size = new System.Drawing.Size(15, 14);
            this.chkWidth.TabIndex = 25;
            this.chkWidth.UseVisualStyleBackColor = true;
            // 
            // txtWidth
            // 
            this.txtWidth.Location = new System.Drawing.Point(30, 227);
            this.txtWidth.Name = "txtWidth";
            this.txtWidth.Size = new System.Drawing.Size(100, 20);
            this.txtWidth.TabIndex = 26;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(30, 210);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(35, 13);
            this.label2.TabIndex = 27;
            this.label2.Text = "Width";
            // 
            // chkLinetype
            // 
            this.chkLinetype.AutoSize = true;
            this.chkLinetype.Location = new System.Drawing.Point(10, 278);
            this.chkLinetype.Name = "chkLinetype";
            this.chkLinetype.Size = new System.Drawing.Size(15, 14);
            this.chkLinetype.TabIndex = 32;
            this.chkLinetype.UseVisualStyleBackColor = true;
            this.chkLinetype.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(30, 258);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(47, 13);
            this.label6.TabIndex = 31;
            this.label6.Text = "Linetype";
            // 
            // cmbLinetypes
            // 
            this.cmbLinetypes.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.cmbLinetypes.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.cmbLinetypes.Location = new System.Drawing.Point(30, 275);
            this.cmbLinetypes.Name = "cmbLinetypes";
            this.cmbLinetypes.Size = new System.Drawing.Size(362, 21);
            this.cmbLinetypes.TabIndex = 30;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(158, 210);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(56, 13);
            this.label7.TabIndex = 35;
            this.label7.Text = "Thickness";
            // 
            // txtThickness
            // 
            this.txtThickness.Location = new System.Drawing.Point(158, 227);
            this.txtThickness.Name = "txtThickness";
            this.txtThickness.Size = new System.Drawing.Size(100, 20);
            this.txtThickness.TabIndex = 34;
            // 
            // chkThickness
            // 
            this.chkThickness.AutoSize = true;
            this.chkThickness.Location = new System.Drawing.Point(137, 230);
            this.chkThickness.Name = "chkThickness";
            this.chkThickness.Size = new System.Drawing.Size(15, 14);
            this.chkThickness.TabIndex = 33;
            this.chkThickness.UseVisualStyleBackColor = true;
            // 
            // chkConvertToPolylines
            // 
            this.chkConvertToPolylines.AutoSize = true;
            this.chkConvertToPolylines.Location = new System.Drawing.Point(6, 19);
            this.chkConvertToPolylines.Name = "chkConvertToPolylines";
            this.chkConvertToPolylines.Size = new System.Drawing.Size(216, 17);
            this.chkConvertToPolylines.TabIndex = 36;
            this.chkConvertToPolylines.Text = "Attempt to convert all objects to polyline.";
            this.chkConvertToPolylines.UseVisualStyleBackColor = true;
            // 
            // chkJoinPolylines
            // 
            this.chkJoinPolylines.AutoSize = true;
            this.chkJoinPolylines.Location = new System.Drawing.Point(6, 42);
            this.chkJoinPolylines.Name = "chkJoinPolylines";
            this.chkJoinPolylines.Size = new System.Drawing.Size(139, 17);
            this.chkJoinPolylines.TabIndex = 37;
            this.chkJoinPolylines.Text = "Attempt to join polylines.";
            this.chkJoinPolylines.UseVisualStyleBackColor = true;
            // 
            // grpPolylines
            // 
            this.grpPolylines.Controls.Add(this.chkConvertToPolylines);
            this.grpPolylines.Controls.Add(this.chkJoinPolylines);
            this.grpPolylines.Location = new System.Drawing.Point(9, 310);
            this.grpPolylines.Name = "grpPolylines";
            this.grpPolylines.Size = new System.Drawing.Size(383, 68);
            this.grpPolylines.TabIndex = 38;
            this.grpPolylines.TabStop = false;
            this.grpPolylines.Text = "Polylines";
            // 
            // menuStrip1
            // 
            this.menuStrip1.BackColor = System.Drawing.Color.Transparent;
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem1});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(410, 24);
            this.menuStrip1.TabIndex = 39;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem1
            // 
            this.fileToolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loadToolStripMenuItem,
            this.toolStripSeparator2,
            this.saveToolStripMenuItem1,
            this.saveAndSendToolStripMenuItem,
            this.revertChangesToolStripMenuItem,
            this.toolStripSeparator1,
            this.closeToolStripMenuItem1,
            this.closeToolStripMenuItem,
            this.toolStripSeparator3,
            this.tsSaveToSources});
            this.fileToolStripMenuItem1.Name = "fileToolStripMenuItem1";
            this.fileToolStripMenuItem1.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem1.Text = "File";
            // 
            // loadToolStripMenuItem
            // 
            this.loadToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fromFileToolStripMenuItem,
            this.fromObjectToolStripMenuItem});
            this.loadToolStripMenuItem.Image = global::autonet.Properties.Resources.close;
            this.loadToolStripMenuItem.ImageAlign = System.Drawing.ContentAlignment.BottomLeft;
            this.loadToolStripMenuItem.Name = "loadToolStripMenuItem";
            this.loadToolStripMenuItem.Size = new System.Drawing.Size(187, 22);
            this.loadToolStripMenuItem.Text = "Load";
            this.loadToolStripMenuItem.Click += new System.EventHandler(this.loadToolStripMenuItem_Click);
            // 
            // fromFileToolStripMenuItem
            // 
            this.fromFileToolStripMenuItem.Name = "fromFileToolStripMenuItem";
            this.fromFileToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.fromFileToolStripMenuItem.Text = "From File";
            this.fromFileToolStripMenuItem.Click += new System.EventHandler(this.fromFileToolStripMenuItem_Click);
            // 
            // fromObjectToolStripMenuItem
            // 
            this.fromObjectToolStripMenuItem.Name = "fromObjectToolStripMenuItem";
            this.fromObjectToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.fromObjectToolStripMenuItem.Text = "From Object";
            this.fromObjectToolStripMenuItem.Click += new System.EventHandler(this.fromObjectToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(184, 6);
            // 
            // saveToolStripMenuItem1
            // 
            this.saveToolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.currentFileToolStripMenuItem,
            this.otherLocationToolStripMenuItem});
            this.saveToolStripMenuItem1.Image = global::autonet.Properties.Resources.save;
            this.saveToolStripMenuItem1.Name = "saveToolStripMenuItem1";
            this.saveToolStripMenuItem1.Size = new System.Drawing.Size(187, 22);
            this.saveToolStripMenuItem1.Text = "Save";
            this.saveToolStripMenuItem1.Click += new System.EventHandler(this.saveToolStripMenuItem1_Click);
            // 
            // currentFileToolStripMenuItem
            // 
            this.currentFileToolStripMenuItem.Name = "currentFileToolStripMenuItem";
            this.currentFileToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this.currentFileToolStripMenuItem.Text = "Current Location";
            this.currentFileToolStripMenuItem.Click += new System.EventHandler(this.currentFileToolStripMenuItem_Click);
            // 
            // otherLocationToolStripMenuItem
            // 
            this.otherLocationToolStripMenuItem.Name = "otherLocationToolStripMenuItem";
            this.otherLocationToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this.otherLocationToolStripMenuItem.Text = "Move To Other Location";
            this.otherLocationToolStripMenuItem.Click += new System.EventHandler(this.otherLocationToolStripMenuItem_Click);
            // 
            // saveAndSendToolStripMenuItem
            // 
            this.saveAndSendToolStripMenuItem.Image = global::autonet.Properties.Resources.email;
            this.saveAndSendToolStripMenuItem.ImageAlign = System.Drawing.ContentAlignment.BottomLeft;
            this.saveAndSendToolStripMenuItem.Name = "saveAndSendToolStripMenuItem";
            this.saveAndSendToolStripMenuItem.Size = new System.Drawing.Size(187, 22);
            this.saveAndSendToolStripMenuItem.Text = "Save And Send";
            this.saveAndSendToolStripMenuItem.Click += new System.EventHandler(this.saveAndSendToolStripMenuItem_Click);
            // 
            // revertChangesToolStripMenuItem
            // 
            this.revertChangesToolStripMenuItem.Image = global::autonet.Properties.Resources.undo;
            this.revertChangesToolStripMenuItem.Name = "revertChangesToolStripMenuItem";
            this.revertChangesToolStripMenuItem.Size = new System.Drawing.Size(187, 22);
            this.revertChangesToolStripMenuItem.Text = "Revert Changes";
            this.revertChangesToolStripMenuItem.Click += new System.EventHandler(this.revertChangesToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(184, 6);
            // 
            // closeToolStripMenuItem1
            // 
            this.closeToolStripMenuItem1.Image = global::autonet.Properties.Resources.save;
            this.closeToolStripMenuItem1.Name = "closeToolStripMenuItem1";
            this.closeToolStripMenuItem1.Size = new System.Drawing.Size(187, 22);
            this.closeToolStripMenuItem1.Text = "Save And Close";
            this.closeToolStripMenuItem1.Click += new System.EventHandler(this.closeToolStripMenuItem1_Click);
            // 
            // closeToolStripMenuItem
            // 
            this.closeToolStripMenuItem.Image = global::autonet.Properties.Resources.X;
            this.closeToolStripMenuItem.Name = "closeToolStripMenuItem";
            this.closeToolStripMenuItem.Size = new System.Drawing.Size(187, 22);
            this.closeToolStripMenuItem.Text = "Close Without Saving";
            this.closeToolStripMenuItem.Click += new System.EventHandler(this.closeToolStripMenuItem_Click);
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(156, 22);
            this.saveToolStripMenuItem.Text = "Save";
            // 
            // saveAsToolStripMenuItem
            // 
            this.saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
            this.saveAsToolStripMenuItem.Size = new System.Drawing.Size(156, 22);
            this.saveAsToolStripMenuItem.Text = "Save As";
            // 
            // revertToolStripMenuItem
            // 
            this.revertToolStripMenuItem.Name = "revertToolStripMenuItem";
            this.revertToolStripMenuItem.Size = new System.Drawing.Size(156, 22);
            this.revertToolStripMenuItem.Text = "Revert Changes";
            this.revertToolStripMenuItem.Click += new System.EventHandler(this.revertToolStripMenuItem_Click);
            // 
            // tsLoad
            // 
            this.tsLoad.Name = "tsLoad";
            this.tsLoad.Size = new System.Drawing.Size(156, 22);
            this.tsLoad.Text = "Load";
            // 
            // tsLoadFIle
            // 
            this.tsLoadFIle.Name = "tsLoadFIle";
            this.tsLoadFIle.Size = new System.Drawing.Size(152, 22);
            this.tsLoadFIle.Text = "From File";
            // 
            // tsLoadObject
            // 
            this.tsLoadObject.Name = "tsLoadObject";
            this.tsLoadObject.Size = new System.Drawing.Size(152, 22);
            this.tsLoadObject.Text = "From Object";
            // 
            // txtConfigName
            // 
            this.txtConfigName.Location = new System.Drawing.Point(30, 48);
            this.txtConfigName.Name = "txtConfigName";
            this.txtConfigName.Size = new System.Drawing.Size(362, 20);
            this.txtConfigName.TabIndex = 41;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(30, 31);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(100, 13);
            this.label4.TabIndex = 42;
            this.label4.Text = "Configuration Name";
            // 
            // txtPath
            // 
            this.txtPath.Location = new System.Drawing.Point(30, 89);
            this.txtPath.Name = "txtPath";
            this.txtPath.ReadOnly = true;
            this.txtPath.Size = new System.Drawing.Size(293, 20);
            this.txtPath.TabIndex = 43;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(30, 72);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(29, 13);
            this.label5.TabIndex = 44;
            this.label5.Text = "Path";
            // 
            // btnBrowse
            // 
            this.btnBrowse.Location = new System.Drawing.Point(329, 89);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(63, 23);
            this.btnBrowse.TabIndex = 45;
            this.btnBrowse.Text = "Browse";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.pictureBox1.Image = global::autonet.Properties.Resources.fries;
            this.pictureBox1.Location = new System.Drawing.Point(305, 167);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(87, 86);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 40;
            this.pictureBox1.TabStop = false;
            // 
            // tsSaveToSources
            // 
            this.tsSaveToSources.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.tsSaveToSources.Items.AddRange(new object[] {
            "Add To Sources",
            "Don\'t Add To Sources"});
            this.tsSaveToSources.Name = "tsSaveToSources";
            this.tsSaveToSources.Size = new System.Drawing.Size(121, 23);
            this.tsSaveToSources.Click += new System.EventHandler(this.tsSaveToSources_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(184, 6);
            // 
            // QQConfigForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(410, 389);
            this.Controls.Add(this.btnBrowse);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.txtPath);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.txtConfigName);
            this.Controls.Add(this.grpPolylines);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.txtThickness);
            this.Controls.Add(this.chkThickness);
            this.Controls.Add(this.chkLinetype);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.cmbLinetypes);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtWidth);
            this.Controls.Add(this.chkWidth);
            this.Controls.Add(this.chkLayer);
            this.Controls.Add(this.chkColor);
            this.Controls.Add(this.lblSpecificColor);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cmbLayers);
            this.Controls.Add(this.clrCustom);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "QQConfigForm";
            this.Text = "QQ Configuration";
            this.Load += new System.EventHandler(this.QQForm_Load);
            this.grpPolylines.ResumeLayout(false);
            this.grpPolylines.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ColorDialog clrPaint;
        private AdamsLair.WinForms.ColorControls.ColorShowBox clrCustom;
        private System.Windows.Forms.ComboBox cmbLayers;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label lblSpecificColor;
        private System.Windows.Forms.CheckBox chkColor;
        private System.Windows.Forms.CheckBox chkLayer;
        private System.Windows.Forms.CheckBox chkWidth;
        private System.Windows.Forms.TextBox txtWidth;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox chkLinetype;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox cmbLinetypes;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox txtThickness;
        private System.Windows.Forms.CheckBox chkThickness;
        private System.Windows.Forms.CheckBox chkConvertToPolylines;
        private System.Windows.Forms.CheckBox chkJoinPolylines;
        private System.Windows.Forms.GroupBox grpPolylines;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveAsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem revertToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem tsLoad;
        private System.Windows.Forms.ToolStripMenuItem tsLoadFIle;
        private System.Windows.Forms.ToolStripMenuItem tsLoadObject;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem loadToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fromFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fromObjectToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem revertChangesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveAndSendToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem closeToolStripMenuItem;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.TextBox txtConfigName;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ToolStripMenuItem closeToolStripMenuItem1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem currentFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem otherLocationToolStripMenuItem;
        private System.Windows.Forms.TextBox txtPath;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.ToolStripComboBox tsSaveToSources;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
    }
}