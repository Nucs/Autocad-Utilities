using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Web;
using nucs.Collections;
using nucs.Monitoring;
using nucs.Web;
using SHDocVw;

namespace nucs.Filesystem.Monitoring.Windows
{
    public delegate void NavigationHandler(DirectoryInfo dir);

    /// <summary>
    ///     Monitors the activity over the open explorer browsers.
    /// </summary>
    public class ExplorerMonitor : MonitorBase<ExplorerWindowRepresentor>
    {
        public event NavigationHandler ExplorerNavigated;

        public override IEnumerable<ExplorerWindowRepresentor> FetchCurrent()
        {
            SHDocVw.ShellWindows shellWindows = new SHDocVw.ShellWindows();

            foreach (InternetExplorer ie in from InternetExplorer ie in shellWindows let filename = Path.GetFileNameWithoutExtension(ie.FullName).ToLower() where filename.Equals("explorer") select ie)
            {
                var ier = new ExplorerWindowRepresentor(ie);
                yield return ier;
            }
        }

        public ExplorerMonitor() : base(
            new DynamicEqualityComparer<ExplorerWindowRepresentor>(
                (x, y) => {
                    if (x.UID == -1 || y.UID == -1) return false;
                    return x.UID.Equals(y.UID);
                },
                representor => representor.UID), ier => ier.UID == -1)
        {

            Entered += item => {
                item.Navigated += dir => {
                    ExplorerNavigated?.Invoke(dir);
                };
                item.Bind();
            };
        }
    }

    public class ExplorerWindowRepresentor
    {

        public event NavigationHandler Navigated;
        public IntPtr hWnd => (IntPtr)IE.HWND;
        public SHDocVw.InternetExplorer IE { get; private set; }
        public int UID
        {
            get
            {
                try
                {
                    return IE.HWND;
                }
                catch (COMException)
                {
                    return -1;
                }
            }
        }

        public void Bind()
        {
            IE.NavigateComplete2 += (object disp, ref object url) => {
                if (Navigated != null)
                    try
                    {
                        var loc = Location;
                        if (loc != null)
                            Navigated(Location);
                    }
                    catch { }
            };



            if (Navigated != null)
                try
                {
                    var loc = Location;
                    if (loc != null)
                        Navigated(Location);
                }
                catch { }
        }

        public DirectoryInfo Location
        {
            get
            {
                try
                {
                    return new DirectoryInfo(HttpUtilities.UrlDecode(IE.LocationURL.Replace("file:///", "")));
                }
                catch
                {
                    return null;
                }
            }
        }



        public ExplorerWindowRepresentor(SHDocVw.InternetExplorer ie, bool bind = false)
        {
            IE = ie;
            if (bind) Bind();
        }
    }
}