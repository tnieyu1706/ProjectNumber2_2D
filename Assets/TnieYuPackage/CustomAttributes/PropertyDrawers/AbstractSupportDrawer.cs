using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace TnieYuPackage.CustomAttributes.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(AbstractSupportAttribute))]
    public class AbstractSupportDrawer : PropertyDrawer
    {
        private Type[] _implementations;

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var attr = (AbstractSupportAttribute)attribute;
            var abstractType = attr.AbstractType ?? fieldInfo.FieldType;

            var root = new VisualElement();
            root.style.flexDirection = FlexDirection.Column;
            root.style.marginBottom = 2;

            if (_implementations == null)
                CacheImplementations(abstractType);

            var header = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center
                }
            };

            var label = new Label(property.displayName)
            {
                style =
                {
                    unityFontStyleAndWeight = FontStyle.Bold,
                    flexGrow = 1
                }
            };
            header.Add(label);

            var fieldContainer = new VisualElement();
            root.Add(header);
            root.Add(fieldContainer);

            void BuildPropertyField()
            {
                fieldContainer.Clear();

                if (property.managedReferenceValue != null)
                {
                    var propertyField = new PropertyField(property, "");
                    propertyField.Bind(property.serializedObject);
                    fieldContainer.Add(propertyField);
                }
            }

            void RefreshUI()
            {
                header.Clear();
                header.Add(label);

                if (property.managedReferenceValue == null)
                {
                    var selectButton = new Button(() =>
                    {
                        var menu = new GenericMenu();
                        foreach (var type in _implementations)
                        {
                            menu.AddItem(new GUIContent(type.Name), false, () =>
                            {
                                var instance = Activator.CreateInstance(type);
                                property.serializedObject.Update();
                                property.managedReferenceValue = instance;
                                property.serializedObject.ApplyModifiedProperties();

                                // Rebuild toàn bộ UI
                                RefreshUI();
                                EditorApplication.delayCall +=
                                    UnityEditorInternal.InternalEditorUtility.RepaintAllViews;
                            });
                        }

                        menu.ShowAsContext();
                    })
                    {
                        text = "+",
                        tooltip = "Select type"
                    };
                    selectButton.style.width = 24;
                    selectButton.style.marginLeft = 2;

                    header.Add(selectButton);
                }
                else
                {
                    var typeLabel = new Label($"({property.managedReferenceFullTypename.Split(' ').Last()})")
                    {
                        style =
                        {
                            color = new Color(0.6f, 0.8f, 0.9f),
                            unityFontStyleAndWeight = FontStyle.Italic,
                            marginLeft = 4
                        }
                    };
                    header.Add(typeLabel);

                    var resetButton = new Button(() =>
                    {
                        property.serializedObject.Update();
                        property.managedReferenceValue = null;
                        property.serializedObject.ApplyModifiedProperties();

                        RefreshUI();
                        EditorApplication.delayCall += UnityEditorInternal.InternalEditorUtility.RepaintAllViews;
                    })
                    {
                        text = "X",
                        tooltip = "Reset value"
                    };
                    resetButton.style.width = 24;
                    resetButton.style.marginLeft = 2;
                    header.Add(resetButton);
                }

                // Cập nhật lại property hiển thị phía dưới
                BuildPropertyField();
            }

            RefreshUI();
            return root;
        }

        private void CacheImplementations(Type abstractType)
        {
            _implementations = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a =>
                {
                    Type[] types;
                    try
                    {
                        types = a.GetTypes();
                    }
                    catch (ReflectionTypeLoadException e)
                    {
                        types = e.Types.Where(t => t != null).ToArray();
                    }

                    return types;
                })
                .Where(t =>
                    abstractType.IsAssignableFrom(t)
                    && !t.IsAbstract
                    && !t.IsInterface
                    && t.GetCustomAttribute<SerializableAttribute>() != null
                )
                .OrderBy(t => t.Name)
                .ToArray();
        }
    }
}