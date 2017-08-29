using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using AdamsLair.WinForms.Properties;

namespace AdamsLair.WinForms.PropertyEditing {
    public class MemberwisePropertyEditor : GroupedPropertyEditor {
        public delegate bool AutoMemberPredicate(MemberInfo member, bool showNonPublic);

        public delegate void FieldValueSetter(FieldInfo field, IEnumerable<object> targetObjects, IEnumerable<object> values);

        public delegate void PropertyValueSetter(PropertyInfo property, IEnumerable<object> targetObjects, IEnumerable<object> values);

        private bool buttonIsCreate;
        private Predicate<MemberInfo> memberAffectsOthers;
        private FieldValueSetter memberFieldSetter;
        private AutoMemberPredicate memberPredicate;
        private PropertyValueSetter memberPropertySetter;

        public MemberwisePropertyEditor() {
            Hints |= HintFlags.HasButton | HintFlags.ButtonEnabled;
            memberPredicate = DefaultMemberPredicate;
            memberAffectsOthers = DefaultMemberAffectsOthers;
            memberPropertySetter = DefaultPropertySetter;
            memberFieldSetter = DefaultFieldSetter;
        }

        public override object DisplayedValue => GetValue().FirstOrDefault();

        public AutoMemberPredicate MemberPredicate {
            get => memberPredicate;
            set {
                if (value == null) value = DefaultMemberPredicate;
                if (memberPredicate != value) {
                    memberPredicate = value;
                    if (ContentInitialized) InitContent();
                }
            }
        }

        public Predicate<MemberInfo> MemberAffectsOthers {
            get => memberAffectsOthers;
            set {
                if (value == null) value = DefaultMemberAffectsOthers;
                if (memberAffectsOthers != value) {
                    memberAffectsOthers = value;
                    if (ContentInitialized) InitContent();
                }
            }
        }

        public PropertyValueSetter MemberPropertySetter {
            get => memberPropertySetter;
            set {
                if (value == null) value = DefaultPropertySetter;
                memberPropertySetter = value;
            }
        }

        public FieldValueSetter MemberFieldSetter {
            get => memberFieldSetter;
            set {
                if (value == null) value = DefaultFieldSetter;
                memberFieldSetter = value;
            }
        }

        public override void InitContent() {
            BeginUpdate();
            {
                // Clear previous contents and invoke base method
                ClearContent();
                base.InitContent();

                // Generate and add property editors for the current type
                if (EditedType != null) {
                    var valueQuery = GetValue();
                    var values = valueQuery != null ? valueQuery.ToArray() : null;
                    BeforeAutoCreateEditors();
                    foreach (var member in QueryEditedMembers())
                        AddEditorForMember(member, values);
                }
            }
            EndUpdate();

            // Update all values for this editor and its children
            PerformGetValue();
        }

        protected void ReInitContent(IEnumerable<PropertyEditor> updateEditors) {
            if (EditedType == null) return;

            BeginUpdate();
            {
                var values = GetValue().ToArray();
                foreach (var editor in updateEditors)
                    AddEditorForMember(editor.EditedMember, values, editor);
            }
            EndUpdate();

            // Update all values for this editor and its children
            PerformGetValue();
        }

