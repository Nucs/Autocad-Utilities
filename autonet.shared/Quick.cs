using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using autonet.Extensions;
using autonet.Settings;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Common;
using YourCAD.Utilities;

namespace autonet {
    public static class Quick {
        /// <summary>
        ///     A holder for session-wide config.
        /// </summary>
        public static SettingsBag Bag { get; } = new SettingsBag(Paths.ConfigFile("StateSave").FullName);
        public static Document Doc => CurrentDocument;
        public static Document MdiDoc => CurrentDocument;
        public static Editor Editor => Doc.Editor;

        public static string CurrentCommand => MdiDoc.CommandInProgress;

#if AUTOCAD13

#else

#endif

        public static Document CurrentDocument =>
#if AUTOCAD13
            Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;

#else
            Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.CurrentDocument;

#endif

        public static void SetImpliedSelection(SelectionSet selectionSet) {
            Editor.SetImpliedSelection(selectionSet);
        }

        /// <summary>
        ///     see: https://knowledge.autodesk.com/search-result/caas/CloudHelp/cloudhelp/2015/ENU/AutoCAD-NET/files/GUID-D4987D00-1164-4217-A82E-B8B49FFB7A29-htm.html
        /// </summary>
        /// <param name="selectedObjects"></param>
        public static void SetImpliedSelection(ObjectId[] selectedObjects) {
            Editor.SetImpliedSelection(selectedObjects);
        }

        public static void WriteLine(object s) {
            Write(s + "\n");
        }

        public static void Write(object s) {
            Editor.WriteMessage(s + "\n");
        }

        public static PromptSelectionResult GetSelection(PromptSelectionOptions options, SelectionFilter filter) {
            return Editor.GetSelection(options, filter);
        }


        public static PromptSelectionResult GetSelection(PromptSelectionOptions options) {
            return Editor.GetSelection(options);
        }

        public static PromptSelectionResult GetSelection(SelectionFilter filter) {
            return Editor.GetSelection(filter);
        }

        public static PromptSelectionResult GetSelection() {
            return Editor.GetSelection();
        }

        public static void Regen() {
            Editor.Regen();
        }

        public static SelectionSet SelectAll(SelectionFilter filter) {
            var sel = Editor.SelectAll(filter);
            if (sel.Status != PromptStatus.OK || sel.Value.Count == 0) {
                return null;
            }
            return sel.Value;
        }

        public static SelectionSet SelectAll() {
            var sel = Editor.SelectAll();
            if (sel.Status != PromptStatus.OK || sel.Value.Count == 0) {
                return null;
            }
            return sel.Value;
        }

        public static PromptSelectionResult SelectCrossingWindow(Point3d pt1, Point3d pt2, SelectionFilter filter, bool forceSubEntitySelection) {
            return Editor.SelectCrossingWindow(pt1, pt2, filter, forceSubEntitySelection);
        }

        public static PromptSelectionResult SelectCrossingWindow(Point3d pt1, Point3d pt2, SelectionFilter filter) {
            return Editor.SelectCrossingWindow(pt1, pt2, filter);
        }

        public static PromptSelectionResult SelectCrossingWindow(Point3d pt1, Point3d pt2) {
            return Editor.SelectCrossingWindow(pt1, pt2);
        }

        public static PromptSelectionResult SelectCrossingPolygon(Point3dCollection polygon, SelectionFilter filter) {
            return Editor.SelectCrossingPolygon(polygon, filter);
        }

        public static PromptSelectionResult SelectCrossingPolygon(Point3dCollection polygon) {
            return Editor.SelectCrossingPolygon(polygon);
        }

        public static PromptSelectionResult SelectFence(Point3dCollection fence, SelectionFilter filter) {
            return Editor.SelectFence(fence, filter);
        }

        public static PromptSelectionResult SelectFence(Point3dCollection fence) {
            return Editor.SelectFence(fence);
        }

        public static PromptSelectionResult SelectWindow(Point3d pt1, Point3d pt2, SelectionFilter filter) {
            return Editor.SelectWindow(pt1, pt2, filter);
        }

        public static PromptSelectionResult SelectWindow(Point3d pt1, Point3d pt2) {
            return Editor.SelectWindow(pt1, pt2);
        }

        public static PromptSelectionResult SelectWindowPolygon(Point3dCollection polygon, SelectionFilter filter) {
            return Editor.SelectWindowPolygon(polygon, filter);
        }

        public static PromptSelectionResult SelectWindowPolygon(Point3dCollection polygon) {
            return Editor.SelectWindowPolygon(polygon);
        }

        public static PromptSelectionResult SelectImplied() {
            return Editor.SelectImplied();
        }

        public static PromptSelectionResult SelectLast() {
            return Editor.SelectLast();
        }

        public static PromptSelectionResult SelectPrevious() {
            return Editor.SelectPrevious();
        }
        public static ObjectId? SelectSingle() {
            var ret = Editor.GetEntity("Select a single object:");
            if (ret.Status == PromptStatus.OK) {
                return ret.ObjectId;
            }
            return null;
        }

