using System;
using System.IO;
using System.Reflection;
using Autodesk.AutoCAD.Customization;
using Autodesk.AutoCAD.Runtime;
using Common;

namespace autonet.CustomCommands {
    public class CreateTestCuix {
 /*       [CommandMethod("CreateTestCuix1")]
        public static void CreateTestCuix1() {
            var editor = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;

            try {
                var debugFolder = Paths.ConfigDirectory.FullName;
                var customizationSection = new CustomizationSection();
                var menuGroup = customizationSection.MenuGroup;
                var tab = customizationSection.AddNewTab("CuiTestTab");
                var panel = tab.AddNewPanel("Panel");
                var row = panel.AddNewRibbonRow();

                menuGroup.Name = "CuiTest1";

                row.AddNewButton(
                    "Smile",
                    "Smile",
                    "KeepSmiling",
                    "How to add BMP icon to Custom Command",
                    Path.Combine(debugFolder + "smile_16.bmp"),
                    Path.Combine(debugFolder + "smile_32.bmp"),
                    RibbonButtonStyle.LargeWithText);

                var fileName = Path.Combine(debugFolder, "CuiTest1.cuix");

                File.Delete(fileName);

                customizationSection.SaveAs(fileName);
            } catch (System.Exception ex) {
                editor.WriteMessage(Environment.NewLine + ex.Message);
            }
        }*/
    }
}