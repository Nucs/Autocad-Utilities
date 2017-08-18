using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Media;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using autonet.Common.Settings;
using autonet.Settings;
using BrightIdeasSoftware;
using Common;
using MailFinder.Helpers;
using MailFinder.Properties;
using NHotkey;
using NHotkey.WindowsForms;

namespace MailFinder {
    public partial class MainForm : Form {
        public const string Version = "1.0.0.0";
        private static SettingsBag _bag;
        private static readonly object locker_bag = new object();

        private readonly Image _smallClip = ResizeImage(Resources.clip, 8, 8);
        public QueueThread<ParentTask> Executer;

        public MainForm() {
            InitializeComponent();
            TopMost = true;
            HotkeyManager.Current.AddOrReplace("Toggle", Keys.F10, true, HotkeyOnKeyPressed);
            //Program.Interface.MouseMove += MouseMovementDetection;
            lstResults.Sorting = SortOrder.Descending;
            lstResults.CustomSorter = CustomSorter;
            lstResults.ShowGroups = false;
            lstResults.PrimarySortColumn = new OLVColumn("Sent On", "Sent");
            lstResults.SecondarySortColumn = new OLVColumn("Title", "Title");
            //this.lstResults.PrimarySortColumn = new OLVColumn("Sent On", "Sent");
            lstResults.RowHeight = -1;
            Executer = new QueueThread<ParentTask>();
            Executer.TaskQueued += ExecuterOnTaskQueued;
            Program.FolderChanged += FolderChangedHandler;
            lstResults.BeforeSorting += LstResultsOnBeforeSorting;
        }

        private void LstResultsOnBeforeSorting(object sender, BeforeSortingEventArgs args) {
            if (args.ColumnToSort.AspectName == "Title") {
                args.SecondaryColumnToSort = new OLVColumn("Sent On", "Sent");
                args.SecondarySortOrder = SortOrder.Descending;
            } else if (args.ColumnToSort.AspectName == "Score") {
                args.SecondaryColumnToSort = new OLVColumn("Sent On", "Sent");
                args.SecondarySortOrder = SortOrder.Descending;
            }
            //lstResults.ListViewItemSorter = new ResultsComparer(args.ColumnToSort, SortOrder.Descending);
        }

        public static SettingsBag Bag {
            get {
                if (_bag != null)
                    return _bag;
                lock (locker_bag) {
                    if (_bag == null) {
                        _bag = _bag = JsonConfiguration.Load<SettingsBag>(Paths.ConfigFile("MailFinder.config").FullName);
                        _bag.Autosave = true;
                    }
                    return _bag;
                }
            }
            private set => _bag = value;
        }

        private void ExecuterOnTaskQueued(QueuedTask<ParentTask> task) {
            task.StateChanged += (t, s) => {
                switch (s) {
                    case TaskState.Executing:
                        Invoke(() => { lblStatus.Text = t.Description; });
                        break;
                    case TaskState.Cancelled:
                    case TaskState.Executed:
                        Invoke(() => {
                            if (lblStatus.Text.Equals(t.Description, StringComparison.InvariantCultureIgnoreCase))
                                lblStatus.Text = "Idle";
                        });
                        break;
                }
            };
        }

        private void lstResults_ColumnClick(object sender, ColumnClickEventArgs e) {
        }

        private void CustomSorter(OLVColumn column, SortOrder sortOrder) {
            if (column.Text == "Sent On") lstResults.ListViewItemSorter = new ResultsComparer(column, sortOrder);
            else
                lstResults.ListViewItemSorter = new ColumnComparer(column, sortOrder);
        }

        private void Form1_Load(object sender, EventArgs e) {
            var pos = Cursor.Position;
            SetDesktopLocation(pos.X, pos.Y);
            txtText.RightToLeft = (Bag["rtl"] as bool? ?? false) ? RightToLeft.Yes : RightToLeft.No;
            btnRecusive.BackgroundImage = Bag.Get("deepfolder", false) == false
                ? Resources.folderoff
                : Resources.folderblue;
            btnRegex.BackgroundImage = Bag.Get("regex", false) == false
                ? Resources.regexoff
                : Resources.regex;
            btnAttachments.BackgroundImage = Bag.Get("deepattachments", false) == false
                ? Resources.clipoff
                : Resources.clip;
        }

        /// <summary>
        ///     Resize the image to the specified width and height.
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

            if (maxWidth.HasValue) widthScale = maxWidth.Value / (double) from.Width;
            if (maxHeight.HasValue) heightScale = maxHeight.Value / (double) from.Height;

