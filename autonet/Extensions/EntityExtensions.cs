using System;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;

namespace autonet.Extensions {
    public static class EntityExtensions {
        public static void SetGlobalWidth(this Polyline p, double width) {
            for (int i = 0; i < p.NumberOfVertices; i++) {
                p.SetEndWidthAt(i, width);
                p.SetStartWidthAt(i, width);
            }
        }

        public static Entity ConvertToPolyline(this QuickTransaction tr, ObjectId id) {
            return ConvertToPolyline(tr,tr.GetObject(id, OpenMode.ForWrite) as Entity ?? throw new InvalidOperationException("The given object id is not an entity!"));
        }

        public static Entity ConvertToPolyline(this Entity id, QuickTransaction tr) {
            return ConvertToPolyline(tr,id);
        }

        public static Entity ConvertToPolyline(this QuickTransaction tr, Entity id) {
            switch (id) {
                case Polyline poly:
                    return id;
                case Line line:
                    return LineToPoly(tr, line);
                case Arc arc:
                    return ArcToPoly(tr, arc);
                case Circle circle:
                default:
                    tr.WriteLine("Unsupported: " + id.GetType().FullName);
                    return null;
            }
        }

        public static void ArcToPoly(this QuickTransaction tr, PromptSelectionResult psRes = null) {
            PromptSelectionOptions psOpts = new PromptSelectionOptions();
            psOpts.MessageForAdding = "\nSelect arcs to convert: ";
            psOpts.MessageForRemoval = "\n...Remove arcs: ";
            TypedValue[] filter = {new TypedValue(0, "ARC")};
            SelectionFilter ssfilter = new SelectionFilter(filter);
            psRes = (psRes?.Status == PromptStatus.OK ? psRes : null) ?? tr.GetSelection(psOpts, ssfilter);
            if (psRes.Status != PromptStatus.OK)
                return;
            foreach (ObjectId id in psRes.Value.GetObjectIds()) {
                Arc arc = (Arc) tr.GetObject(id, OpenMode.ForWrite);
                Polyline poly = new Polyline();
                poly.AddVertexAt(0, new Point2d(arc.StartPoint.X, arc.StartPoint.Y), arc.GetArcBulge(), 0, 0);
                poly.AddVertexAt(1, new Point2d(arc.EndPoint.X, arc.EndPoint.Y), 0, 0, 0);
                poly.LayerId = arc.LayerId;
                tr.BlockTableRecordCurrentSpace.AppendEntity(poly);
                tr.AddNewlyCreatedDBObject(poly, true);
                arc.Erase();
            }
        }

        public static Polyline ArcToPoly(this QuickTransaction tr, Arc arc) {
            BlockTableRecord btr = tr.BlockTableRecordCurrentSpace;
            arc = tr.EnsureWritable(arc);
            Polyline poly = new Polyline();
            poly.AddVertexAt(0, new Point2d(arc.StartPoint.X, arc.StartPoint.Y), arc.GetArcBulge(), 0, 0);
            poly.AddVertexAt(1, new Point2d(arc.EndPoint.X, arc.EndPoint.Y), 0, 0, 0);
            poly.LayerId = arc.LayerId;
            btr.AppendEntity(poly);
            tr.AddNewlyCreatedDBObject(poly, true);
            arc.Erase();
            return poly;
        }

        public static void LineToPoly(this QuickTransaction tr, PromptSelectionResult psRes = null) {
            PromptSelectionOptions psOpts = new PromptSelectionOptions();
            psOpts.MessageForAdding = "\nSelect lines to convert: ";
            psOpts.MessageForRemoval = "\n...Remove lines: ";
            TypedValue[] filter = {new TypedValue(0, "LINE")};
            SelectionFilter ssfilter = new SelectionFilter(filter);
            psRes = (psRes?.Status == PromptStatus.OK ? psRes : null) ?? tr.GetSelection(psOpts, ssfilter);
            if (psRes.Status != PromptStatus.OK)
                return;
            foreach (ObjectId id in psRes.Value.GetObjectIds()) {
                Line line = (Line) tr.GetObject(id, OpenMode.ForWrite);
                Polyline poly = new Polyline();
                poly.AddVertexAt(0, new Point2d(line.StartPoint.X, line.StartPoint.Y), 0, 0, 0);
                poly.AddVertexAt(1, new Point2d(line.EndPoint.X, line.EndPoint.Y), 0, 0, 0);
                poly.LayerId = line.LayerId;
                tr.BlockTableRecordCurrentSpace.AppendEntity(poly);
                tr.AddNewlyCreatedDBObject(poly, true);
                line.Erase();
            }
        }

        public static Polyline LineToPoly(this QuickTransaction tr, Line line) {
            if (tr == null) throw new ArgumentNullException(nameof(tr));
            if (line == null) throw new ArgumentNullException(nameof(line));
            BlockTableRecord btr = tr.BlockTableRecordCurrentSpace;
            line = tr.EnsureWritable(line);

            if (!line.IsWriteEnabled)
                line = (Line) tr.GetObject(line.ObjectId, OpenMode.ForWrite);
            Polyline poly = new Polyline();
            poly.AddVertexAt(0, new Point2d(line.StartPoint.X, line.StartPoint.Y), 0, 0, 0);
            poly.AddVertexAt(1, new Point2d(line.EndPoint.X, line.EndPoint.Y), 0, 0, 0);
            poly.LayerId = line.LayerId;
            btr.AppendEntity(poly);
            tr.AddNewlyCreatedDBObject(poly, true);
            line.Erase();
            return poly;
        }
    }
}