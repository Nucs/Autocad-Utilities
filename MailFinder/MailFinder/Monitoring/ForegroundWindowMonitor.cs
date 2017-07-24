using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;
using nucs.SystemCore.Boolean;

namespace nucs.Monitoring.Inline {
    public class ForegroundWindowMonitor : MonitorSingleBase<ProcessWindow> {
        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowThreadProcessId(IntPtr hWnd, out uint ProcessId);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        public ForegroundWindowMonitor() : base(new DynamicEqualityComparer<ProcessWindow>((p, pp) => p.Equals(pp) == true, p => p.GetHashCode()), t1 => false) { }

        ProcessWindow GetActiveProcessFileName() {
            IntPtr hwnd = GetForegroundWindow();
            uint pid;
            GetWindowThreadProcessId(hwnd, out pid);
            return new ProcessWindow(Process.GetProcessById((int)pid),hwnd);
        }

        public override ProcessWindow FetchCurrent() {
            return GetActiveProcessFileName();
        }
    }

    public class ProcessWindow {
        public Process Process { get; set; }
        public IntPtr hWnd { get; set; }

        public ProcessWindow(Process process, IntPtr hWnd) {
            Process = process;
            this.hWnd = hWnd;
        }

        protected bool Equals(ProcessWindow other) {
            return Equals(Process?.Id, other.Process?.Id) && hWnd.Equals(other.hWnd);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ProcessWindow) obj);
        }

        public override int GetHashCode() {
            unchecked {
                return ((Process != null ? Process.Id.GetHashCode() : 0) * 397) ^ hWnd.GetHashCode();
            }
        }
    }
    
}