        /// <summary>
        ///     Execute a command much a like in LISP. Instead of spaces write commas.<br></br>
        ///     An empty string is equivalent to pressing enter.
        ///     <example>
        ///         //LISP <br></br>//selection is set loaded before the method call<br></br>
        ///         (command "_.pedit" "_m" selection "" "_j" "" "")<br></br>
        ///         //C#<br></br>
        ///         quicktrans.Command("_.pedit", "_m", <br></br>quicktrans.GetSelection().Value, "", "_j", "", "");
        ///     </example>
        /// </summary>
        /// <param name="parameter"></param>
        public static void Command(params object[] parameter) {
            Editor.Command(parameter);
        }

#if !AUTOCAD13

        /// <summary>
        ///     Execute a command much a like in LISP. Instead of spaces write commas.<br></br>
        ///     An empty string is equivalent to pressing enter.
        ///     <example>
        ///         //LISP <br></br>//selection is set loaded before the method call<br></br>
        ///         (command "_.pedit" "_m" selection "" "_j" "" "")<br></br>
        ///         //C#<br></br>
        ///         quicktrans.Command("_.pedit", "_m", <br></br>quicktrans.GetSelection().Value, "", "_j", "", "");
        ///     </example>
        /// </summary>
        /// <param name="parameter"></param>
        public static Autodesk.AutoCAD.EditorInput.Editor.CommandResult CommandAsync(params object[] parameter) {
            return Editor.CommandAsync(parameter);
        }

#endif

        /// <summary>
        ///     Sends a string command to the commandline, dont forget to add space for it to execute!
        /// </summary>
        public static void StringCommand(string command, bool hide = false) {
            Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument.SendStringToExecute(command, true, false, !hide);
        }

        public static SelectionSet GetImpliedOrSelect() {
            var sel = Quick.SelectImplied();
            if (sel.Status != PromptStatus.OK || sel.Value.Count == 0) {
                sel = Quick.GetSelection();
                if (sel.Status != PromptStatus.OK || sel.Value.Count == 0) {
                    return null;
                }
            }
            return sel.Value;
        }

        public static SelectionSet GetImpliedOrSelect(SelectionFilter f) {
            var sel = Quick.SelectImplied();
            if (sel.Status != PromptStatus.OK || sel.Value.Count == 0) {
                sel = Quick.GetSelection(f);
                if (sel.Status != PromptStatus.OK || sel.Value.Count == 0) {
                    return null;
                }
            }
            return sel.Value;
        }

        public static SelectionSet GetImpliedOrSelect(PromptSelectionOptions f) {
            var sel = Quick.SelectImplied();
            if (sel.Status != PromptStatus.OK || sel.Value.Count == 0) {
                sel = Quick.GetSelection(f);
                if (sel.Status != PromptStatus.OK || sel.Value.Count == 0) {
                    return null;
                }
            }
            return sel.Value;
        }

        public static SelectionSet GetImpliedOrSelect(PromptSelectionOptions f, SelectionFilter ff) {
            var sel = Quick.SelectImplied();
            if (sel.Status != PromptStatus.OK || sel.Value.Count == 0) {
                sel = Quick.GetSelection(f, ff);
                if (sel.Status != PromptStatus.OK || sel.Value.Count == 0) {
                    return null;
                }
            }
            return sel.Status != PromptStatus.OK ? null : sel.Value;
        }
        public static void SetSelected(ObjectId[] ss, bool runsssetfirst = false) {
            Quick.SetImpliedSelection(ss);
            //Quick.SetImpliedSelection(Quick.SelectImplied().Value);
            //if (runsssetfirst)
            //Quick.Command("(sssetfirst nil (ssget \"I\")) ", false);
        }
        public static void SetSelected(SelectionSet ss, bool runsssetfirst = false) {
            Quick.SetImpliedSelection(ss);
            //Quick.SetImpliedSelection(Quick.SelectImplied().Value);
            //if (runsssetfirst)
            //Quick.Command("(sssetfirst nil (ssget \"I\")) ", false);
        }

        public static void ClearSelected() {
            Quick.SetImpliedSelection(selectionSet: null);
        }

        public static List<LinetypeTableRecord> Linetypes {
            get {
                using (var tr = new QuickTransaction())
                    return tr.LineTable.Cast<ObjectId>()
                        .Select(o => tr.GetObject(o, OpenMode.ForRead) as LinetypeTableRecord)
                        .ToList();
            }
        }

        public static List<LayerTableRecord> Layers {
            get {
                using (var tr = new QuickTransaction())
                    return tr.LayerTable.Cast<ObjectId>()
                        .Select(o => tr.GetObject(o, OpenMode.ForRead) as LayerTableRecord)
                        .ToList();
            }
        }

        /// <summary>
        ///     Ask a question, dont add : or ? at the end nor \n at the beggining
        /// </summary>
        /// <param name="question"></param>
        /// <param name="defaultval"></param>
        /// <returns></returns>
        public static bool? AskQuestion(string question, bool defaultval) {
            recapture:
            PromptStringOptions opts = new PromptStringOptions($"\n{question}: [Y/N] ");
            opts.AllowSpaces = false;
            opts.DefaultValue = defaultval ? "Y" : "N";
            opts.UseDefaultValue = true;
            
            PromptResult ret = Editor.GetString(opts);
            if (ret.Status == PromptStatus.Cancel)
                return null;
            var txt = ret.StringResult;
            switch (txt.ToLowerInvariant()) {
                case "true":
                case "yes":
                case "y":
                case "1":
                    return true;
                case "false":
                case "no":
                case "n":
                case "0":
                    return false;
            }

            Editor.WriteMessage("\nInvalid Input [Y/N].");
            goto recapture;
        }

    }
}