        public PropertyEditor AddEditorForMember(MemberInfo member, IEnumerable<object> values = null, PropertyEditor replaceOld = null) {
            if (!(member is FieldInfo) && !(member is PropertyInfo))
                throw new ArgumentException("Only PropertyInfo and FieldInfo members are supported");

            PropertyEditor e;
            var editType = ReflectTypeForMember(member, values ?? GetValue());
            e = AutoCreateMemberEditor(member);
            if (e == null) e = ParentGrid.CreateEditor(editType, this);
            if (e == null) return null;

            e.BeginUpdate();
            {
                if (member is PropertyInfo) {
                    var property = member as PropertyInfo;
                    e.Getter = CreatePropertyValueGetter(property);
                    e.Setter = property.CanWrite ? CreatePropertyValueSetter(property) : null;
                    e.PropertyName = property.Name;
                    e.EditedMember = property;
                    e.NonPublic = !memberPredicate(property, false);
                } else if (member is FieldInfo) {
                    var field = member as FieldInfo;
                    e.Getter = CreateFieldValueGetter(field);
                    e.Setter = CreateFieldValueSetter(field);
                    e.PropertyName = field.Name;
                    e.EditedMember = field;
                    e.NonPublic = !memberPredicate(field, false);
                }

                if (replaceOld != null) {
                    AddPropertyEditor(e, replaceOld);
                    RemovePropertyEditor(replaceOld);
                    replaceOld.Dispose();
                } else {
                    AddPropertyEditor(e);
                }
                ParentGrid.ConfigureEditor(e);
            }
            e.EndUpdate();

            return e;
        }

        protected override void VerifyReflectedTypeEditors(IEnumerable<object> values) {
            if (EditedType == null) return;
            if (!ContentInitialized) return;

            List<PropertyEditor> invalidEditors = null;
            foreach (var editor in ChildEditors) {
                if (editor.EditedMember == null) continue;
                if (editor.EditedType == null) continue;
                if (!IsAutoCreateMember(editor.EditedMember)) continue;

                var reflectedType = ReflectTypeForMember(editor.EditedMember, values);
                if (reflectedType != editor.EditedType) {
                    if (invalidEditors == null) invalidEditors = new List<PropertyEditor>();
                    invalidEditors.Add(editor);
                }
            }

            if (invalidEditors != null)
                ReInitContent(invalidEditors);
        }

        /// <summary>
        ///     Determines the Type to use as a basis for generating a PropertyEditor for the specified member
        ///     by evaluating the members current value and static Type.
        /// </summary>
        /// <param name="member"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        protected Type ReflectTypeForMember(MemberInfo member, IEnumerable<object> values) {
            if (member is FieldInfo) {
                var field = member as FieldInfo;
                if (values != null)
                    return ReflectDynamicType(field.FieldType, values.Where(v => v != null).Select(v => field.GetValue(v)));
                return field.FieldType;
            }
            if (member is PropertyInfo) {
                var property = member as PropertyInfo;
                if (values != null) {
                    var propertyValues = new List<object>();
                    foreach (var obj in values) {
                        if (obj == null) continue;
                        try {
                            var value = property.GetValue(obj, null);
                            propertyValues.Add(value);
                        } catch (TargetInvocationException) { }
                    }
                    return ReflectDynamicType(property.PropertyType, propertyValues);
                }
                return property.PropertyType;
            }
            throw new ArgumentException("Only PropertyInfo and FieldInfo members are supported");
        }

        protected IEnumerable<MemberInfo> QueryEditedMembers() {
            var propArr = EditedType.GetProperties(
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic);
            var fieldArr = EditedType.GetFields(
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic);

            if (ParentGrid.SortEditorsByName)
                return (
                    from p in propArr
                    where p.CanRead && p.GetIndexParameters().Length == 0 && IsAutoCreateMember(p)
                    orderby GetTypeHierarchyLevel(p.DeclaringType), p.Name
                    select p
                ).Concat((IEnumerable<MemberInfo>)
                    from f in fieldArr
                    where IsAutoCreateMember(f)
                    orderby GetTypeHierarchyLevel(f.DeclaringType), f.Name
                    select f
                );
            return (
                from p in propArr
                where p.CanRead && p.GetIndexParameters().Length == 0 && IsAutoCreateMember(p)
                orderby GetTypeHierarchyLevel(p.DeclaringType)
                select p
            ).Concat((IEnumerable<MemberInfo>)
                from f in fieldArr
                where IsAutoCreateMember(f)
                orderby GetTypeHierarchyLevel(f.DeclaringType)
                select f
            );
        }

