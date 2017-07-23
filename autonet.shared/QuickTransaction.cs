using System;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace autonet {
    public class QuickTransaction : IDisposable {
        private BlockTable _btbl;
        private BlockTableRecord _btr;
        private LayerTable _lytbl;
        private LinetypeTable _lttbl;
        public Database Db;

        private bool disposed;
        public Document Doc;
        public Document MdiDoc;
        public Editor Editor;
        public Transaction Transaction;

        public QuickTransaction() {
#if AUTOCAD13 
            Doc = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
#else
            Doc = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
#endif
            Db = Doc.Database;
            Editor = Doc.Editor;
            MdiDoc = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
            Transaction = Db.TransactionManager.StartTransaction();
        }

        public BlockTableRecord BlockTableRecordCurrentSpace {
            get {
                if (disposed)
                    return null;
                if (_btr != null)
                    return _btr;
                lock (this) {
                    if (disposed)
                        return null;
                    return _btr ?? (_btr = (BlockTableRecord) Transaction.GetObject(Db.CurrentSpaceId, OpenMode.ForWrite));
                }
            }
        }

        public LayerTable LayerTable {
            get {
                if (disposed)
                    return null;
                if (_lytbl != null)
                    return _lytbl;
                lock (this) {
                    if (disposed)
                        return null;
                    return _lytbl ?? (_lytbl = (LayerTable) Transaction.GetObject(Db.LayerTableId, OpenMode.ForRead));
                }
            }
        }
        public LinetypeTable LineTable {
            get {
                if (disposed)
                    return null;
                if (_lttbl != null)
                    return _lttbl;
                lock (this) {
                    if (disposed)
                        return null;
                    return _lttbl ?? (_lttbl = (LinetypeTable) Transaction.GetObject(Db.LinetypeTableId, OpenMode.ForRead));
                }
            }
        }

        public BlockTable BlockTable {
            get {
                if (disposed)
                    return null;
                if (_btbl != null)
                    return _btbl;
                lock (this) {
                    if (disposed)
                        return null;
                    return _btbl ?? (_btbl = (BlockTable) Db.BlockTableId.GetObject(OpenMode.ForRead));
                }
            }
        }

        /// <summary>
        ///     Is the transaction disposed or this object disposed.
        /// </summary>
        public bool IsDisposed => Transaction.IsDisposed || disposed;


        public void Dispose() {
            lock (this) {
                disposed = true;
                _btbl = null;
                _btr = null;
                _lytbl = null;
                if (Transaction?.IsDisposed == false)
                    try {
                        Transaction.Dispose();
                    } catch (InvalidOperationException) { }
            }
        }

        public void SetImpliedSelection(SelectionSet selectionSet) {
            Editor.SetImpliedSelection(selectionSet);
        }

        /// <summary>
        ///     see: https://knowledge.autodesk.com/search-result/caas/CloudHelp/cloudhelp/2015/ENU/AutoCAD-NET/files/GUID-D4987D00-1164-4217-A82E-B8B49FFB7A29-htm.html
        /// </summary>
        /// <param name="selectedObjects"></param>
        public void SetImpliedSelection(ObjectId[] selectedObjects) {
            Editor.SetImpliedSelection(selectedObjects);
        }

        public void AddNewlyCreatedDBObject(DBObject obj, bool add) {
            Transaction.AddNewlyCreatedDBObject(obj, add);
        }

        public DBObject GetObject(ObjectId id, OpenMode mode, bool openErased, bool forceOpenOnLockedLayer) {
            return Transaction.GetObject(id, mode, openErased, forceOpenOnLockedLayer);
        }

        public DBObject GetObject(ObjectId id, OpenMode mode, bool openErased) {
            return Transaction.GetObject(id, mode, openErased);
        }

        public DBObject GetObject(ObjectId id, OpenMode mode) {
            return Transaction.GetObject(id, mode);
        }

        public DBObjectCollection GetAllObjects() {
            return Transaction.GetAllObjects();
        }

        private bool __hascommited = false;

        public void Commit() {
            if (__hascommited)
                throw new InvalidOperationException("Can't commit twice with the same transaction! Autocad will crash.");
            try {
                Transaction.Commit();
            } catch (InvalidOperationException) { }
            __hascommited = true;
        }

        public void Abort() {
            Transaction.Abort();
        }

        public void WriteLine(object s) {
            Write(s + "\n");
        }

        public void Write(object s) {
            Editor.WriteMessage(s + "\n");
        }

        public PromptSelectionResult GetSelection(PromptSelectionOptions options, SelectionFilter filter) {
            return Editor.GetSelection(options, filter);
        }


        public PromptSelectionResult GetSelection(PromptSelectionOptions options) {
            return Editor.GetSelection(options);
        }

        public PromptSelectionResult GetSelection(SelectionFilter filter) {
            return Editor.GetSelection(filter);
        }

        public PromptSelectionResult GetSelection() {
            return Editor.GetSelection();
        }

        public void Regen() {
            Editor.Regen();
        }

        public PromptSelectionResult SelectAll(SelectionFilter filter) {
            return Editor.SelectAll(filter);
        }

        public PromptSelectionResult SelectAll() {
            return Editor.SelectAll();
        }


        public PromptSelectionResult SelectCrossingWindow(Point3d pt1, Point3d pt2, SelectionFilter filter, bool forceSubEntitySelection) {
            return Editor.SelectCrossingWindow(pt1, pt2, filter, forceSubEntitySelection);
        }

        public PromptSelectionResult SelectCrossingWindow(Point3d pt1, Point3d pt2, SelectionFilter filter) {
            return Editor.SelectCrossingWindow(pt1, pt2, filter);
        }

        public PromptSelectionResult SelectCrossingWindow(Point3d pt1, Point3d pt2) {
            return Editor.SelectCrossingWindow(pt1, pt2);
        }

        public PromptSelectionResult SelectCrossingPolygon(Point3dCollection polygon, SelectionFilter filter) {
            return Editor.SelectCrossingPolygon(polygon, filter);
        }

        public PromptSelectionResult SelectCrossingPolygon(Point3dCollection polygon) {
            return Editor.SelectCrossingPolygon(polygon);
        }

        public PromptSelectionResult SelectFence(Point3dCollection fence, SelectionFilter filter) {
            return Editor.SelectFence(fence, filter);
        }

        public PromptSelectionResult SelectFence(Point3dCollection fence) {
            return Editor.SelectFence(fence);
        }

        public PromptSelectionResult SelectWindow(Point3d pt1, Point3d pt2, SelectionFilter filter) {
            return Editor.SelectWindow(pt1, pt2, filter);
        }

        public PromptSelectionResult SelectWindow(Point3d pt1, Point3d pt2) {
            return Editor.SelectWindow(pt1, pt2);
        }

        public PromptSelectionResult SelectWindowPolygon(Point3dCollection polygon, SelectionFilter filter) {
            return Editor.SelectWindowPolygon(polygon, filter);
        }

        public PromptSelectionResult SelectWindowPolygon(Point3dCollection polygon) {
            return Editor.SelectWindowPolygon(polygon);
        }

        public PromptSelectionResult SelectImplied() {
            return Editor.SelectImplied();
        }

        public PromptSelectionResult SelectLast() {
            return Editor.SelectLast();
        }

        public PromptSelectionResult SelectPrevious() {
            return Editor.SelectPrevious();
        }

        /// <summary>
        ///     Execute a command much a like in LISP. Instead of spaces write commas.<br></br>
        ///     An empty string is equivalent to pressing enter.
        ///     <example>
        ///         //LISP <br></br>//selection is set loaded before the method call<br></br>
        ///         (command "_.pedit" "_m" selection "" "_j" "" "")<br></br>
        ///         //C#<br></br>
        ///         quicktrans.Command("_.pedit", "_m", <br></br>quicktrans.GetSelection().Value, "", "_j", "", "");
        ///     </example>
        /// </summary>
        /// <param name="parameter"></param>
        public void Command(params object[] parameter) {
            Quick.Command(parameter);
        }

#if !AUTOCAD13

        /// <summary>
        ///     Execute a command much a like in LISP. Instead of spaces write commas.<br></br>
        ///     An empty string is equivalent to pressing enter.
        ///     <example>
        ///         //LISP <br></br>//selection is set loaded before the method call<br></br>
        ///         (command "_.pedit" "_m" selection "" "_j" "" "")<br></br>
        ///         //C#<br></br>
        ///         quicktrans.Command("_.pedit", "_m", <br></br>quicktrans.GetSelection().Value, "", "_j", "", "");
        ///     </example>
        /// </summary>
        /// <param name="parameter"></param>
        public Editor.CommandResult CommandAsync(params object[] parameter) {
            return Editor.CommandAsync(parameter);
        }

#endif

        /// <summary>
        ///     Sends a string command to the commandline, dont forget to add space for it to execute!
        /// </summary>
        public void StringCommand(string command, bool hide = false) {
            Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument.SendStringToExecute(command, true, false, !hide);
        }
    }
}
