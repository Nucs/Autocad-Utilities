using System.Linq;
using System.Threading;
using autonet.Extensions;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.GraphicsInterface;
using Autodesk.AutoCAD.Runtime;
using MoreLinq;

namespace autonet.Forms {
    public static class HighlevelCommands {

        
        /// <summary>
        ///     Custom method to make my work faster..
        /// </summary>
        [CommandMethod("Quicky", "qq", CommandFlags.UsePickSet | CommandFlags.Redraw | CommandFlags.NoPaperSpace)]
        public static void QuickQuackCommand() {
            var qc = QQManager.Selected;
            //todo Paste.

            var set = Quick.GetImpliedOrSelect();
            if (set == null || set.Count == 0)
                return;

            using (var tr = new QuickTransaction()) {
                //PromptSelectionOptions opts = new PromptSelectionOptions {MessageForAdding = "\nSelect cables to apply magic dust on: ", MessageForRemoval = "\n...Remove cables: "};
                //var set = tr.GetImpliedOrSelect(opts);
                var objs = set.Cast<SelectedObject>().Select(so => so.ObjectId).ToArray();
                foreach (var oid in objs) {
                    var o = tr.GetObject(oid, true);
                    //layer
                    if (qc.EnabledLayer)
                        o.Layer = qc.Layer;
                    //linetype
                    if (qc.EnabledLinetype)
                        o.Linetype = qc.Linetype;
                    //color
                    if (qc.EnabledColor) {
                        if (!string.IsNullOrEmpty(qc.ColorLabel)) {
                            var block = qc.ColorLabel == "BYBLOCK";
                            var layer = qc.ColorLabel == "BYLAYER";
                            if (block) {
                                o.ColorIndex = 0;
                                goto _postcolor;
                            }
                            if (layer) {
                                o.ColorIndex = 256;
                                goto _postcolor;
                            }
                            if (!block && !layer) {
                                goto _postcolor;
                            }
                        }
                        o.Color = Color.FromColor(qc.Color);
                    }
                    _postcolor:;
                }
/*
                if (tr.LayerTable.Has("EL-LT-CABL-160")) {
                    var lyr = tr.LayerTable["EL-LT-CABL-160"];
                    //check the layer and apply.
                    foreach (var e in set.GetObjectIds().Select(oid => oid.GetObject(tr))) {
                        e.SetLayerId(lyr, true);
                        //e.DowngradeOpen();
                    }
                }*/
                tr.Commit();

                //tr.Command("_.pedit", "_m", set, "_y", "_j", "", "_j", "", "_j", "", "_w", "0.2", "");
            }
        }

        [CommandMethod("Quicky", "qqconfig", CommandFlags.UsePickSet | CommandFlags.Redraw | CommandFlags.Modal | CommandFlags.NoPaperSpace)]
        public static void QuickConfigurationCommand() {
            QQManager.OpenConfiguration();
        }

        [CommandMethod("Quicky", "qqselect", CommandFlags.UsePickSet | CommandFlags.Redraw | CommandFlags.Modal | CommandFlags.NoPaperSpace)]
        public static void QuickSelectCommand() {
            QQManager.Select();
        }

        [CommandMethod("Quicky", "qqnew", CommandFlags.UsePickSet | CommandFlags.Redraw | CommandFlags.Modal | CommandFlags.NoPaperSpace)]
        public static void QuickNewCommand() {
            QQManager.CreateNewConfig();
        }

        [CommandMethod("Quicky", "asd", CommandFlags.UsePickSet | CommandFlags.Redraw | CommandFlags.NoPaperSpace)]
        public static void DoitCommand() {
                var f = new QQForm();
                f.ShowDialog();
        }

        [CommandMethod("Quicky", "ws", CommandFlags.UsePickSet | CommandFlags.Redraw | CommandFlags.NoPaperSpace)]
        public static void WindowSwapCommand() {
            var imp = Quick.GetImpliedOrSelect();
            if (imp == null) {
                Quick.WriteLine("[ws] No objects were selected.");
                return;
            }

            var all = Quick.SelectAll();
            if (all == null) {
                Quick.WriteLine("[ws] Failed selecting All.");
                return;
            }

            var rest = all.Cast<SelectedObject>().ExceptBy(imp.Cast<SelectedObject>(), o => o.ObjectId.Handle.Value).Select(o => o.ObjectId).ToSelectionSet(SelectionMethod.Crossing);
            Quick.SetSelected(rest);
        }

        [CommandMethod("Quicky", "wsw", CommandFlags.UsePickSet | CommandFlags.Redraw | CommandFlags.NoPaperSpace)]
        public static void WindowSwapSelectCommand() {
            var imp = Quick.GetImpliedOrSelect();
            if (imp == null) {
                Quick.WriteLine("[wsw] No objects were selected.");
                return;
            }
            Quick.ClearSelected();
            var all = Quick.GetImpliedOrSelect();
            if (all == null) {
                Quick.WriteLine("[wsw] Failed selecting Other.");
                return;
            }

            var rest = all.Cast<SelectedObject>().ExceptBy(imp.Cast<SelectedObject>(), o => o.ObjectId.Handle.Value).Select(o => o.ObjectId).ToSelectionSet(SelectionMethod.Crossing);
            Quick.SetSelected(rest);
        }