        protected IEnumerable<FieldInfo> QueryEditedFields() {
            var fieldArr = EditedType.GetFields(
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic);
            return
                from f in fieldArr
                where IsAutoCreateMember(f)
                orderby GetTypeHierarchyLevel(f.DeclaringType), f.Name
                select f;
        }

        protected override void OnGetValue() {
            base.OnGetValue();
            var valueQuery = GetValue();
            var values = valueQuery != null ? valueQuery.ToArray() : null;

            VerifyReflectedTypeEditors(values);
            BeginUpdate();
            if (values == null) {
                HeaderValueText = null;
                return;
            }
            OnUpdateFromObjects(values);
            EndUpdate();

            foreach (var e in ChildEditors)
                e.PerformGetValue();
        }

        protected override void OnSetValue() {
            if (ReadOnly) return;
            if (!ChildEditors.Any()) return;
            base.OnSetValue();

            foreach (var e in ChildEditors)
                e.PerformSetValue();
        }

        protected virtual void OnUpdateFromObjects(object[] values) {
            string valString = null;

            if (values == null || !values.Any() || values.All(o => o == null)) {
                ClearContent();

                Hints &= ~HintFlags.ExpandEnabled;
                ButtonIcon = ResourcesCache.ImageAdd;
                buttonIsCreate = true;
                Expanded = false;

                valString = "null";
            } else {
                Hints |= HintFlags.ExpandEnabled;
                if (!CanExpand) Expanded = false;
                ButtonIcon = ResourcesCache.ImageDelete;
                buttonIsCreate = false;

                var firstValue = values.First();
                var valueCount = values.Count();

                if (valueCount == 1 && firstValue != null) {
                    var displayedType = firstValue.GetType();
                    var methods = displayedType.GetMethods(BindingFlags.Instance | BindingFlags.Public);
                    var customToStringImplementation = methods.Any(m => m.DeclaringType != typeof(object) && m.Name == "ToString");

                    if (customToStringImplementation)
                        valString = firstValue.ToString();
                    else
                        valString = displayedType.GetTypeCSCodeName(true);
                } else {
                    valString = string.Format(
                        Resources.PropertyGrid_N_Objects,
                        valueCount);
                }
            }

            HeaderValueText = valString;
        }

        protected override void OnEditedTypeChanged() {
            base.OnEditedTypeChanged();
            if (EditedType.IsValueType)
                Hints &= ~HintFlags.HasButton;
            else
                Hints |= HintFlags.HasButton;
            if (ContentInitialized) InitContent();
        }

        protected override void OnEditedMemberChanged() {
            base.OnEditedTypeChanged();
            if (ContentInitialized) InitContent();
        }

        protected override void OnButtonPressed() {
            base.OnButtonPressed();
            if (EditedType.IsValueType) {
                SetValue(ParentGrid.CreateObjectInstance(EditedType));
            } else {
                if (buttonIsCreate) {
                    var objectsToCreate = GetValue().Count();
                    var createdObjects = new object[objectsToCreate];
                    for (var i = 0; i < createdObjects.Length; i++)
                        createdObjects[i] = ParentGrid.CreateObjectInstance(EditedType);
                    SetValues(createdObjects);
                    Expanded = true;
                } else {
                    SetValue(null);
                }
            }

            PerformGetValue();
        }

        protected virtual void BeforeAutoCreateEditors() { }

        protected virtual bool IsAutoCreateMember(MemberInfo info) {
            return memberPredicate(info, ParentGrid.ShowNonPublic);
        }

        protected virtual PropertyEditor AutoCreateMemberEditor(MemberInfo info) {
            return null;
        }

        protected internal override void OnKeyDown(KeyEventArgs e) {
            base.OnKeyDown(e);
            if (!buttonIsCreate && e.KeyCode == Keys.Delete) {
                OnButtonPressed();
                e.Handled = true;
            } else if (buttonIsCreate && e.KeyCode == Keys.Return) {
                OnButtonPressed();
                e.Handled = true;
            }
        }

