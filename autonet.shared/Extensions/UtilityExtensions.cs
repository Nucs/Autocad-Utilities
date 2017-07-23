using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;

namespace autonet.Extensions {
    public static class UtilityExtensions {
        //[CommandMethod("Quicky", "qJoinConnected", CommandFlags.Modal | CommandFlags.UsePickSet)] 
        private static void TestConnectedLines() {
            var doc = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
            var ed = doc.Editor;
            var db = doc.Database;
            var pso = new PromptEntityOptions("\nPick a single line to join: ");
            pso.SetRejectMessage("\nObject must be of type Line!");
            pso.AddAllowedClass(typeof(Line), false);
            var res = ed.GetEntity(pso);
            if (res.Status != PromptStatus.OK) return;
            using (var tr = db.TransactionManager.StartTransaction()) {
                var btr = (BlockTableRecord) tr.GetObject(db.CurrentSpaceId, OpenMode.ForRead);
                var ids = JoinLines(btr, res.ObjectId);
                var lines = ids.ToArray();
                ed.SetImpliedSelection(lines);
                //or
                //ed.SetImpliedSelection(ids.OfType<ObjectId>().ToArray());
                var chres = ed.SelectImplied();
                if (chres.Status != PromptStatus.OK) {
                    ed.WriteMessage("\nNothing added in the chain!");
                    return;
                }

                foreach (SelectedObject selobj in chres.Value) {
                    var subent = tr.GetObject(selobj.ObjectId, OpenMode.ForWrite) as Entity;
                    subent.ColorIndex = 4;
                }

                tr.Commit();
            }
        }

        private static void SelectConnectedLines(BlockTableRecord btr, List<ObjectId> ids, ObjectId id) {
            var en = id.GetObject(OpenMode.ForRead, false) as Entity;
            var ln = en as Line;
            if (ln != null)
                foreach (var idx in btr) {
                    var ex = idx.GetObject(OpenMode.ForRead, false) as Entity;
                    var lx = ex as Line;
                    if (ln.StartPoint == lx.StartPoint || ln.StartPoint == lx.EndPoint || ln.EndPoint == lx.StartPoint || ln.EndPoint == lx.EndPoint)
                        if (!ids.Contains(idx)) {
                            ids.Add(idx);
                            SelectConnectedLines(btr, ids, idx);
                        }
                }
        }

        private static void joinPolylines() {
            Document document =
                Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
            Editor ed = document.Editor;
            Database db = document.Database;

            PromptEntityOptions peo1
                = new PromptEntityOptions("\nSelect source polyline : ");

            peo1.SetRejectMessage("\nInvalid selection...");

            peo1.AddAllowedClass
            (
                typeof(Autodesk.AutoCAD.DatabaseServices.Polyline),
                true
            );

            peo1.AddAllowedClass
            (
                typeof(Autodesk.AutoCAD.DatabaseServices.Polyline2d),
                true
            );

            peo1.AddAllowedClass
            (
                typeof(Autodesk.AutoCAD.DatabaseServices.Polyline3d),
                true
            );

            PromptEntityResult pEntrs = ed.GetEntity(peo1);
            if (PromptStatus.OK != pEntrs.Status)
                return;

            ObjectId srcId = pEntrs.ObjectId;

            PromptEntityOptions peo2
                = new PromptEntityOptions("\nSelect polyline to join : ");

            peo2.SetRejectMessage("\nInvalid selection...");
            peo2.AddAllowedClass
            (
                typeof(Autodesk.AutoCAD.DatabaseServices.Polyline),
                true
            );

            peo2.AddAllowedClass
            (
                typeof(Autodesk.AutoCAD.DatabaseServices.Polyline2d),
                true
            );

            peo2.AddAllowedClass
            (
                typeof(Autodesk.AutoCAD.DatabaseServices.Polyline3d),
                true
            );

            pEntrs = ed.GetEntity(peo2);
            if (PromptStatus.OK != pEntrs.Status)
                return;

            ObjectId joinId = pEntrs.ObjectId;
            try {
                using (Transaction trans
                    = db.TransactionManager.StartTransaction()) {
                    Entity srcPLine
                        = trans.GetObject(
                            srcId,
                            OpenMode.ForRead
                        ) as Entity;

                    Entity addPLine
                        = trans.GetObject(
                            joinId,
                            OpenMode.ForRead
                        ) as Entity;

                    srcPLine.UpgradeOpen();
                    srcPLine.JoinEntity(addPLine);

                    addPLine.UpgradeOpen();
                    addPLine.Erase();

                    trans.Commit();
                }
            } catch (System.Exception ex) {
                ed.WriteMessage(ex.Message);
            }
        }


        /// <summary>
        ///     Checks if entity is writable, if not - calls <see cref="QuickTransaction.GetObject"/> and returns writable one.
        /// </summary>
        public static T EnsureWritable<T>(this QuickTransaction tr, T entity) where T : Entity {
            if (!entity.IsWriteEnabled)
                entity = (T)tr.GetObject(entity.ObjectId, OpenMode.ForWrite);
            return entity;
        }

         /// <summary>
        ///     Checks if entity is writable, if not - calls <see cref="QuickTransaction.GetObject"/> and returns writable one.
        /// </summary>
        public static T EnsureWritable<T>(this T entity, QuickTransaction tr) where T : Entity {
             return EnsureWritable(tr, entity); //overload...
        }

        public static Entity GetObject(this QuickTransaction tr, ObjectId id, bool writable=true) {
            return (Entity) tr.GetObject(id, writable ? OpenMode.ForWrite : OpenMode.ForRead);
        }

        public static Entity GetObject(this ObjectId id, QuickTransaction tr, bool writable=true) {
            return GetObject(tr, id, writable);
        }

        public static double GetLength(this Curve curve) {
            if (curve == null) return 0;
            return curve.GetDistanceAtParameter(curve.EndParam) - curve.GetDistanceAtParameter(curve.StartParam);
        }

        public static double GetArcBulge(this Arc arc) {
            double deltaAng = arc.EndAngle - arc.StartAngle;
            if (deltaAng < 0)
                deltaAng += 2 * Math.PI;
            return Math.Tan(deltaAng * 0.25);
        }
        public static double GetArcBulge(this CircularArc3d arc) {
            double deltaAng = arc.EndAngle - arc.StartAngle;
            if (deltaAng < 0)
                deltaAng += 2 * Math.PI;
            return Math.Tan(deltaAng * 0.25);
        }
        public static List<ObjectId> JoinLines(BlockTableRecord btr, ObjectId id) {
            var ids = new List<ObjectId>();

            SelectConnectedLines(btr, ids, id);

            return ids;
        }
    }
}