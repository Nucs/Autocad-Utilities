namespace MailFinder
{
    partial class Form1
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
            this.txtText = new System.Windows.Forms.TextBox();
            this.btnExit = new System.Windows.Forms.Button();
            this.btnLtr = new System.Windows.Forms.Button();
            this.btnRtl = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // txtText
            // 
            this.txtText.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtText.Location = new System.Drawing.Point(12, 32);
            this.txtText.Multiline = true;
            this.txtText.Name = "txtText";
            this.txtText.Size = new System.Drawing.Size(214, 131);
            this.txtText.TabIndex = 0;
            // 
            // btnExit
            // 
            this.btnExit.BackgroundImage = global::MailFinder.Properties.Resources.cross;
            this.btnExit.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.btnExit.FlatAppearance.BorderSize = 0;
            this.btnExit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExit.ImageAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnExit.Location = new System.Drawing.Point(12, 12);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(16, 16);
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
            this.btnLtr.Location = new System.Drawing.Point(182, 12);
            this.btnLtr.Name = "btnLtr";
            this.btnLtr.Size = new System.Drawing.Size(16, 16);
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
            this.btnRtl.Location = new System.Drawing.Point(208, 12);
            this.btnRtl.Name = "btnRtl";
            this.btnRtl.Size = new System.Drawing.Size(16, 16);
            this.btnRtl.TabIndex = 1;
            this.btnRtl.UseVisualStyleBackColor = true;
            this.btnRtl.Click += new System.EventHandler(this.btnRtl_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(235, 195);
            this.Controls.Add(this.btnExit);
            this.Controls.Add(this.btnLtr);
            this.Controls.Add(this.btnRtl);
            this.Controls.Add(this.txtText);
            this.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtText;
        private System.Windows.Forms.Button btnRtl;
        private System.Windows.Forms.Button btnLtr;
        private System.Windows.Forms.Button btnExit;
    }
}

