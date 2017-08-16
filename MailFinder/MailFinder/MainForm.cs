using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using autonet.Common.Settings;
using autonet.Extensions;
using autonet.Settings;
using BrightIdeasSoftware;
using MailFinder.FileTypes;
using MailFinder.Properties;
using MsgReader.Outlook;
using nucs.Filesystem;
using NHotkey;
using NHotkey.WindowsForms;
using Paths = Common.Paths;

namespace MailFinder {
    public partial class MainForm : Form {
        private SettingsBag _bag;

        public MainForm() {
            InitializeComponent();
            this.TopMost = true;
            HotkeyManager.Current.AddOrReplace("Toggle", Keys.F10, true, HotkeyOnKeyPressed);
            Program.Interface.MouseMove += MouseMovementDetection;
            this.lstResults.PrimarySortColumn = new OLVColumn("Path", "Path");
            lstResults.RowHeight = -1;
        }

        public SettingsBag Bag {
            get {
                lock (this) {
                    if (_bag == null) {
                        _bag = _bag = JsonConfiguration.Load<SettingsBag>(Paths.ConfigFile("MailFinder.config").FullName);
                        _bag.Autosave = true;
                    }
                    return _bag;
                }
            }
            set { _bag = value; }
        }


        private void MouseMovementDetection(object sender, MouseEventArgs args) {
            /*if (this.DistanceFromForm(args.Location) > 15)
                this.SetDesktopLocation(args.X + 5, args.Y + 5);*/
            FlyttaMot(0);
        }

        #region Editor Buttons

        private void btnRtl_Click(object sender, EventArgs e) {
            txtText.RightToLeft = RightToLeft.Yes;
            Bag["rtl"] = true;
        }

        private void btnLtr_Click(object sender, EventArgs e) {
            txtText.RightToLeft = RightToLeft.No;
            Bag["rtl"] = false;
        }


        private void btnExit_Click(object sender, EventArgs e) {
            this.Close();
        }

        #endregion

        private void Form1_Load(object sender, EventArgs e) {
            var pos = Cursor.Position;
            this.SetDesktopLocation(pos.X, pos.Y);
            this.txtText.RightToLeft = (Bag["rtl"] as bool? ?? false) == true ? RightToLeft.Yes : RightToLeft.No;
            this.btnRecusive.BackgroundImage = Bag.Get("deepfolder", false) == false
                ? Properties.Resources.folderoff
                : Properties.Resources.folderblue;
            this.btnAttachments.BackgroundImage = Bag.Get("deepattachments", false) == false
                ? global::MailFinder.Properties.Resources.clipoff
                : global::MailFinder.Properties.Resources.clip;

            //test:
            var smaller = ResizeImage(Resources.clip, 10, 10);
            var smaller2 = ResizeImage(Resources.folder, 10, 10);
            var a = new SearchResult() {Path = "C:\\lol\\wtf", Term = "sometext more", Image = smaller};
            var b = new SearchResult() {Path = "C:\\lol\\wtf2", Term = "sometext more more", Image = smaller2};
            this.lstResults.SetObjects(new[] {a, b, b, a, a, a, a, a});
        }

