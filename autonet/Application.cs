using System;
using System.IO;
using autonet.Common.Settings;
using autonet.CustomCommands;
using autonet.Extensions;
using autonet.lsp;
using autonet.Settings;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Customization;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Interop;
using Autodesk.AutoCAD.Runtime;
using Autodesk.Windows;
using Common;
using App= Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace autonet {
    public class Application : IExtensionApplication {
        public static QuickySettings Settings = JsonConfiguration.Load<QuickySettings>();
        public void Initialize() {
            //Load Directories!
            Paths.ConfigDirectory.EnsureCreated();

            Console.WriteLine(Settings.Test);
            Settings.Save();
            App.DocumentManager.CurrentDocument.Editor.WriteMessage(" ");
            App.DocumentManager.CurrentDocument.Editor.WriteMessage(Settings.Test);
            LspLoader.Load("CirclePolylineSwap");
            LspLoader.Load("E2P");
        }
        [CommandMethod("TLOD", CommandFlags.Modal)]
        public static void TLOD()
        {
            /*
            ;;
            ;; contents of "C:/_TLOAD.lsp"
            ;;
            (defun c:LISPCOM1 ()
            (prompt "\nLoaded: C:/_TLOAD.lsp")
            (prompt "\nLISPCOM1 Executed!")
            (prompt "\nNice")
            (princ)
            )
            */

            var s = App.DocumentManager.CurrentDocument.UserData;
/*            string lispPath = "C:/_TLOAD.lsp";
            string loadStr = String.Format("(load \"{0}\") "/*space after closing paren!!!#1#, lispPath);
            curAcadDoc.SendCommand(loadStr);
            curAcadDoc.SendCommand("LISPCOM1 "/*space after command name!!!#1#);*/
        }
        public void Terminate() {
        }
    }
}