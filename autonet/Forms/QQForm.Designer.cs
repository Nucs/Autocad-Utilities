namespace autonet
{
    partial class QQForm
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
            this.lstSources = new System.Windows.Forms.ListBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sourcesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txtPath = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtConfigName = new System.Windows.Forms.TextBox();
            this.grpPolylines = new System.Windows.Forms.GroupBox();
            this.chkConvertAllToPolylines = new System.Windows.Forms.CheckBox();
            this.chkJoinPolylines = new System.Windows.Forms.CheckBox();
            this.label7 = new System.Windows.Forms.Label();
            this.txtThickness = new System.Windows.Forms.TextBox();
            this.chkThickness = new System.Windows.Forms.CheckBox();
            this.chkLinetype = new System.Windows.Forms.CheckBox();
            this.label6 = new System.Windows.Forms.Label();
            this.cmbLinetypes = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtWidth = new System.Windows.Forms.TextBox();
            this.chkWidth = new System.Windows.Forms.CheckBox();
            this.chkLayer = new System.Windows.Forms.CheckBox();
            this.chkColor = new System.Windows.Forms.CheckBox();
            this.lblSpecificColor = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.cmbLayers = new System.Windows.Forms.ComboBox();
            this.clrCustom = new AdamsLair.WinForms.ColorControls.ColorShowBox();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.chkHidePath = new System.Windows.Forms.CheckBox();
            this.btnEdit = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.newToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fileToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.directoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.staticDirectoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteSelectedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.grpPolylines.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // lstSources
            // 
            this.lstSources.FormattingEnabled = true;
            this.lstSources.Location = new System.Drawing.Point(12, 27);
            this.lstSources.Name = "lstSources";
            this.lstSources.ScrollAlwaysVisible = true;
            this.lstSources.Size = new System.Drawing.Size(431, 394);
            this.lstSources.TabIndex = 0;
            this.lstSources.SelectedIndexChanged += new System.EventHandler(this.lstSources_SelectedIndexChanged);
            // 
            // menuStrip1
            // 
            this.menuStrip1.BackColor = System.Drawing.Color.Transparent;
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.sourcesToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(863, 24);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // sourcesToolStripMenuItem
            // 
            this.sourcesToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addToolStripMenuItem,
            this.deleteSelectedToolStripMenuItem});
            this.sourcesToolStripMenuItem.Name = "sourcesToolStripMenuItem";
            this.sourcesToolStripMenuItem.Size = new System.Drawing.Size(60, 20);
            this.sourcesToolStripMenuItem.Text = "Sources";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.txtPath);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.txtConfigName);
            this.groupBox1.Controls.Add(this.grpPolylines);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.pictureBox1);
            this.groupBox1.Controls.Add(this.txtThickness);
            this.groupBox1.Controls.Add(this.chkThickness);
            this.groupBox1.Controls.Add(this.chkLinetype);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.cmbLinetypes);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.txtWidth);
            this.groupBox1.Controls.Add(this.chkWidth);
            this.groupBox1.Controls.Add(this.chkLayer);
            this.groupBox1.Controls.Add(this.chkColor);
            this.groupBox1.Controls.Add(this.lblSpecificColor);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.cmbLayers);
            this.groupBox1.Controls.Add(this.clrCustom);
            this.groupBox1.Location = new System.Drawing.Point(449, 27);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(402, 394);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Details";
            this.groupBox1.Enter += new System.EventHandler(this.groupBox1_Enter);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(29, 70);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(29, 13);
            this.label5.TabIndex = 65;
            this.label5.Text = "Path";
            // 
            // txtPath
            // 
            this.txtPath.Location = new System.Drawing.Point(29, 87);
            this.txtPath.Name = "txtPath";
            this.txtPath.ReadOnly = true;
            this.txtPath.Size = new System.Drawing.Size(362, 20);
            this.txtPath.TabIndex = 64;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(29, 28);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(100, 13);
            this.label4.TabIndex = 62;
            this.label4.Text = "Configuration Name";
            // 
            // txtConfigName
            // 
            this.txtConfigName.Enabled = false;
            this.txtConfigName.Location = new System.Drawing.Point(29, 45);
            this.txtConfigName.Name = "txtConfigName";
            this.txtConfigName.Size = new System.Drawing.Size(362, 20);
            this.txtConfigName.TabIndex = 61;
            // 
            // grpPolylines
            // 
            this.grpPolylines.Controls.Add(this.chkConvertAllToPolylines);
            this.grpPolylines.Controls.Add(this.chkJoinPolylines);
            this.grpPolylines.Location = new System.Drawing.Point(5, 308);
            this.grpPolylines.Name = "grpPolylines";
            this.grpPolylines.Size = new System.Drawing.Size(383, 68);
            this.grpPolylines.TabIndex = 59;
            this.grpPolylines.TabStop = false;
            this.grpPolylines.Text = "Polylines";
            // 
            // chkConvertAllToPolylines
            // 
            this.chkConvertAllToPolylines.AutoSize = true;
            this.chkConvertAllToPolylines.Enabled = false;
            this.chkConvertAllToPolylines.Location = new System.Drawing.Point(6, 19);
            this.chkConvertAllToPolylines.Name = "chkConvertAllToPolylines";
            this.chkConvertAllToPolylines.Size = new System.Drawing.Size(216, 17);
            this.chkConvertAllToPolylines.TabIndex = 36;
            this.chkConvertAllToPolylines.Text = "Attempt to convert all objects to polyline.";
            this.chkConvertAllToPolylines.UseVisualStyleBackColor = true;
            // 
            // chkJoinPolylines
            // 
            this.chkJoinPolylines.AutoSize = true;
            this.chkJoinPolylines.Enabled = false;
            this.chkJoinPolylines.Location = new System.Drawing.Point(6, 42);
            this.chkJoinPolylines.Name = "chkJoinPolylines";
            this.chkJoinPolylines.Size = new System.Drawing.Size(139, 17);
            this.chkJoinPolylines.TabIndex = 37;
            this.chkJoinPolylines.Text = "Attempt to join polylines.";
            this.chkJoinPolylines.UseVisualStyleBackColor = true;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(154, 208);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(56, 13);
            this.label7.TabIndex = 58;
            this.label7.Text = "Thickness";
            // 
            // txtThickness
            // 
            this.txtThickness.Enabled = false;
            this.txtThickness.Location = new System.Drawing.Point(154, 225);
            this.txtThickness.Name = "txtThickness";
            this.txtThickness.Size = new System.Drawing.Size(100, 20);
            this.txtThickness.TabIndex = 57;
            // 
            // chkThickness
            // 
            this.chkThickness.AutoSize = true;
            this.chkThickness.Enabled = false;
            this.chkThickness.Location = new System.Drawing.Point(133, 228);
            this.chkThickness.Name = "chkThickness";
            this.chkThickness.Size = new System.Drawing.Size(15, 14);
            this.chkThickness.TabIndex = 56;
            this.chkThickness.UseVisualStyleBackColor = true;
            // 
            // chkLinetype
            // 
            this.chkLinetype.AutoSize = true;
            this.chkLinetype.Enabled = false;
            this.chkLinetype.Location = new System.Drawing.Point(6, 276);
            this.chkLinetype.Name = "chkLinetype";
            this.chkLinetype.Size = new System.Drawing.Size(15, 14);
            this.chkLinetype.TabIndex = 55;
            this.chkLinetype.UseVisualStyleBackColor = true;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(26, 256);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(47, 13);
            this.label6.TabIndex = 54;
            this.label6.Text = "Linetype";
            // 
            // cmbLinetypes
            // 
            this.cmbLinetypes.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.cmbLinetypes.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.cmbLinetypes.Enabled = false;
            this.cmbLinetypes.Location = new System.Drawing.Point(26, 273);
            this.cmbLinetypes.Name = "cmbLinetypes";
            this.cmbLinetypes.Size = new System.Drawing.Size(362, 21);
            this.cmbLinetypes.TabIndex = 53;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(26, 208);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(35, 13);
            this.label2.TabIndex = 52;
            this.label2.Text = "Width";
            // 
            // txtWidth
            // 
            this.txtWidth.Enabled = false;
            this.txtWidth.Location = new System.Drawing.Point(26, 225);
            this.txtWidth.Name = "txtWidth";
            this.txtWidth.Size = new System.Drawing.Size(100, 20);
            this.txtWidth.TabIndex = 51;
            // 
            // chkWidth
            // 
            this.chkWidth.AutoSize = true;
            this.chkWidth.Enabled = false;
            this.chkWidth.Location = new System.Drawing.Point(5, 228);
            this.chkWidth.Name = "chkWidth";
            this.chkWidth.Size = new System.Drawing.Size(15, 14);
            this.chkWidth.TabIndex = 50;
            this.chkWidth.UseVisualStyleBackColor = true;
            // 
            // chkLayer
            // 
            this.chkLayer.AutoSize = true;
            this.chkLayer.Enabled = false;
            this.chkLayer.Location = new System.Drawing.Point(6, 131);
            this.chkLayer.Name = "chkLayer";
            this.chkLayer.Size = new System.Drawing.Size(15, 14);
            this.chkLayer.TabIndex = 49;
            this.chkLayer.UseVisualStyleBackColor = true;
            // 
            // chkColor
            // 
            this.chkColor.AutoSize = true;
            this.chkColor.Enabled = false;
            this.chkColor.Location = new System.Drawing.Point(6, 181);
            this.chkColor.Name = "chkColor";
            this.chkColor.Size = new System.Drawing.Size(15, 14);
            this.chkColor.TabIndex = 48;
            this.chkColor.UseVisualStyleBackColor = true;
            // 
            // lblSpecificColor
            // 
            this.lblSpecificColor.AutoSize = true;
            this.lblSpecificColor.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.lblSpecificColor.Location = new System.Drawing.Point(96, 182);
            this.lblSpecificColor.Name = "lblSpecificColor";
            this.lblSpecificColor.Size = new System.Drawing.Size(110, 13);
            this.lblSpecificColor.TabIndex = 47;
            this.lblSpecificColor.Text = "BYBLOCK/BYLAYER";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(26, 158);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(72, 13);
            this.label3.TabIndex = 46;
            this.label3.Text = "Specific Color";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(26, 111);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(33, 13);
            this.label1.TabIndex = 45;
            this.label1.Text = "Layer";
            // 
            // cmbLayers
            // 
            this.cmbLayers.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.cmbLayers.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.cmbLayers.Enabled = false;
            this.cmbLayers.Location = new System.Drawing.Point(26, 128);
            this.cmbLayers.Name = "cmbLayers";
            this.cmbLayers.Size = new System.Drawing.Size(362, 21);
            this.cmbLayers.TabIndex = 44;
            // 
            // clrCustom
            // 
            this.clrCustom.Color = System.Drawing.Color.Transparent;
            this.clrCustom.Location = new System.Drawing.Point(26, 177);
            this.clrCustom.LowerColor = System.Drawing.Color.Transparent;
            this.clrCustom.Name = "clrCustom";
            this.clrCustom.Size = new System.Drawing.Size(65, 24);
            this.clrCustom.TabIndex = 43;
            this.clrCustom.Text = "Line/Polygon Color";
            this.clrCustom.UpperColor = System.Drawing.Color.Transparent;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(12, 450);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 3;
            this.button1.Text = "Single";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(93, 450);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 4;
            this.button2.Text = "Multiple";
            this.button2.UseVisualStyleBackColor = true;
            // 
            // chkHidePath
            // 
            this.chkHidePath.AutoSize = true;
            this.chkHidePath.Location = new System.Drawing.Point(12, 427);
            this.chkHidePath.Name = "chkHidePath";
            this.chkHidePath.Size = new System.Drawing.Size(73, 17);
            this.chkHidePath.TabIndex = 5;
            this.chkHidePath.Text = "Hide Path";
            this.chkHidePath.UseVisualStyleBackColor = true;
            this.chkHidePath.CheckedChanged += new System.EventHandler(this.chkHidePath_CheckedChanged);
            // 
            // btnEdit
            // 
            this.btnEdit.Location = new System.Drawing.Point(454, 427);
            this.btnEdit.Name = "btnEdit";
            this.btnEdit.Size = new System.Drawing.Size(75, 23);
            this.btnEdit.TabIndex = 63;
            this.btnEdit.Text = "Edit";
            this.btnEdit.UseVisualStyleBackColor = true;
            this.btnEdit.Click += new System.EventHandler(this.btnEdit_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.pictureBox1.Image = global::autonet.Properties.Resources.fries;
            this.pictureBox1.Location = new System.Drawing.Point(301, 165);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(87, 86);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 60;
            this.pictureBox1.TabStop = false;
            // 
            // newToolStripMenuItem
            // 
            this.newToolStripMenuItem.Image = global::autonet.Properties.Resources._new;
            this.newToolStripMenuItem.Name = "newToolStripMenuItem";
            this.newToolStripMenuItem.Size = new System.Drawing.Size(137, 22);
            this.newToolStripMenuItem.Text = "New Config";
            this.newToolStripMenuItem.Click += new System.EventHandler(this.newToolStripMenuItem_Click);
            // 
            // addToolStripMenuItem
            // 
            this.addToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem1,
            this.directoryToolStripMenuItem,
            this.staticDirectoryToolStripMenuItem});
            this.addToolStripMenuItem.Image = global::autonet.Properties.Resources.plus_circle;
            this.addToolStripMenuItem.Name = "addToolStripMenuItem";
            this.addToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.addToolStripMenuItem.Text = "Add";
            this.addToolStripMenuItem.Click += new System.EventHandler(this.addToolStripMenuItem_Click);
            // 
            // fileToolStripMenuItem1
            // 
            this.fileToolStripMenuItem1.Image = global::autonet.Properties.Resources._new;
            this.fileToolStripMenuItem1.Name = "fileToolStripMenuItem1";
            this.fileToolStripMenuItem1.Size = new System.Drawing.Size(154, 22);
            this.fileToolStripMenuItem1.Text = "File";
            this.fileToolStripMenuItem1.Click += new System.EventHandler(this.fileToolStripMenuItem1_Click);
            // 
            // directoryToolStripMenuItem
            // 
            this.directoryToolStripMenuItem.Image = global::autonet.Properties.Resources.add;
            this.directoryToolStripMenuItem.Name = "directoryToolStripMenuItem";
            this.directoryToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
            this.directoryToolStripMenuItem.Text = "Directory";
            this.directoryToolStripMenuItem.Click += new System.EventHandler(this.directoryToolStripMenuItem_Click);
            // 
            // staticDirectoryToolStripMenuItem
            // 
            this.staticDirectoryToolStripMenuItem.Image = global::autonet.Properties.Resources.add;
            this.staticDirectoryToolStripMenuItem.Name = "staticDirectoryToolStripMenuItem";
            this.staticDirectoryToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
            this.staticDirectoryToolStripMenuItem.Text = "Static Directory";
            this.staticDirectoryToolStripMenuItem.Click += new System.EventHandler(this.staticDirectoryToolStripMenuItem_Click);
            // 
            // deleteSelectedToolStripMenuItem
            // 
            this.deleteSelectedToolStripMenuItem.Image = global::autonet.Properties.Resources.edit;
            this.deleteSelectedToolStripMenuItem.Name = "deleteSelectedToolStripMenuItem";
            this.deleteSelectedToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.deleteSelectedToolStripMenuItem.Text = "Edit Sources";
            this.deleteSelectedToolStripMenuItem.Click += new System.EventHandler(this.deleteSelectedToolStripMenuItem_Click);
            // 
            // QQForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(863, 489);
            this.Controls.Add(this.btnEdit);
            this.Controls.Add(this.chkHidePath);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.lstSources);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "QQForm";
            this.Text = "QQ";
            this.Load += new System.EventHandler(this.QQForm_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.grpPolylines.ResumeLayout(false);
            this.grpPolylines.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox lstSources;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem sourcesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addToolStripMenuItem;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtConfigName;
        private System.Windows.Forms.GroupBox grpPolylines;
        private System.Windows.Forms.CheckBox chkConvertAllToPolylines;
        private System.Windows.Forms.CheckBox chkJoinPolylines;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.TextBox txtThickness;
        private System.Windows.Forms.CheckBox chkThickness;
        private System.Windows.Forms.CheckBox chkLinetype;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox cmbLinetypes;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtWidth;
        private System.Windows.Forms.CheckBox chkWidth;
        private System.Windows.Forms.CheckBox chkLayer;
        private System.Windows.Forms.CheckBox chkColor;
        private System.Windows.Forms.Label lblSpecificColor;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cmbLayers;
        private AdamsLair.WinForms.ColorControls.ColorShowBox clrCustom;
        private System.Windows.Forms.CheckBox chkHidePath;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem directoryToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem staticDirectoryToolStripMenuItem;
        private System.Windows.Forms.Button btnEdit;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtPath;
        private System.Windows.Forms.ToolStripMenuItem deleteSelectedToolStripMenuItem;
    }
}