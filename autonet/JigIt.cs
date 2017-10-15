using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;

namespace autonet {
    internal class CoSnapJig : EntityJig {
        private readonly Editor _ed;
        private Point3d _position;
        private Point3d _previousPos;

        public CoSnapJig() : base(new DBText()) {
            _ed = Quick.Editor;
            _ed.PointMonitor +=ed_PointMonitor;
        }

        public new Entity Entity => base.Entity;

        private void ed_PointMonitor(object sender, PointMonitorEventArgs e) {
            _position = e.Context.ComputedPoint;
        }

        protected override SamplerStatus Sampler(JigPrompts prompts) {
            var options =
                new JigPromptPointOptions("\nSpecify position: ");

            options.UserInputControls = UserInputControls.NoZeroResponseAccepted;
            options.Cursor = CursorType.Crosshair;

            var promptRes = prompts.AcquirePoint(options);

            if (_previousPos == promptRes.Value)
                return SamplerStatus.NoChange;

            _previousPos = promptRes.Value;

            return SamplerStatus.OK;
        }

        protected override bool Update() {
            var oTxt = (DBText) Entity;
            oTxt.Position = _position;
            return true;
        }

        public PromptStatus Run() {
            PromptResult promptResult =
                Quick.Editor.Drag(this);

            return promptResult.Status;
        }
    }
}