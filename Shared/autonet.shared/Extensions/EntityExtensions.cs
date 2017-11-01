using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Exception = Autodesk.AutoCAD.Runtime.Exception;

namespace autonet.Extensions {
    public static class EntityExtensions {
        public static void SetGlobalWidth(this Polyline p, double width) {
            for (var i = 0; i < p.NumberOfVertices; i++) {
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
            var psOpts = new PromptSelectionOptions();
            psOpts.MessageForAdding = "\nSelect arcs to convert: ";
            psOpts.MessageForRemoval = "\n...Remove arcs: ";
            TypedValue[] filter = {new TypedValue(0, "ARC")};
            var ssfilter = new SelectionFilter(filter);
            psRes = (psRes?.Status == PromptStatus.OK ? psRes : null) ?? tr.GetSelection(psOpts, ssfilter);
            if (psRes.Status != PromptStatus.OK)
                return;
            foreach (var id in psRes.Value.GetObjectIds()) {
                var arc = (Arc) tr.GetObject(id, OpenMode.ForWrite);
                var poly = new Polyline();
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
            var poly = new Polyline();
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
            var btr = tr.BlockTableRecordCurrentSpace;
            arc = tr.EnsureWritable(arc);
            var poly = new Polyline();
            poly.AddVertexAt(0, new Point2d(arc.StartPoint.X, arc.StartPoint.Y), arc.GetArcBulge(), 0, 0);
            poly.AddVertexAt(1, new Point2d(arc.EndPoint.X, arc.EndPoint.Y), 0, 0, 0);
            poly.LayerId = arc.LayerId;
            btr.AppendEntity(poly);
            tr.AddNewlyCreatedDBObject(poly, true);
            arc.Erase();
            return poly;
        }

        public static void LineToPoly(this QuickTransaction tr, PromptSelectionResult psRes = null) {
            var psOpts = new PromptSelectionOptions();
            psOpts.MessageForAdding = "\nSelect lines to convert: ";
            psOpts.MessageForRemoval = "\n...Remove lines: ";
            TypedValue[] filter = {new TypedValue(0, "LINE")};
            var ssfilter = new SelectionFilter(filter);
            psRes = (psRes?.Status == PromptStatus.OK ? psRes : null) ?? tr.GetSelection(psOpts, ssfilter);
            if (psRes.Status != PromptStatus.OK)
                return;
            foreach (var id in psRes.Value.GetObjectIds()) {
                var line = (Line) tr.GetObject(id, OpenMode.ForWrite);
                var poly = new Polyline();
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
            var btr = tr.BlockTableRecordCurrentSpace;
            line = tr.EnsureWritable(line);

            if (!line.IsWriteEnabled)
                line = (Line) tr.GetObject(line.ObjectId, OpenMode.ForWrite);
            var poly = new Polyline();
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
            var i = pline.Closed ? 0 : 1;
            for (var j = 0; j < pline.NumberOfVertices - i; j += 1 + pline.FilletAt(j, radius)) { }
        }

        // Adds an arc (fillet) at the specified vertex. Returns 1 if the operation succeeded, 0 if it failed.
        public static int FilletAt(this Polyline pline, int index, double radius) {
            var prev = index == 0 && pline.Closed ? pline.NumberOfVertices - 1 : index - 1;
            if (pline.GetSegmentType(prev) != SegmentType.Line ||
                pline.GetSegmentType(index) != SegmentType.Line)
                return 0;
            var seg1 = pline.GetLineSegment2dAt(prev);
            var seg2 = pline.GetLineSegment2dAt(index);
            var vec1 = seg1.StartPoint - seg1.EndPoint;
            var vec2 = seg2.EndPoint - seg2.StartPoint;
            var angle = (Math.PI - vec1.GetAngleTo(vec2)) / 2.0;
            var dist = radius * Math.Tan(angle);
            if (dist > seg1.Length || dist > seg2.Length)
                return 0;
            var pt1 = seg1.EndPoint + vec1.GetNormal() * dist;
            var pt2 = seg2.StartPoint + vec2.GetNormal() * dist;
            var bulge = Math.Tan(angle / 2.0);
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
            return (p2.X - p1.X) * (p3.Y - p1.Y) - (p2.Y - p1.Y) * (p3.X - p1.X) < 1e-8;
        }

        /// <summary>
        ///     Convert to 2d point, simply ignores Z.
        /// </summary>
        /// <param name="pt"></param>
        /// <returns></returns>
        public static Point2d ToPoint2D(this Point3d pt) {
            return new Point2d(pt.X, pt.Y);
        }

        /// <summary>
        ///     Convert to 3d point, simply ignores Z.
        /// </summary>
        /// <param name="pt"></param>
        /// <returns></returns>
        public static Point3d ToPoint3D(this Point2d pt) {
            return new Point3d(pt.X, pt.Y, 0);
        }

        public static int ClosestVertexIndex(this Polyline pl, Point3d pt) {
            return ClosestVertexIndex(pl, pt.ToPoint2D());
        }

        public static int ClosestVertexIndex(this Polyline pl, Point2d pt) {
            pt = pl.GetClosestPointTo(pt.ToPoint3D(), true).ToPoint2D();

            for (var i = 0; i < pl.NumberOfVertices; i++) {
                var dbl = new double();
                var ons = pl.OnSegmentAt(i, pt, dbl);
                if (ons)
                    return i;
            }

            return -1;
        }

        public static Entity[] BreakOnPoint(this Curve ent, Point2d at, QuickTransaction tr, bool commitAtEnd = false) {
            if (ent == null) throw new ArgumentNullException(nameof(ent));
            if (tr == null) throw new ArgumentNullException(nameof(tr));
            try {
                var btr = (BlockTableRecord) tr.GetObject(tr.Db.CurrentSpaceId, OpenMode.ForWrite);
                Point3d att = at.ToPoint3D();
                att.TransformBy(tr.Editor.CurrentUserCoordinateSystem.Inverse());
                // Check that the point is on the curve 
                att = ent.GetClosestPointTo(att, false);
                var breakPoints = new Point3dCollection();
                var newCurves = new DBObjectCollection();
                // Get the segments according to the trim points   
                breakPoints.Add(att);
                newCurves = ent.GetSplitCurves(breakPoints);
                if (newCurves == null) {
                    tr.Editor.WriteMessage("\nGetSplitCurves failed :  Error");
                    return new Entity[0];
                }
                var ret = new List<Entity>();

                // Here we add the segments to the database with different colors   
                for (var i = 0; i < newCurves.Count; i++) {
                    var pent = (Entity) newCurves[i];
                    pent.SetPropertiesFrom(ent);
                    btr.AppendEntity(pent);
                    tr.AddNewlyCreatedDBObject(pent, true);
                    ret.Add(pent);
                }

                ent.UpgradeOpen();
                ent.Erase();
                if (commitAtEnd)
                    tr.Commit();
                return ret.ToArray();
            } catch (Exception ex) {
                Autodesk.AutoCAD.ApplicationServices.Core.Application.ShowAlertDialog(ex.Message + "\n" + ex.StackTrace);
                return new Entity[0];
            } finally { }
        }
        /// <summary>
        ///     Shifts on line from a specific point for <paramref name="distance"/> length.<br></br>Returns true if successful, false if exceeded the limits - will return basepoint!
        /// </summary>
        /// <param name="ent">Ent</param>
        /// <param name="basepoint">the point to offset from</param>
        /// <param name="distance">The distance to offset</param>
        /// <returns>true if successful, false if exceeded the limits - will return basepoint!</returns>
        public static Tuple<Point2d, bool> OffsetToStart(this Curve ent, Point3d basepoint, double distance) {
            return OffsetToEnd(ent, basepoint.ToPoint2D(), -distance);
        }
        /// <summary>
        ///     Shifts on line from a specific point for <paramref name="distance"/> length.<br></br>Returns true if successful, false if exceeded the limits - will return basepoint!
        /// </summary>
        /// <param name="ent">Ent</param>
        /// <param name="basepoint">the point to offset from</param>
        /// <param name="distance">The distance to offset</param>
        /// <returns>true if successful, false if exceeded the limits - will return basepoint!</returns>
        public static Tuple<Point2d, bool> OffsetToStart(this Curve ent, Point2d basepoint, double distance) {
            return OffsetToEnd(ent, basepoint, -distance);
        }

        /// <summary>
        ///     Shifts on line from a specific point for <paramref name="distance"/> length.<br></br>Returns true if successful, false if exceeded the limits - will return basepoint!
        /// </summary>
        /// <param name="ent">Ent</param>
        /// <param name="basepoint">the point to offset from</param>
        /// <param name="distance">The distance to offset</param>
        /// <returns>true if successful, false if exceeded the limits - will return basepoint!</returns>
        public static Tuple<Point2d,bool> OffsetToEnd(this Curve ent, Point3d basepoint, double distance) {
            return OffsetToEnd(ent, basepoint.ToPoint2D(), distance);
        }

        /// <summary>
        ///     Shifts on line from a specific point for <paramref name="distance"/> length.<br></br>Returns true if successful, false if exceeded the limits - will return basepoint!
        /// </summary>
        /// <param name="ent">Ent</param>
        /// <param name="basepoint">the point to offset from</param>
        /// <param name="distance">The distance to offset</param>
        /// <returns>true if successful, false if exceeded the limits - will return basepoint!</returns>
        public static Tuple<Point2d, bool> OffsetToEnd(this Curve ent, Point2d basepoint, double distance) {
            var basepoint3 = ent.GetClosestPointTo(basepoint.ToPoint3D(), true);
            basepoint = basepoint3.ToPoint2D();
            try {
                var dis = ent.GetDistAtPoint(basepoint3);
                var extra = dis + distance;
                var pt = ent.GetPointAtDist(extra);
                return Tuple.Create(pt.ToPoint2D(),true);
            } catch (Exception e) {
                Quick.WriteLine("\nInvalid Input, offset exceeds curve's limits.\n"+e);
                return Tuple.Create(basepoint,false);
            }
        }
        public static Point3d GetPosition(this DBObject obj, Point3d @default) {
            try {
                return GetPosition(obj) ?? @default;
            } catch {
                return @default;
            }
        }

        public static Point3d? GetPosition(this DBObject obj) {
            switch (obj) {
                case BlockReference blockReference:
                    return blockReference.Position;
                case Circle circle:
                    return circle.Center;
                case DBText dbText:
                    return dbText.Position;
                case Ellipse ellipse:
                    return ellipse.Center;
                case Hatch hatch:
                    return hatch.Origin.ToPoint3D();
                case Leader leader:
                    return leader.GetPointAtDist(leader.GetLength() / 2);
                case Line line:
                    return line.GetPointAtDist(line.GetLength() / 2);
                case MLeader mLeader:
                    return mLeader.BlockPosition;
                case Mline mline:
                    return new LineSegment3d(mline.VertexAt(0), mline.VertexAt(mline.NumberOfVertices-1)).MidPoint;
                case MText mText:
                    return mText.Location;
                case PdfReference pdfReference:
                    return pdfReference.Position;
                case Polyline polyline:
                    return polyline.GetPointAtDist(polyline.GetLength() / 2);
                case Polyline2d polyline2D:
                    return polyline2D.GetPointAtDist(polyline2D.GetLength() / 2);
                case Polyline3d polyline3D:
                    return polyline3D.GetPointAtDist(polyline3D.GetLength() / 2);
                case PolylineVertex3d polylineVertex3D:
                    return polylineVertex3D.Position;
                case Shape shape:
                    return shape.Position;
                case Spline spline:
                    return spline.GetPointAtDist(spline.GetLength() / 2);
                case Viewport vp:
                    return vp.CenterPoint;
                case DBPoint point:
                    return point.Position;
                case RotatedDimension rdim:
                    return new LineSegment3d(rdim.XLine1Point, rdim.XLine2Point).MidPoint;
                case Ole2Frame ole2Frame:
                    return ole2Frame.Location;
                case Face face:
                    return new LineSegment3d(face.GetVertexAt(1), face.GetVertexAt(4)).MidPoint;
                case AlignedDimension adim:
                    return new LineSegment3d(adim.XLine1Point, adim.XLine2Point).MidPoint;
                case null:
                    return null;
                default:
                    throw new ArgumentOutOfRangeException(nameof(obj));
            }
        }

        /// <summary>
        /// Check if the point is within the polyline
        /// </summary>
        /// <param name="polygon"></param>
        /// <param name="pt"></param>
        /// <returns></returns>
        public static bool IsInsidePolygon(this Polyline polygon, Point3d pt) {
            int n = polygon.NumberOfVertices;
            double angle = 0;

            for (int i = 0; i < n; i++) {
                Point pt1 = new Point {
                    X = polygon.GetPoint2dAt(i).X - pt.X,
                    Y = polygon.GetPoint2dAt(i).Y - pt.Y
                };
                Point pt2 = new Point {
                    X = polygon.GetPoint2dAt((i + 1) % n).X - pt.X,
                    Y = polygon.GetPoint2dAt((i + 1) % n).Y - pt.Y
                };
                angle += Angle2D(pt1.X, pt1.Y, pt2.X, pt2.Y);
            }

            return !(Math.Abs(angle) < Math.PI);
        }

        /// <summary>
        /// Point structure to add IsInsidePolygon function
        /// </summary>
        private struct Point {
            public double X, Y;
        };

        /*
           
        */
        /// <summary>
        /// Return the angle between two vectors on a plane
        /// The angle is from vector 1 to vector 2, positive anticlockwise
        /// The result is between -pi -> pi
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <returns></returns>
        public static double Angle2D(double x1, double y1, double x2, double y2) {
            double dtheta, theta1, theta2;

            theta1 = Math.Atan2(y1, x1);
            theta2 = Math.Atan2(y2, x2);
            dtheta = theta2 - theta1;
            while (dtheta > Math.PI)
                dtheta -= (Math.PI * 2);
            while (dtheta < -Math.PI)
                dtheta += (Math.PI * 2);
            return(dtheta);
        }

        public static IEnumerable<DBObject> AreInsidePolyline(this IEnumerable<ObjectId> objs, QuickTransaction tr, Polyline pl) {
            return AreInsidePolyline(objs.Select(o=>tr.GetObject(o, OpenMode.ForRead)),tr, pl);
        }

        public static IEnumerable<DBObject> AreInsidePolyline(this IEnumerable<DBObject> objs, QuickTransaction tr, Polyline pl) {
            Point3dCollection pntCol = new Point3dCollection();
            for (int i = 0; i < pl.NumberOfVertices-1; i++) {
                pntCol.Add(pl.GetPoint3dAt(i));
            }

            int numOfEntsFound = 0;
            //new PointCollector();
            var pmtSelRes = Quick.Editor.SelectWindowPolygon(pntCol);
            if (pmtSelRes.Status != PromptStatus.OK)
                yield break;
            // May not find entities in the UCS area
            // between p1 and p3 if not PLAN view
            // pmtSelRes =
            //    ed.SelectCrossingWindow(p1, p3, selFilter);
            var _objs = objs.ToArray();
            if (pmtSelRes.Status == PromptStatus.OK) {
                foreach (ObjectId objId in pmtSelRes.Value.GetObjectIds()) {
                    numOfEntsFound++;
                    if (_objs.Any(o=>o.ObjectId.Handle.Value==objId.Handle.Value))
                        yield return tr.GetObject(objId, false);
                }
                Quick.Editor.WriteMessage("Entities found " + numOfEntsFound.ToString());
            }
            else
                Quick.Editor.WriteMessage("\nDid not find entities");
        }
        
    }
}