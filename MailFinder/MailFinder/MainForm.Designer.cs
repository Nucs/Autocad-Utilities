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
            this.txtText = new System.Windows.Forms.TextBox();
            this.lblPath = new System.Windows.Forms.Label();
            this.lstResults = new BrightIdeasSoftware.ObjectListView();
            this.clmSent = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.clmScore = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.clmTitle = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.clmSource = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.clmPath = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.lblStatus = new System.Windows.Forms.Label();
            this.lblResultsCount = new System.Windows.Forms.Label();
            this.btnClearHistory = new System.Windows.Forms.Button();
            this.btnAttachments = new System.Windows.Forms.Button();
            this.btnRecusive = new System.Windows.Forms.Button();
            this.btnExit = new System.Windows.Forms.Button();
            this.btnLtr = new System.Windows.Forms.Button();
            this.btnRtl = new System.Windows.Forms.Button();
            this.btnRegex = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.lstResults)).BeginInit();
            this.SuspendLayout();
            // 
            // txtText
            // 
            this.txtText.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtText.Location = new System.Drawing.Point(6, 25);
            this.txtText.Multiline = true;
            this.txtText.Name = "txtText";
            this.txtText.Size = new System.Drawing.Size(214, 283);
            this.txtText.TabIndex = 0;
            this.txtText.TextChanged += new System.EventHandler(this.TextChangedHandler);
            // 
            // lblPath
            // 
            this.lblPath.AutoSize = true;
            this.lblPath.Location = new System.Drawing.Point(231, 5);
            this.lblPath.Name = "lblPath";
            this.lblPath.Size = new System.Drawing.Size(129, 16);
            this.lblPath.TabIndex = 7;
            this.lblPath.Text = "Please open a folder.";
            this.lblPath.Click += new System.EventHandler(this.lblPath_Click);
            // 
            // lstResults
            // 
            this.lstResults.AllColumns.Add(this.clmSent);
            this.lstResults.AllColumns.Add(this.clmScore);
            this.lstResults.AllColumns.Add(this.clmTitle);
            this.lstResults.AllColumns.Add(this.clmSource);
            this.lstResults.AllColumns.Add(this.clmPath);
            this.lstResults.AutoArrange = false;
            this.lstResults.CellEditUseWholeCell = false;
            this.lstResults.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.clmSent,
            this.clmScore,
            this.clmTitle,
            this.clmSource});
            this.lstResults.Cursor = System.Windows.Forms.Cursors.Default;
            this.lstResults.Font = new System.Drawing.Font("Arial", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.lstResults.FullRowSelect = true;
            this.lstResults.GridLines = true;
            this.lstResults.HasCollapsibleGroups = false;
            this.lstResults.Location = new System.Drawing.Point(226, 25);
            this.lstResults.MultiSelect = false;
            this.lstResults.Name = "lstResults";
            this.lstResults.Size = new System.Drawing.Size(793, 283);
            this.lstResults.TabIndex = 8;
            this.lstResults.UseCompatibleStateImageBehavior = false;
            this.lstResults.View = System.Windows.Forms.View.Details;
            this.lstResults.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.lstResults_ColumnClick);
            this.lstResults.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.lstResults_MouseDoubleClick);
            // 
            // clmSent
            // 
            this.clmSent.AspectName = "Sent";
            this.clmSent.AspectToStringFormat = "";
            this.clmSent.DisplayIndex = 2;
            this.clmSent.IsEditable = false;
            this.clmSent.Text = "Sent On";
            this.clmSent.Width = 115;
            // 
            // clmScore
            // 
            this.clmScore.AspectName = "Score";
            this.clmScore.DisplayIndex = 3;
            this.clmScore.Text = "Score";
            // 
            // clmTitle
            // 
            this.clmTitle.AspectName = "Title";
            this.clmTitle.DisplayIndex = 1;
            this.clmTitle.IsEditable = false;
            this.clmTitle.Text = "Title";
            this.clmTitle.Width = 542;
            this.clmTitle.WordWrap = true;
            // 
            // clmSource
            // 
            this.clmSource.AspectName = "";
            this.clmSource.AutoCompleteEditor = false;
            this.clmSource.AutoCompleteEditorMode = System.Windows.Forms.AutoCompleteMode.None;
            this.clmSource.DisplayIndex = 0;
            this.clmSource.Groupable = false;
            this.clmSource.ImageAspectName = "Image";
            this.clmSource.IsEditable = false;
            this.clmSource.Text = "";
            this.clmSource.Width = 20;
            // 
            // clmPath
            // 
            this.clmPath.AspectName = "Path";
            this.clmPath.DisplayIndex = 2;
            this.clmPath.IsEditable = false;
            this.clmPath.IsVisible = false;
            this.clmPath.Text = "Path";
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(10, 311);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(28, 16);
            this.lblStatus.TabIndex = 10;
            this.lblStatus.Text = "Idle";
            this.lblStatus.Click += new System.EventHandler(this.lblStatus_Click);
            // 
            // lblResultsCount
            // 
            this.lblResultsCount.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblResultsCount.Location = new System.Drawing.Point(925, 311);
            this.lblResultsCount.Name = "lblResultsCount";
            this.lblResultsCount.Size = new System.Drawing.Size(94, 21);
            this.lblResultsCount.TabIndex = 11;
            this.lblResultsCount.Text = "0 Results";
            this.lblResultsCount.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // btnClearHistory
            // 
            this.btnClearHistory.BackgroundImage = global::MailFinder.Properties.Resources.refresh;
            this.btnClearHistory.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnClearHistory.FlatAppearance.BorderSize = 0;
            this.btnClearHistory.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClearHistory.ImageAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnClearHistory.Location = new System.Drawing.Point(24, 4);
            this.btnClearHistory.Name = "btnClearHistory";
            this.btnClearHistory.Size = new System.Drawing.Size(14, 14);
            this.btnClearHistory.TabIndex = 9;
            this.btnClearHistory.UseVisualStyleBackColor = true;
            this.btnClearHistory.Click += new System.EventHandler(this.btnClearHistory_Click);
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
            // btnRegex
            // 
            this.btnRegex.BackgroundImage = global::MailFinder.Properties.Resources.regex;
            this.btnRegex.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnRegex.FlatAppearance.BorderSize = 0;
            this.btnRegex.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRegex.ImageAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnRegex.Location = new System.Drawing.Point(115, 6);
            this.btnRegex.Name = "btnRegex";
            this.btnRegex.Size = new System.Drawing.Size(14, 14);
            this.btnRegex.TabIndex = 12;
            this.btnRegex.UseVisualStyleBackColor = true;
            this.btnRegex.Click += new System.EventHandler(this.btnRegex_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.ClientSize = new System.Drawing.Size(1031, 336);
            this.Controls.Add(this.btnRegex);
            this.Controls.Add(this.lblResultsCount);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.btnClearHistory);
            this.Controls.Add(this.lstResults);
            this.Controls.Add(this.lblPath);
            this.Controls.Add(this.btnAttachments);
            this.Controls.Add(this.btnRecusive);
            this.Controls.Add(this.btnExit);
            this.Controls.Add(this.btnLtr);
            this.Controls.Add(this.btnRtl);
            this.Controls.Add(this.txtText);
            this.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.VisibleChanged += new System.EventHandler(this.MainForm_Shown);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.MainForm_Paint);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.MainForm_MouseDown);
            ((System.ComponentModel.ISupportInitialize)(this.lstResults)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtText;
        private System.Windows.Forms.Button btnRtl;
        private System.Windows.Forms.Button btnLtr;
        private System.Windows.Forms.Button btnExit;
        private System.Windows.Forms.Button btnRecusive;
        private System.Windows.Forms.Button btnAttachments;
        private System.Windows.Forms.Label lblPath;
        private BrightIdeasSoftware.OLVColumn clmTitle;
        private BrightIdeasSoftware.OLVColumn clmSource;
        public BrightIdeasSoftware.ObjectListView lstResults;
        private BrightIdeasSoftware.OLVColumn clmPath;
        private BrightIdeasSoftware.OLVColumn clmSent;
        private BrightIdeasSoftware.OLVColumn clmScore;
        private System.Windows.Forms.Button btnClearHistory;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Label lblResultsCount;
        private System.Windows.Forms.Button btnRegex;
    }
}

