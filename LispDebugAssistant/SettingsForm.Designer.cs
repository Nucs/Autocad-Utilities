namespace LispDebugAssistant
{
    partial class SettingsForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingsForm));
            this.chkAutoLaunch = new System.Windows.Forms.CheckBox();
            this.chkAutostart = new System.Windows.Forms.CheckBox();
            this.chkStartMinimized = new System.Windows.Forms.CheckBox();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.chkReloadOnStartup = new System.Windows.Forms.CheckBox();
            this.chkLoadOnTurningOn = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // chkAutoLaunch
            // 
            this.chkAutoLaunch.AutoSize = true;
            this.chkAutoLaunch.Location = new System.Drawing.Point(13, 13);
            this.chkAutoLaunch.Name = "chkAutoLaunch";
            this.chkAutoLaunch.Size = new System.Drawing.Size(206, 17);
            this.chkAutoLaunch.TabIndex = 0;
            this.chkAutoLaunch.Text = "Launch debugger on Autocad startup.";
            this.chkAutoLaunch.UseVisualStyleBackColor = true;
            // 
            // chkAutostart
            // 
            this.chkAutostart.AutoSize = true;
            this.chkAutostart.Location = new System.Drawing.Point(13, 37);
            this.chkAutostart.Name = "chkAutostart";
            this.chkAutostart.Size = new System.Drawing.Size(257, 17);
            this.chkAutostart.TabIndex = 1;
            this.chkAutostart.Text = "On debugger startup, start listening automatically.";
            this.chkAutostart.UseVisualStyleBackColor = true;
            // 
            // chkStartMinimized
            // 
            this.chkStartMinimized.AutoSize = true;
            this.chkStartMinimized.Location = new System.Drawing.Point(13, 61);
            this.chkStartMinimized.Name = "chkStartMinimized";
            this.chkStartMinimized.Size = new System.Drawing.Size(154, 17);
            this.chkStartMinimized.TabIndex = 2;
            this.chkStartMinimized.Text = "Start minimized to tray icon.";
            this.chkStartMinimized.UseVisualStyleBackColor = true;
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(12, 142);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 23);
            this.btnSave.TabIndex = 4;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(219, 142);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 5;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // chkReloadOnStartup
            // 
            this.chkReloadOnStartup.AutoSize = true;
            this.chkReloadOnStartup.Location = new System.Drawing.Point(13, 86);
            this.chkReloadOnStartup.Name = "chkReloadOnStartup";
            this.chkReloadOnStartup.Size = new System.Drawing.Size(155, 17);
            this.chkReloadOnStartup.TabIndex = 6;
            this.chkReloadOnStartup.Text = "Load all lisp files on startup.";
            this.chkReloadOnStartup.UseVisualStyleBackColor = true;
            // 
            // chkLoadOnTurningOn
            // 
            this.chkLoadOnTurningOn.AutoSize = true;
            this.chkLoadOnTurningOn.Location = new System.Drawing.Point(12, 109);
            this.chkLoadOnTurningOn.Name = "chkLoadOnTurningOn";
            this.chkLoadOnTurningOn.Size = new System.Drawing.Size(170, 17);
            this.chkLoadOnTurningOn.TabIndex = 7;
            this.chkLoadOnTurningOn.Text = "Load all lisp files on turning on.";
            this.chkLoadOnTurningOn.UseVisualStyleBackColor = true;
            // 
            // SettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(306, 177);
            this.Controls.Add(this.chkLoadOnTurningOn);
            this.Controls.Add(this.chkReloadOnStartup);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.chkStartMinimized);
            this.Controls.Add(this.chkAutostart);
            this.Controls.Add(this.chkAutoLaunch);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "SettingsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Settings | J.ENG. Eli Belash © 2017";
            this.Load += new System.EventHandler(this.SettingsForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox chkAutoLaunch;
        private System.Windows.Forms.CheckBox chkAutostart;
        private System.Windows.Forms.CheckBox chkStartMinimized;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.CheckBox chkReloadOnStartup;
        private System.Windows.Forms.CheckBox chkLoadOnTurningOn;
    }
}