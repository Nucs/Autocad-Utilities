using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using autonet.Common.Settings;
using autonet.Extensions;
#if AdamsLairWinForms
using AdamsLair.WinForms.ColorControls;
#endif
using BizArk.Core;
namespace autonet.Forms {
    public class SettingsForm<TSettings> : Form, ISaveable {
        public virtual TSettings Settings { get; }

        public delegate TVal ValueGetter<out TVal, in TCtrl>(TCtrl control);

        public delegate void ValueSetter<in TVal>(TSettings settings, TVal val);

        public BlockingCollection<IBindable> Binds { get; } = new BlockingCollection<IBindable>();

        public SettingsForm() {
            bool designMode = (LicenseManager.UsageMode == LicenseUsageMode.Designtime); //check if it is the designer that attacks this 
            if (designMode == false && typeof(TSettings).IsSubclassOf(typeof(JsonConfiguration)) == false)
                throw new InvalidOperationException("TSettings is not of type JsonConfiguration!");
            snapshot = (TSettings) ((JsonConfiguration) (object) Settings).Clone();
        }

        private TSettings snapshot { get; }
        public string FileName { get; }

        public void Save(string filename) {
            lock (Binds) {
                foreach (var bindable in Binds) {
                    bindable.ControlToSettings();
                }
            }
            var s = (JsonConfiguration) (object) Settings;
            BeforeSave();
            s.Save(filename);
            AfterSave();
            this.Invalidate();
        }

        /// <summary>
        ///     
        /// </summary>
        public virtual void Save() {
            lock (Binds) {
                foreach (var bindable in Binds) {
                    bindable.ControlToSettings();
                }
            }
            var s = (JsonConfiguration) (object) Settings;
            BeforeSave();
            s.Save();
            AfterSave();
            this.Invalidate();
        }

        public void AfterLoad() {
            (Settings as JsonConfiguration)?.AfterLoad();
        }

        public void BeforeSave() {
            (Settings as JsonConfiguration)?.BeforeSave();
        }

        public void AfterSave() {
            (Settings as JsonConfiguration)?.AfterSave();
        }

        /// <summary>
        ///     Basicly reloads the initial data from a snapshot from loading the settings
        /// </summary>
        public virtual void RevertChanges() {
            if (snapshot==null)
                throw new ArgumentNullException("snapshot");
            //todo when saving and then reverting, for some reason snapshot is not copied properly.
            lock (Binds) {
                foreach (var bindable in Binds) {
                    bindable.SettingsToControl(snapshot);
                }
            }
            var s = (JsonConfiguration) (object) snapshot;
            LoadSettings();
            this.Invalidate();
        }

        public virtual void Cancel() {
            Close();
        }

        public virtual void LoadSettings() {
            lock (Binds) {
                foreach (var bindable in Binds) {
                    bindable.SettingsToControl();
                }
            }

            var s = (JsonConfiguration) (object) Settings;
            s.AfterLoad();
            this.Invalidate();
        }

        /// <summary>
        ///     Binds a settings property to control via ONLY the same type.
        /// </summary>
        /// <typeparam name="TControl"></typeparam>
        /// <param name="ctrl"></param>
        /// <param name="sexp"></param>
        protected void Bind<TControl>(TControl ctrl, Expression<Func<TSettings, object>> sexp, Expression<Func<TControl, object>> cexp) where TControl : Control {
            var sprop = GetPropertyInfo(Settings, sexp); //Settings propery.
            var cprop = GetPropertyInfo(ctrl, cexp); //Settings propery.
            if (sprop.PropertyType != cprop.PropertyType)
                throw new InvalidOperationException($"Error binding {typeof(TControl).Name} to {Settings.GetType().Name}.{(((cexp.Body) as MemberExpression).Member.Name)}, Cannot bind {(cprop.PropertyType.Name)} value to {sprop.PropertyType.Name} value.");
            var vbind = new Bindable(Settings, ctrl, sprop, cprop);
            Bind(vbind);
        }

        /// <summary>
        ///     Binds a settings property to control via auto type mapping to common form controls.
        /// </summary>
        /// <typeparam name="TControl"></typeparam>
        /// <param name="ctrl"></param>
        /// <param name="sexp"></param>
        protected void Bind<TControl>(TControl ctrl, Expression<Func<TSettings, object>> sexp) where TControl : Control {
            var sprop = GetPropertyInfo(Settings, sexp); //Settings propery.
            Bind(ctrl, sprop);
        }

        protected void Bind(IBindable bindable) {
            lock (Binds) {
                Binds.Add(bindable);
            }
        }

