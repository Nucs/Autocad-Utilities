using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace autonet {
    public partial class SourcesEditorForm : Form {
        public List<string> List { get; } = new List<string>();

        public SourcesEditorForm() {
            InitializeComponent();
        }

        public SourcesEditorForm(List<string> data) : this() {
            List.AddRange(data);
            textBox1.Text = string.Join(Environment.NewLine, List);
        }

        private void btnSave_Click(object sender, EventArgs e) {
            var data = this.textBox1.Text.Trim('\n', ' ', '\r', '\t').Replace("\r", "\n").Replace("\n\n", "\n").Replace("\n\n", "\n").Split(new char[] {'\n'}, StringSplitOptions.RemoveEmptyEntries).Where(l => l != null && l.Trim() != "").ToArray();
            foreach (var line in data) {
                try {
                    var sys = Path.HasExtension(line) ? (FileSystemInfo)new FileInfo(line) : (FileSystemInfo)new DirectoryInfo(line);
                    if (Uri.IsWellFormedUriString(line, UriKind.Absolute)==false)
                        throw new InvalidOperationException();
                } catch {
                    MessageBox.Show("An invalid path was found:\n" + line);
                    return;
                }
            }
            List.Clear();
            List.AddRange(data);
            Close();
        }

        private void btnRevert_Click(object sender, EventArgs e) {
            textBox1.Text=string.Join(Environment.NewLine, List);
        }

        private void btnCancel_Click(object sender, EventArgs e) {
            Close();
        }

        public List<string> RetShowDialog(IWin32Window parent) {
            this.ShowDialog(parent);
            return List.ToList();
        }
    }
}