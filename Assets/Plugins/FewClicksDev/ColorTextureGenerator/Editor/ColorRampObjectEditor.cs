namespace FewClicksDev.ColorTextureGenerator
{
    using FewClicksDev.Core;
    using UnityEditor;
    using UnityEngine;

    using static FewClicksDev.Core.EditorDrawer;

    [CustomEditor(typeof(ColorRampObject))]
    public class ColorRampObjectEditor : ColorTextureObjectEditor
    {
        private const float INDEX_WIDTH = 30f;
        private const float COVERAGE_WIDTH = 50f;

        private ColorRampObject colorRamp = null;
        private SerializedProperty colorRampProperty = null;

        protected override void OnEnable()
        {
            base.OnEnable();

            colorRamp = target as ColorRampObject;
            colorRampProperty = serializedObject.FindProperty("colorRamp").FindPropertyRelative("colors");
        }

        protected override void drawInspectorGUI()
        {
            drawScript();
            SmallSpace();

            using (new DisabledScope())
            {
                for (int i = 0; i < colorRampProperty.arraySize; i++)
                {
                    using (new HorizontalScope(Styles.BoxButton, FixedHeight(DEFAULT_LINE_HEIGHT)))
                    {
                        GUIStyle _label = new GUIStyle(EditorStyles.label);
                        _label.alignment = TextAnchor.MiddleLeft;

                        GUILayout.Label($" {(i + 1).NumberToString(2)}", _label, FixedWidth(INDEX_WIDTH));
                        EditorGUILayout.ColorField(colorRampProperty.GetArrayElementAtIndex(i).FindPropertyRelative("rampColor").colorValue);
                        EditorGUILayout.FloatField(colorRampProperty.GetArrayElementAtIndex(i).FindPropertyRelative("coverage").floatValue, FixedWidth(COVERAGE_WIDTH));

                    }
                }
            }

            drawVariables();

            using (new ScopeGroup(new HorizontalScope(), ColorScope.Background(ColorTextureGenerator.MAIN_COLOR)))
            {
                FlexibleSpace();

                if (DrawClearBoxButton(EDIT_IN_WINDOW, FixedWidthAndHeight(windowWidthWithPaddings / 2f, DEFAULT_TOOLBAR_HEIGHT)))
                {
                    colorRamp.OpenInWindow();
                }

                FlexibleSpace();
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