        /// <summary>
        /// Resize the image to the specified width and height.
        /// </summary>
        /// <param name="image">The image to resize.</param>
        /// <param name="width">The width to resize to.</param>
        /// <param name="height">The height to resize to.</param>
        /// <returns>The resized image.</returns>
        public static Bitmap ResizeImage(Image image, int width, int height) {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage)) {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes()) {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        // Passing null for either maxWidth or maxHeight maintains aspect ratio while
        //        the other non-null parameter is guaranteed to be constrained to
        //        its maximum value.
        //
        //  Example: maxHeight = 50, maxWidth = null
        //    Constrain the height to a maximum value of 50, respecting the aspect
        //    ratio, to any width.
        //
        //  Example: maxHeight = 100, maxWidth = 90
        //    Constrain the height to a maximum of 100 and width to a maximum of 90
        //    whichever comes first.
        //
        private static Size ScaleSize(Size from, int? maxWidth, int? maxHeight) {
            if (!maxWidth.HasValue && !maxHeight.HasValue) throw new ArgumentException("At least one scale factor (toWidth or toHeight) must not be null.");
            if (from.Height == 0 || from.Width == 0) throw new ArgumentException("Cannot scale size from zero.");

            double? widthScale = null;
            double? heightScale = null;

            if (maxWidth.HasValue) {
                widthScale = maxWidth.Value / (double) from.Width;
            }
            if (maxHeight.HasValue) {
                heightScale = maxHeight.Value / (double) from.Height;
            }

            double scale = Math.Min((double) (widthScale ?? heightScale),
                (double) (heightScale ?? widthScale));

            return new Size((int) Math.Floor(from.Width * scale), (int) Math.Ceiling(from.Height * scale));
        }

        #region Searching

        private void txtText_TextChanged(object sender, EventArgs e) {
            var term = txtText?.Text.Trim(' ', '\n', '\r', '\t') ?? "";
            if (string.IsNullOrEmpty(term))
                return;

            //todo handle new text search

            //Process(term);
        }

        private readonly CultureInfo ILCulture = CultureInfo.CreateSpecificCulture("he-IL");

        public const string Version = "1.0.0.0";
/*        private void Process(string term) {
            var current = Program.CurrentFolder;
            if (current == null)
                return;
            var index = current.SubFolder("$index").EnsureCreated();

            index.Attributes = FileAttributes.Hidden;

            lblPath.Invoke(new MethodInvoker(() => lblPath.Text = current.FullName));
            var recusive = Bag.Data["deepfolder"] as bool? ?? false;
            var attachments = Bag.Data["deepattachments"] as bool? ?? false;
            var files = recusive ? FileSearch.EnumerateFilesDeep(current, "*.msg") : FileSearch.GetFiles(current, "*.msg");
            foreach (var file in files) {
                var f = file.FullName;
                using (var msg = new Storage.Message(f)) {
                    var main = MessageToString(msg);
                    var attchs = (attachments ? _deep_attachments(msg, new[] { "msg" }) : extractMessages(msg, new[] { "msg" })).Select(MessageToString).ToArray();
                    var n = new SearchableFile() {Version = Version, Content = main, Attachments = attchs, Path = file.FullName};
                    try {
                        n.MD5 = file.CalculateMD5();
                    } catch (SecurityException) {} catch (IOException) {} catch (UnauthorizedAccessException) {}
                }
            }
            hoot.Save();
            //todo add hidden msgtext file 
        }*/

        #endregion

        #region Draggable

        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [System.Runtime.InteropServices.DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        [System.Runtime.InteropServices.DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();


        private void MainForm_MouseDown(object sender, MouseEventArgs e) {
            if (e.Button == MouseButtons.Left) {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        #endregion

        #region Hotkey

        private void HotkeyOnKeyPressed(object sender, HotkeyEventArgs args) {
            this.Invoke(new MethodInvoker(HotkeyPressed));
        }

        private void HotkeyPressed() {
            if (this.Visible) {
                Hide();
                return;
            }
            this.Show();
            Form1_Load(this, EventArgs.Empty);
            txtText.Focus();
            txtText.SelectAll();
            this.BringToFront();
        }

        public Point CursorPoint => Cursor.Position;

        public void FlyttaMot(int safedistance = 0) {
            var bounds = this.Bounds;
            var c = CursorPoint;
            if (bounds.Contains(c))
                return;

            int xadd = 0, yadd = 0;
            if (bounds.Bottom < c.Y) {
                yadd += c.Y - bounds.Bottom;
            }

            if (bounds.Right < c.X) {
                xadd += c.X - bounds.Right;
            }

            if (bounds.Left > c.X)
                xadd -= Math.Abs(bounds.Left - c.X);
            if (bounds.Top > c.Y) {
                yadd -= Math.Abs(bounds.Top - c.Y);
            }
            var x = bounds.X + xadd;
            var y = bounds.Y + yadd;
            if (Math.Abs(xadd) <= safedistance)
                x = bounds.X;
            if (Math.Abs(yadd) <= safedistance)
                y = bounds.Y;
            this.SetDesktopLocation(x, y);
        }

        #endregion

        private void btnRecusive_Click(object sender, EventArgs e) {
            var val = Bag.Get("deepfolder", false);
            if (val == false) {
                Bag.Set("deepfolder", true);
                this.btnRecusive.BackgroundImage = global::MailFinder.Properties.Resources.folderblue;
            } else {
                Bag.Set("deepfolder", false);
                this.btnRecusive.BackgroundImage = global::MailFinder.Properties.Resources.folderoff;
            }
        }

        private void btnAttachments_Click(object sender, EventArgs e) {
            var val = Bag.Get("deepattachments", false);
            if (val == false) {
                Bag.Set("deepattachments", true);
                this.btnAttachments.BackgroundImage = global::MailFinder.Properties.Resources.clip;
            } else {
                Bag.Set("deepattachments", false);
                this.btnAttachments.BackgroundImage = global::MailFinder.Properties.Resources.clipoff;
            }
        }

        private void MainForm_Paint(object sender, PaintEventArgs e) {
            var r = this.DisplayRectangle;
            r = new Rectangle(r.Location.X, r.Location.Y, r.Width - 1, r.Height - 1);
            e.Graphics.DrawRectangle(new Pen(Color.Black, 1), r);
        }
    }
}

namespace nucs.Winforms.Maths {
    public static class FormExtensions {
        public static double DistanceFromForm(this Form frm, Point p) {
            var rect = frm.Bounds;
            if (rect.Contains(p))
                return 0d;

            var corners = new[] {new Point(rect.X, rect.Y), new Point(rect.X + rect.Width, rect.Y), new Point(rect.X, rect.Y + rect.Height), new Point(rect.X + rect.Width, rect.Y + rect.Height),};
            return corners.Select(c => c.Distance(p)).Min();
        }

        public static double Distance(this Point p1, Point p2) {
            var x1 = p1.X;
            var x2 = p2.X;
            var y1 = p1.Y;
            var y2 = p2.Y;
            return Math.Sqrt(Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2));
        }
    }
}