        private PropertyInfo GetPropertyInfo<TSource, TProperty>(TSource source, Expression<Func<TSource, TProperty>> propertyLambda) {
            Type type = typeof(TSource);

            MemberExpression member;
            if (propertyLambda.Body is UnaryExpression e) {
                member = e.Operand as MemberExpression;
            } else {
                member = propertyLambda.Body as MemberExpression;
            }

            if (member == null)
                throw new ArgumentException($"Expression '{propertyLambda.ToString()}' refers to a method, not a property.");

            PropertyInfo propInfo = member.Member as PropertyInfo;
            if (propInfo == null)
                throw new ArgumentException($"Expression '{propertyLambda.ToString()}' refers to a field, not a property.");

            if (type != propInfo.ReflectedType && !type.IsSubclassOf(propInfo.ReflectedType))
                throw new ArgumentException($"Expresion '{propertyLambda.ToString()}' refers to a property that is not from type {type}.");

            return propInfo;
        }

        protected void Bind<TCtrl>(TCtrl ctrl, string propertyname) where TCtrl : Control {
            PropertyInfo setprop = Settings.GetType().GetProperty(propertyname);
            if (setprop == null) throw new ArgumentNullException(nameof(setprop), "Could not find given property");
            Bind(ctrl, setprop);
        }

        protected void Bind<TCtrl>(TCtrl ctrl, PropertyInfo setprop) where TCtrl : Control {
            if (ctrl == null) throw new ArgumentNullException(nameof(ctrl));
            if (setprop == null) throw new ArgumentNullException(nameof(setprop));
            IBindable binder = null;
            var propertyname = setprop.Name;
            switch ((Control) ctrl) {
                case TextBoxBase tbox:
                    if (setprop.PropertyType != typeof(string) && !ConvertHelper.CanChangeType(setprop.PropertyType, typeof(string)))
                        throw new InvalidOperationException($"Error binding {typeof(TCtrl).Name} to {Settings.GetType().Name}.{propertyname}, Cannot bind string value to {setprop.PropertyType.Name} value.");
                    var pname = nameof(tbox.Text);
                    binder = new Bindable(Settings, ctrl, setprop, ctrl.GetType().GetProperty(nameof(tbox.Text)));
                    break;
                case CheckBox chk:
                    if (setprop.PropertyType != typeof(bool) && !ConvertHelper.CanChangeType(setprop.PropertyType, typeof(bool)))
                        throw new InvalidOperationException($"Error binding {typeof(TCtrl).Name} to {Settings.GetType().Name}.{propertyname}, Cannot bind boolean value to {setprop.PropertyType.Name} value.");
                    binder = new Bindable(Settings, ctrl, setprop, ctrl.GetType().GetProperty(nameof(chk.Checked)));
                    break;
                case Label lbl:
                    if (setprop.PropertyType != typeof(string) && !ConvertHelper.CanChangeType(setprop.PropertyType, typeof(string)))
                        throw new InvalidOperationException($"Error binding {typeof(TCtrl).Name} to {Settings.GetType().Name}.{propertyname}, Cannot bind string value to {setprop.PropertyType.Name} value.");
                    binder = new Bindable(Settings, ctrl, setprop, ctrl.GetType().GetProperty(nameof(lbl.Text)));
                    break;
                case PictureBox pic:
                    if (setprop.PropertyType != typeof(string) && !ConvertHelper.CanChangeType(setprop.PropertyType, typeof(string)))
                        throw new InvalidOperationException($"Error binding {typeof(TCtrl).Name} to {Settings.GetType().Name}.{propertyname}, Cannot bind string value to {setprop.PropertyType.Name} value.");
                    binder = new Bindable(Settings, ctrl, setprop, ctrl.GetType().GetProperty(nameof(pic.ImageLocation)));
                    break;
                case ListView lst:
                    if (setprop.PropertyType != typeof(string) && !ConvertHelper.CanChangeType(setprop.PropertyType, typeof(string)))
                        throw new InvalidOperationException($"Error binding {typeof(TCtrl).Name} to {Settings.GetType().Name}.{propertyname}, Cannot bind string value to {setprop.PropertyType.Name} value.");
                    binder = new Bindable(Settings, ctrl, setprop, ctrl.GetType().GetProperty(nameof(lst.Items)));
                    break;
                case ComboBox cmb:
                    if (setprop.PropertyType != typeof(string) && !ConvertHelper.CanChangeType(setprop.PropertyType, typeof(string)))
                        throw new InvalidOperationException($"Error binding {typeof(TCtrl).Name} to {Settings.GetType().Name}.{propertyname}, Cannot bind string value to {setprop.PropertyType.Name} value.");
                    binder = new Bindable(Settings, ctrl, setprop, ctrl.GetType().GetProperty(nameof(cmb.Text)));
                    break;
#if AdamsLairWinForms
                case ColorShowBox clr:
                    if (setprop.PropertyType != typeof(Color) && !ConvertHelper.CanChangeType(setprop.PropertyType, typeof(Color)))
                        throw new InvalidOperationException($"Error binding {typeof(TCtrl).Name} to {Settings.GetType().Name}.{propertyname}, Cannot bind string value to {setprop.PropertyType.Name} value.");
                    binder = new ActionBindable(Settings, clr,
                        (s, c) => setprop.SetValue(s, ((ColorShowBox)c).Color),
                        (c) => ((ColorShowBox)c).Color,
                        (s, c) => {
                            ((ColorShowBox)c).Color = (Color) ConvertEx.To(setprop.GetValue(s), typeof(Color));
                            c.Invalidate();
                        },
                        (s) => setprop.GetValue(s)
                    );
                    binder = new Bindable(Settings, ctrl, setprop, ctrl.GetType().GetProperty(nameof(clr.Color)));
                    break;
#endif
                default:
                    throw new NotSupportedException("This control is not supported for automatic binding.");
            }

            Bind(binder);
        }

