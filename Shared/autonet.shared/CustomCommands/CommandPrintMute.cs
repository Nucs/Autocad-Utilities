using System;
using System.Collections.Generic;
using System.Text;

namespace autonet.addons {
    public class CommandPrintMute : IDisposable {
        public CommandPrintMute() {
            LastNoMuttValue = Convert.ToInt32(Autodesk.AutoCAD.ApplicationServices.Core.Application.GetSystemVariable("nomutt"));
            Autodesk.AutoCAD.ApplicationServices.Core.Application.SetSystemVariable("nomutt", 0);
        }

        public int LastNoMuttValue { get; set; }

        public void Dispose() {
            Autodesk.AutoCAD.ApplicationServices.Core.Application.SetSystemVariable("nomutt", LastNoMuttValue);
        }
    }
}