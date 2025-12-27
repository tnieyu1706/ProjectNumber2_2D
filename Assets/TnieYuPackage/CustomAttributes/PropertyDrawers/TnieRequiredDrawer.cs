using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace TnieYuPackage.CustomAttributes.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(TnieRequiredAttribute))]
    public class TnieRequiredDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var root = new VisualElement();
            root.style.flexDirection = FlexDirection.Column;

            // Base ObjectField
            var field = new PropertyField(property, property.displayName);

            // HelpBox error
            var help = new HelpBox("Missing Reference!", HelpBoxMessageType.Error);
            help.style.display = DisplayStyle.None;

            root.Add(field);
            root.Add(help);

            // Update khi user thay đổi value
            field.RegisterValueChangeCallback(evt =>
            {
                Validate(property, field, help);
            });

            // Validate lần đầu
            Validate(property, field, help);

            return root;
        }

        private void Validate(SerializedProperty property, VisualElement field, VisualElement help)
        {
            bool missing = property.propertyType == SerializedPropertyType.ObjectReference &&
                           property.objectReferenceValue == null;

            if (missing)
            {
                help.style.display = DisplayStyle.Flex;
            }
            else
            {
                help.style.display = DisplayStyle.None;
            }
        }
    }
}
