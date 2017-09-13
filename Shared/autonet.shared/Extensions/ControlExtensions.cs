using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace autonet.Extensions
{
    public static class ControlExtensions {
        public static void AcceptDoublesOnly(this TextBox txtbx) {
            if (txtbx == null) throw new ArgumentNullException(nameof(txtbx));
            if (txtbx.InvokeRequired) {
                txtbx.Invoke(new MethodInvoker(() => AcceptDoublesOnly(txtbx)));
                return;
            }

            if (string.IsNullOrEmpty(txtbx.Text))
                txtbx.Text = "0";

            txtbx.TextChanged += TextChanged_HandleEmptyTextForDoubles;
            txtbx.KeyPress += KeyPress_TextBoxFilterDouble;

        }

        private static void TextChanged_HandleEmptyTextForDoubles(object sender, EventArgs e)
        {
            var txtbx = (sender as TextBox);
            if (txtbx == null) return;
            if (string.IsNullOrEmpty(txtbx.Text))
                txtbx.Text = "0";
        }

        private static void KeyPress_TextBoxFilterDouble(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar < 48 || e.KeyChar > 57) && e.KeyChar != 46
                && e.KeyChar != 8)
                e.Handled = true;
        }
    }
}
