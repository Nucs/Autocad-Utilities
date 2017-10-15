using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;

namespace autonet.Extensions {
    public enum EntityType {
        Any,
        _3dface,
        _3dsolid,
        Acad_Proxy_Entity,
        Arc,
        Attdef,
        Attrib,
        Body,
        Circle,
        Dimension,
        Ellipse,
        Hatch,
        Image,
        Insert,
        Leader,
        Line,
        Lwpolyline,
        Mline,
        Mtext,
        Oleframe,
        Ole2frame,
        Point,
        Polyline,
        Ray,
        Region,
        Seqend,
        Shape,
        Solid,
        Spline,
        Text,
        Tolerance,
        Trace,
        Vertex,
        Viewport,
        Xline,
        AnyPolyline
    }

    public static class SelectionFilters {
        private static readonly Dictionary<EntityType, string> entToString = new Dictionary<EntityType, string> {
            {EntityType.Any, "*"},
            {EntityType._3dface, "3dface"},
            {EntityType._3dsolid, "3dsolid"},
            {EntityType.Acad_Proxy_Entity, "Acad_Proxy_Entity"},
            {EntityType.Arc, "Arc"},
            {EntityType.Attdef, "Attdef"},
            {EntityType.Attrib, "Attrib"},
            {EntityType.Body, "Body"},
            {EntityType.Circle, "Circle"},
            {EntityType.Dimension, "Dimension"},
            {EntityType.Ellipse, "Ellipse"},
            {EntityType.Hatch, "Hatch"},
            {EntityType.Image, "Image"},
            {EntityType.Insert, "Insert"},
            {EntityType.Leader, "Leader"},
            {EntityType.Line, "Line"},
            {EntityType.Lwpolyline, "Lwpolyline"},
            {EntityType.Mline, "Mline"},
            {EntityType.Mtext, "Mtext"},
            {EntityType.Oleframe, "Oleframe"},
            {EntityType.Ole2frame, "Ole2frame"},
            {EntityType.Point, "Point"},
            {EntityType.Polyline, "Polyline"},
            {EntityType.Ray, "Ray"},
            {EntityType.Region, "Region"},
            {EntityType.Seqend, "Seqend"},
            {EntityType.Shape, "Shape"},
            {EntityType.Solid, "Solid"},
            {EntityType.Spline, "Spline"},
            {EntityType.Text, "Text"},
            {EntityType.Tolerance, "Tolerance"},
            {EntityType.Trace, "Trace"},
            {EntityType.Vertex, "Vertex"},
            {EntityType.Viewport, "Viewport"},
            {EntityType.Xline, "Xline"},
            {EntityType.AnyPolyline, "*Polyline"}
        };

        /// <summary>
        ///     Returns a filter that will accept any of these types.
        /// </summary>
        public static SelectionFilter AllowAny(params EntityType[] types) {
            if (types == null || types.Length == 0) return new SelectionFilter(new[] {new TypedValue((int) DxfCode.Start, EntityType.Any.AsString()) });
            return new SelectionFilter(new[] {new TypedValue((int) DxfCode.Start, string.Join(",", types.Select(e => e.AsString())))});
        }

        /// <summary>
        ///     Returns a filter that will accept any of these types.<br></br>
        ///     Example: "Circle","Image","MLine".
        /// </summary>
        public static SelectionFilter AllowAny(params string[] types) {
            if (types == null || types.Length == 0) return new SelectionFilter(new[] {new TypedValue((int) DxfCode.Start, EntityType.Any.AsString())});
            return new SelectionFilter(new[] {new TypedValue((int) DxfCode.Start, string.Join(",", types.Where(e=>string.IsNullOrEmpty(e)==false).Select(e => e?.ToUpperInvariant())))});
        }

        /// <summary>
        ///     Will convert the entity type to it's valid string version.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string AsString(this EntityType type) {
            try {
                return entToString[type]?.ToUpperInvariant();
            } catch {
                return "";
            }
        }
    }
}