using System.Media;
using System.Threading;
using System.Windows.Forms;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;
using Application = System.Windows.Forms.Application;

namespace LispDebugAssistant {
    public class Main : IExtensionApplication {
        public static MainForm MainForm { get; internal set; }
        /// <summary>
        ///     Signals when the mainform finishes loading.
        /// </summary>
        internal static ManualResetEventSlim _signal { get; set; } = new ManualResetEventSlim();

        public void Initialize() {
            if (MainForm.Config.AutoLaunch)
                MainCommands.InitiateCommand();
        }

        public void Terminate() {
            MainForm.Dispose();
            MainForm = null;
        }

        public static bool WaitForMainForm(int ms = -1) {
            return _signal.Wait(ms);
        }
    }

    public static class MainCommands {
        /// <summary>
        ///     Starts a lsplistener app.
        /// </summary>
        [CommandMethod("lspdbg")]
        public static void InitiateCommand() {
            var t = new Thread(() => {
                Mutex mutex = new System.Threading.Mutex(false, "lspdbg_instance");
                try {
                    if (mutex.WaitOne(0, false) || MessageBox.Show("An instance of Lisp Debugging Assistant is already running.\nMake sure to check on the tray icons.\nWould you still like to open a new instance?", "Instance Already Exists", MessageBoxButtons.YesNo, MessageBoxIcon.Error) == DialogResult.Yes) {
                        SystemSounds.Asterisk.Play();
                        Application.Run(Main.MainForm = new MainForm());
                    }
                } finally {
                    mutex?.Close();
                }
            });
            t.SetApartmentState(ApartmentState.STA);
            t.IsBackground = true;
            t.Start();
        }
    }
}