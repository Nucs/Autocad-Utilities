using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using autonet.Extensions;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Common;
using Linq.Extras;
using MoreLinq;
using nucs.JsonSettings;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using Exception = System.Exception;

namespace autonet.Forms {
    public static class HighlevelCommands {
        public static dynamic Settings = JsonSettings.Load<SettingsBag>(Paths.ConfigFile("autonet-commands.json").FullName).EnableAutosave().AsDynamic();

        private const double Rad2Deg = 180.0 / Math.PI;

        private const double Deg2Rad = Math.PI / 180.0;

        /*/// <summary>
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
                                }#1#
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
*/

        [CommandMethod("Quicky", "asd", CommandFlags.UsePickSet | CommandFlags.Redraw | CommandFlags.NoPaperSpace)]
        public static void DoitCommand() {
            _retry:
            var result = Quick.Editor.GetSelection(new PromptSelectionOptions {SingleOnly = true, AllowDuplicates = false, }, SelectionFilters.AllowAny(EntityType.AnyPolyline, EntityType.Arc));
            var ptr = AcadProperties.Cursor;
            /*var result = Quick.Editor.GetEntity(options);*/
            if (result.Status != PromptStatus.OK) {
                if (result.Status == PromptStatus.Cancel)
                    return;
                goto _retry;
            }

            using (var tr = new QuickTransaction()) ;
            ptr = OSnapping.SnapIfEnabled(ptr);
            
            //var Quick.Editor.Get
            var lineId = result.Value[0].ObjectId;
            //var pickpoint = result.PickedPoint.ToPoint2D();
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
                foreach (var o in rest) o.Visible = true;
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
                foreach (var o in rest) o.Visible = false;
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
                var val = (double) (Quick.Bag["[w]width"] = dbl.Value);
                //tr.Command("_.pedit", "_m", set, "_n", "_w", dbl.Value.ToString(), "");
                foreach (var e in set.GetObjectIds().Select(o => tr.GetObject(o, true)))
                    switch (e) {
                        case Polyline p:
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
                var val = (double) (Quick.Bag[$"[{cmd}]width"] = dbl.Value);
                //tr.Command("_.pedit", "_m", set, "_n", "_w", dbl.Value.ToString(), "");
                foreach (var e in set.GetObjectIds().Select(o => tr.GetObject(o, true)))
                    switch (e) {
                        case Polyline p:
                            p.FilletAll(val);
                            break;
                        default: break;
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



        #region Magic Family
        /*public static void MagicRotateCommand() {
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
                            if (block == null) {
                                //not block..
                                notparsed++;
                                continue;
                            }
                            var a = block.Rotation;
                            var e = DegreeToRadian((RadianToDegree(block.Rotation) + degrees) % 360);
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
        }*/
        [CommandMethod("mreplace", CommandFlags.Session | CommandFlags.Modal | CommandFlags.UsePickSet | CommandFlags.Redraw)]
        public static void MagicReplaceCommand() {
            // objects initializing
            var nomutt = Convert.ToInt32(Autodesk.AutoCAD.ApplicationServices.Core.Application.GetSystemVariable("nomutt"));
            var doc = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
            var ed = doc.Editor;
            var db = doc.Database;
            try {
                using (doc.LockDocument()) {
                    using (var tr = new QuickTransaction()) {
                        var toreplace = tr.GetImpliedOrSelect(new PromptSelectionOptions());
                        if (toreplace == null)
                            return;
                        ed.WriteMessage("\nSelect destinion block: ");
                        var totype = Quick.SelectSingle();
                        ed.WriteMessage("\n");
                        if (totype == null)
                            return;
                        var masterblock = (BlockTableRecord) tr.GetObject(((BlockReference) tr.GetObject(totype ?? ObjectId.Null, OpenMode.ForWrite)).BlockTableRecord, OpenMode.ForRead);
                        var bt = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                        var @new = bt[masterblock.Name];
                        Autodesk.AutoCAD.ApplicationServices.Core.Application.SetSystemVariable("nomutt", 0);
                        var notparsed = 0;
                        var flattern = false;
                        var objs = toreplace.Cast<SelectedObject>().Select(o => tr.GetObject(o.ObjectId, OpenMode.ForRead) as Entity).ToArray();
                        if (objs.Any(o => (o as BlockReference)?.Position.Z > 0)) flattern = Quick.AskQuestion("Should flattern blocks with Z value", true) ?? false;
                        var os = new List<ObjectId>();
                        foreach (var ent in objs) {
                            var oldblk = ent as BlockReference;
                            if (oldblk == null) {
                                //not block..
                                notparsed++;
                                continue;
                            }
                            var p = oldblk.Position;
                            var ip = flattern ? new Point3d(p.X, p.Y, 0) : p;
                            var scl = oldblk.ScaleFactors;
                            var rot = oldblk.Rotation;
                            var newblk = new BlockReference(ip, @new);
                            newblk.SetPropertiesFrom(ent);
                            newblk.Rotation = rot;
                            newblk.ScaleFactors = scl;
                            tr.BlockTableRecordCurrentSpace.AppendEntity(newblk);
                            tr.AddNewlyCreatedDBObject(newblk, true);
                            ApplyAttributes(db, tr, newblk);
                            oldblk.UpgradeOpen();
                            oldblk.Erase();
                            oldblk.Dispose();
                            os.Add(newblk.ObjectId);
                        }
                        Autodesk.AutoCAD.ApplicationServices.Core.Application.SetSystemVariable("nomutt", 1);
                        tr.Commit();
                        if (notparsed > 0)
                            ed.WriteMessage($"{notparsed} are not blocks and were not replaced.\n");
                        ed.WriteMessage($"{toreplace.Count} were replaced to block {masterblock.Name} successfully.\n");
                        Quick.SetSelected(os.ToArray());
                    }
                }
            } catch (Exception ex) {
                ed.WriteMessage(ex.Message + "\n" + ex.StackTrace);
            } finally {
                Autodesk.AutoCAD.ApplicationServices.Core.Application.SetSystemVariable("nomutt", nomutt);
            }
        }

        [CommandMethod("mrotate", CommandFlags.Modal | CommandFlags.UsePickSet | CommandFlags.Redraw)]
        public static void MRotateCommand() {
            using (var tr = new QuickTransaction()) {
                // objects initializing
                var nomutt = Convert.ToInt32(Autodesk.AutoCAD.ApplicationServices.Core.Application.GetSystemVariable("nomutt"));
                try {
                    // Get the current document and database
                    // Start a transaction
                    // Open the Block table for read
                    var _sel = Quick.GetImpliedOrSelect();
                    if (_sel == null || _sel.Count == 0) {
                        tr.Editor.WriteMessage("Nothing was selected.");
                        return;
                    }
                    var howmuch = tr.Editor.GetAngle("How many degrees (Anti Clockwise) to add (Negative will go Clockwise)");
                    if (howmuch.Status == PromptStatus.Cancel)
                        return;
                    var curUCSMatrix = tr.Doc.Editor.CurrentUserCoordinateSystem;
                    var curUCS = curUCSMatrix.CoordinateSystem3d;
                    foreach (SelectedObject o in _sel) {
                        var obj = (BlockReference) tr.GetObject(o.ObjectId) ?? throw new NullReferenceException("obj");
                        var matrix = Matrix3d.Rotation(howmuch.Value, curUCS.Zaxis, obj.Position);
                        obj.TransformBy(matrix);
                    }
                    // Add the new object to the block table record and the transaction
                    // Save the new objects to the database
                    tr.Commit();
                    Quick.SetSelected(_sel);
                } catch (Exception ex) {
                    Quick.WriteLine(ex.Message + "\n" + ex.StackTrace);
                } finally {
                    Autodesk.AutoCAD.ApplicationServices.Core.Application.SetSystemVariable("nomutt", nomutt);
                }
            }
        }

        [CommandMethod("mtolines", CommandFlags.Modal | CommandFlags.UsePickSet | CommandFlags.Redraw)]
        public static void AlignBlocks() {
            try {
                var options = new PromptEntityOptions("\nSelect polyline: ");
                options.SetRejectMessage("Objet non valide.");
                options.AddAllowedClass(typeof(Polyline), true);
                var result = Quick.Editor.GetEntity(options);
                if (result.Status != PromptStatus.OK)
                    return;
                var lineId = result.ObjectId;
                var sel = Quick.GetImpliedOrSelect();
                if (result.Status != PromptStatus.OK)
                    return;
                var db = lineId.Database;
                foreach (var blockId in sel.GetObjectIds())
                    using (var trans = db.TransactionManager.StartTransaction()) {
                        try {
                            var line = trans.GetObject(lineId, OpenMode.ForRead) as Polyline;
                            var blockRef = trans.GetObject(blockId, OpenMode.ForWrite) as BlockReference;
                            if (blockRef == null) continue;
                            var blockpos = blockRef.Position;
                            // better use the center point, instead min/max
                            var pointOverLine = line.GetClosestPointTo(blockRef.Position, false);
                            //var vectorto = pointOverLine.GetVectorTo(blockRef.Position);
                            //var ang = vectorto.AngleOnPlane()
                            blockRef.Position = pointOverLine; // move
                            // assuming a well behaved 2D block aligned with XY
                            //Vector3d lineDirection = line.GetFirstDerivative(pointOverLine);
                            //double angleToRotate = Vector3d.XAxis.GetAngleTo(lineDirection, Vector3d.ZAxis);
                            //angel between block to the nearest point
                            var b2near = blockpos.Convert2d(new Plane()).GetVectorTo(pointOverLine.Convert2d(new Plane())).Angle * Rad2Deg;
                            //var pos1 = Math.Atan2(blockpos.Y - pointOverLine.Y, pointOverLine.X - blockpos.X) * Rad2Deg;
                            //var angeltoblock = lineDirection.GetAngleTo(blockRef.Position.GetAsVector())*Rad2Deg;
                            blockRef.Rotation = (b2near - 90) * Deg2Rad; //-90 to convert to block plane (0 is downwards).
                            trans.Commit();
                        } catch (Exception ex) {
                            Quick.WriteLine(ex.Message + "\n" + ex.StackTrace);
                        }
                    }
                Quick.SetSelected(sel);
            } catch (Exception ex) {
                Quick.WriteLine(ex.Message + "\n" + ex.StackTrace);
            }
        }

        [CommandMethod("mtoline", CommandFlags.Modal | CommandFlags.UsePickSet | CommandFlags.Redraw)]
        public static void AlignBlockCommand() {
            var options = new PromptEntityOptions("\nSelect polyline: ");
            options.SetRejectMessage("Objet non valide.");
            options.AddAllowedClass(typeof(Polyline), true);
            var result = Quick.Editor.GetEntity(options);
            if (result.Status != PromptStatus.OK)
                return;
            var lineId = result.ObjectId;
            options.Message = "\nSelect block: ";
            options.RemoveAllowedClass(typeof(Polyline));
            options.AddAllowedClass(typeof(BlockReference), true);
            result = Quick.Editor.GetEntity(options);
            if (result.Status != PromptStatus.OK)
                return;
            var blockId = result.ObjectId;
            var db = lineId.Database;
            using (var trans = db.TransactionManager.StartTransaction()) {
                try {
                    var line = trans.GetObject(lineId, OpenMode.ForRead) as Polyline;
                    var blockRef = trans.GetObject(blockId, OpenMode.ForWrite) as BlockReference;
                    var blockpos = blockRef.Position;
                    var pointOverLine = line.GetClosestPointTo(blockRef.Position, false);
                    blockRef.Position = pointOverLine; // move
                    //angel between block to the nearest point
                    var b2near = blockpos.Convert2d(new Plane()).GetVectorTo(pointOverLine.Convert2d(new Plane())).Angle * Rad2Deg;
                    blockRef.Rotation = (b2near - 90) * Deg2Rad; //-90 to convert to block plane (0 is downwards).
                    trans.Commit();
                } catch (Exception ex) {
                    Quick.WriteLine(ex.Message + "\n" + ex.StackTrace);
                }
            }
        }

        public static void ApplyAttributes(Database db, QuickTransaction tr, BlockReference bref) {
            if (bref == null)
                return;
            var _brec = tr.GetObject(bref.BlockTableRecord, OpenMode.ForRead);
            var btrec = _brec as BlockTableRecord;
            if (btrec == null)
                return;
            if (btrec.HasAttributeDefinitions) {
                var atcoll = bref.AttributeCollection;
                foreach (var subid in btrec) {
                    var ent = (Entity) subid.GetObject(OpenMode.ForRead);
                    var attDef = ent as AttributeDefinition;
                    if (attDef != null) {
                        var attRef = new AttributeReference();
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


        [CommandMethod("ac", CommandFlags.Modal | CommandFlags.UsePickSet | CommandFlags.Redraw)]
        [CommandMethod("addcurve", CommandFlags.Modal | CommandFlags.UsePickSet | CommandFlags.Redraw)]
        public static void AddCurveCommand() {
            double GetAngle(Point2d a, Point2d b) {
                double xDiff = b.X - a.X;
                double yDiff = b.Y - a.Y;
                return Math.Atan2(yDiff, xDiff) * 180.0d / Math.PI;
            }

            try {
                var options = new PromptEntityOptions("\nSelect a point on a polyline: ");
                options.SetRejectMessage("Invalid Object.");
                options.AddAllowedClass(typeof(Polyline), true);
                
                var result = Quick.Editor.GetEntity(options);
                if (result.Status != PromptStatus.OK)
                    return;
                var lineId = result.ObjectId;
                var pickpoint = result.PickedPoint.ToPoint2D();
                var qtarget = Quick.Editor.GetPoint(new PromptPointOptions("Pick the strech point.") {AllowNone = false, BasePoint = pickpoint.ToPoint3D(), UseBasePoint = true, UseDashedLine = true});
                if (qtarget.Status != PromptStatus.OK)
                    return;
                var target = qtarget.Value.ToPoint2D();
                using (var tr = new QuickTransaction()) {
                    try {
                        var poly = tr.GetObject(lineId, OpenMode.ForWrite) as Polyline;
                        Curve2d part = null;
                        var distance = target.GetDistanceTo(pickpoint);

                        var pointRight = poly.OffsetToEnd(result.PickedPoint, distance);
                        var pointLeft = poly.OffsetToStart(result.PickedPoint, distance);
                        if (!pointRight.Item2 || !pointLeft.Item2) {
                            return;
                        }

                        var rightcut = poly.BreakOnPoint(pointRight.Item1, tr, false);
                        poly = (Polyline) rightcut[0]; //right poly
                        var rightpoly = (Polyline) rightcut[1];
                        var leftcut = poly.BreakOnPoint(pointLeft.Item1, tr, false);
                        var leftpoly = (Polyline) leftcut[0];
                        leftcut[1].UpgradeOpen();
                        leftcut[1].Erase();

                        var buldge = Math.Tan((90d * 0.85 * Deg2Rad) / 4);
                        leftpoly.AddVertexAt(leftpoly.NumberOfVertices, target, 0, leftpoly.GetStartWidthAt(leftpoly.NumberOfVertices - 1), leftpoly.GetEndWidthAt(leftpoly.NumberOfVertices - 1));
                        leftpoly.SetBulgeAt(leftpoly.NumberOfVertices - 2, buldge);
                        rightpoly.AddVertexAt(0, target, buldge, leftpoly.GetStartWidthAt(0), leftpoly.GetEndWidthAt(0));
                        tr.Transaction.TransactionManager.QueueForGraphicsFlush();

                        void setdir(bool right) {
                            if (right) {
                                leftpoly.SetBulgeAt(leftpoly.NumberOfVertices - 2, buldge);
                                rightpoly.SetBulgeAt(0, buldge);
                            } else {
                                leftpoly.SetBulgeAt(leftpoly.NumberOfVertices - 2, -buldge);
                                rightpoly.SetBulgeAt(0, -buldge);
                            }
                        }

                        setdir(Settings.addcurve_direction as bool? ?? true);

                        if (Quick.AskQuestion("Reverse Direction?", false) ?? false == true) {
                            var val = Settings["addcurve_direction"] = !((Settings["addcurve_direction"] as bool?) ?? false);
                            setdir(val);
                        }

                        tr.Commit();
                    } catch (Exception ex) {
                        Quick.WriteLine(ex.Message + "\n" + ex.StackTrace);
                    }
                }
            } catch (Exception ex) {
                Quick.WriteLine(ex.Message + "\n" + ex.StackTrace);
            }
        }
        /*        [CommandMethod("cccc", CommandFlags.Modal | CommandFlags.UsePickSet | CommandFlags.Redraw)]
                public static void AddCurveCommandy() {
                    try {
                        var options = new PromptEntityOptions("\nSelect a point on a polyline: ");
                        options.SetRejectMessage("Invalid Object.");
                        options.AddAllowedClass(typeof(Polyline), true);
                        var result = Quick.Editor.GetEntity(options);
                        if (result.Status != PromptStatus.OK)
                            return;
                        var lineId = result.ObjectId;

                        using (var tr = new QuickTransaction()) {
                            try {
                                var poly = tr.GetObject(lineId, OpenMode.ForWrite) as Polyline;
                                for (int i = 0; i < poly.NumberOfVertices; i++) {
                                    Quick.WriteLine($"\n{poly.GetSegmentType(i)}  |  {poly.GetBulgeAt(i)}");
                                }
                                if (poly != null) poly.SetBulgeAt(poly.NumberOfVertices - 1, Quick.Editor.GetDouble("Get Bulge").Value);

                                tr.Commit();
                            } catch (Exception ex) {
                                Quick.WriteLine(ex.Message + "\n" + ex.StackTrace);
                            }
                        }
                    } catch (Exception ex) {
                        Quick.WriteLine(ex.Message + "\n" + ex.StackTrace);
                    }
                }*/

        /*     [CommandMethod("bro")]
             public static void BreakOnPointOffsetCommand() {
                 try {
                     var options = new PromptEntityOptions("\nSelect a point on a polyline: ");
                     options.SetRejectMessage("Invalid Object.");
                     options.AddAllowedClass(typeof(Curve), false);
                     options.AllowObjectOnLockedLayer = false;

                     var result = Quick.Editor.GetEntity(options);
                     if (result.Status != PromptStatus.OK)
                         return;
                     var curveId = result.ObjectId;

                     using (var tr = new QuickTransaction()) {
                         var ent = (Curve) tr.GetObject(curveId, OpenMode.ForRead, false);
                         ent.BreakOnPoint(ent.OffsetToEnd(result.PickedPoint, 10d).Item1, tr, true).SetSelected();
                     }
                 } catch (Autodesk.AutoCAD.Runtime.Exception ex) {
                     Autodesk.AutoCAD.ApplicationServices.Core.Application.ShowAlertDialog("\n" + ex.Message + "\n" + ex.StackTrace);
                 }
             }*/

        [CommandMethod("br")]
        public static void BreakOnPointCommand() {
            try {
                var options = new PromptEntityOptions("\nSelect a point on a polyline: ");
                options.SetRejectMessage("Invalid Object.");
                options.AddAllowedClass(typeof(Curve), false);
                options.AllowObjectOnLockedLayer = false;

                var result = Quick.Editor.GetEntity(options);
                if (result.Status != PromptStatus.OK)
                    return;
                var curveId = result.ObjectId;

                using (var tr = new QuickTransaction()) {
                    var ent = (Curve) tr.GetObject(curveId, OpenMode.ForRead, false);
                    ent.BreakOnPoint(result.PickedPoint.ToPoint2D(), tr, true).SetSelected();
                }
            } catch (Autodesk.AutoCAD.Runtime.Exception ex) {
                Autodesk.AutoCAD.ApplicationServices.Core.Application.ShowAlertDialog(ex.Message + "\n" + ex.StackTrace);
            }
        }
#endregion

        [CommandMethod("SELECTBLOCKS", CommandFlags.UsePickSet | CommandFlags.Redraw | CommandFlags.NoPaperSpace)]
        public static void SelectBlocksCommand() {
            var ed = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument.Editor;
            var db = HostApplicationServices.WorkingDatabase;
            var tr = db.TransactionManager.StartTransaction();
            // Start the transaction
            try {
                //Request name filter:
                var psto = new PromptStringOptions("\nEnter the regex formula: ");
                psto.AllowSpaces = true;
                psto.DefaultValue = "*"; //old block
                var stres = ed.GetString(psto);
                if (stres.Status != PromptStatus.OK)
                    return;
                var oldblock = stres.StringResult;
                // Build a filter list so that only
                // block references are selected
                var filList = new TypedValue[1] {
                    new TypedValue((int) DxfCode.Start, "INSERT")
                };
                var filter = new SelectionFilter(filList);
                var opts = new PromptSelectionOptions();
                opts.MessageForAdding = "Select blocks to filter: ";
                var res = Quick.GetImpliedOrSelect(opts, filter);
                // Do nothing if selection is unsuccessful
                if (res == null) return;
                var selSet = res;
                var idArray = selSet.GetObjectIds();
                var aa = new List<ObjectId>();
                foreach (var blkId in idArray) {
                    var blkRef = (BlockReference) tr.GetObject(blkId, OpenMode.ForRead);
                    var btr = (BlockTableRecord) tr.GetObject(blkRef.BlockTableRecord, OpenMode.ForRead);
                    if (WildcardMatch(btr.Name, oldblock, false)) aa.Add(blkId);
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
                ed.WriteMessage("Exception: " + ex.Message);
            } finally {
                tr.Dispose();
            }
        }

        [CommandMethod("LISTATT", CommandFlags.UsePickSet | CommandFlags.Redraw | CommandFlags.NoPaperSpace)]
        public static void ListAttributes() {
            var ed = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument.Editor;
            var db = HostApplicationServices.WorkingDatabase;
            var tr = db.TransactionManager.StartTransaction();
            // Start the transaction
            try {
                //Request name filter:
                var psto = new PromptStringOptions("\nEnter a replacement block name [*]: ");
                psto.AllowSpaces = true;
                psto.DefaultValue = "*"; //old block
                var stres = ed.GetString(psto);
                if (stres.Status != PromptStatus.OK)
                    return;
                var oldblock = stres.StringResult;
                // Build a filter list so that only
                // block references are selected
                var filList = new TypedValue[1] {
                    new TypedValue((int) DxfCode.Start, "INSERT")
                };
                var filter = new SelectionFilter(filList);
                var opts = new PromptSelectionOptions();
                opts.MessageForAdding = "Select blocks to filter: ";
                var res = Quick.GetImpliedOrSelect(opts, filter);
                // Do nothing if selection is unsuccessful
                if (res == null) return;
                var selSet = res;
                var idArray = selSet.GetObjectIds();
                var aa = new List<ObjectId>();
                foreach (var blkId in idArray) {
                    var blkRef = (BlockReference) tr.GetObject(blkId, OpenMode.ForRead);
                    var btr = (BlockTableRecord) tr.GetObject(blkRef.BlockTableRecord, OpenMode.ForRead);
                    if (WildcardMatch(btr.Name, oldblock, false)) {
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
                ed.WriteMessage("\nException: " + ex.Message);
            } finally {
                tr.Dispose();
            }
        }

        private static bool WildcardMatch(string s, string wildcard, bool case_sensitive) {
            // Replace the * with an .* and the ? with a dot. Put ^ at the
            // beginning and a $ at the end
            var pattern = "^" + Regex.Escape(wildcard).Replace(@"\*", ".*").Replace(@"\?", ".") + "$";
            // Now, run the Regex as you already know
            Regex regex;
            if (case_sensitive)
                regex = new Regex(pattern);
            else
                regex = new Regex(pattern, RegexOptions.IgnoreCase);
            return regex.IsMatch(s);
        }
    }
}