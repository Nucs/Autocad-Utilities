using autonet.Settings;

namespace autonet.Forms
{
    partial class QQForm : SettingsForm<QQSettings>
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
            this.btnSave = new System.Windows.Forms.Button();
            this.btnRevert = new System.Windows.Forms.Button();
            this.PGrid = new AdamsLair.WinForms.PropertyEditing.PropertyGrid();
            this.SuspendLayout();
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(316, 42);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 23);
            this.btnSave.TabIndex = 1;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnRevert
            // 
            this.btnRevert.Location = new System.Drawing.Point(316, 13);
            this.btnRevert.Name = "btnRevert";
            this.btnRevert.Size = new System.Drawing.Size(75, 23);
            this.btnRevert.TabIndex = 2;
            this.btnRevert.Text = "Revert";
            this.btnRevert.UseVisualStyleBackColor = true;
            this.btnRevert.Click += new System.EventHandler(this.btnRevert_Click);
            // 
            // PGrid
            // 
            this.PGrid.AllowDrop = true;
            this.PGrid.AutoScroll = true;
            this.PGrid.Location = new System.Drawing.Point(13, 13);
            this.PGrid.Name = "PGrid";
            this.PGrid.ReadOnly = false;
            this.PGrid.ShowNonPublic = false;
            this.PGrid.Size = new System.Drawing.Size(297, 577);
            this.PGrid.SortEditorsByName = true;
            this.PGrid.SplitterPosition = 119;
            this.PGrid.SplitterRatio = 0.4F;
            this.PGrid.TabIndex = 3;
            // 
            // QQForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(406, 602);
            this.Controls.Add(this.PGrid);
            this.Controls.Add(this.btnRevert);
            this.Controls.Add(this.btnSave);
            this.Name = "QQForm";
            this.Text = "QQForm";
            this.Load += new System.EventHandler(this.QQForm_Load);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnRevert;
        public AdamsLair.WinForms.PropertyEditing.PropertyGrid PGrid;
    }
}