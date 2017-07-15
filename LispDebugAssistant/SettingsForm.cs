using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LispDebugAssistant {
    public partial class SettingsForm : Form {
        private AppConfig Cfg { get; }

        public SettingsForm(AppConfig cfg) {
            Cfg = cfg;
            InitializeComponent();
            chkAutoLaunch.Checked = cfg.AutoLaunch;
            chkAutostart.Checked = cfg.AutoStart;
            chkStartMinimized.Checked = cfg.StartMinimized;
            chkReloadOnStartup.Checked = cfg.LoadAllOnStartup;
            chkLoadOnTurningOn.Checked = cfg.LoadAllOnTurnOn;
        }

        private void btnSave_Click(object sender, EventArgs e) {
            Cfg.AutoLaunch = chkAutoLaunch.Checked;
            Cfg.AutoStart = chkAutostart.Checked;
            Cfg.StartMinimized = chkStartMinimized.Checked;
            Cfg.LoadAllOnStartup = chkReloadOnStartup.Checked;
            Cfg.LoadAllOnTurnOn = chkLoadOnTurningOn.Checked;
            Cfg.Save();
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e) {
            this.Close();
        }

        private void SettingsForm_Load(object sender, EventArgs e) {
            
        }
    }
}