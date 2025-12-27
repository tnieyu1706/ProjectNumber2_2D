using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TnieCustomPackage.SerializeInterface.Support;
using UnityEditor;
using UnityEngine;

namespace TnieCustomPackage.SerializeInterface.Drawer
{
    [CustomPropertyDrawer(typeof(InterfaceReferenceGUIProp<>))]
    [CustomPropertyDrawer(typeof(InterfaceReferenceGUIProp<,>))]
    public class InterfaceReferenceGUIPropDrawer : PropertyDrawer
    {
        private const string _fieldName = "_underlyingValue";

        // Cache trạng thái foldout (mở/đóng)
        private static readonly Dictionary<string, bool> _foldoutStates = new();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var prop = property.FindPropertyRelative(_fieldName);
            var args = GetArguments(fieldInfo);
            string key = property.propertyPath;

            // --- Foldout Header ---
            Rect foldoutRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            bool foldout = GetFoldout(key);
            foldout = EditorGUI.Foldout(foldoutRect, foldout, label, true);
            _foldoutStates[key] = foldout;

            // --- Object Field ---
            Rect objectRect = new Rect(
                position.x + EditorGUI.indentLevel * 15f,
                foldoutRect.y + EditorGUIUtility.singleLineHeight + 2f,
                position.width - EditorGUI.indentLevel * 15f,
                EditorGUIUtility.singleLineHeight
            );

            prop.objectReferenceValue = InterfaceReferenceGUIUtility.OnGUI(objectRect, prop, GUIContent.none, args);

            // --- Draw inner inspector if foldout is open ---
            if (foldout && prop.objectReferenceValue != null)
            {
                EditorGUI.indentLevel++;
                float innerStartY = objectRect.y + EditorGUIUtility.singleLineHeight + 4f;

                DrawInnerObject(prop.objectReferenceValue, position.x, innerStartY, position.width);
                EditorGUI.indentLevel--;
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var prop = property.FindPropertyRelative(_fieldName);
            var args = GetArguments(fieldInfo);
            string key = property.propertyPath;

            // Chiều cao mặc định = foldout + object field
            float height = EditorGUIUtility.singleLineHeight * 2 + 4f;

            // Nếu foldout mở => cộng thêm phần bên trong object
            if (prop.objectReferenceValue != null && GetFoldout(key))
            {
                var so = new SerializedObject(prop.objectReferenceValue);
                var iterator = so.GetIterator();
                iterator.NextVisible(true);

                while (iterator.NextVisible(false))
                    height += EditorGUI.GetPropertyHeight(iterator, true) + EditorGUIUtility.standardVerticalSpacing;

                height += 4f;
            }

            return height;
        }

        private void DrawInnerObject(UnityEngine.Object target, float x, float startY, float width)
        {
            SerializedObject so = new SerializedObject(target);
            so.Update();

            float y = startY;
            var iterator = so.GetIterator();
            iterator.NextVisible(true); // Bỏ "m_Script"

            while (iterator.NextVisible(false))
            {
                float h = EditorGUI.GetPropertyHeight(iterator, true);
                Rect r = new Rect(x + 15f, y, width - 15f, h);
                EditorGUI.PropertyField(r, iterator, true);
                y += h + EditorGUIUtility.standardVerticalSpacing;
            }

            so.ApplyModifiedProperties();
        }

        private bool GetFoldout(string key)
        {
            if (!_foldoutStates.TryGetValue(key, out bool val))
                val = false;
            return val;
        }

        private static void GetObjectAndInterfaceType(Type fieldType, out Type objectType, out Type interfaceType)
        {
            if (TryGetTypesFromInterfaceReference(fieldType, out objectType, out interfaceType))
                return;
            TryGetTypesFromList(fieldType, out objectType, out interfaceType);
        }

        private static bool TryGetTypesFromInterfaceReference(Type fieldType, out Type objectType, out Type interfaceType)
        {
            var fieldBaseType = fieldType;
            if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(InterfaceReferenceGUIProp<>))
                fieldBaseType = fieldType.BaseType;

            if (fieldBaseType.IsGenericType && fieldBaseType.GetGenericTypeDefinition() == typeof(InterfaceReferenceGUIProp<,>))
            {
                var types = fieldBaseType.GetGenericArguments();
                interfaceType = types[0];
                objectType = types[1];
                return true;
            }

            objectType = null;
            interfaceType = null;
            return false;
        }

        private static bool TryGetTypesFromList(Type fieldType, out Type objectType, out Type interfaceType)
        {
            Type listType = fieldType.GetInterfaces().FirstOrDefault(x =>
                x.IsGenericType &&
                x.GetGenericTypeDefinition() == typeof(IList<>));

            if (listType != null)
                return TryGetTypesFromInterfaceReference(listType.GetGenericArguments()[0], out objectType, out interfaceType);

            objectType = null;
            interfaceType = null;
            return false;
        }

        private static InterfaceObjectArguments GetArguments(FieldInfo fieldInfo)
        {
            GetObjectAndInterfaceType(fieldInfo.FieldType, out var objectType, out var interfaceType);
            return new InterfaceObjectArguments(objectType, interfaceType);
        }
    }
}
