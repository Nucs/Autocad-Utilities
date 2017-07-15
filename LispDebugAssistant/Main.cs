using System.Media;
using System.Threading;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;
using Application = System.Windows.Forms.Application;

namespace LispDebugAssistant {
    public class Main : IExtensionApplication {
        public static MainForm MainForm { get; internal set; }
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
                SystemSounds.Asterisk.Play();
                Application.Run(Main.MainForm = new MainForm());
            });
            t.SetApartmentState(ApartmentState.STA);
            t.IsBackground = true;
            t.Start();
        }
    }
}