            var scale = Math.Min((double) (widthScale ?? heightScale),
                (double) (heightScale ?? widthScale));

            return new Size((int) Math.Floor(from.Width * scale), (int) Math.Ceiling(from.Height * scale));
        }

        #region Searching & Index

        private void FolderChangedHandler(DirectoryInfo dir) {
            if (!Visible || dir == null || dir.Parent == null)
                return;
            Executer.QueueTask(new DirectoryIndexTask(FolderChangedTask) {Search = dir, Description = $"Indexing folder " + dir.FullName});
            Invoke(new MethodInvoker(() => lblPath.Text = dir.FullName));
            TextChangedHandler(null, null);
        }

        public readonly List<string> ProcessedFoldersHistory = new List<string>();

        private void FolderChangedTask(ParentTask task, CancellationToken token, ProgressDesciber progress) {
            try {
                if (!Visible || token.IsCancellationRequested)
                    return;
                var t = (DirectoryIndexTask) task;
                var depth = Bag.Get("deepfolder", false) ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
                if (depth == SearchOption.AllDirectories && t.Search.Parent == null) //skip root folders
                    return;
                var p = ProcessedFoldersHistory.ToArray();
                var files = depth == SearchOption.AllDirectories
                    ? FileSearches.EnumerateFilesDeep(t.Search, token, "*.msg")
                    : t.Search.EnumerateFiles("*.msg", depth)
                        .Select(fi => {
                            if (token.IsCancellationRequested || p.Any(used => Paths.CompareTo(used, fi.FullName)))
                                return null;
                            ProcessedFoldersHistory.Add(fi.FullName);
                            return fi;
                        })
                        .Where(fi => fi != null);
                InvertedApi.IndexFiles(files, token, false);
            } catch (Exception e) {
                //todo log errors.
            }
        }

        private void TextChangedHandler(object sender, EventArgs e) {
            var term = txtText?.Text.Trim(' ', '\n', '\r', '\t').Replace('*', '%') ?? "";
            if (string.IsNullOrEmpty(term))
                return;
            var current = Program.CurrentFolder;
            if (current == null) return;
            Executer.CancelQueuedWhere(task => task is DirectoryIndexTask);
            Executer.CancelCurrentIf(task => task is DirectoryIndexTask);
            Executer.QueueTask(new SearchTask((task, token, progress) => TextChangedTask(current, term, token, progress)) {Description = $"Searching term \"{term}\" inside {current.FullName}"});
        }

        private void TextChangedTask(DirectoryInfo current, string term, CancellationToken token, ProgressDesciber progress) {
            if (token.IsCancellationRequested)
                return;

            var recusive = Bag.Get("deepfolder", false);
            if (recusive && current.Parent == null)
                recusive = false;
            if (token.IsCancellationRequested)
                return;
            var _results = InvertedApi.Search(term, recusive ? FileSearches.EnumerateDirectoriesDeep(current, token) : new[] {current})
                .Cast<ScoredIndexedFile>()
                .OrderByDescending(o => o.TotalScore)
                .ToArray();

            if (_results.Length == 0) {
                Invoke(() => {
                    lstResults.ClearObjects();
                    lblResultsCount.Text = "0 Results";
                });
                return;
            }

            var maxscore = _results[0].TotalScore;
            var results = _results.Select(o => {
                    var r = new SearchResult {
                        Sent = o.Date,
                        Path = o.Path,
                        Title = o.Title,
                        Score = Math.Round(o.TotalScore / (maxscore * 1d) * 100, 2, MidpointRounding.AwayFromZero)
                    };
                    if (o.TotalScore == 0) return null;
                    if (o.Score > 0 && o.Innerscore == 0) r.Image = null;
                    else if (o.Score == 0d && o.Innerscore > 0) r.Image = _smallClip;
                    return r;
                })
                .Where(o => o != null)
                .ToArray();
            Invoke(() => {
                lstResults.SetObjects(results);
                lblResultsCount.Text = $"{results.Length} Results";
            });
        }

        #endregion

        #region Editor Buttons

        private void btnRtl_Click(object sender, EventArgs e) {
            txtText.RightToLeft = RightToLeft.Yes;
            Bag["rtl"] = true;
            txtText.Focus();
        }

        private void btnLtr_Click(object sender, EventArgs e) {
            txtText.RightToLeft = RightToLeft.No;
            Bag["rtl"] = false;
            txtText.Focus();
        }


        private void btnExit_Click(object sender, EventArgs e) {
            Close();
        }

        #endregion

        #region Draggable

