/*namespace autonet.Forms {
    //Microsoft
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    //AutoCAD
    using Autodesk.AutoCAD.ApplicationServices;
    using Autodesk.AutoCAD.Colors;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.EditorInput;
    using Autodesk.AutoCAD.Geometry;
    using Autodesk.AutoCAD.Runtime;


    namespace Edit_Entity_AutoCad
    {
        public class Edit_Entity : IExtensionApplication
        {
            #region utility
            static public bool GetPoint(out Point3d point, string str)
            {
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
                PromptPointOptions ptopts = new PromptPointOptions(str);
                PromptPointResult ptRes = ed.GetPoint(ptopts);
                point = ptRes.Value;
                if (ptRes.Status == PromptStatus.Cancel)
                {
                    return false;
                }
                else
                    return true;
            }

            static private Point3d PolarPoint(Point3d basept, Double angle, Double distance)
            {
                return new Point3d(basept.X + distance * Math.Cos(angle),
                    basept.Y + distance * Math.Sin(angle), basept.Z);
            }

            static private Double Angle(Point3d start_pt, Point3d end_pt)
            {
                double dx = Math.Abs(end_pt.X - start_pt.X);
                double dy = Math.Abs(end_pt.Y - start_pt.Y);
                if (end_pt.X < start_pt.X && end_pt.Y > start_pt.Y && dx != 0 && dy != 0)
                    return Math.PI - Math.Atan(dy / dx);
                if (end_pt.X < start_pt.X && end_pt.Y < start_pt.Y && dx != 0 && dy != 0)
                    return Math.Atan(dy / dx) + Math.PI;
                if (end_pt.X > start_pt.X && end_pt.Y < start_pt.Y && dx != 0 && dy != 0)
                    return 2 * Math.PI - Math.Atan(dy / dx);
                if (end_pt.Y < start_pt.Y && dx == 0)
                    return 1.5 * Math.PI;
                if (end_pt.X < start_pt.X && dy == 0)
                    return Math.PI;
                return Math.Atan(dy / dx);
            }

            static private Double Distance(Point3d start_pt, Point3d end_pt)
            {
                return Math.Sqrt(Math.Pow(end_pt.X - start_pt.X, 2) + Math.Pow(end_pt.Y - start_pt.Y, 2));
            }

            static private Double Distance(Point2d start_pt, Point2d end_pt)
            {
                return Math.Sqrt(Math.Pow(end_pt.X - start_pt.X, 2) + Math.Pow(end_pt.Y - start_pt.Y, 2));
            }

            static private Double Distance(double delta_X, double delta_Y)
            {
                return Math.Sqrt(Math.Pow(delta_X, 2) + Math.Pow(delta_Y, 2));
            }

            static private double BulgeSegmentsPline(Point3d p1, Point3d p2, Point3d p3)
            {
                if ((int)ScalarMultiplicationVectors(p2, p1, p2, p3) <= 0 && (int)MultiplicationVectors(p1, p3, p1, p2) == 0)
                    return 0;
                Point2d tp;
                Point3d ph1 = PolarPoint(p1, Angle(p1, p2), 0.5 * Distance(p1, p2));
                Point3d ph2 = PolarPoint(p2, Angle(p2, p3), 0.5 * Distance(p2, p3));
                Inters_line(out tp, ph1, PolarPoint(ph1, 0.5 * Math.PI + Angle(p1, p2), 10), ph2, PolarPoint(ph2, 0.5 * Math.PI + Angle(p2, p3), 10), true);

                double alfa = AngleBetweenVectors(new Point3d(tp.X, tp.Y, 0.0), p1, new Point3d(tp.X, tp.Y, 0.0), p3);

                if ((MultiplicationVectors(p1, p3, p1, p2) * MultiplicationVectors(p1, p3, p1, new Point3d(tp.X, tp.Y, 0.0))) > 0)
                    alfa = 2 * Math.PI - alfa;
                if (MultiplicationVectors(p1, p3, p1, p2) > 0)
                    return Math.Tan(-0.25 * alfa);
                else
                    return Math.Tan(0.25 * alfa);
            }

            //косое произведение векторов
            static private double MultiplicationVectors(Point3d p1V1, Point3d p2V1, Point3d p1V2, Point3d p2V2)
            {
                return (p2V1.X - p1V1.X) * (p2V2.Y - p1V2.Y) - (p2V2.X - p1V2.X) * (p2V1.Y - p1V1.Y);
            }

            static private double MultiplicationVectors(Line vector1, Line vector2, double eps)
            {
                Point3d p1V1 = vector1.StartPoint;
                Point3d p2V1 = vector1.EndPoint;
                Point3d p1V2 = vector2.StartPoint;
                Point3d p2V2 = vector2.EndPoint;
                double a = (p2V1.X - p1V1.X) * (p2V2.Y - p1V2.Y) - (p2V2.X - p1V2.X) * (p2V1.Y - p1V1.Y);
                if (Math.Abs(a) < eps)
                    return 0.0;
                else
                    return a;
            }

            //скалярное произведение векторов
            static private double ScalarMultiplicationVectors(Point3d p1V1, Point3d p2V1, Point3d p1V2, Point3d p2V2)
            {
                return (p2V1.X - p1V1.X) * (p2V2.X - p1V2.X) + (p2V1.Y - p1V1.Y) * (p2V2.Y - p1V2.Y);
            }

            static private double ScalarMultiplicationVectors(Line vector1, Line vector2, double eps)
            {
                Point3d p1V1 = vector1.StartPoint;
                Point3d p2V1 = vector1.EndPoint;
                Point3d p1V2 = vector2.StartPoint;
                Point3d p2V2 = vector2.EndPoint;
                double a = (p2V1.X - p1V1.X) * (p2V2.X - p1V2.X) + (p2V1.Y - p1V1.Y) * (p2V2.Y - p1V2.Y);
                if (Math.Abs(a) < eps)
                    return 0.0;
                else
                    return a;
            }
            #endregion

            private bool PolylineEqualsEntity(Transaction tr, ref Polyline pl,
                ref List<ObjectId> ss, List<ObjectId> ids, double eps)
            {
                Plane pla = new Plane(new Point3d(0, 0, 0), new Vector3d(0, 0, 1));
                bool j = false;
                foreach (ObjectId id in ids)
                {
                    Point3d pt1 = pl.GetPoint3dAt(0);
                    Point3d pt2 = pl.GetPoint3dAt(pl.NumberOfVertices - 1);
                    Curve ob = tr.GetObject(id, OpenMode.ForRead) as Curve;
                    if (ob == null || (ob as Entity).IsErased || ob.Closed == true ||
                        (
                            !(ob is Line) &&
                            !(ob is Arc) &&
                            !(ob is Polyline) &&
                            !(ob is Polyline2d)
                        )
                    )
                    {
                        ss.Remove(id);
                        continue;
                    }
                    #region "LINE,ARC"
                    if (ob is Line || ob is Arc)
                    {
                        if (Distance(pt1, ob.StartPoint) < eps)
                        {
                            pl.AddVertexAt(0, ob.EndPoint.Convert2d(pla), 0, 0, 0);
                            if (ob is Arc)
                                pl.SetBulgeAt(0, BulgeSegmentsPline(ob.EndPoint, ob.GetPointAtDist(0.5 * ob.GetDistanceAtParameter(ob.EndParam)), ob.StartPoint));
                            ob.UpgradeOpen();
                            ob.Erase(true);
                            ss.Remove(id);
                            j = true;
                            continue;
                        }
                        if (Distance(pt1, ob.EndPoint) < eps)
                        {
                            pl.AddVertexAt(0, ob.StartPoint.Convert2d(pla), 0, 0, 0);
                            if (ob is Arc)
                                pl.SetBulgeAt(0, BulgeSegmentsPline(ob.StartPoint, ob.GetPointAtDist(0.5 * ob.GetDistanceAtParameter(ob.EndParam)), ob.EndPoint));
                            ob.UpgradeOpen();
                            ob.Erase(true);
                            ss.Remove(id);
                            j = true;
                            continue;
                        }
                        if (Distance(pt2, ob.StartPoint) < eps)
                        {
                            pl.AddVertexAt(pl.NumberOfVertices, ob.EndPoint.Convert2d(pla), 0, 0, 0);
                            if (ob is Arc)
                                pl.SetBulgeAt(pl.NumberOfVertices - 2, BulgeSegmentsPline(ob.StartPoint, ob.GetPointAtDist(0.5 * ob.GetDistanceAtParameter(ob.EndParam)), ob.EndPoint));
                            ob.UpgradeOpen();
                            ob.Erase(true);
                            ss.Remove(id);
                            j = true;
                            continue;
                        }
                        if (Distance(pt2, ob.EndPoint) < eps)
                        {
                            pl.AddVertexAt(pl.NumberOfVertices, ob.StartPoint.Convert2d(pla), 0, 0, 0);
                            if (ob is Arc)
                                pl.SetBulgeAt(pl.NumberOfVertices - 2, BulgeSegmentsPline(ob.EndPoint, ob.GetPointAtDist(0.5 * ob.GetDistanceAtParameter(ob.EndParam)), ob.StartPoint));
                            ob.UpgradeOpen();
                            ob.Erase(true);
                            ss.Remove(id);
                            j = true;
                            continue;
                        }
                    }//LINE, ARC
                    #endregion

                    if (ob is Polyline2d)
                    {
                        BlockTableRecord btr = tr.GetObject(Application.DocumentManager.MdiActiveDocument.Database.CurrentSpaceId, OpenMode.ForWrite) as BlockTableRecord;
                        Polyline2d pl2d = (Polyline2d)ob;
                        Polyline pl1 = new Polyline();
                        ObjectId id1;
                        pl2d.UpgradeOpen();
                        try
                        {
                            if (pl2d.PolyType != Poly2dType.CubicSplinePoly && pl2d.PolyType != Poly2dType.QuadSplinePoly)
                                pl1.ConvertFrom(pl2d, false);
                            id1 = btr.AppendEntity(pl1);
                            tr.AddNewlyCreatedDBObject(pl1, true);
                        }
                        catch
                        {
                            pl2d.Dispose();
                            pl1.Dispose();
                            ss.Remove(id);
                            continue;
                        }
                        if (pl1.NumberOfVertices != 0)
                        {
                            pl2d.Erase();
                            ss.RemoveAt(0);
                            if (pl1.Closed == true)
                                continue;
                            ob = tr.GetObject(id1, OpenMode.ForRead) as Curve;
                        }
                        else
                        {
                            pl2d.Dispose();
                            pl1.Dispose();
                            ss.Remove(id);
                            continue; ;
                        }
                    }

                    #region "POLYLINE"
                    if (ob is Polyline)
                    {
                        if (Distance(pt1, ob.StartPoint) < eps)
                        {
                            for (int i = 1; i < (ob as Polyline).NumberOfVertices; i++)
                            {
                                pl.AddVertexAt(0, (ob as Polyline).GetPoint2dAt(i), 0.0, 0.0, 0.0);
                                pl.SetBulgeAt(0, -(ob as Polyline).GetBulgeAt(i - 1));
                            }
                            ob.UpgradeOpen();
                            ob.Erase(true);
                            ss.Remove(id);
                            j = true;
                            continue;
                        }
                        if (Distance(pt1, ob.EndPoint) < eps)
                        {
                            for (int i = (ob as Polyline).NumberOfVertices - 2; i >= 0; i--)
                            {
                                pl.AddVertexAt(0, (ob as Polyline).GetPoint2dAt(i), 0.0, 0.0, 0.0);
                                pl.SetBulgeAt(0, (ob as Polyline).GetBulgeAt(i));
                            }
                            ob.UpgradeOpen();
                            ob.Erase(true);
                            ss.Remove(id);
                            j = true;
                            continue;
                        }
                        if (Distance(pt2, ob.StartPoint) < eps)
                        {
                            for (int i = 1; i < (ob as Polyline).NumberOfVertices; i++)
                            {
                                pl.AddVertexAt(pl.NumberOfVertices, (ob as Polyline).GetPoint2dAt(i), 0.0, 0.0, 0.0);
                                pl.SetBulgeAt(pl.NumberOfVertices - 2, (ob as Polyline).GetBulgeAt(i - 1));
                            }
                            ob.UpgradeOpen();
                            ob.Erase(true);
                            ss.Remove(id);
                            j = true;
                            continue;
                        }
                        if (Distance(pt2, ob.EndPoint) < eps)
                        {
                            for (int i = (ob as Polyline).NumberOfVertices - 2; i >= 0; i--)
                            {
                                pl.AddVertexAt(pl.NumberOfVertices, (ob as Polyline).GetPoint2dAt(i), 0.0, 0.0, 0.0);
                                pl.SetBulgeAt(pl.NumberOfVertices - 2, -(ob as Polyline).GetBulgeAt(i));
                            }
                            ob.UpgradeOpen();
                            ob.Erase(true);
                            ss.Remove(id);
                            j = true;
                            continue;
                        }
                    }//POLYLINE
                    #endregion
                }//foreach
                return j;
            }

            private Point3dCollection SelectCrossingPolygonLEps(Point3d pt, double eps)
            {
                Point3dCollection pc = new Point3dCollection();
                for (int i = 0; i < 8; i++)
                    pc.Add(PolarPoint(pt, i * 0.25 * Math.PI, eps));
                return pc;
            }

            private List<ObjectId> SelectAt2Point(List<ObjectId> ss, Point3d pt1, Point3d pt2, double eps)
            {
                if (ss.Count == 0) return new List<ObjectId>();
                Editor editor = Application.DocumentManager.MdiActiveDocument.Editor;
                PromptSelectionResult Select;
                Select = editor.SelectCrossingPolygon(SelectCrossingPolygonLEps(pt1, eps));
                List<ObjectId> ss1 = new List<ObjectId>();
                if (Select.Status == PromptStatus.OK)
                {
                    IEnumerable<ObjectId> b = Select.Value.GetObjectIds().Cast<ObjectId>();
                    ss1 = b.Where(id => ss.Contains(id)).Select(id => id).ToList<ObjectId>();
                }
                Select = editor.SelectCrossingPolygon(SelectCrossingPolygonLEps(pt2, eps));
                if (Select.Status == PromptStatus.OK)
                {
                    foreach (ObjectId id in Select.Value.GetObjectIds())
                    {
                        if (ss.Contains(id) && !ss1.Contains(id))
                            ss1.Add(id);
                    }
                }
                return ss1;
            }

            [CommandMethod("jo")]
            public void jo()
            {
                Document doc = Application.DocumentManager.MdiActiveDocument;
                Database db = doc.Database;
                Editor editor = doc.Editor;
                double eps = 0.001;
                PromptSelectionResult Select;
                Select = editor.GetSelection();
                SelectionSet acSSet;
                List<ObjectId> ss = new List<ObjectId>();
                if (Select.Status == PromptStatus.OK)
                {
                    acSSet = Select.Value;
                    IEnumerable<ObjectId> b = acSSet.GetObjectIds().Cast<ObjectId>();
                    ss = b.Select(id => id).ToList<ObjectId>();
                }
                else
                {
                    editor.WriteMessage("\nОбъекты не выбраны!");
                    return;
                }

                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    BlockTableRecord btr = tr.GetObject(db.CurrentSpaceId, OpenMode.ForWrite) as BlockTableRecord;
                    Plane pla = new Plane(new Point3d(0, 0, 0), new Vector3d(0, 0, 1));
                    int ssl = ss.Count;
                    while (ss.Count != 0)
                    {
                        Polyline pl = new Polyline();
                        pl.SetDatabaseDefaults();
                        Curve ob = tr.GetObject(ss[0], OpenMode.ForRead) as Curve;

                        if (ob == null || (ob as Entity).IsErased ||
                            (
                                !(ob is Line) &&
                                !(ob is Arc) &&
                                !(ob is Polyline) &&
                                !(ob is Polyline2d)
                            )
                        )
                        {
                            ss.RemoveAt(0);
                            continue;
                        }

                        pl.SetPropertiesFrom(ob);
                        if (ob is Line || ob is Arc)
                        {
                            pl.AddVertexAt(pl.NumberOfVertices, ob.StartPoint.Convert2d(pla), 0, 0, 0);
                            pl.AddVertexAt(pl.NumberOfVertices, ob.EndPoint.Convert2d(pla), 0, 0, 0);
                            if (ob is Arc)
                                pl.SetBulgeAt(0, BulgeSegmentsPline(ob.StartPoint, ob.GetPointAtDist(0.5 * ob.GetDistanceAtParameter(ob.EndParam)), ob.EndPoint));
                            ss.RemoveAt(0);
                            ob.UpgradeOpen();
                            ob.Erase(true);
                        }

                        if (ob is Polyline)
                        {
                            pl = ob as Polyline;
                            ss.RemoveAt(0);
                            if (pl.Closed == true)
                                continue;
                            ob.UpgradeOpen();
                        }

                        if (ob is Polyline2d)
                        {
                            Polyline2d pl2d = (Polyline2d)tr.GetObject(ss[0], OpenMode.ForWrite);
                            if (pl2d.PolyType != Poly2dType.CubicSplinePoly && pl2d.PolyType != Poly2dType.QuadSplinePoly)
                                pl.ConvertFrom(pl2d, false);
                            pl2d.Erase();
                            ss.RemoveAt(0);
                            if (pl.Closed == true)
                                continue;
                        }

                        List<ObjectId> ss1 = SelectAt2Point(ss, pl.GetPoint3dAt(0), pl.GetPoint3dAt(pl.NumberOfVertices - 1), eps);
                        while (ss1.Count != 0)
                        {
                            if (PolylineEqualsEntity(tr, ref pl, ref ss, ss1, eps))
                                ss1 = SelectAt2Point(ss, pl.GetPoint3dAt(0), pl.GetPoint3dAt(pl.NumberOfVertices - 1), eps);
                            else
                                ss1.Clear(); ;
                        }


                        if (Distance(pl.GetPoint3dAt(0), pl.GetPoint3dAt(pl.NumberOfVertices - 1)) < eps)
                        {
                            pl.RemoveVertexAt(pl.NumberOfVertices - 1);
                            pl.Closed = true;
                        }
                        if (!(ob is Polyline))
                        {
                            btr.AppendEntity(pl);
                            tr.AddNewlyCreatedDBObject(pl, true);
                        }
                    }
                    tr.Commit();
                }

                editor.WriteMessage("\n");
                //editor.Regen();
            }// function "jo"

            #region Initialize
            public void Initialize()
            {
                Editor editor = Application.DocumentManager.MdiActiveDocument.Editor;
                editor.WriteMessage("Инициализация плагина.." + Environment.NewLine);
            }

            public void Terminate()
            {
            }
            #endregion
        }//public class Entity_AutoCad : IExtensionApplication
    }
}*/