        [CommandMethod("Quicky", "uh", CommandFlags.UsePickSet | CommandFlags.Redraw | CommandFlags.NoPaperSpace)]
        public static void UnHideCommand() {
            var cmd = "uh";
            
            var all = Quick.SelectAll();
            if (all == null) {
                Quick.WriteLine($"[{cmd}] Failed selecting All.");
                return;
            }

            using (var tr = new QuickTransaction()) {
                var rest = all.Cast<SelectedObject>().Select(o => o.ObjectId.GetObject(tr,true));
                foreach (var o in rest) {
                    o.Visible = true;
                }

                tr.Commit();
                Quick.ClearSelected();
            }
        }
        [CommandMethod("Quicky", "h", CommandFlags.UsePickSet | CommandFlags.Redraw | CommandFlags.NoPaperSpace)]
        public static void HideCommand() {
            var cmd = "h";
            var imp = Quick.GetImpliedOrSelect();
            if (imp == null) {
                Quick.WriteLine($"[{cmd}] No objects were selected.");
                return;
            }

            using (var tr = new QuickTransaction()) {
                var rest = imp.Cast<SelectedObject>().Select(o => o.ObjectId.GetObject(tr,true));
                foreach (var o in rest) {
                    o.Visible = false;
                }

                tr.Commit();
                Quick.ClearSelected();
            }
        }

        [CommandMethod("Quicky", "w", CommandFlags.UsePickSet | CommandFlags.Redraw | CommandFlags.NoPaperSpace)]
        public static void WidthCommand() {
            var set = Quick.GetImpliedOrSelect();
            if (set == null) {
                Quick.WriteLine($"[{Quick.CurrentCommand}] No objects were selected.");
                return;
            }
            using (var tr = new QuickTransaction()) {
                var dbl = Quick.Editor.GetDouble(new PromptDoubleOptions("Please select width: "){AllowNegative = false, DefaultValue = Quick.Bag.Get("[w]width", 0.4d)});
                if (dbl.Status != PromptStatus.OK) {
                    Quick.WriteLine($"[{Quick.CurrentCommand}] Failed selecting double.");
                    return;
                }
                double val = (double) (Quick.Bag["[w]width"] = dbl.Value);
                //tr.Command("_.pedit", "_m", set, "_n", "_w", dbl.Value.ToString(), "");
                foreach (var e in set.GetObjectIds().Select(o=>tr.GetObject(o,true))) {
                    switch (e) {
                        case Autodesk.AutoCAD.DatabaseServices.Polyline p:
                            p.SetGlobalWidth(val);
                            break;
                        case Circle c:
                            //c.Thickness = val;
                            break;
                        case Arc a:
                            //a.Thickness = val;
                            break;
                        case Line l:
                            //l.Thickness = val;
                            break;
                    }
                }
                tr.Commit();
            }
        }
        
        [CommandMethod("Quicky", "f", CommandFlags.UsePickSet | CommandFlags.Redraw | CommandFlags.NoPaperSpace)]
        public static void FilletCommand() {
            var cmd = "f";
            var set = Quick.GetImpliedOrSelect();
            if (set == null) {
                Quick.WriteLine($"[{cmd}] No objects were selected.");
                return;
            }
            using (var tr = new QuickTransaction()) {
                var dbl = Quick.Editor.GetDouble(new PromptDoubleOptions("Please select width: "){AllowNegative = false, DefaultValue = Quick.Bag.Get($"[{cmd}]width", 0.4d)});
                if (dbl.Status != PromptStatus.OK) {
                    Quick.WriteLine($"[{cmd}] Failed selecting double.");
                    return;
                }

                double val = (double) (Quick.Bag[$"[{cmd}]width"] = dbl.Value);
                //tr.Command("_.pedit", "_m", set, "_n", "_w", dbl.Value.ToString(), "");
                foreach (var e in set.GetObjectIds().Select(o=>tr.GetObject(o,true))) {
                    switch (e) {
                        case Autodesk.AutoCAD.DatabaseServices.Polyline p:
                            p.FilletAll(val);
                            break;
                        default: break;
                    }
                }
                tr.Commit();
            }
        }

        /*
        [CommandMethod("Quicky", "", CommandFlags.UsePickSet | CommandFlags.Redraw | CommandFlags.NoPaperSpace)]
        public static void Command() {
            var set = Quick.GetImpliedOrSelect();
            if (set == null) {
                Quick.WriteLine($"[{Quick.CurrentCommand}] No objects were selected.");
                return;
            }
            using (var tr = new QuickTransaction()) {
                
            }

            Quick.SetSelected();
        }
                */
    }
}