        private void MouseMovementDetection(object sender, MouseEventArgs args) {
            //FlyttaMot(0);
        }

        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        [DllImport("user32.dll")]
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
            Invoke(new MethodInvoker(HotkeyPressed));
        }

        private void HotkeyPressed() {
            if (Visible) {
                Hide();
                return;
            }
            Show();
            Form1_Load(this, EventArgs.Empty);
            txtText.Focus();
            txtText.SelectAll();
            BringToFront();
        }

        public Point CursorPoint => Cursor.Position;

        public void FlyttaMot(int safedistance = 0) {
            var bounds = Bounds;
            var c = CursorPoint;
            if (bounds.Contains(c))
                return;

            int xadd = 0, yadd = 0;
            if (bounds.Bottom < c.Y) yadd += c.Y - bounds.Bottom;

            if (bounds.Right < c.X) xadd += c.X - bounds.Right;

            if (bounds.Left > c.X)
                xadd -= Math.Abs(bounds.Left - c.X);
            if (bounds.Top > c.Y) yadd -= Math.Abs(bounds.Top - c.Y);
            var x = bounds.X + xadd;
            var y = bounds.Y + yadd;
            if (Math.Abs(xadd) <= safedistance)
                x = bounds.X;
            if (Math.Abs(yadd) <= safedistance)
                y = bounds.Y;
            SetDesktopLocation(x, y);
        }

        #endregion

        #region UI

        private void MainForm_Shown(object sender, EventArgs e) {
            if (Visible)
                FolderChangedHandler(Program.CurrentFolder);
        }

        private void btnRecusive_Click(object sender, EventArgs e) {
            var val = Bag.Get("deepfolder", false);
            if (val == false) {
                Bag.Set("deepfolder", true);
                btnRecusive.BackgroundImage = Resources.folderblue;
            } else {
                Bag.Set("deepfolder", false);
                btnRecusive.BackgroundImage = Resources.folderoff;
            }
            FolderChangedHandler(Program.CurrentFolder);
        }

        private void btnAttachments_Click(object sender, EventArgs e) {
            var val = Bag.Get("deepattachments", false);
            if (val == false) {
                Bag.Set("deepattachments", true);
                btnAttachments.BackgroundImage = Resources.clip;
            } else {
                Bag.Set("deepattachments", false);
                btnAttachments.BackgroundImage = Resources.clipoff;
            }
            TextChangedHandler(null, null);
        }

        private void btnRegex_Click(object sender, EventArgs e) {
            var val = Bag.Get("regex", false);
            if (val == false) {
                Bag.Set("regex", true);
                btnRegex.BackgroundImage = Resources.regex;
            } else {
                Bag.Set("regex", false);
                btnRegex.BackgroundImage = Resources.regexoff;
            }
            TextChangedHandler(null, null);
        }

        /// <summary>
        ///     1px border
        /// </summary>
        private void MainForm_Paint(object sender, PaintEventArgs e) {
            var r = DisplayRectangle;
            r = new Rectangle(r.Location.X, r.Location.Y, r.Width - 1, r.Height - 1);
            e.Graphics.DrawRectangle(new Pen(Color.Black, 1), r);
        }

        private void lstResults_MouseDoubleClick(object sender, MouseEventArgs e) {
            var i = lstResults.HitTest(e.X, e.Y).Item as OLVListItem;
            if (i != null)
                try {
                    Process.Start((i.RowObject as SearchResult).Path);
                } catch {
                    SystemSounds.Beep.Play();
                }
        }

        private void Invoke(Action act) {
            if (InvokeRequired == false) act();
            else
                Invoke(new MethodInvoker(act));
        }

        private void btnClearHistory_Click(object sender, EventArgs e) {
            ProcessedFoldersHistory.Clear();
            var current = Program.CurrentFolder;
            if (current == null) return;
            FolderChangedHandler(current);
        }

        private void lblStatus_Click(object sender, EventArgs e) { }

        private void lblPath_Click(object sender, EventArgs e) {
            try {
                Process.Start(new DirectoryInfo(lblPath.Text).FullName);
            } catch { }
        }

        #endregion
    }
}

namespace nucs.Winforms.Maths {
    public static class FormExtensions {
        public static double DistanceFromForm(this Form frm, Point p) {
            var rect = frm.Bounds;
            if (rect.Contains(p))
                return 0d;

            var corners = new[] {new Point(rect.X, rect.Y), new Point(rect.X + rect.Width, rect.Y), new Point(rect.X, rect.Y + rect.Height), new Point(rect.X + rect.Width, rect.Y + rect.Height)};
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