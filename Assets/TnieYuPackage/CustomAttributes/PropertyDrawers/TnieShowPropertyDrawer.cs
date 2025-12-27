using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace TnieYuPackage.CustomAttributes.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(TnieShowPropertyAttribute))]
    public class TnieShowPropertyDrawer : PropertyDrawer
    {
        private static readonly Dictionary<string, bool> _foldoutStates = new();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var target = property.serializedObject.targetObject;
            var attr = (TnieShowPropertyAttribute)attribute;

            var propInfo = target.GetType().GetProperty(attr.PropertyName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (propInfo == null)
            {
                EditorGUI.LabelField(position, $"Missing property: {attr.PropertyName}");
                return;
            }

            object value = propInfo.GetValue(target);
            if (value == null)
            {
                EditorGUI.LabelField(position, propInfo.Name, "null");
                return;
            }

            string key = $"{target.GetInstanceID()}_{attr.PropertyName}";
            _foldoutStates.TryAdd(key, false);

            EditorGUI.BeginProperty(position, label, property);

            Rect lineRect = new(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

            // Nếu là kiểu đơn giản → field readonly
            if (IsSimple(value.GetType()))
            {
                DrawSimpleField(lineRect, propInfo.Name, value);
                EditorGUI.EndProperty();
                return;
            }

            // Nếu là UnityEngine.Object → ObjectField readonly + foldout serialize
            if (value is UnityEngine.Object unityObj)
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUI.ObjectField(lineRect, propInfo.Name, unityObj, value.GetType(), true);
                EditorGUI.EndDisabledGroup();

                float y = lineRect.y + EditorGUIUtility.singleLineHeight + 2f;
                DrawUnityObjectSerialized(unityObj, key, position, ref y);
                EditorGUI.EndProperty();
                return;
            }

            // Nếu là Serializable class
            _foldoutStates[key] = EditorGUI.Foldout(lineRect, _foldoutStates[key], propInfo.Name, true);
            float currentY = lineRect.y + EditorGUIUtility.singleLineHeight + 2f;

            if (_foldoutStates[key])
            {
                EditorGUI.indentLevel++;
                DrawSerializableFields(value, key, position, ref currentY);
                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }

        private void DrawSerializableFields(object obj, string key, Rect position, ref float y)
        {
            foreach (var field in obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (field.IsNotSerialized) continue;

                object val = field.GetValue(obj);
                Rect rect = new(position.x, y, position.width, EditorGUIUtility.singleLineHeight);

                if (val == null)
                {
                    EditorGUI.LabelField(EditorGUI.IndentedRect(rect), field.Name, "null");
                    y += EditorGUIUtility.singleLineHeight + 2f;
                    continue;
                }

                var type = val.GetType();

                // Simple field
                if (IsSimple(type))
                {
                    DrawSimpleField(EditorGUI.IndentedRect(rect), field.Name, val);
                    y += EditorGUIUtility.singleLineHeight + 2f;
                }
                // UnityEngine.Object
                else if (val is UnityEngine.Object unityObj)
                {
                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUI.ObjectField(EditorGUI.IndentedRect(rect), field.Name, unityObj, type, true);
                    EditorGUI.EndDisabledGroup();
                    y += EditorGUIUtility.singleLineHeight + 2f;

                    DrawUnityObjectSerialized(unityObj, key + "_" + field.Name, position, ref y);
                }
                // Serializable class
                else
                {
                    string subKey = key + "_" + field.Name;
                    _foldoutStates.TryAdd(subKey, false);
                    _foldoutStates[subKey] = EditorGUI.Foldout(EditorGUI.IndentedRect(rect), _foldoutStates[subKey], field.Name, true);
                    y += EditorGUIUtility.singleLineHeight + 2f;

                    if (_foldoutStates[subKey])
                    {
                        EditorGUI.indentLevel++;
                        DrawSerializableFields(val, subKey, position, ref y);
                        EditorGUI.indentLevel--;
                    }
                }
            }
        }

        private void DrawSimpleField(Rect rect, string label, object value)
        {
            EditorGUI.BeginDisabledGroup(true);

            switch (value)
            {
                case int i:
                    EditorGUI.IntField(rect, label, i);
                    break;
                case float f:
                    EditorGUI.FloatField(rect, label, f);
                    break;
                case bool b:
                    EditorGUI.Toggle(rect, label, b);
                    break;
                case string s:
                    EditorGUI.TextField(rect, label, s);
                    break;
                case Vector2 v2:
                    EditorGUI.Vector2Field(rect, label, v2);
                    break;
                case Vector3 v3:
                    EditorGUI.Vector3Field(rect, label, v3);
                    break;
                case Vector4 v4:
                    EditorGUI.Vector4Field(rect, label, v4);
                    break;
                case Color c:
                    EditorGUI.ColorField(rect, label, c);
                    break;
                case Quaternion q:
                    EditorGUI.Vector4Field(rect, label, new Vector4(q.x, q.y, q.z, q.w));
                    break;
                default:
                    EditorGUI.LabelField(rect, label, value.ToString());
                    break;
            }

            EditorGUI.EndDisabledGroup();
        }

        private void DrawUnityObjectSerialized(UnityEngine.Object unityObj, string key, Rect position, ref float y)
        {
            _foldoutStates.TryAdd(key, false);
            Rect foldRect = new(position.x, y, position.width, EditorGUIUtility.singleLineHeight);
            _foldoutStates[key] = EditorGUI.Foldout(EditorGUI.IndentedRect(foldRect), _foldoutStates[key], "Serialized Fields", true);
            y += EditorGUIUtility.singleLineHeight + 2f;

            if (!_foldoutStates[key]) return;

            SerializedObject so = new SerializedObject(unityObj);
            so.Update();

            EditorGUI.indentLevel++;
            var iterator = so.GetIterator();
            bool expanded = true;
            while (iterator.NextVisible(expanded))
            {
                if (iterator.name == "m_Script") continue;

                float height = EditorGUI.GetPropertyHeight(iterator, true);
                var fieldRect = new Rect(position.x, y, position.width, height);

                EditorGUI.BeginDisabledGroup(true);
                EditorGUI.PropertyField(EditorGUI.IndentedRect(fieldRect), iterator, true);
                EditorGUI.EndDisabledGroup();

                y += height + EditorGUIUtility.standardVerticalSpacing;
            }
            EditorGUI.indentLevel--;
            so.ApplyModifiedProperties();
        }

        private bool IsSimple(Type type)
        {
            return type.IsPrimitive || type == typeof(string) ||
                   type == typeof(Vector2) || type == typeof(Vector3) ||
                   type == typeof(Vector4) || type == typeof(Color) ||
                   type == typeof(Quaternion);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var target = property.serializedObject.targetObject;
            var attr = (TnieShowPropertyAttribute)attribute;
            var propInfo = target.GetType().GetProperty(attr.PropertyName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (propInfo == null)
                return EditorGUIUtility.singleLineHeight;

            object value = propInfo.GetValue(target);
            if (value == null || IsSimple(value.GetType()))
                return EditorGUIUtility.singleLineHeight;

            string key = $"{target.GetInstanceID()}_{attr.PropertyName}";
            _foldoutStates.TryAdd(key, false);

            float height = EditorGUIUtility.singleLineHeight + 4f;
            if (value is UnityEngine.Object unityObj)
            {
                height += GetUnityObjectHeight(unityObj, key);
            }
            else if (_foldoutStates[key])
            {
                height += GetSerializableHeight(value, key);
            }

            return height;
        }

        private float GetSerializableHeight(object obj, string key)
        {
            float h = 0f;

            foreach (var field in obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (field.IsNotSerialized) continue;
                h += EditorGUIUtility.singleLineHeight + 2f;

                object val = field.GetValue(obj);
                if (val == null) continue;

                if (val is UnityEngine.Object subUnity)
                    h += GetUnityObjectHeight(subUnity, key + "_" + field.Name);
                else if (!_foldoutStates.ContainsKey(key + "_" + field.Name))
                    continue;
                else if (_foldoutStates[key + "_" + field.Name])
                    h += GetSerializableHeight(val, key + "_" + field.Name);
            }

            return h;
        }

        private float GetUnityObjectHeight(UnityEngine.Object unityObj, string key)
        {
            float h = EditorGUIUtility.singleLineHeight + 2f;
            _foldoutStates.TryAdd(key, false);

            if (!_foldoutStates[key]) return h;

            SerializedObject so = new SerializedObject(unityObj);
            var iterator = so.GetIterator();
            bool expanded = true;
            while (iterator.NextVisible(expanded))
            {
                if (iterator.name == "m_Script") continue;
                h += EditorGUI.GetPropertyHeight(iterator, true) + EditorGUIUtility.standardVerticalSpacing;
            }

            return h;
        }
    }
}
