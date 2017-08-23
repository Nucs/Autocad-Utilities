using System.Linq;
using System.Threading;
using autonet.Extensions;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.GraphicsInterface;
using Autodesk.AutoCAD.Runtime;
using Linq.Extras;
using MoreLinq;
using Polyline = Autodesk.AutoCAD.DatabaseServices.Polyline;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using BizArk.Core.Extensions.ObjectExt;

namespace autonet.Forms {
    public static class HighlevelCommands {
        /// <summary>
        ///     Custom method to make my work faster..
        /// </summary>
        [CommandMethod("Quicky", "qq", CommandFlags.UsePickSet | CommandFlags.Redraw)]
        public static void QuickQuackCommand() {
            var qc = QQManager.Selected;

            var set = Quick.GetImpliedOrSelect();
            if (set == null || set.Count == 0)
                return;

            using (var tr = new QuickTransaction()) {
                //PromptSelectionOptions opts = new PromptSelectionOptions {MessageForAdding = "\nSelect cables to apply magic dust on: ", MessageForRemoval = "\n...Remove cables: "};
                //var set = tr.GetImpliedOrSelect(opts);
                List<ObjectId> polylines = new List<ObjectId>();
                List<ObjectId> linearc = new List<ObjectId>();
                ObjectId[] objs = set.Cast<SelectedObject>().Select(so => so.ObjectId).ToArray();
                for (var i = 0; i < objs.Length; i++) {
                    var oid = objs[i];
                    var o = tr.GetObject(oid, true);

                    if (qc.ConvertAllToPolyline && o is Polyline == false) {
                        var poly = tr.ConvertToPolyline(o);
                        if (poly != null) //if was successful
                            objs[i] = (o = poly).ObjectId;
                    }

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
                    if (qc.EnabledWidth && o is Polyline p) {
                        p.SetGlobalWidth(qc.Width);
                    }

                    if (qc.EnabledThickness) {
                        o.GetType().GetProperty("Thickness")?.SetValue(o, qc.Thickness, null);
                    }

                    _postcolor:
                    ;
                }

                if (qc.EnabledWidth) { }
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

        [CommandMethod("Quicky", "qqconfig", CommandFlags.UsePickSet | CommandFlags.Redraw | CommandFlags.Modal)]
        public static void QuickConfigurationCommand() {
            QQManager.OpenConfiguration();
        }

        [CommandMethod("Quicky", "qqselect", CommandFlags.UsePickSet | CommandFlags.Redraw | CommandFlags.Modal)]
        public static void QuickSelectCommand() {
            QQManager.Select();
        }

        [CommandMethod("Quicky", "qqnew", CommandFlags.UsePickSet | CommandFlags.Redraw | CommandFlags.Modal | CommandFlags.NoPaperSpace)]
        public static void QuickNewCommand() {
            QQManager.CreateNewConfig();
        }

        [CommandMethod("Quicky", "asd", CommandFlags.UsePickSet | CommandFlags.Redraw | CommandFlags.NoPaperSpace)]
        public static void DoitCommand() {
            var imp = Quick.GetImpliedOrSelect();
            using (var tr = new QuickTransaction()) {
                tr.Commit();
            }
        }

        [CommandMethod("Quicky", "ws", CommandFlags.UsePickSet | CommandFlags.Redraw)]
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

        [CommandMethod("Quicky", "ww", CommandFlags.UsePickSet | CommandFlags.Redraw)]
        public static void WindowOnlyInsideCommand() {
            var imp = Quick.GetImpliedOrSelect();
            if (imp == null) {
                Quick.WriteLine("[ww] No objects were selected.");
                return;
            }

            Quick.ClearSelected();
            var all = Quick.GetImpliedOrSelect();
            if (all == null) {
                Quick.WriteLine("[ww] Failed selecting Other.");
                return;
            }

            var rest = all.Cast<SelectedObject>().IntersectBy(imp.Cast<SelectedObject>(), o => o.ObjectId.Handle.Value).Select(o => o.ObjectId).ToSelectionSet(SelectionMethod.Crossing);
            Quick.SetSelected(rest);
        }

        [CommandMethod("Quicky", "wsw", CommandFlags.UsePickSet | CommandFlags.Redraw)]
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
                var rest = all.Cast<SelectedObject>().Select(o => o.ObjectId.GetObject(tr, true));
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
                var rest = imp.Cast<SelectedObject>().Select(o => o.ObjectId.GetObject(tr, true));
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
                var dbl = Quick.Editor.GetDouble(new PromptDoubleOptions("Please select width: ") {AllowNegative = false, DefaultValue = Quick.Bag.Get("[w]width", 0.4d)});
                if (dbl.Status != PromptStatus.OK) {
                    Quick.WriteLine($"[{Quick.CurrentCommand}] Failed selecting double.");
                    return;
                }
                double val = (double) (Quick.Bag["[w]width"] = dbl.Value);
                //tr.Command("_.pedit", "_m", set, "_n", "_w", dbl.Value.ToString(), "");
                foreach (var e in set.GetObjectIds().Select(o => tr.GetObject(o, true))) {
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
                var dbl = Quick.Editor.GetDouble(new PromptDoubleOptions("Please select width: ") {AllowNegative = false, DefaultValue = Quick.Bag.Get($"[{cmd}]width", 0.4d)});
                if (dbl.Status != PromptStatus.OK) {
                    Quick.WriteLine($"[{cmd}] Failed selecting double.");
                    return;
                }

                double val = (double) (Quick.Bag[$"[{cmd}]width"] = dbl.Value);
                //tr.Command("_.pedit", "_m", set, "_n", "_w", dbl.Value.ToString(), "");
                foreach (var e in set.GetObjectIds().Select(o => tr.GetObject(o, true))) {
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


        

        //[CommandMethod("mrotate", CommandFlags.Session | CommandFlags.Modal | CommandFlags.UsePickSet | CommandFlags.Redraw)]
        public static void MagicRotateCommand() {
            double DegreeToRadian(double angle) => Math.PI * angle / 180.0;
            double RadianToDegree(double angle) => angle * (180.0 / Math.PI);
            // objects initializing
            var nomutt = Convert.ToInt32(Autodesk.AutoCAD.ApplicationServices.Core.Application.GetSystemVariable("nomutt"));
            Document doc = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            Database db = doc.Database;
            int notparsed = 0;
            try {
                using (doc.LockDocument()) {
                    using (QuickTransaction tr = new QuickTransaction()) {
                        var toreplace = tr.GetImpliedOrSelect(new PromptSelectionOptions() { });
                        if (toreplace == null)
                            return;
                        var objs = toreplace.Cast<SelectedObject>().Select(o => tr.GetObject(o.ObjectId, OpenMode.ForRead) as Entity).ToArray();

                        var howmuch = tr.Editor.GetAngle("How many degrees (Anti Clockwise) to add (Negative will go Clockwise)");
                        if (howmuch.Status == PromptStatus.Cancel)
                            return;

                        var degrees = howmuch.Value > 6.28319 ? howmuch.Value : RadianToDegree(howmuch.Value);
                        foreach (Entity ent in objs) {
                            BlockReference block = ent as BlockReference;
                            if (block == null) { //not block..
                                notparsed++;
                                continue;
                            }
                            var a = block.Rotation;
                            var e = DegreeToRadian((RadianToDegree(block.Rotation) + degrees)%360);
                            block.Rotation = a;
                        }

                        tr.Commit();
                        if (notparsed > 0)
                            ed.WriteMessage($"{notparsed} were not blocks and could not be rotated.\n");
                        ed.WriteMessage($"{toreplace.Count} were rotated successfully.\n");
                    }
                }
            } catch (System.Exception ex) {
                ed.WriteMessage(ex.Message + "\n" + ex.StackTrace);
            } 
        }
        [CommandMethod("mreplace", CommandFlags.Session | CommandFlags.Modal | CommandFlags.UsePickSet | CommandFlags.Redraw)]
        public static void MagicReplaceCommand() {
            // objects initializing
            var nomutt = Convert.ToInt32(Autodesk.AutoCAD.ApplicationServices.Core.Application.GetSystemVariable("nomutt"));
            Document doc = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            Database db = doc.Database;
            try {
                using (doc.LockDocument()) {
                    using (QuickTransaction tr = new QuickTransaction()) {
                        var toreplace = tr.GetImpliedOrSelect(new PromptSelectionOptions() { });
                        if (toreplace == null)
                            return;
                        ed.WriteMessage("\nSelect destinion block: ");
                        var totype = Quick.SelectSingle();
                        ed.WriteMessage("\n");
                        if (totype == null)
                            return;

                        var masterblock = (BlockTableRecord)tr.GetObject(((BlockReference)tr.GetObject(totype ?? ObjectId.Null, OpenMode.ForWrite)).BlockTableRecord, OpenMode.ForRead);
                        BlockTable bt = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                        var @new = bt[masterblock.Name];

                        Autodesk.AutoCAD.ApplicationServices.Core.Application.SetSystemVariable("nomutt", 0);
                        int notparsed = 0;
                        bool flattern = false;
                        var objs = toreplace.Cast<SelectedObject>().Select(o => tr.GetObject(o.ObjectId, OpenMode.ForRead) as Entity).ToArray();
                        if (objs.Any(o => (o as BlockReference)?.Position.Z > 0 == true)) {
                            flattern = Quick.AskQuestion("Should flattern blocks with Z value", true) ?? false;
                        }

                        foreach (Entity ent in objs) {
                            BlockReference oldblk = ent as BlockReference;
                            if (oldblk == null) { //not block..
                                notparsed++;
                                continue;
                            }
                            var p = oldblk.Position;
                            Point3d ip = flattern ? new Point3d(p.X,p.Y,0) : p;
                            Scale3d scl = oldblk.ScaleFactors;
                            double rot = oldblk.Rotation;
                            BlockReference newblk = new BlockReference(ip, @new);
                            newblk.SetPropertiesFrom(ent);
                            newblk.Rotation = rot;
                            newblk.ScaleFactors = scl;
                            tr.BlockTableRecordCurrentSpace.AppendEntity(newblk);
                            tr.AddNewlyCreatedDBObject(newblk, true);
                            ApplyAttributes(db, tr, newblk);
                            oldblk.UpgradeOpen();
                            oldblk.Erase();
                            oldblk.Dispose();
                        }

                        Autodesk.AutoCAD.ApplicationServices.Core.Application.SetSystemVariable("nomutt", 1);

                        tr.Commit();
                        if (notparsed>0)
                            ed.WriteMessage($"{notparsed} are not blocks and were not replaced.\n");
                        ed.WriteMessage($"{toreplace.Count} were replaced to block {masterblock.Name} successfully.\n");
                    }
                }
            } catch (System.Exception ex) {
                ed.WriteMessage(ex.Message + "\n" + ex.StackTrace);
            } finally {
                Autodesk.AutoCAD.ApplicationServices.Core.Application.SetSystemVariable("nomutt", nomutt);
            }
        }

        public static void ApplyAttributes(Database db, QuickTransaction tr, BlockReference bref) {
            if (bref == null)
                return;
            var _brec = tr.GetObject(bref.BlockTableRecord, OpenMode.ForRead);
            BlockTableRecord btrec = _brec as BlockTableRecord;
            if (btrec == null)
                return;
            if (btrec.HasAttributeDefinitions) {
                Autodesk.AutoCAD.DatabaseServices.AttributeCollection atcoll = bref.AttributeCollection;

                foreach (ObjectId subid in btrec) {
                    Entity ent = (Entity) subid.GetObject(OpenMode.ForRead);

                    AttributeDefinition attDef = ent as AttributeDefinition;

                    if (attDef != null) {
                        AttributeReference attRef = new AttributeReference();
                        attRef.SetDatabaseDefaults(); //optional
                        attRef.SetAttributeFromBlock(attDef, bref.BlockTransform);
                        attRef.Position = attDef.Position.TransformBy(bref.BlockTransform);
                        attRef.Tag = attDef.Tag;
                        attRef.AdjustAlignment(db);
                        atcoll.AppendAttribute(attRef);
                        tr.AddNewlyCreatedDBObject(attRef, true);
                    }
                }
            }
        }
        
        [CommandMethod("SELECTBLOCKS", CommandFlags.UsePickSet | CommandFlags.Redraw | CommandFlags.NoPaperSpace)]
        public static void SelectBlocksCommand() {
            Editor ed = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument.Editor;
            Database db = HostApplicationServices.WorkingDatabase;
            Transaction tr = db.TransactionManager.StartTransaction();

            // Start the transaction
            try {
                //Request name filter:
                PromptStringOptions psto = new PromptStringOptions("\nEnter the regex formula: ");
                psto.AllowSpaces = true;
                psto.DefaultValue = "*"; //old block

                PromptResult stres = ed.GetString(psto);

                if (stres.Status != PromptStatus.OK)
                    return;

                string oldblock = stres.StringResult;

                // Build a filter list so that only
                // block references are selected
                TypedValue[] filList = new TypedValue[1] {
                    new TypedValue((int) DxfCode.Start, "INSERT")
                };
                SelectionFilter filter = new SelectionFilter(filList);
                PromptSelectionOptions opts = new PromptSelectionOptions();
                opts.MessageForAdding = "Select blocks to filter: ";
                SelectionSet res = Quick.GetImpliedOrSelect(opts, filter);

                // Do nothing if selection is unsuccessful
                if (res == null) return;

                SelectionSet selSet = res;
                ObjectId[] idArray = selSet.GetObjectIds();
                List<ObjectId> aa = new List<ObjectId>();

                foreach (ObjectId blkId in idArray)
                {
                    BlockReference blkRef = (BlockReference)tr.GetObject(blkId, OpenMode.ForRead);
                    BlockTableRecord btr = (BlockTableRecord)tr.GetObject(blkRef.BlockTableRecord, OpenMode.ForRead);
                    if (WildcardMatch(btr.Name, oldblock, false))
                    {
                        aa.Add(blkId);
                    }
                    btr.Dispose();

                    /*AttributeCollection attCol = blkRef.AttributeCollection;
                    foreach (ObjectId attId in attCol)
                    {
                        AttributeReference attRef = (AttributeReference)tr.GetObject(attId, OpenMode.ForRead);

                        string str = ("\n  Attribute Tag: " + attRef.Tag + "\n    Attribute String: " + attRef.TextString);
                        ed.WriteMessage(str);
                    }*/
                }
                Quick.SetSelected(aa.ToArray());
            } catch (Autodesk.AutoCAD.Runtime.Exception ex) {
                ed.WriteMessage(("Exception: " + ex.Message));
            } finally {
                tr.Dispose();
            }
        }
        [CommandMethod("LISTATT", CommandFlags.UsePickSet | CommandFlags.Redraw | CommandFlags.NoPaperSpace)]
        public static void ListAttributes() {
            Editor ed = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument.Editor;
            Database db = HostApplicationServices.WorkingDatabase;
            Transaction tr = db.TransactionManager.StartTransaction();

            // Start the transaction
            try {
                //Request name filter:
                PromptStringOptions psto = new PromptStringOptions("\nEnter a replacement block name [*]: ");
                psto.AllowSpaces = true;
                psto.DefaultValue = "*"; //old block

                PromptResult stres = ed.GetString(psto);

                if (stres.Status != PromptStatus.OK)
                    return;

                string oldblock = stres.StringResult;

                // Build a filter list so that only
                // block references are selected
                TypedValue[] filList = new TypedValue[1] {
                    new TypedValue((int) DxfCode.Start, "INSERT")
                };
                SelectionFilter filter = new SelectionFilter(filList);
                PromptSelectionOptions opts = new PromptSelectionOptions();
                opts.MessageForAdding = "Select blocks to filter: ";
                SelectionSet res = Quick.GetImpliedOrSelect(opts, filter);

                // Do nothing if selection is unsuccessful
                if (res == null) return;

                SelectionSet selSet = res;
                ObjectId[] idArray = selSet.GetObjectIds();
                List<ObjectId> aa = new List<ObjectId>();

                foreach (ObjectId blkId in idArray) {
                    BlockReference blkRef = (BlockReference) tr.GetObject(blkId, OpenMode.ForRead);
                    BlockTableRecord btr = (BlockTableRecord) tr.GetObject(blkRef.BlockTableRecord, OpenMode.ForRead);
                    if (WildcardMatch(btr.Name, oldblock, false))
                    {
                        aa.Add(blkId);
                        ed.WriteMessage("\nBlock: " + btr.Name);
                    }
                    btr.Dispose();

                    /*AttributeCollection attCol = blkRef.AttributeCollection;
                    foreach (ObjectId attId in attCol)
                    {
                        AttributeReference attRef = (AttributeReference)tr.GetObject(attId, OpenMode.ForRead);

                        string str = ("\n  Attribute Tag: " + attRef.Tag + "\n    Attribute String: " + attRef.TextString);
                        ed.WriteMessage(str);
                    }*/
                }
                Quick.SetSelected(aa.ToArray());
            } catch (Autodesk.AutoCAD.Runtime.Exception ex) {
                ed.WriteMessage(("Exception: " + ex.Message));
            } finally {
                tr.Dispose();
            }
        }

        private static bool WildcardMatch(string s, string wildcard, bool case_sensitive) {
            // Replace the * with an .* and the ? with a dot. Put ^ at the
            // beginning and a $ at the end
            String pattern = "^" + Regex.Escape(wildcard).Replace(@"\*", ".*").Replace(@"\?", ".") + "$";

            // Now, run the Regex as you already know
            Regex regex;
            if (case_sensitive)
                regex = new Regex(pattern);
            else
                regex = new Regex(pattern, RegexOptions.IgnoreCase);

            return (regex.IsMatch(s));
        }
    }
}