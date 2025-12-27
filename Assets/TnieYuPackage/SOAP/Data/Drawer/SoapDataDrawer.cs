#if UNITY_EDITOR
using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace TnieYuPackage.SOAP.Data.Drawer
{
    [CustomPropertyDrawer(typeof(SoapData<>))]
    [CustomPropertyDrawer(typeof(SoapAbstractData<>))]
    public class SoapDataDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var root = new VisualElement();

            // Lấy SerializedProperty "value"
            var valueProp = property.FindPropertyRelative("value");

            // Vẽ field bằng UIToolkit
            var field = new PropertyField(valueProp, property.displayName);
            root.Add(field);

            // Khi user thay đổi UI Toolkit field
            field.RegisterValueChangeCallback(evt =>
            {
                property.serializedObject.ApplyModifiedProperties();

                // Lấy parent object thật (ScriptableObject hoặc Component chứa struct SoapData)
                var parent = GetParentObject(property);
                if (parent == null)
                    return;

                // Lấy boxed SoapData<T> (struct)
                var fieldInfo = parent.GetType().GetField(property.name,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (fieldInfo == null)
                    return;

                object boxedStruct = fieldInfo.GetValue(parent);

                // Lấy field "value" thật bên trong SoapData<T>
                var innerValueField = boxedStruct.GetType()
                    .GetField("value", BindingFlags.Instance | BindingFlags.NonPublic);

                var runtimeValue = innerValueField.GetValue(boxedStruct);

                // Lấy OnValueChange
                var onValueProp = boxedStruct.GetType().GetProperty("OnValueChange");
                var onValueDelegate = onValueProp.GetValue(boxedStruct) as Delegate;

                if (onValueDelegate != null)
                {
                    // Invoke event đúng T
                    onValueDelegate.DynamicInvoke(runtimeValue);
                }
            });

            return root;
        }

        // Helper: Lấy parent object chứa property
        private object GetParentObject(SerializedProperty prop)
        {
            object obj = prop.serializedObject.targetObject;
            string[] elements = prop.propertyPath.Split('.');

            for (int i = 0; i < elements.Length - 1; i++)
                obj = GetFieldOrProperty(obj, elements[i]);

            return obj;
        }

        private object GetFieldOrProperty(object source, string name)
        {
            if (source == null)
                return null;

            var type = source.GetType();

            var field = type.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (field != null)
                return field.GetValue(source);

            var prop = type.GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (prop != null)
                return prop.GetValue(source);

            return null;
        }
    }
}
#endif