        protected Func<IEnumerable<object>> CreatePropertyValueGetter(PropertyInfo property) {
            return () => {
                var propertyValues = new List<object>();
                foreach (var obj in GetValue())
                    if (obj == null)
                        propertyValues.Add(null);
                    else
                        try {
                            var value = property.GetValue(obj, null);
                            propertyValues.Add(value);
                        } catch (TargetInvocationException) {
                            propertyValues.Add(null);
                        }
                return propertyValues;
            };
        }

        protected Func<IEnumerable<object>> CreateFieldValueGetter(FieldInfo field) {
            return () => GetValue().Select(o => o != null ? field.GetValue(o) : null);
        }

        protected Action<IEnumerable<object>> CreatePropertyValueSetter(PropertyInfo property) {
            var affectsOthers = ParentGrid.ShowNonPublic || memberAffectsOthers(property);
            return delegate(IEnumerable<object> values) {
                var targetArray = GetValue().ToArray();

                // Set value
                memberPropertySetter(property, targetArray, values);

                // Fixup struct values by assigning the modified struct copy to its original member
                if (EditedType.IsValueType || ForceWriteBack) SetValues(targetArray);

                OnPropertySet(property, targetArray);
                if (affectsOthers)
                    PerformGetValue();
                else
                    OnUpdateFromObjects(GetValue().ToArray());
            };
        }

        protected Action<IEnumerable<object>> CreateFieldValueSetter(FieldInfo field) {
            var affectsOthers = ParentGrid.ShowNonPublic || memberAffectsOthers(field);
            return delegate(IEnumerable<object> values) {
                var targetArray = GetValue().ToArray();

                // Set value
                memberFieldSetter(field, targetArray, values);

                // Fixup struct values by assigning the modified struct copy to its original member
                if (EditedType.IsValueType || ForceWriteBack) SetValues(targetArray);

                OnFieldSet(field, targetArray);
                if (affectsOthers)
                    PerformGetValue();
                else
                    OnUpdateFromObjects(GetValue().ToArray());
            };
        }

        protected virtual void OnPropertySet(PropertyInfo property, IEnumerable<object> targets) { }
        protected virtual void OnFieldSet(FieldInfo property, IEnumerable<object> targets) { }

        protected static bool DefaultMemberPredicate(MemberInfo info, bool showNonPublic) {
            if (showNonPublic)
                return true;
            if (info is PropertyInfo) {
                var property = info as PropertyInfo;
                var getter = property.GetGetMethod(true);
                return getter != null && getter.IsPublic;
            }
            if (info is FieldInfo) {
                var field = info as FieldInfo;
                return field.IsPublic;
            }
            return false;
        }

        protected static bool DefaultMemberAffectsOthers(MemberInfo info) {
            return false;
        }

        protected static void DefaultPropertySetter(PropertyInfo property, IEnumerable<object> targetObjects, IEnumerable<object> values) {
            var valuesEnum = values.GetEnumerator();
            object curValue = null;

            if (valuesEnum.MoveNext()) curValue = valuesEnum.Current;
            foreach (var target in targetObjects) {
                if (target != null)
                    try {
                        property.SetValue(target, curValue, null);
                    } catch (TargetInvocationException) { }
                if (valuesEnum.MoveNext()) curValue = valuesEnum.Current;
            }
        }

        protected static void DefaultFieldSetter(FieldInfo field, IEnumerable<object> targetObjects, IEnumerable<object> values) {
            var valuesEnum = values.GetEnumerator();
            object curValue = null;

            if (valuesEnum.MoveNext()) curValue = valuesEnum.Current;
            foreach (var target in targetObjects) {
                if (target != null) field.SetValue(target, curValue);
                if (valuesEnum.MoveNext()) curValue = valuesEnum.Current;
            }
        }

        private static int GetTypeHierarchyLevel(Type t) {
            var level = 0;
            while (t.BaseType != null) {
                t = t.BaseType;
                level++;
            }
            return level;
        }
    }
}