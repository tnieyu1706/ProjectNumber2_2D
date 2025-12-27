using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TnieCustomPackage.SerializeInterface.Support;
using UnityEditor;
using UnityEngine;

namespace TnieCustomPackage.SerializeInterface.Drawer
{
    [CustomPropertyDrawer(typeof(InterfaceReferenceGUI<>))]
    [CustomPropertyDrawer(typeof(InterfaceReferenceGUI<,>))]
    public class InterfaceReferenceGUIDrawer : PropertyDrawer
    {
        private const string _fieldName = "_underlyingValue";

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var prop = property.FindPropertyRelative(_fieldName);
            InterfaceReferenceGUIUtility.OnGUI(position, prop, label, GetArguments(fieldInfo));
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var prop = property.FindPropertyRelative(_fieldName);
            return InterfaceReferenceGUIUtility.GetPropertyHeight(prop, label, GetArguments(fieldInfo));
        }

        // ------------------ TYPE EXTRACTION ------------------

        private static InterfaceObjectArguments GetArguments(FieldInfo fieldInfo)
        {
            ExtractInterfaceRefTypes(fieldInfo.FieldType, out var objType, out var iface);
            return new InterfaceObjectArguments(objType, iface);
        }

        private static void ExtractInterfaceRefTypes(Type fieldType, out Type objectType, out Type interfaceType)
        {
            // 1) DIRECT: InterfaceReferenceGUI<T>
            if (fieldType.IsGenericType &&
                fieldType.GetGenericTypeDefinition() == typeof(InterfaceReferenceGUI<>))
            {
                // InterfaceReferenceGUI<T> inherits from InterfaceReferenceGUI<T, Object>
                var baseType = fieldType.BaseType;
                var args = baseType.GetGenericArguments();
                interfaceType = args[0];
                objectType = args[1];
                return;
            }

            // 2) DIRECT: InterfaceReferenceGUI<T, UObject>
            if (fieldType.IsGenericType &&
                fieldType.GetGenericTypeDefinition() == typeof(InterfaceReferenceGUI<,>))
            {
                var args = fieldType.GetGenericArguments();
                interfaceType = args[0];
                objectType = args[1];
                return;
            }

            // 3) LIST<T> or ARRAY[]
            TryExtractListElementType(fieldType, out objectType, out interfaceType);
        }

        private static bool TryExtractListElementType(Type fieldType, out Type objectType, out Type interfaceType)
        {
            objectType = null;
            interfaceType = null;

            // ARRAY [] case
            if (fieldType.IsArray)
            {
                var element = fieldType.GetElementType();
                return ExtractFromGeneric(element, out objectType, out interfaceType);
            }

            // LIST<T> case
            var listType = fieldType
                .GetInterfaces()
                .FirstOrDefault(x =>
                    x.IsGenericType &&
                    x.GetGenericTypeDefinition() == typeof(IList<>));

            if (listType == null) return false;

            return ExtractFromGeneric(listType.GetGenericArguments()[0], out objectType, out interfaceType);
        }

        private static bool ExtractFromGeneric(Type type, out Type objectType, out Type interfaceType)
        {
            objectType = null;
            interfaceType = null;

            if (!type.IsGenericType) return false;

            if (type.GetGenericTypeDefinition() == typeof(InterfaceReferenceGUI<>))
            {
                var baseType = type.BaseType;
                var args = baseType.GetGenericArguments();
                interfaceType = args[0];
                objectType = args[1];
                return true;
            }

            if (type.GetGenericTypeDefinition() == typeof(InterfaceReferenceGUI<,>))
            {
                var args = type.GetGenericArguments();
                interfaceType = args[0];
                objectType = args[1];
                return true;
            }

            return false;
        }
    }
}
