using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using autonet.Common.Settings;
using autonet.Settings;
using Autodesk.AutoCAD.DatabaseServices;

namespace autonet.Forms {
    public partial class QQForm : SettingsForm<QQSettings> {
        public override QQSettings Settings { get; } = JsonConfiguration.Load<QQSettings>();
        public QQForm() : base() {
            InitializeComponent();
        }

        public QQForm(Entity e) : this() {
            PGrid.SelectObject(e);
            //Bind(this.propertyGrid1, qq=>qq.texty);
        }
        private void QQForm_Load(object sender, EventArgs e) {
            LoadSettings();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            Save();
        }

        private void btnRevert_Click(object sender, EventArgs e)
        {
            RevertChanges();

        }
    }
}