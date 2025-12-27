using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace TnieYuPackage.CustomAttributes.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(TnieLayerMaskDropdownAttribute))]
    public class LayerMaskDropdownDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Get available Layer
            string[] layerNames = InternalEditorUtility.layers;
            int[] layerIndices = new int[layerNames.Length];

            for (int i = 0; i < layerNames.Length; i++)
                layerIndices[i] = LayerMask.NameToLayer(layerNames[i]);

            // Get LayerIndices from available Layer
            int currentMask = property.intValue;
            int displayMask = 0;

            for (int i = 0; i < layerIndices.Length; i++)
            {
                if ((currentMask & (1 << layerIndices[i])) != 0)
                    displayMask |= 1 << i;
            }

            // Dropdown layerMask and get selected available Layer
            int newDisplayMask = EditorGUI.MaskField(position, label, displayMask, layerNames);

            // Checking available Layer <-> LayerIndices and get result.
            int newMask = 0;
            for (int i = 0; i < layerIndices.Length; i++)
            {
                if ((newDisplayMask & (1 << i)) != 0)
                    newMask |= 1 << layerIndices[i];
            }

            property.intValue = newMask;
        }
    }
}