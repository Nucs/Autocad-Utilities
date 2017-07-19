using System;
using System.IO;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace XrefAttachAtZero {
    public static class Extensions {
        /// <summary>
        /// Attaches the specified Xref to the current space in the current drawing.
        /// </summary>
        /// <param name="path">Path to the drawing file to attach as an Xref.</param>
        /// <param name="pos">Position of Xref in WCS coordinates.</param>
        /// <param name="name">Optional name for the Xref.</param>
        /// <returns>Whether the attach operation succeeded.</returns>
        public static bool XrefAttachAndInsert(this Database db, string path, Point3d pos, string name = null) {
            var ret = false;
            if (!File.Exists(path))
                return ret;

            if (String.IsNullOrEmpty(name))
                name = Path.GetFileNameWithoutExtension(path);

            try {
                using (var tr = db.TransactionManager.StartOpenCloseTransaction()) {
                    var xId = db.AttachXref(path, name);
                    if (xId.IsValid) {
                        var btr =
                            (BlockTableRecord) tr.GetObject(db.CurrentSpaceId, OpenMode.ForWrite);

                        var br = new BlockReference(pos, xId);
                        btr.AppendEntity(br);
                        tr.AddNewlyCreatedDBObject(br, true);

                        ret = true;
                    }
                    tr.Commit();
                }
            } catch (Autodesk.AutoCAD.Runtime.Exception) { }

            return ret;
        }
    }

    public class Commands {
        [CommandMethod("XAO")]
        public void XrefAttachAtOrigin() {
            var doc = Application.DocumentManager.MdiActiveDocument;
            if (doc == null)
                return;
            var db = doc.Database;
            var ed = doc.Editor;

            // Ask the user to specify a file to attach

            var opts = new PromptOpenFileOptions("Select Reference File");
            opts.Filter = "Drawing (*.dwg)|*.dwg";
            var pr = ed.GetFileNameForOpen(opts);

            if (pr.Status == PromptStatus.OK) {
                // Attach the specified file and insert it at the origin

                var res = db.XrefAttachAndInsert(pr.StringResult, Point3d.Origin);

                ed.WriteMessage(
                    "External reference {0}attached at the origin.",
                    res ? "" : "not "
                );
            }
        }
    }
}