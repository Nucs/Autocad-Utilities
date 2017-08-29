using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using AdamsLair.WinForms.Drawing;
using AdamsLair.WinForms.NativeWinAPI;
using AdamsLair.WinForms.PropertyEditing.Editors;

namespace AdamsLair.WinForms.PropertyEditing {
    public class ProviderContext {
        public ProviderContext(PropertyGrid grid, PropertyEditor editor = null) {
            ParentEditor = editor;
            ParentGrid = editor != null ? editor.ParentGrid : grid;
        }

        public PropertyGrid ParentGrid { get; }

        public PropertyEditor ParentEditor { get; }
    }

    public interface IPropertyEditorProvider {
        int IsResponsibleFor(Type baseType, ProviderContext context);
        PropertyEditor CreateEditor(Type baseType, ProviderContext context);
    }

    public class ExpandState {
        private readonly HashSet<string> expandedNodes = new HashSet<string>();

        public ExpandState() { }

        public ExpandState(ExpandState cc) {
            expandedNodes = new HashSet<string>(cc.expandedNodes);
        }

        public void CopyTo(ExpandState other) {
            other.expandedNodes.Clear();
            foreach (var n in expandedNodes) other.expandedNodes.Add(n);
        }

        public bool IsEditorExpanded(GroupedPropertyEditor editor) {
            if (editor == null) return false;
            var id = GetEditorFullId(editor);
            return expandedNodes.Contains(id);
        }

        public void SetEditorExpanded(GroupedPropertyEditor editor, bool expanded) {
            if (editor == null) return;
            var id = GetEditorFullId(editor);
            if (expanded) expandedNodes.Add(id);
            else expandedNodes.Remove(id);
        }

        public void Clear() {
            expandedNodes.Clear();
        }

        public void UpdateFrom(PropertyEditor mainEditor) {
            if (mainEditor == null) return;
            foreach (var child in mainEditor.GetChildEditorsDeep(false).OfType<GroupedPropertyEditor>())
                SetEditorExpanded(child, child.Expanded);
        }

        public void ApplyTo(PropertyEditor mainEditor, bool dontCollapse = true) {
            if (mainEditor == null) return;
            foreach (var child in mainEditor.GetChildEditorsDeep(false).OfType<GroupedPropertyEditor>()) {
                if (child.Expanded && dontCollapse) continue;
                child.Expanded = IsEditorExpanded(child);
            }
        }

        private static string GetEditorId(PropertyEditor editor) {
            if (editor == null) return "";
            return editor.PropertyName + editor.EditedType;
        }

        private static string GetEditorFullId(PropertyEditor editor) {
            if (editor == null) return "";
            var fullId = "";
            while (editor != null) {
                fullId = GetEditorId(editor) + "/" + fullId;
                editor = editor.ParentEditor;
            }
            return fullId;
        }
    }

    public class PropertyGrid : UserControl {
        public const int EditorPriority_None = 0;
        public const int EditorPriority_General = 20;
        public const int EditorPriority_Specialized = 50;
        public const int EditorPriority_Override = 100;
        private bool deferredSizeUpdate;


        private readonly MainEditorProvider editorProvider = new MainEditorProvider();
        private MouseButtons mouseDownTemp = MouseButtons.None;
        private bool mouseInside;
        private bool readOnly;
        private readonly List<object> selectedObjects = new List<object>();
        private bool showNonPublic;
        private bool sortEditorsByName = true;
        private Point splitterDragPos = Point.Empty;
        private int splitterDragValue;
        private float splitterRatio = 2.0f / 5.0f;
        private SplitterState splitterState = SplitterState.None;
        private bool updateScheduled;
        private readonly Timer updateTimer;
        private int updateTimerChangeMs;


