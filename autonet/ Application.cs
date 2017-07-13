using System;
using autonet.Common.Settings;
using autonet.Settings;
using Autodesk.AutoCAD.Runtime;

namespace autonet {
    public class Application : IExtensionApplication {
        public static QuickySettings Settings = JsonConfiguration.Load<QuickySettings>();
        public void Initialize() {
            Console.WriteLine(Settings.Test);
        }

        public void Terminate() {
        }
    }
}