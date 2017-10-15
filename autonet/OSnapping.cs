using System;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Geometry;

namespace autonet {
    public static class OSnapping {
        public static Point3d Snap(Point3d? point) {
            var q = new CoSnapJig();
            q.Run();
            return AcadProperties.ComputedCursor ?? point ?? AcadProperties.Default3d;
        }

        public static Point3d SnapIfEnabled(Point3d? point) {
            if (AcadProperties.IsOSnapEnabled == false)
                return point ?? AcadProperties.Default3d;
            return Snap(point);
        }
    }
}