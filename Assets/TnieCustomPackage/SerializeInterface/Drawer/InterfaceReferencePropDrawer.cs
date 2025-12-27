using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace TnieCustomPackage.SerializeInterface.Drawer
{
    [CustomPropertyDrawer(typeof(InterfaceReferenceProp<>))]
    [CustomPropertyDrawer(typeof(InterfaceReferenceProp<,>))]
    public class InterfaceReferencePropDrawer : PropertyDrawer
    {
        private const string _fieldName = "_underlyingValue";

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var root = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Column,
                    marginBottom = 4
                }
            };

            var prop = property.FindPropertyRelative(_fieldName);
            var args = GetArguments(fieldInfo);

            // --- Header: foldout button + ObjectField + interface hint ---
            var header = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center
                }
            };
            root.Add(header);

            // Foldout button
            var foldoutButton = new Label("▸")
            {
                style =
                {
                    width = 14,
                    unityTextAlign = TextAnchor.MiddleCenter,
                    marginRight = 4,
                    color = new Color(0.8f, 0.8f, 0.8f)
                }
            };
            foldoutButton.AddToClassList("unity-foldout__text");
            header.Add(foldoutButton);

            // ObjectField
            var objectField = new ObjectField(property.displayName)
            {
                allowSceneObjects = true,
                objectType = args.ObjectType ?? typeof(UnityEngine.Object),
                style =
                {
                    flexGrow = 1,
                    marginRight = 4
                }
            };
            objectField.BindProperty(prop);
            header.Add(objectField);

            // Interface type hint
            var typeHint = new Label($"[{args.InterfaceType.Name}]")
            {
                style =
                {
                    unityTextAlign = TextAnchor.MiddleLeft,
                    color = new Color(0.6f, 0.8f, 1f, 0.9f),
                    fontSize = 11,
                    unityFontStyleAndWeight = FontStyle.Italic,
                    marginLeft = 2,
                    minWidth = 70
                }
            };
            header.Add(typeHint);

            // --- Inner container for foldout ---
            var innerContainer = new VisualElement
            {
                style =
                {
                    marginLeft = 18,
                    marginTop = 2,
                    marginBottom = 4
                }
            };
            root.Add(innerContainer);

            bool expanded = false;

            void RefreshInnerInspector()
            {
                innerContainer.Clear();

                if (!expanded || prop.objectReferenceValue == null)
                    return;

                var so = new SerializedObject(prop.objectReferenceValue);
                var iterator = so.GetIterator();
                iterator.NextVisible(true); // skip m_Script

                while (iterator.NextVisible(false))
                {
                    var field = new PropertyField(iterator.Copy());
                    field.Bind(so);
                    innerContainer.Add(field);
                }
            }

            // --- FIX FUNCTION: validates and auto-corrects dragged object ---
            void TryFixReference(SerializedProperty p)
            {
                if (!(p.objectReferenceValue is UnityEngine.Object obj))
                    return;

                // 1️⃣ If GameObject dragged
                if (obj is GameObject go)
                {
                    var component = go.GetComponent(args.InterfaceType);
                    if (component != null)
                    {
                        p.objectReferenceValue = component;
                        p.serializedObject.ApplyModifiedProperties();
                        return;
                    }

                    Debug.LogWarning($"The assigned GameObject does not contain a component implementing interface {args.InterfaceType.Name}.");
                    p.objectReferenceValue = null;
                    p.serializedObject.ApplyModifiedProperties();
                    return;
                }

                // 2️⃣ If Component dragged
                if (obj is Component comp)
                {
                    if (!args.InterfaceType.IsAssignableFrom(comp.GetType()))
                    {
                        Debug.LogWarning($"The assigned Component must implement interface {args.InterfaceType.Name}.");
                        p.objectReferenceValue = null;
                        p.serializedObject.ApplyModifiedProperties();
                        return;
                    }

                    p.objectReferenceValue = comp;
                    p.serializedObject.ApplyModifiedProperties();
                    return;
                }

                // 3️⃣ If ScriptableObject dragged
                if (obj is ScriptableObject so)
                {
                    if (!args.InterfaceType.IsAssignableFrom(so.GetType()))
                    {
                        Debug.LogWarning($"The assigned ScriptableObject must implement interface {args.InterfaceType.Name}.");
                        p.objectReferenceValue = null;
                        p.serializedObject.ApplyModifiedProperties();
                        return;
                    }

                    p.objectReferenceValue = so;
                    p.serializedObject.ApplyModifiedProperties();
                    return;
                }

                // 4️⃣ Invalid type
                Debug.LogWarning($"The assigned object must be a Component, GameObject, or ScriptableObject implementing interface {args.InterfaceType.Name}.");
                p.objectReferenceValue = null;
                p.serializedObject.ApplyModifiedProperties();
            }

            // --- Events ---
            foldoutButton.RegisterCallback<MouseDownEvent>(_ =>
            {
                expanded = !expanded;
                foldoutButton.text = expanded ? "▾" : "▸";
                RefreshInnerInspector();
            });

            objectField.RegisterValueChangedCallback(_ =>
            {
                TryFixReference(prop);
                if (expanded)
                    RefreshInnerInspector();
            });

            return root;
        }

        // ----------- TYPE HELPER METHODS -----------

        private static void GetObjectAndInterfaceType(Type fieldType, out Type objectType, out Type interfaceType)
        {
            if (TryGetTypesFromInterfaceReference(fieldType, out objectType, out interfaceType))
                return;
            TryGetTypesFromList(fieldType, out objectType, out interfaceType);
        }

        private static bool TryGetTypesFromInterfaceReference(Type fieldType, out Type objectType, out Type interfaceType)
        {
            var fieldBaseType = fieldType;
            if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(InterfaceReferenceProp<>))
                fieldBaseType = fieldType.BaseType;

            if (fieldBaseType != null && fieldBaseType.IsGenericType &&
                fieldBaseType.GetGenericTypeDefinition() == typeof(InterfaceReferenceProp<,>))
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
            var listType = fieldType.GetInterfaces().FirstOrDefault(x =>
                x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IList<>));

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

        private readonly struct InterfaceObjectArguments
        {
            public readonly Type ObjectType;
            public readonly Type InterfaceType;

            public InterfaceObjectArguments(Type objectType, Type interfaceType)
            {
                ObjectType = objectType;
                InterfaceType = interfaceType;
            }
        }
    }
}