        public delegate void ActionBindableMethod(TSettings settings, Control control);

        public delegate object ReturnControlValue(Control control);
        public delegate object ReturnSettingsValue(TSettings settings);

        public class ActionBindable : IBindable {
            public TSettings Settings { get; }
            public Control Control { get; }
            public ActionBindableMethod ControlToSettingsMethod { get; set; }
            public ReturnControlValue ControlToObjectMethod { get; set; }
            public ActionBindableMethod SettingsToControlMethod { get; set; }
            public ReturnSettingsValue SettingsToObjectMethod { get; set; }

            public void ControlToSettings() {
                ControlToSettingsMethod(Settings, Control);
            }

            public object ControlToObject() {
                return ControlToObjectMethod(Control);
            }

            public object SettingsToObject() {
                return SettingsToObjectMethod(Settings);
            }
            public void SettingsToControl() {
                SettingsToControlMethod(Settings, Control);
            }

            public void SettingsToControl(object snapshot) {
                SettingsToControlMethod((TSettings) snapshot, Control);
            }

            public ActionBindable(TSettings settings, Control control, ActionBindableMethod controlToSettingsMethod, ReturnControlValue controlToObjectMethod, ActionBindableMethod settingsToControlMethod, ReturnSettingsValue settingsToObjectMethod) {
                Settings = settings;
                Control = control;
                ControlToSettingsMethod = controlToSettingsMethod;
                ControlToObjectMethod = controlToObjectMethod;
                SettingsToControlMethod = settingsToControlMethod;
                SettingsToObjectMethod = settingsToObjectMethod;
            }
        }

        public class Bindable : IBindable {
            private ActionBindable ActionBindable { get; }
            public PropertyInfo SettingsProp { get; }
            public PropertyInfo ControlProperty { get; }
            public TSettings Settings { get; }
            public Control Control { get; }

            public Bindable(TSettings settings, Control control, PropertyInfo settingsProp, PropertyInfo controlProperty) {
                SettingsProp = settingsProp;
                ControlProperty = controlProperty;
                Settings = settings;
                Control = control;
                ActionBindable = new ActionBindable(settings,control,
                    (s, c) => SettingsProp.SetValue(s, ConvertEx.To(ControlProperty.GetValue(c), SettingsProp.PropertyType)),
                    (c) => ControlProperty.GetValue(c),
                    (s, c) => {
                        ControlProperty.SetValue(c, ConvertEx.To(SettingsProp.GetValue(s), ControlProperty.PropertyType), null);
                        c.Refresh();
                    }, 
                    (s) => SettingsProp.GetValue(s)
                    );
            }

            public void ControlToSettings() {
                ActionBindable.ControlToSettings();
                
            }

            public object ControlToObject() {
                return ActionBindable.ControlToObject();
            }

            public void SettingsToControl() {
                ActionBindable.SettingsToControl();
                
            }

            public void SettingsToControl(object snapshot) {
                ActionBindable.SettingsToControl(snapshot);
            }

            public object SettingsToObject() {
                return ActionBindable.SettingsToObject();
            }
        }

        public interface IBindable {
            Control Control { get; }

            /// <summary>
            ///     Writes to settings from control.
            /// </summary>
            void ControlToSettings();

            /// <summary>
            ///     Read from control to object
            /// </summary>
            /// <returns></returns>
            object ControlToObject();

            /// <summary>
            ///     Loads from settings to control
            /// </summary>
            void SettingsToControl();

            /// <summary>
            ///     Loads from settings to control from a snapshot of settings
            /// </summary>
            void SettingsToControl(object snapshot);

            /// <summary>
            ///     Returns the object from settings
            /// </summary>
            /// <returns></returns>
            object SettingsToObject();
        }
    }
}