using System.IO;
using System.Runtime.InteropServices;

namespace nucs.SConsole {
    public static class ConsoleHelper {
        [DllImport("kernel32.dll")]
        private static extern int FreeConsole();

        [DllImport("kernel32")]
        private static extern bool AllocConsole();

        public static void StartConsole() {
            FreeConsole();
            AllocConsole();
        }

        public static void StartConsole(string title) {
            StartConsole();
            try { 
                System.Console.Title = title;
            } catch (IOException) { }
        }
    }
}