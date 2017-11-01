using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using autonet.Extensions;
using autonet.Settings;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using Common;
using YourCAD.Utilities;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace autonet {
    public static class QuickCommands {
        //("Quicky", "fq", CommandFlags.UsePickSet | CommandFlags.Redraw | CommandFlags.NoPaperSpace)]
        /*/// <summary>
        ///     does hbreg to selection
        /// </summary>
        [CommandMethod("Quicky", "qo", CommandFlags.UsePickSet | CommandFlags.Modal | CommandFlags.NoPaperSpace)]
        public static void QuickOutlineCommand() {
            using (var tr = new QuickTransaction()) {
                var sel = tr.Doc.Editor.GetSelection();
                if (sel.Status != PromptStatus.OK)
                    return;
                if (sel.Value.Count == 0)
                    return;

                tr.Command("hbreg");
                tr.Commit();
            }
        }

        /// <summary>
        ///     does hbreg to selection
        /// </summary>
        [CommandMethod("Quicky", "qod", CommandFlags.UsePickSet | CommandFlags.Modal | CommandFlags.NoPaperSpace)]
        public static void QuickOutlineDeleteCommand() {
            using (var tr = new QuickTransaction()) {
                var sel = tr.Doc.Editor.GetSelection();
                if (sel.Status != PromptStatus.OK)
                    return;
                if (sel.Value.Count == 0)
                    return;

                tr.Command("hbreg", sel.Value);

                foreach (SelectedObject sl in sel.Value) {
                    sl.ObjectId.GetObject(tr).Erase();
                }
                tr.Commit();
            }
        }*/
        /// <summary>
        ///     Counts the lengths of all the selected objects.<br></br>
        ///     In case of a BlockReference, it will search for a property named Distance or Length.
        /// </summary>
        [CommandMethod("Quicky", "sq", CommandFlags.UsePickSet | CommandFlags.Redraw | CommandFlags.NoPaperSpace)]
        public static void SummarizeLengthsCommand() {
            using (var tr = new QuickTransaction()) {
                var set = tr.GetImpliedOrSelect();
                double l = 0;
                if (set == null || set.Count == 0)
                    return;
                var an = new List<string>(); //already announced list..
                foreach (SelectedObject o in set) {
                    if (o != null) {
                        var e = tr.GetObject(o.ObjectId, OpenMode.ForWrite) as Entity;
                        string name = null;
                        switch (e) {
                            case null:
                                continue;
                            case Polyline p:
                                l += p.Length;
                                break;
                            case Circle c:
                                l += c.Diameter;
                                break;
                            case Arc a:
                                l += a.Length;
                                break;
                            case Line li:
                                l += li.Length;
                                break;
                            case Spline sp:
                                l += sp.GetLength();
                                break;
                            case Curve sp:
                                l += sp.GetLength();
                                break;
                            case Mline mline:
                                if (mline.NumberOfVertices > 1)
                                    for (int i = 1; i < mline.NumberOfVertices; i++)
                                        l += mline.VertexAt(i - 1).DistanceTo(mline.VertexAt(i));

                                break;
                            case BlockReference br:
                                if (br.DynamicBlockReferencePropertyCollection.Count == 0) {
                                    if (an.Contains(br.Name))
                                        break;
                                    an.Add(br.Name);
                                    tr.WriteLine($"Name: {br.Name}, {br.BlockName} doesn't have Distance property!");
                                    break;
                                }

                                foreach (var att in br.DynamicBlockReferencePropertyCollection.Cast<DynamicBlockReferenceProperty>()) {
                                    if (att.PropertyName.Equals("distance", StringComparison.InvariantCultureIgnoreCase) || att.PropertyName.Equals("length", StringComparison.InvariantCultureIgnoreCase)) {
                                        var val = att.Value ?? string.Empty;
                                        if (string.IsNullOrEmpty(val.ToString()) == false && double.TryParse(val.ToString(), out double res)) {
                                            l += res;
                                            goto _br_exit;
                                        } else {
                                            tr.WriteLine($"{br.Name}, {br.BlockName} with property named {att.PropertyName} has no numeric value.");
                                            goto _br_exit;
                                        }
                                    }
                                }

                                if (an.Contains(br.Name))
                                    break;
                                an.Add(br.Name);
                                tr.WriteLine($"{br.Name}, {br.BlockName} doesn't have Distance/Length property!");
                                _br_exit:

                                break;
                            case DBText _:
                            case MLeader _:
                            case AlignedDimension _:
                            case Wipeout _:
                                name = e.GetType().Name;
                                if (an.Contains(name))
                                    break;
                                an.Add(name);
                                tr.WriteLine($"{name} cant be calculated using this method!");
                                break;
                            default:
                                tr.WriteLine("Unmappped: " + e.GetType().FullName);
                                break;
                        }
                    }
                }

                tr.WriteLine($"Length: " + l.ToString("##.000"));
            }
        }

        [CommandMethod("XrefGraph")]
        public static void XrefGraph() {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            foreach (var node in GetNodes(doc)) {
                doc.Editor.WriteMessage($"\n{node.Name} | {node.Loaded} | {node.FileName}");
            }
        }

        public static XrefNode[] GetNodes(Document doc, QuickTransaction tr=null) {
            string strnullfy(string s) => string.IsNullOrEmpty(s.Trim()) ? null : s;
            Database db = doc.Database;
            Editor ed = doc.Editor;
            bool tr_new = tr == null;
            var ret = new List<XrefNode>();
            try {
                tr = tr_new ? new QuickTransaction() : tr;

                db.ResolveXrefs(true, false);
                XrefGraph xg = db.GetHostDwgXrefGraph(true);

                GraphNode root = xg.RootNode;
                for (int o = 0; o < root.NumOut; o++) {
                    XrefGraphNode child = root.Out(o) as XrefGraphNode;
                    if (child == null) {
                        ed.WriteMessage($"\nUnable to load xref of type {root.Out(o)?.GetType().FullName}");
                        continue;
                    }
                    //if (child.XrefStatus == XrefStatus.Resolved) {
                    //BlockTableRecord bl = tr.GetObject(child.BlockTableRecordId, OpenMode.ForRead) as BlockTableRecord;
                    var t = child.GetType();
                    try {
                        Quick.Editor.WriteMessage("\n" + child.Database.Filename);
                    } catch { }
                    var status = child.XrefStatus;

                    ret.Add(new XrefNode(
                        strnullfy(child.Name) ?? strnullfy(child.Database?.ProjectName) ?? strnullfy(Path.GetFileNameWithoutExtension(child.Database?.Filename)) ?? "",
                        child.Database.Filename,
                        status == XrefStatus.Resolved || status == XrefStatus.FileNotFound || status == XrefStatus.Unresolved));
                }
            } finally {
                if (tr_new) 
                    tr?.Dispose();
            }
            return ret.ToArray();
        } 

        public class XrefNode {
            public string Name { get; set; }
            public string FileName { get; set; }
            public bool Loaded { get; set; }

            public XrefNode(string name, string fileName, bool loaded) {
                Name = name;
                FileName = fileName;
                Loaded = loaded;
            }
            /*/// <summary>
            /// Should this xref be loaded at any stage.
            /// </summary>
            public bool ShouldBeLoaded => */
        }

        // Recursively prints out information about the XRef's hierarchy
        private static void printChildren(GraphNode i_root, string i_indent, Editor i_ed, Transaction i_Tx) { }

        /// <summary>
        ///     Counts the lengths of all the selected objects.<br></br>
        ///     In case of a BlockReference, it will search for a property named Distance or Length.
        /// </summary>
        [CommandMethod("Quicky", "ss", CommandFlags.NoPaperSpace | CommandFlags.UsePickSet | CommandFlags.Redraw)]
        public static void SelectCommand() {
            using (var tr = new QuickTransaction()) {
                var set = tr.GetImpliedOrSelect();
                if (set == null)
                    return;

                tr.WriteLine($"Count: " + set.Count);

                tr.SetImpliedSelection(set);
                tr.StringCommand("C2P ");
                tr.SetImpliedSelection(set = tr.SelectLast().Value ?? SelectionSet.FromObjectIds(new ObjectId[0]));
                tr.Commit();
            }
        }

        /// <summary>
        ///     Counts the lengths of all the selected objects.<br></br>
        ///     In case of a BlockReference, it will search for a property named Distance or Length.
        /// </summary>
        [CommandMethod("Quicky", "se", CommandFlags.NoPaperSpace | CommandFlags.Redraw | CommandFlags.UsePickSet)]
        public static void SelectOtherCommand() {
            using (var tr = new QuickTransaction()) {
                tr.SetSelected(tr.SelectAll().Value);
            }
        }

        [CommandMethod("Quicky", "sw", CommandFlags.UsePickSet)]
        public static void SelectInWindowCommand() {
            //todo Finish this later..
            using (var tr = new QuickTransaction()) {
                var sel = tr.GetSelection();
                if (sel.Status != PromptStatus.OK || sel.Value.Count == 0) {
                    tr.WriteLine("No objects were selected in the first place.");
                    return;
                }
                var window = tr.GetSelection();
                if (window.Status != PromptStatus.OK || window.Value.Count == 0) {
                    return;
                }

                var a = sel.Value.GetObjectIds();
                var win = window.Value.GetObjectIds();

                tr.SetImpliedSelection(SelectionSet.FromObjectIds(a.Intersect(win).ToArray()));
                /*foreach (var entity in s.Value.GetObjectIds().Select(o=>o.GetObject(tr))) {
                        
                    }*/
                tr.Commit();
            }
        }

        [CommandMethod("Quicky", "l2p", CommandFlags.UsePickSet | CommandFlags.Modal | CommandFlags.NoPaperSpace)]
        public static void LineToPolyCommand() {
            using (var tr = new QuickTransaction()) {
                EntityExtensions.LineToPoly(tr);
                tr.Commit();
            }
        }

        [CommandMethod("Quicky", "sss", CommandFlags.NoPaperSpace | CommandFlags.UsePickSet | CommandFlags.Redraw)]
        public static void AAAnyToPolyCommand() {
            using (var tr = new QuickTransaction()) {
                CommandLineHelper.ExecuteStringOverInvoke("E2P ");
                tr.Commit();
            }
        }

        [CommandMethod("Quicky", "2p", CommandFlags.NoPaperSpace | CommandFlags.UsePickSet | CommandFlags.Redraw)]
        public static void AnyToPolyCommand() {
            using (var tr = new QuickTransaction()) {
                PromptSelectionOptions psOpts = new PromptSelectionOptions {MessageForAdding = "\nSelect lines to convert: ", MessageForRemoval = "\n...Remove lines: "};
                var psRes = tr.GetSelection(psOpts);
                if (psRes.Status != PromptStatus.OK)
                    return;
                var sel = psRes.Value.GetObjectIds().Select(o => o.GetObject(tr)).ToList();
                var circles = sel.TakeoutWhereType<Entity, Circle>().ToSelectionSet();
                var elipses = sel.TakeoutWhereType<Entity, Ellipse>().ToSelectionSet();
                var l = new List<SelectionSet>();

                if (sel.Count > 0) {
                    tr.SetSelected(sel.ToSelectionSet());
                    tr.Command("_.pedit", "_m", sel.ToSelectionSet(), "", "_j", "", "_j", "", "_j", "", "");
                    //l.Add(tr.SelectPrevious().Value);
                }
                if (elipses.Count > 0) {
                    using (var trr = new QuickTransaction()) {
                        trr.SetSelected(elipses);
                        CommandLineHelper.ExecuteStringOverInvoke("E2P ");
                        //trr.StringCommand("E2P ");
                        // l.Add(trr.SelectImplied().Value);
                        trr.Commit();
                    }
                }

                if (circles.Count > 0) {
                    using (var trr = new QuickTransaction()) {
                        trr.SetSelected(circles);
                        CommandLineHelper.ExecuteStringOverInvoke("C2P ");
                        //l.Add(trr.SelectImplied().Value);
                        trr.Commit();
                    }
                }


                tr.Commit();
            }
        }
    }
}