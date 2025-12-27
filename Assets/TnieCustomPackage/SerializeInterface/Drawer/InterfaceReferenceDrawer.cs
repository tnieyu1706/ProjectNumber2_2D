using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TnieCustomPackage.SerializeInterface.Support;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace TnieCustomPackage.SerializeInterface.Drawer
{
    [CustomPropertyDrawer(typeof(InterfaceReference<>))]
    [CustomPropertyDrawer(typeof(InterfaceReference<,>))]
    public class InterfaceReferenceDrawer : PropertyDrawer
    {
        private const string _fieldName = "_underlyingValue";

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            // Root container
            var root = new VisualElement();

            // Label (Unity will pass the correct label)
            var objectField = new ObjectField(property.displayName)
            {
                allowSceneObjects = true
            };

            // Get underlying serialized UnityEngine.Object field
            var objectProp = property.FindPropertyRelative(_fieldName);

            // Find interface + object type
            var args = GetArguments(fieldInfo);
            var requiredInterface = args.InterfaceType;
            var objectType = args.ObjectType;

            // ObjectField needs a concrete type
            objectField.objectType = objectType;

            // Set initial value
            objectField.value = objectProp.objectReferenceValue;

            // Filtering: only allow Object that implements TInterface
            objectField.RegisterValueChangedCallback(evt =>
            {
                if (evt.newValue == null)
                {
                    objectProp.objectReferenceValue = null;
                    objectProp.serializedObject.ApplyModifiedProperties();
                    return;
                }

                if (!requiredInterface.IsAssignableFrom(evt.newValue.GetType()))
                {
                    Debug.LogError(
                        $"Assigned object {evt.newValue} does not implement required interface {requiredInterface.Name}");

                    // reset
                    objectField.SetValueWithoutNotify(objectProp.objectReferenceValue);
                    return;
                }

                objectProp.objectReferenceValue = evt.newValue;
                objectProp.serializedObject.ApplyModifiedProperties();
            });

            // Add to UI
            root.Add(objectField);

            return root;
        }

        // ====================== TYPE EXTRACTION ==========================

        private static InterfaceObjectArguments GetArguments(FieldInfo fieldInfo)
        {
            GetObjectAndInterfaceType(fieldInfo.FieldType, out var objectType, out var interfaceType);
            return new InterfaceObjectArguments(objectType, interfaceType);
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

            // Handle InterfaceReference<TInterface> : InterfaceReference<TInterface, Object>
            if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(InterfaceReference<>))
                fieldBaseType = fieldType.BaseType;

            if (fieldBaseType.IsGenericType && fieldBaseType.GetGenericTypeDefinition() == typeof(InterfaceReference<,>))
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
            Type listType = fieldType
                .GetInterfaces()
                .FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IList<>));

            return TryGetTypesFromInterfaceReference(
                listType?.GetGenericArguments()[0],
                out objectType,
                out interfaceType);
        }
    }
}
