using System;
using System.Diagnostics;
using System.Threading;
using autonet.Extensions;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Common;
using Exception = System.Exception;

namespace autonet {
    public class AcadProperties : IExtensionApplication {
        private static readonly object _lock = new object();
        public static Point3d Cursor { get; private set; }
        public static Point3d? ComputedCursor { get; private set; }
        public static InputPointContext InputPointContext { get; private set; }

        public static bool IsOSnapEnabled => (Int16) Application.GetSystemVariable("OSMODE") > 0;

        public static Point3d Default3d { get; } = new Point3d(double.MinValue, double.MinValue, double.MinValue);
        public static Point2d Default2d { get; } = new Point2d(double.MinValue, double.MinValue);

        /// <summary>
        ///     Signals when the mainform finishes loading.
        /// </summary>
        internal static ManualResetEventSlim _signal { get; set; } = new ManualResetEventSlim();

        public void Initialize() {
            try {
                Quick.Editor.PointMonitor += EditorOnPointMonitor;
                Paths.ConfigDirectory.EnsureCreated();
            } catch (Exception e) {
                Console.WriteLine(e);
                Debug.WriteLine(e);
                Quick.Write(" " + e);
            }
        }

        public void Terminate() {
            Quick.Editor.PointMonitor -= EditorOnPointMonitor;
        }

        private void EditorOnPointMonitor(object sender, PointMonitorEventArgs args) {
            lock (_lock) {
                InputPointContext = args.Context;
                Cursor = args.Context.RawPoint;
                if (args.Context.PointComputed) {
                    ComputedCursor = args.Context.ComputedPoint;
                } else
                    ComputedCursor = null;
            }
        }
    }
}