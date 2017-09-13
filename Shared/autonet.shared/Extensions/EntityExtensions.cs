using System;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;

namespace autonet.Extensions {
    public static class EntityExtensions {
        public static void SetGlobalWidth(this Polyline p, double width) {
            for (int i = 0; i < p.NumberOfVertices; i++) {
                p.SetEndWidthAt(i, width);
                p.SetStartWidthAt(i, width);
            }
        }

        public static Polyline ConvertToPolyline(this Polyline2d pl2d, QuickTransaction tr) {
            var mSpace = (BlockTableRecord) SymbolUtilityServices.GetBlockModelSpaceId(tr.Db).GetObject(OpenMode.ForWrite);
            if (pl2d.PolyType == Poly2dType.CubicSplinePoly || pl2d.PolyType == Poly2dType.QuadSplinePoly)
                return null;
            using (var pline = new Polyline()) {
                pline.ConvertFrom(pl2d, false);
                mSpace.AppendEntity(pline);
                tr.AddNewlyCreatedDBObject(pline, true);
                pl2d.Erase();
                return pline;
            }
        }

        public static Polyline ConvertToPolyline(this QuickTransaction tr, ObjectId id) {
            return ConvertToPolyline(tr, tr.GetObject(id, OpenMode.ForWrite) as Entity ?? throw new InvalidOperationException("The given object id is not an entity!"));
        }

        public static Polyline ConvertToPolyline(this Entity id, QuickTransaction tr) {
            return ConvertToPolyline(tr, id);
        }

        public static Polyline ConvertToPolyline(this QuickTransaction tr, Entity id) {
            switch (id) {
                case Polyline poly:
                    return poly;
                case Line line:
                    return LineToPoly(tr, line);
                case Arc arc:
                    return ArcToPoly(tr, arc);
                case Circle c:
                    return CircleToPoly(tr, c);
                case Spline sp:
                    return SplineToPoly(tr, sp);
                case Polyline2d p2d:
                    return ConvertToPolyline(p2d, tr);
                default:
                    tr.WriteLine("Unsupported to polyline conversion: " + id.GetType().FullName);
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

        public static Polyline CircleToPoly(this QuickTransaction tr, Circle c) {
            var r = c.Radius;
            new Ellipse();
            var top = c.Center.Add(new Vector3d(0, r, 0));
            var bottom = c.Center.Add(new Vector3d(0, -r, 0));
            var right = c.Center.Add(new Vector3d(r, 0, 0));
            var left = c.Center.Add(new Vector3d(-r, 0, 0));
            var right_cir = new CircularArc3d(bottom, right, top);
            var left_cir = new CircularArc3d(top, left, bottom);
            Polyline poly = new Polyline();
            poly.AddVertexAt(0, new Point2d(right_cir.StartPoint.X, right_cir.StartPoint.Y), right_cir.GetArcBulge(), 0, 0);
            poly.AddVertexAt(1, new Point2d(right_cir.EndPoint.X, right_cir.EndPoint.Y), 0, 0, 0);
            poly.AddVertexAt(2, new Point2d(left_cir.StartPoint.X, left_cir.StartPoint.Y), left_cir.GetArcBulge(), 0, 0);
            poly.AddVertexAt(3, new Point2d(left_cir.EndPoint.X, left_cir.EndPoint.Y), 0, 0, 0);
            poly.LayerId = c.LayerId;
            tr.BlockTableRecordCurrentSpace.AppendEntity(poly);
            tr.AddNewlyCreatedDBObject(poly, true);
            c.Erase();
            return poly;
        }

        public static Polyline EllipseToPoly(this QuickTransaction tr, Ellipse c) {
            return null;
        }

        public static Polyline SplineToPoly(this QuickTransaction tr, Spline c) {
            var p = c.ToPolyline();
            tr.BlockTableRecordCurrentSpace.AppendEntity(p);
            tr.AddNewlyCreatedDBObject(p, true);
            c.Erase();
            return (Polyline) p;
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

        // Adds an arc (fillet) at each vertex, if able.
        public static void FilletAll(this Polyline pline, double radius) {
            int i = pline.Closed ? 0 : 1;
            for (int j = 0; j < pline.NumberOfVertices - i; j += 1 + pline.FilletAt(j, radius)) { }
        }

        // Adds an arc (fillet) at the specified vertex. Returns 1 if the operation succeeded, 0 if it failed.
        public static int FilletAt(this Polyline pline, int index, double radius) {
            int prev = index == 0 && pline.Closed ? pline.NumberOfVertices - 1 : index - 1;
            if (pline.GetSegmentType(prev) != SegmentType.Line ||
                pline.GetSegmentType(index) != SegmentType.Line)
                return 0;
            LineSegment2d seg1 = pline.GetLineSegment2dAt(prev);
            LineSegment2d seg2 = pline.GetLineSegment2dAt(index);
            Vector2d vec1 = seg1.StartPoint - seg1.EndPoint;
            Vector2d vec2 = seg2.EndPoint - seg2.StartPoint;
            double angle = (Math.PI - vec1.GetAngleTo(vec2)) / 2.0;
            double dist = radius * Math.Tan(angle);
            if (dist > seg1.Length || dist > seg2.Length)
                return 0;
            Point2d pt1 = seg1.EndPoint + vec1.GetNormal() * dist;
            Point2d pt2 = seg2.StartPoint + vec2.GetNormal() * dist;
            double bulge = Math.Tan(angle / 2.0);
            if (Clockwise(seg1.StartPoint, seg1.EndPoint, seg2.EndPoint))
                bulge = -bulge;
            pline.AddVertexAt(index, pt1, bulge, 0.0, 0.0);
            pline.SetEndWidthAt(index, pline.GetStartWidthAt(index + 1));
            pline.SetStartWidthAt(index, pline.GetEndWidthAt(index - 1));
            pline.SetPointAt(index + 1, pt2);
            return 1;
        }

        // Evaluates if the points are clockwise.
        private static bool Clockwise(Point2d p1, Point2d p2, Point2d p3) {
            return ((p2.X - p1.X) * (p3.Y - p1.Y) - (p2.Y - p1.Y) * (p3.X - p1.X)) < 1e-8;
        }
    }
}