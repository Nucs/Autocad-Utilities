using System.Linq;
using autonet.Extensions;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.GraphicsInterface;
using Autodesk.AutoCAD.Runtime;
using MoreLinq;

namespace autonet.Forms {
    public static class HighlevelCommands {
        [CommandMethod("Quicky", "asd", CommandFlags.UsePickSet | CommandFlags.Redraw | CommandFlags.NoPaperSpace)]
        public static void DoitCommand() {
            var imp = Quick.GetImpliedOrSelect();
            using (var tr = new QuickTransaction()) {
                var c = imp?.Count > 0 ? (Entity) imp[0].ObjectId.GetObject(tr) : (Entity) new Circle();
                var f = new QQForm(c);
                f.ShowDialog();
                ;
                tr.Commit();
            }
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