namespace MailFinder
{
    partial class MainForm
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
            this.components = new System.ComponentModel.Container();
            this.txtText = new System.Windows.Forms.TextBox();
            this.ttT = new System.Windows.Forms.ToolTip(this.components);
            this.btnAttachments = new System.Windows.Forms.Button();
            this.btnRecusive = new System.Windows.Forms.Button();
            this.btnExit = new System.Windows.Forms.Button();
            this.btnLtr = new System.Windows.Forms.Button();
            this.btnRtl = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // txtText
            // 
            this.txtText.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtText.Location = new System.Drawing.Point(6, 25);
            this.txtText.Multiline = true;
            this.txtText.Name = "txtText";
            this.txtText.Size = new System.Drawing.Size(214, 131);
            this.txtText.TabIndex = 0;
            this.txtText.TextChanged += new System.EventHandler(this.txtText_TextChanged);
            // 
            // ttT
            // 
            this.ttT.ToolTipTitle = "Aaaa";
            // 
            // btnAttachments
            // 
            this.btnAttachments.BackgroundImage = global::MailFinder.Properties.Resources.clip;
            this.btnAttachments.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnAttachments.FlatAppearance.BorderSize = 0;
            this.btnAttachments.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAttachments.ImageAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnAttachments.Location = new System.Drawing.Point(135, 5);
            this.btnAttachments.Name = "btnAttachments";
            this.btnAttachments.Size = new System.Drawing.Size(14, 14);
            this.btnAttachments.TabIndex = 5;
            this.btnAttachments.UseVisualStyleBackColor = true;
            this.btnAttachments.Click += new System.EventHandler(this.btnAttachments_Click);
            // 
            // btnRecusive
            // 
            this.btnRecusive.BackgroundImage = global::MailFinder.Properties.Resources.folder;
            this.btnRecusive.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.btnRecusive.FlatAppearance.BorderSize = 0;
            this.btnRecusive.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRecusive.ImageAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnRecusive.Location = new System.Drawing.Point(155, 5);
            this.btnRecusive.Name = "btnRecusive";
            this.btnRecusive.Size = new System.Drawing.Size(14, 14);
            this.btnRecusive.TabIndex = 4;
            this.btnRecusive.UseVisualStyleBackColor = true;
            this.btnRecusive.Click += new System.EventHandler(this.btnRecusive_Click);
            // 
            // btnExit
            // 
            this.btnExit.BackgroundImage = global::MailFinder.Properties.Resources.cross;
            this.btnExit.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.btnExit.FlatAppearance.BorderSize = 0;
            this.btnExit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExit.ImageAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnExit.Location = new System.Drawing.Point(6, 5);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(12, 12);
            this.btnExit.TabIndex = 3;
            this.btnExit.UseVisualStyleBackColor = true;
            this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
            // 
            // btnLtr
            // 
            this.btnLtr.BackgroundImage = global::MailFinder.Properties.Resources.ltr;
            this.btnLtr.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.btnLtr.FlatAppearance.BorderSize = 0;
            this.btnLtr.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLtr.ImageAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnLtr.Location = new System.Drawing.Point(184, 5);
            this.btnLtr.Name = "btnLtr";
            this.btnLtr.Size = new System.Drawing.Size(12, 12);
            this.btnLtr.TabIndex = 2;
            this.btnLtr.UseVisualStyleBackColor = true;
            this.btnLtr.Click += new System.EventHandler(this.btnLtr_Click);
            // 
            // btnRtl
            // 
            this.btnRtl.BackgroundImage = global::MailFinder.Properties.Resources.rtl;
            this.btnRtl.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.btnRtl.FlatAppearance.BorderSize = 0;
            this.btnRtl.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRtl.ImageAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnRtl.Location = new System.Drawing.Point(202, 5);
            this.btnRtl.Name = "btnRtl";
            this.btnRtl.Size = new System.Drawing.Size(12, 12);
            this.btnRtl.TabIndex = 1;
            this.btnRtl.UseVisualStyleBackColor = true;
            this.btnRtl.Click += new System.EventHandler(this.btnRtl_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(226, 160);
            this.Controls.Add(this.btnAttachments);
            this.Controls.Add(this.btnRecusive);
            this.Controls.Add(this.btnExit);
            this.Controls.Add(this.btnLtr);
            this.Controls.Add(this.btnRtl);
            this.Controls.Add(this.txtText);
            this.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "MainForm";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.MainForm_MouseDown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtText;
        private System.Windows.Forms.Button btnRtl;
        private System.Windows.Forms.Button btnLtr;
        private System.Windows.Forms.Button btnExit;
        private System.Windows.Forms.Button btnRecusive;
        private System.Windows.Forms.ToolTip ttT;
        private System.Windows.Forms.Button btnAttachments;
    }
}

