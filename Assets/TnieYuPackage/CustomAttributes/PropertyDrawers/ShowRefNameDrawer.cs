using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace TnieYuPackage.CustomAttributes.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(ShowRefNameAttribute))]
    public class ShowRefNameDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            // Root container
            var root = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center
                }
            };

            // Left: Unity default SerializeReference UI
            var field = new PropertyField(property, property.displayName)
            {
                style = { flexGrow = 1 }
            };
            root.Add(field);

            // Right: class name (ToString)
            var label = new Label
            {
                style =
                {
                    unityFontStyleAndWeight = FontStyle.Bold,
                    marginLeft = 8,
                    width = 120,
                    unityTextAlign = TextAnchor.MiddleRight
                }
            };
            root.Add(label);

            // Update name
            void Refresh()
            {
                object obj = property.managedReferenceValue;

                if (obj == null)
                {
                    label.text = "(null)";
                }
                else
                {
                    label.text = obj.ToString();
                }
            }

            // Initial update
            Refresh();

            // Update when changed
            field.RegisterValueChangeCallback(evt => Refresh());

            return root;
        }
    }
}