        public PropertyGrid() {
            Renderer = new ControlRenderer();

            updateTimer = new Timer();
            updateTimer.Interval = 100;
            updateTimer.Tick += updateTimer_Tick;
            updateTimer.Enabled = true;

            AllowDrop = true;
            AutoScroll = true;
            //this.DoubleBuffered = true;

            SetStyle(ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.Opaque, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
        }


        public IEnumerable<object> Selection => selectedObjects;

        public bool ReadOnly {
            get { return readOnly; }
            set {
                if (readOnly != value) {
                    readOnly = value;
                    if (MainEditor != null) UpdatePropertyEditor();
                }
            }
        }

        public bool ShowNonPublic {
            get { return showNonPublic; }
            set {
                if (showNonPublic != value) {
                    showNonPublic = value;
                    if (MainEditor != null) {
                        var state = new ExpandState();
                        state.UpdateFrom(MainEditor);
                        DisposePropertyEditor();
                        UpdateFromObjects();
                        state.ApplyTo(MainEditor);
                    }
                }
            }
        }

        public PropertyEditor MainEditor { get; private set; }

        public PropertyEditor FocusEditor { get; private set; }

        public ControlRenderer Renderer { get; }

        public float SplitterRatio {
            get => splitterRatio;
            set {
                splitterRatio = Math.Max(50.0f / Width, Math.Min(value, (Width - 50) / (float) Width));
                UpdatePropertyEditor();
                Invalidate();
            }
        }

        public int SplitterPosition {
            get => (int) Math.Round(SplitterRatio * Width);
            set => SplitterRatio = value / (float) Width;
        }

        public bool SortEditorsByName {
            get { return sortEditorsByName; }
            set {
                if (sortEditorsByName != value) {
                    sortEditorsByName = value;
                    if (MainEditor != null) {
                        var state = new ExpandState();
                        state.UpdateFrom(MainEditor);
                        DisposePropertyEditor();
                        UpdateFromObjects();
                        state.ApplyTo(MainEditor);
                    }
                }
            }
        }

        protected override CreateParams CreateParams {
            get {
                var cp = base.CreateParams;
                // This somehow fixes the "ghost scrollbar" bug. Note that it also isn't animated anymore and doesn't react properly to hover events.
                cp.ExStyle |= (int) ExtendedWindowStyles.Composited;
                return cp;
            }
        }

        public event EventHandler<PropertyEditorValueEventArgs> EditingFinished;
        public event EventHandler<PropertyEditorValueEventArgs> ValueChanged;

        public void SelectObject(object obj, bool readOnly = false, int scheduleMs = 0) {
            if (obj == null)
                SelectObjects(new object[0], readOnly, scheduleMs);
            else
                SelectObjects(new[] {obj}, readOnly, scheduleMs);
        }

        public void SelectObjects(IEnumerable<object> objEnum, bool readOnly = false, int scheduleMs = 0) {
            selectedObjects.Clear();
            selectedObjects.AddRange(objEnum);
            this.readOnly = readOnly;

            UpdateFromObjects(scheduleMs);
        }

        public void UpdateFromObjects(int scheduleMs = 0) {
            if (scheduleMs > 0) {
                updateTimerChangeMs = scheduleMs;
                updateScheduled = true;
                return;
            }
            updateScheduled = false;

            if (selectedObjects.Count > 0) {
                var commonType = selectedObjects.First().GetType();
                foreach (var o in selectedObjects.Skip(1)) {
                    var curType = o.GetType();
                    while (commonType != curType && !commonType.IsAssignableFrom(curType))
                        commonType = commonType.BaseType;
                }

                if (MainEditor == null || MainEditor.EditedType != commonType)
                    InitPropertyEditor(commonType);
                else
                    UpdatePropertyEditor();

                MainEditor.PerformGetValue();
            } else if (MainEditor != null) {
                DisposePropertyEditor();
            }
        }

        protected void InitPropertyEditor(Type type) {
            if (MainEditor != null) DisposePropertyEditor();

            FocusEditor = null;
            MainEditor = editorProvider.CreateEditor(type, new ProviderContext(this));
            MainEditor.SizeChanged += mainEditor_SizeChanged;
            MainEditor.ValueChanged += mainEditor_ValueChanged;
            MainEditor.EditingFinished += mainEditor_EditingFinished;
            UpdatePropertyEditor();
            ConfigureEditor(MainEditor);

            if (MainEditor is GroupedPropertyEditor) {
                var mainGroupEditor = MainEditor as GroupedPropertyEditor;
                mainGroupEditor.Expanded = true;
            }

            Invalidate();
        }

        protected void UpdatePropertyEditor() {
            if (MainEditor == null) return;

            MainEditor.ParentGrid = this;
            MainEditor.ParentEditor = null;
            MainEditor.Hints &= ~(PropertyEditor.HintFlags.HasButton | PropertyEditor.HintFlags.ButtonEnabled);
            MainEditor.Getter = ValueGetter;
            MainEditor.Setter = readOnly ? null : (Action<IEnumerable<object>>) ValueSetter;
            MainEditor.Location = Point.Empty;
            MainEditor.Width = ClientSize.Width;
            if (MainEditor is GroupedPropertyEditor) {
                var mainGroupEditor = MainEditor as GroupedPropertyEditor;
                mainGroupEditor.HeaderStyle = GroupedPropertyEditor.GroupHeaderStyle.Emboss;
                mainGroupEditor.Hints &= ~PropertyEditor.HintFlags.HasExpandCheck;
            }
        }

        protected void DisposePropertyEditor() {
            if (MainEditor == null) return;

            MainEditor.SizeChanged -= mainEditor_SizeChanged;
            MainEditor.Dispose();
            MainEditor = null;
            FocusEditor = null;

            Invalidate();
        }

        private void mainEditor_SizeChanged(object sender, EventArgs e) {
            if (AutoScrollMinSize.Height != MainEditor.Height)
                if (MouseButtons != MouseButtons.None)
                    deferredSizeUpdate = true;
                else
                    AutoScrollMinSize = new Size(0, MainEditor.Height);
            Invalidate();
        }

        private void mainEditor_ValueChanged(object sender, PropertyEditorValueEventArgs e) {
            OnValueChanged(e);
        }

        private void mainEditor_EditingFinished(object sender, PropertyEditorValueEventArgs e) {
            OnEditingFinished(e);
        }

        public void RegisterEditorProvider(IPropertyEditorProvider provider) {
            if (editorProvider.SubProviders.Contains(provider)) return;
            editorProvider.SubProviders.Add(provider);
        }

        public void RegisterEditorProvider(IEnumerable<IPropertyEditorProvider> provider) {
            foreach (var prov in provider)
                RegisterEditorProvider(prov);
        }

        public PropertyEditor CreateEditor(Type editedType, PropertyEditor parentEditor) {
            if (parentEditor == null) throw new ArgumentNullException("parentEditor");
            if (parentEditor.ParentGrid != this) throw new ArgumentException("The specified editor is not a child of this PropertyGrid", "parentEditor");
            var e = editorProvider.CreateEditor(editedType, new ProviderContext(this, parentEditor));
            e.EditedType = editedType;
            e.ParentGrid = this;
            if (e.ReadOnly)
                return null;
            return e;
        }

        public virtual void ConfigureEditor(PropertyEditor editor, object configureData = null) {
            editor.ConfigureEditor(configureData);
        }

        public virtual object CreateObjectInstance(Type objectType) {
            return objectType.CreateInstanceOf();
        }

        protected internal virtual void PrepareSetValue() { }
        protected internal virtual void PostSetValue() { }

        protected virtual void OnValueChanged(PropertyEditorValueEventArgs e) {
            if (ValueChanged != null)
                ValueChanged(this, e);
        }

        protected virtual void OnEditingFinished(PropertyEditorValueEventArgs e) {
            if (EditingFinished != null)
                EditingFinished(this, e);
        }

        public void Focus(PropertyEditor editor) {
            if (FocusEditor == editor) return;
            editor = GetFocusReciever(editor);

            if (FocusEditor != null && Focused) FocusEditor.OnLostFocus(EventArgs.Empty);

            FocusEditor = editor;
            ScrollToEditor(FocusEditor);

            if (FocusEditor != null)
                if (!Focused)
                    Focus();
                else
                    FocusEditor.OnGotFocus(EventArgs.Empty);
        }

        private PropertyEditor GetNextFocusEditor(PropertyEditor current) {
            if (current.ParentEditor == null) return null;
            var foundCurrent = false;
            foreach (var child in current.ParentEditor.VisibleChildEditors) {
                if (foundCurrent) return child;
                if (child == current) foundCurrent = true;
            }
            return null;
        }

        private PropertyEditor GetPrevFocusEditor(PropertyEditor current) {
            if (current.ParentEditor == null) return null;
            PropertyEditor last = null;
            foreach (var child in current.ParentEditor.VisibleChildEditors) {
                if (child == current) return last;
                last = child;
            }
            return null;
        }

        public PropertyEditor GetFocusReciever(PropertyEditor primary, bool secondaryNext = true) {
            if (primary == null) return null;
            while (!primary.CanGetFocus) {
                if (secondaryNext) {
                    var childEditorsDeep = primary.GetChildEditorsDeep(true).ToArray();
                    var nextEditor = GetNextFocusEditor(primary);
                    if (childEditorsDeep.Any(e => e.CanGetFocus))
                        primary = childEditorsDeep.FirstOrDefault(e => e.CanGetFocus);
                    else if (nextEditor != null)
                        primary = nextEditor;
                    else if (primary.ParentEditor != null)
                        primary = GetFocusReciever(GetNextFocusEditor(primary.ParentEditor), secondaryNext);
                    else
                        return null;
                } else {
                    var prevEditor = GetPrevFocusEditor(primary);
                    if (prevEditor != null)
                        primary = prevEditor;
                    else
                        primary = GetFocusReciever(primary.ParentEditor, secondaryNext);
                }
                if (primary == null) break;
            }
            return primary;
        }

        public PropertyEditor PickEditorAt(int x, int y, bool scrolled = false) {
            if (MainEditor == null) return null;
            if (scrolled) {
                x -= AutoScrollPosition.X;
                y -= AutoScrollPosition.Y;
            }
            var groupedMainEdit = MainEditor as GroupedPropertyEditor;
            return groupedMainEdit != null ? groupedMainEdit.PickEditorAt(x - ClientRectangle.X, y - ClientRectangle.Y) : MainEditor;
        }

        public void ScrollToEditor(PropertyEditor editor) {
            var editorLoc = editor.Location;
            var editorRect = new Rectangle(editorLoc, editor.Size);
            var scrollPos = AutoScrollPosition;

            if (editorRect.Bottom > ClientRectangle.Y - scrollPos.Y + ClientRectangle.Height)
                scrollPos.Y = -editorRect.Bottom + ClientRectangle.Y + ClientRectangle.Height;
            if (editorRect.Y < ClientRectangle.Y - scrollPos.Y)
                scrollPos.Y = ClientRectangle.Y - editorRect.Y;

            AutoScrollPosition = new Point(-scrollPos.X, -scrollPos.Y);
        }

        protected IEnumerable<object> ValueGetter() {
            return selectedObjects;
        }

        protected void ValueSetter(object val) {
            // Don't set anything. The PropertyGrid doesn't actually contain any value data.
            // What should be changed here anyway? The selection?
        }

        protected override void OnPaint(PaintEventArgs e) {
            base.OnPaint(e);
            e.Graphics.FillRectangle(new SolidBrush(BackColor), ClientRectangle);

            var originalState = e.Graphics.Save();
            if (MainEditor != null) {
                var editorRect = new Rectangle(ClientRectangle.Location, MainEditor.Size);
                editorRect.Intersect(ClientRectangle);
                RectangleF clipRect = editorRect;
                clipRect.Intersect(e.Graphics.ClipBounds);
                e.Graphics.SetClip(clipRect);
                e.Graphics.TranslateTransform(ClientRectangle.X, ClientRectangle.Y + AutoScrollPosition.Y);
                MainEditor.OnPaint(e);
            }
            e.Graphics.Restore(originalState);
        }

        protected override void OnMouseEnter(EventArgs e) {
            if (mouseInside) return; // Ignore if called multiple times - Windows.Forms bug due to dropdown stuff?

            base.OnMouseEnter(e);
            mouseInside = true;

            if (updateScheduled) UpdateFromObjects();

            if (MainEditor != null) MainEditor.OnMouseEnter(EventArgs.Empty);
        }

        protected override void OnMouseLeave(EventArgs e) {
            if (!mouseInside) return; // Ignore if called before MouseEnter - Windows.Forms bug due to dropdown stuff?

            base.OnMouseLeave(e);
            mouseInside = false;

            if (splitterState.HasFlag(SplitterState.Hovered)) {
                splitterState &= ~SplitterState.Hovered;
                Cursor = Cursors.Default;
            } else if (MainEditor != null) {
                MainEditor.OnMouseLeave(EventArgs.Empty);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e) {
            if (!mouseInside) OnMouseEnter(EventArgs.Empty);

            base.OnMouseMove(e);

            if (splitterState.HasFlag(SplitterState.Dragged)) {
                SplitterPosition = splitterDragValue + e.X - splitterDragPos.X;
                if (MainEditor != null) MainEditor.OnGridSplitterChanged();
            } else {
                var splitterPos = SplitterPosition;
                var splitterHovered = e.Button == MouseButtons.None && e.X < splitterPos && e.X >= splitterPos - 6;
                if (splitterHovered && !splitterState.HasFlag(SplitterState.Hovered)) {
                    splitterState |= SplitterState.Hovered;
                    if (MainEditor != null) MainEditor.OnMouseLeave(EventArgs.Empty);
                    Cursor = Cursors.VSplit;
                } else if (!splitterHovered && splitterState.HasFlag(SplitterState.Hovered)) {
                    splitterState &= ~SplitterState.Hovered;
                    Cursor = Cursors.Default;
                    if (MainEditor != null)
                        MainEditor.OnMouseEnter(new MouseEventArgs(
                            e.Button,
                            e.Clicks,
                            e.X - ClientRectangle.X,
                            e.Y - ClientRectangle.Y - AutoScrollPosition.Y,
                            e.Delta));
                }
            }

            if (MainEditor != null && splitterState == SplitterState.None)
                MainEditor.OnMouseMove(new MouseEventArgs(
                    e.Button,
                    e.Clicks,
                    e.X - ClientRectangle.X,
                    e.Y - ClientRectangle.Y - AutoScrollPosition.Y,
                    e.Delta));
        }

        protected override void OnMouseDown(MouseEventArgs e) {
            // Emulate stuff if called in wrong order - Windows.Forms bug due to dropdown stuff?
            if (!mouseInside) OnMouseMove(e);
            if ((mouseDownTemp & e.Button) == e.Button) OnMouseUp(new MouseEventArgs(mouseDownTemp & e.Button, e.Clicks, e.X, e.Y, e.Delta));

            base.OnMouseDown(e);
            mouseDownTemp |= e.Button;

            if (splitterState.HasFlag(SplitterState.Hovered)) {
                splitterState |= SplitterState.Dragged;
                splitterDragPos = e.Location;
                splitterDragValue = SplitterPosition;
            } else if (MainEditor != null) {
                MainEditor.OnMouseDown(new MouseEventArgs(
                    e.Button,
                    e.Clicks,
                    e.X - ClientRectangle.X,
                    e.Y - ClientRectangle.Y - AutoScrollPosition.Y,
                    e.Delta));
            }
        }

        protected override void OnMouseUp(MouseEventArgs e) {
            // Emulate stuff if called in wrong order - Windows.Forms bug due to dropdown stuff?
            if (!mouseInside) OnMouseMove(e);
            if ((mouseDownTemp & e.Button) == MouseButtons.None) OnMouseDown(new MouseEventArgs(e.Button, e.Clicks, e.X, e.Y, e.Delta));

            base.OnMouseUp(e);
            mouseDownTemp &= ~e.Button;

            if (deferredSizeUpdate && MouseButtons == MouseButtons.None) {
                mainEditor_SizeChanged(this, EventArgs.Empty);
                deferredSizeUpdate = false;
            }

            if (splitterState.HasFlag(SplitterState.Dragged)) splitterState &= ~SplitterState.Dragged;
            else if (MainEditor != null)
                MainEditor.OnMouseUp(new MouseEventArgs(
                    e.Button,
                    e.Clicks,
                    e.X - ClientRectangle.X,
                    e.Y - ClientRectangle.Y - AutoScrollPosition.Y,
                    e.Delta));
        }

        protected override void OnMouseClick(MouseEventArgs e) {
            base.OnMouseClick(e);

            if (MainEditor != null && splitterState == SplitterState.None)
                MainEditor.OnMouseClick(new MouseEventArgs(
                    e.Button,
                    e.Clicks,
                    e.X - ClientRectangle.X,
                    e.Y - ClientRectangle.Y - AutoScrollPosition.Y,
                    e.Delta));
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e) {
            base.OnMouseDoubleClick(e);

            if (MainEditor != null && splitterState == SplitterState.None)
                MainEditor.OnMouseDoubleClick(new MouseEventArgs(
                    e.Button,
                    e.Clicks,
                    e.X - ClientRectangle.X,
                    e.Y - ClientRectangle.Y - AutoScrollPosition.Y,
                    e.Delta));
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData) {
            if (keyData.HasFlag(Keys.Up) || keyData.HasFlag(Keys.Down) || keyData.HasFlag(Keys.Left) || keyData.HasFlag(Keys.Right) || keyData.HasFlag(Keys.Tab)) {
                var args = new KeyEventArgs(keyData);
                args.Handled = false;
                OnKeyDown(args);
                if (args.Handled) return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        protected override void OnKeyDown(KeyEventArgs e) {
            base.OnKeyDown(e);
            if (FocusEditor != null) {
                var current = FocusEditor;
                while (current != null) {
                    current.OnKeyDown(e);
                    if (e.Handled) break;
                    current = current.ParentEditor;
                }
            }

            if (!e.Handled)
                if (FocusEditor != null)
                    if (e.KeyCode == Keys.Down) {
                        var current = FocusEditor;
                        var next = GetNextFocusEditor(current);
                        if (next == null)
                            next = current.VisibleChildEditors.FirstOrDefault();
                        if (next == null && current.ParentEditor != null)
                            next = GetNextFocusEditor(current.ParentEditor);

                        next = GetFocusReciever(next, true);
                        if (next != null) next.Focus();
                        e.Handled = true;
                    } else if (e.KeyCode == Keys.Up) {
                        var current = FocusEditor;
                        var prev = GetPrevFocusEditor(current);
                        if (prev == null)
                            prev = current.ParentEditor;

                        prev = GetFocusReciever(prev, false);
                        if (prev != null) prev.Focus();
                        e.Handled = true;
                    } else if (e.KeyCode == Keys.Left) {
                        var current = FocusEditor;
                        while (current != null) {
                            current = current.ParentEditor;
                            while (current != null && !current.CanGetFocus) current = current.ParentEditor;
                            if (current != null) {
                                current.Focus();
                                break;
                            }
                        }
                        e.Handled = true;
                    } else if (e.KeyCode == Keys.Right) {
                        if (FocusEditor != null) {
                            if (FocusEditor is GroupedPropertyEditor) (FocusEditor as GroupedPropertyEditor).Expanded = true;
                            PropertyEditor current = null;
                            current = FocusEditor.VisibleChildEditors.FirstOrDefault(editor => editor.CanGetFocus);
                            if (current == null) current = FocusEditor.GetChildEditorsDeep(true).FirstOrDefault(editor => editor.CanGetFocus);
                            if (current != null) current.Focus();
                        }
                        e.Handled = true;
                    } else if (e.KeyCode == Keys.PageUp || e.KeyCode == Keys.Home) {
                        if (FocusEditor.ParentEditor != null) {
                            var current = FocusEditor.ParentEditor;
                            current = current.VisibleChildEditors.FirstOrDefault(editor => editor.CanGetFocus);
                            if (current != null) current.Focus();
                        }
                        e.Handled = true;
                    } else if (e.KeyCode == Keys.PageDown || e.KeyCode == Keys.End) {
                        if (FocusEditor.ParentEditor != null) {
                            var current = FocusEditor.ParentEditor;
                            current = current.VisibleChildEditors.LastOrDefault(editor => editor.CanGetFocus);
                            if (current != null) current.Focus();
                        }
                        e.Handled = true;
                    }
        }

        protected override void OnKeyUp(KeyEventArgs e) {
            base.OnKeyUp(e);
            if (FocusEditor != null) {
                var current = FocusEditor;
                while (current != null) {
                    current.OnKeyUp(e);
                    if (e.Handled) break;
                    current = current.ParentEditor;
                }
            }
        }

        protected override void OnKeyPress(KeyPressEventArgs e) {
            base.OnKeyPress(e);
            if (FocusEditor != null) {
                var current = FocusEditor;
                while (current != null) {
                    current.OnKeyPress(e);
                    if (e.Handled) break;
                    current = current.ParentEditor;
                }
            }
        }

        protected override void OnDragEnter(DragEventArgs e) {
            base.OnDragEnter(e);

            if (MainEditor != null) {
                var localPoint = PointToClient(new Point(e.X, e.Y));
                var subEvent = new DragEventArgs(
                    e.Data,
                    e.KeyState,
                    localPoint.X - ClientRectangle.X,
                    localPoint.Y - ClientRectangle.Y - AutoScrollPosition.Y,
                    e.AllowedEffect,
                    e.Effect);
                MainEditor.OnDragEnter(subEvent);
                e.Effect = subEvent.Effect;
            }
        }

        protected override void OnDragLeave(EventArgs e) {
            base.OnDragLeave(e);

            if (MainEditor != null) MainEditor.OnDragLeave(EventArgs.Empty);
        }

        protected override void OnDragOver(DragEventArgs e) {
            //Console.WriteLine("OnDragOver");
            base.OnDragOver(e);

            if (MainEditor != null) {
                var localPoint = PointToClient(new Point(e.X, e.Y));
                var subEvent = new DragEventArgs(
                    e.Data,
                    e.KeyState,
                    localPoint.X - ClientRectangle.X,
                    localPoint.Y - ClientRectangle.Y - AutoScrollPosition.Y,
                    e.AllowedEffect,
                    e.Effect);
                MainEditor.OnDragOver(subEvent);
                e.Effect = subEvent.Effect;
            }
        }

        protected override void OnDragDrop(DragEventArgs e) {
            base.OnDragDrop(e);

            if (MainEditor != null) {
                var localPoint = PointToClient(new Point(e.X, e.Y));
                var subEvent = new DragEventArgs(
                    e.Data,
                    e.KeyState,
                    localPoint.X - ClientRectangle.X,
                    localPoint.Y - ClientRectangle.Y - AutoScrollPosition.Y,
                    e.AllowedEffect,
                    e.Effect);
                MainEditor.OnDragDrop(subEvent);
                e.Effect = subEvent.Effect;
            }
        }

        protected override void OnGotFocus(EventArgs e) {
            base.OnGotFocus(e);
            if (FocusEditor != null) FocusEditor.OnGotFocus(EventArgs.Empty);
        }

        protected override void OnLostFocus(EventArgs e) {
            base.OnLostFocus(e);
            if (FocusEditor != null) FocusEditor.OnLostFocus(e);

            // Emulate leaving mouse if losing focus to something that might be a dropdown popup
            if (!Application.OpenForms.OfType<Form>().Any(c => c.Focused || c.ContainsFocus))
                OnMouseLeave(EventArgs.Empty);
        }

        protected override void OnSizeChanged(EventArgs e) {
            base.OnSizeChanged(e);
            UpdatePropertyEditor();
        }

        protected override void OnScroll(ScrollEventArgs se) {
            base.OnScroll(se);
            Invalidate();
            if (ClientRectangle.Contains(PointToClient(Cursor.Position))) {
                var childArgs = new MouseEventArgs(MouseButtons, 0, MousePosition.X, MousePosition.Y, 0);
                OnMouseMove(childArgs);
            }
        }

        protected override Point ScrollToControl(Control activeControl) {
            // Prevent AutoScroll on focus or content resize - will always scroll to top.
            // Solution: Just don't scroll. Won't be needed here anyway.
            return AutoScrollPosition;
            //return base.ScrollToControl(activeControl);
        }

        private void updateTimer_Tick(object sender, EventArgs e) {
            if (updateScheduled) {
                updateTimerChangeMs -= updateTimer.Interval;
                if (updateTimerChangeMs <= 0) UpdateFromObjects();
            }
        }

        [Flags]
        private enum SplitterState {
            None = 0x0,
            Hovered = 0x1,
            Dragged = 0x2
        }

        private class MainEditorProvider : IPropertyEditorProvider {
            public List<IPropertyEditorProvider> SubProviders { get; } = new List<IPropertyEditorProvider>();

            public int IsResponsibleFor(Type baseType, ProviderContext context) {
                return EditorPriority_General;
            }

            public PropertyEditor CreateEditor(Type baseType, ProviderContext context) {
                PropertyEditor e = null;

                // Basic numeric data types
                if (baseType == typeof(sbyte) || baseType == typeof(byte) ||
                    baseType == typeof(short) || baseType == typeof(ushort) ||
                    baseType == typeof(int) || baseType == typeof(uint) ||
                    baseType == typeof(long) || baseType == typeof(ulong) ||
                    baseType == typeof(float) || baseType == typeof(double) || baseType == typeof(decimal)) {
                    e = new NumericPropertyEditor();
                }
                // Basic data type: Boolean
                else if (baseType == typeof(bool)) {
                    e = new BoolPropertyEditor();
                }
                // Basic data type: Flagged Enum
                else if (baseType.IsEnum && baseType.GetCustomAttributes(typeof(FlagsAttribute), true).Any()) {
                    e = new FlaggedEnumPropertyEditor();
                }
                // Basic data type: Other Enums
                else if (baseType.IsEnum) {
                    e = new EnumPropertyEditor();
                }
                // Basic data type: String
                else if (baseType == typeof(string)) {
                    e = new StringPropertyEditor();
                }
                // IList
                else if (typeof(IList).IsAssignableFrom(baseType)) {
                    e = new IListPropertyEditor();
                }
                // IList
                else if (typeof(IDictionary).IsAssignableFrom(baseType)) {
                    e = new IDictionaryPropertyEditor();
                }
                // Unknown data type
                else {
                    // Ask around if any sub-editor can handle it and choose the most specialized
                    var availSubProviders =
                        from p in SubProviders
                        where p.IsResponsibleFor(baseType, context) != EditorPriority_None
                        orderby p.IsResponsibleFor(baseType, context) descending
                        select p;
                    var subProvider = availSubProviders.FirstOrDefault();
                    if (subProvider != null) {
                        e = subProvider.CreateEditor(baseType, context);
                        e.EditedType = baseType;
                        return e;
                    }

                    // If not, default to reflection-driven MemberwisePropertyEditor.
                    // Except for MemberInfo types, since we can't edit them in any
                    // meaningful anyway, and they clutter up the grid.
                    if (typeof(MemberInfo).IsAssignableFrom(baseType))
                        e = new LabelPropertyEditor();
                    else
                        e = new MemberwisePropertyEditor();
                }

                e.EditedType = baseType;
                return e;
            }
        }
    }
}