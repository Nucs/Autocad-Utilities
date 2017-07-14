using System;
using System.Collections.Generic;
using System.Linq;
using autonet.Extensions;
using autonet.lsp;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;

namespace autonet {
    public static class QuickCommands {
        /// <summary>
        ///     Custom method to make my work faster..
        /// </summary>
        [CommandMethod("Quicky", "qq", CommandFlags.UsePickSet | CommandFlags.Modal | CommandFlags.NoPaperSpace)]
        public static void QuickCableCommand() {
            using (var tr = new QuickTransaction()) {
                tr.WriteLine(Application.Settings.FileName);
                PromptSelectionOptions psOpts = new PromptSelectionOptions {MessageForAdding = "\nSelect cables to apply magic dust on: ", MessageForRemoval = "\n...Remove cables: "};
                var psRes = tr.GetSelection(psOpts);
                if (psRes.Status != PromptStatus.OK)
                    return;

                if (tr.LayerTable.Has("EL-LT-CABL-160")) {
                    var lyr = tr.LayerTable["EL-LT-CABL-160"];
                    //check the layer and apply.
                    foreach (var e in psRes.Value.GetObjectIds().Select(oid=>oid.GetObject(tr))) {
                        e.SetLayerId(lyr, true);   
                        //e.DowngradeOpen();
                    }
                }
                tr.Commit();

                tr.Command("_.pedit", "_m", psRes.Value, "_y", "_j", "", "_j", "", "_j", "", "_w", "0.2", "");
            }
        }

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
        ///     Fits all selected polylines.
        /// </summary>
        [CommandMethod("Quicky", "fq", CommandFlags.UsePickSet | CommandFlags.Modal | CommandFlags.NoPaperSpace)]
        public static void FilletPolylinesCommand() {
            using (var tr = new QuickTransaction()) {
                PromptSelectionOptions psOpts = new PromptSelectionOptions {MessageForAdding = "\nSelect cables to fillet: ", MessageForRemoval = "\n...Remove cables: "};
                var selq = tr.GetSelection(psOpts);
                if (selq.Status != PromptStatus.OK)
                    return;

                tr.Command("_.pedit", "_m", selq.Value, "_f","");
                tr.Commit();
            }
        }

        /// <summary>
        ///     Counts the lengths of all the selected objects.<br></br>
        ///     In case of a BlockReference, it will search for a property named Distance or Length.
        /// </summary>
        [CommandMethod("Quicky", "sq", CommandFlags.UsePickSet | CommandFlags.Modal | CommandFlags.NoPaperSpace)]
        public static void SummarizeLengthsCommand() {
            using (var tr = new QuickTransaction()) {
                var sel = tr.Doc.Editor.GetSelection();
                double l = 0;
                if (sel.Status != PromptStatus.OK)
                    return;
                string name = null;
                var set = sel.Value;
                var an = new List<string>(); //already announced list..
                foreach (SelectedObject o in set) {
                    if (o != null) {
                        var e = tr.GetObject(o.ObjectId, OpenMode.ForWrite) as Entity;
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
                                break; //l+= sp.ToPolyline().
                            case Curve sp:
                                l += sp.GetLength();
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

                            case MLeader ldr:
                                name = ldr.GetType().Name;
                                if (an.Contains(name))
                                    break;
                                an.Add(name);
                                tr.WriteLine($"{name} are not counted using this method!");
                                break;
                            case DBText dbtext:
                                name = dbtext.GetType().Name;
                                if (an.Contains(name))
                                    break;
                                an.Add(name);
                                tr.WriteLine($"{name} are not counted using this method!");
                                break;
                            case AlignedDimension dm:
                                name = dm.GetType().Name;
                                if (an.Contains(name))
                                    break;
                                an.Add(name);
                                tr.WriteLine($"{name} are not counted using this method!");
                                break;
                            case Wipeout wp:
                                name = wp.GetType().Name;
                                if (an.Contains(name))
                                    break;
                                an.Add(name);
                                tr.WriteLine($"{name} are not counted using this method!");
                                break;
                            default:
                                tr.WriteLine("Unmappped: " + e.GetType().FullName);
                                break;
                        }
                    }
                }
                tr.WriteLine($"Length: " + l.ToString("##.000"));
                tr.Commit();
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

        [CommandMethod("Quicky", "2p", CommandFlags.UsePickSet | CommandFlags.Modal | CommandFlags.NoPaperSpace)]
        public static void AnyToPolyCommand() {
            using (var tr = new QuickTransaction()) {
                PromptSelectionOptions psOpts = new PromptSelectionOptions {MessageForAdding = "\nSelect lines to convert: ", MessageForRemoval = "\n...Remove lines: "};
                var psRes = tr.GetSelection(psOpts);
                if (psRes.Status != PromptStatus.OK)
                    return;
                var sel = psRes.Value.GetObjectIds().Select(o => o.GetObject(tr)).ToList();
                var circles = sel.TakeoutWhereType<Entity,Circle>().ToSelectionSet();
                var elipses = sel.TakeoutWhereType<Entity, Ellipse>().ToSelectionSet();
                if (sel.Count>0)
                    tr.Command("_.pedit", "_m", sel.ToSelectionSet(), "", "_j", "", "_j", "", "_j", "", "");
                tr.SetImpliedSelection(circles);
                if (circles.Count>0)
                        tr.Command("CircleToPoly", circles);

                tr.Commit();
            }
        }
    }
}