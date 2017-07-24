using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MailFinder {
    public partial class Form1 : Form {
        public Form1() {
            InitializeComponent();
        }

        private void btnRtl_Click(object sender, EventArgs e) {
            txtText.RightToLeft = RightToLeft.Yes;
        }

        private void btnLtr_Click(object sender, EventArgs e) {
            txtText.RightToLeft = RightToLeft.No;
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}