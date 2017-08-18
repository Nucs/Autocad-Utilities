using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Gma.System.MouseKeyHook;
using nucs.Automation;
using nucs.Automation.Mirror;
using nucs.Database;
using nucs.Filesystem.Monitoring.Windows;
using nucs.Monitoring.Inline;
using nucs.SConsole;
using nucs.Winforms.Maths;
using SHDocVw;

namespace MailFinder {
    static class Program {
        public static IKeyboardMouseEvents Interface;
        public static DirectoryInfo CurrentFolder { get; private set; }
        public static event Action<DirectoryInfo> FolderChanged;
        public static readonly CultureInfo ILCulture = CultureInfo.CreateSpecificCulture("he-IL");
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main() {
            var path = "C:\\tests\\";
            Db.ChangeConnectionString("Server=127.0.0.1;Database=mailfinder;Uid=root;Pwd=qweqwe;");
            //Db.ChangeConnectionString("Server=192.168.102.95;Database=mailfinder;Uid=worldwide;Pwd=qweqweqwe!@#;");
            if (Db.TestConnection() is Exception e) {
                MessageBox.Show(e.ToString(), "Exception Cought.");
                return;
            }
            WindowsExplorerListener p = null;
            try {
                Interface = Hook.GlobalEvents();
                p = new WindowsExplorerListener();
                p.ChangedDirectory += dir => {
#if DEBUG
                    Console.WriteLine("Changed: "+(CurrentFolder = dir).FullName);
#endif
                    FolderChanged?.Invoke(dir);
                };
                p.Start();
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MainForm());
            } finally {
                (Interface as IKeyboardMouseEvents)?.Dispose();
                p?.Dispose();
            }
        }
    }

    public class WindowsExplorerListener : IDisposable {
        public delegate void ChangedDirectoryHandler(DirectoryInfo dir);

        public event ChangedDirectoryHandler ChangedDirectory;

        public DirectoryInfo Current { get; private set; }
        private ExplorerMonitor p;
        private ForegroundWindowMonitor s;

        public void Start() {
            p.Start();
            s.Start();
        }

        public WindowsExplorerListener() {
            p = new ExplorerMonitor();
            p.ExplorerNavigated += dir => {
                if (dir == null)
                    return;
                lock (this) {
                    Current = dir;
                    ChangedDirectory?.Invoke(Current);
                }
            };
            s = new ForegroundWindowMonitor();
            s.Changed += item => {
                if (item == null)
                    return;
                if (item.Process.ProcessName.Contains("explorer") == false)
                    return;
                var ret = FindForeground(SmartProcess.Get(item.Process), item.hWnd);
                var c = ret?.Location;
                if (c == null)
                    return;
                lock (this) {
                    Current = c;
                    ChangedDirectory?.Invoke(Current);
                }
            };
        }

        static ExplorerWindowRepresentor FindForeground(SmartProcess p, IntPtr hwnd) {
            ShellWindows shellWindows = new ShellWindows();
            foreach (InternetExplorer ie in from InternetExplorer ie in shellWindows let filename = Path.GetFileNameWithoutExtension(ie.FullName).ToLower() where filename.Equals("explorer") select ie) {
                if (new IntPtr(ie.HWND).Equals(hwnd)) {
                    return new ExplorerWindowRepresentor(ie);
                }
            }
            return null;
        }

        public void Dispose() {
            p?.Dispose();
            s?.Dispose();
        }
    }
}