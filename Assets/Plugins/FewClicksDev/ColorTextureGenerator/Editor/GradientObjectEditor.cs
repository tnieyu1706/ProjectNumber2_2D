namespace FewClicksDev.ColorTextureGenerator
{
    using FewClicksDev.Core;
    using UnityEditor;

    using static FewClicksDev.Core.EditorDrawer;

    [CustomEditor(typeof(GradientObject))]
    public class GradientObjectEditor : ColorTextureObjectEditor
    {
        private GradientObject gradient = null;
        private SerializedProperty gradientProperty = null;

        protected override void OnEnable()
        {
            base.OnEnable();

            gradient = target as GradientObject;
            gradientProperty = serializedObject.FindProperty("gradient");
        }

        protected override void drawInspectorGUI()
        {
            drawScript();
            SmallSpace();
            EditorGUILayout.PropertyField(gradientProperty);
            drawVariables();

            using (new ScopeGroup(new HorizontalScope(), ColorScope.Background(ColorTextureGenerator.MAIN_COLOR)))
            {
                FlexibleSpace();

                if (DrawClearBoxButton(EDIT_IN_WINDOW, FixedWidthAndHeight(windowWidthWithPaddings / 2f, DEFAULT_TOOLBAR_HEIGHT)))
                {
                    gradient.OpenInWindow();
                }

                FlexibleSpace();
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
