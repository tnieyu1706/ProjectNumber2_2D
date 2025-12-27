namespace FewClicksDev.ColorTextureGenerator
{
    using FewClicksDev.Core;
    using UnityEditor;

    using static FewClicksDev.Core.EditorDrawer;

    public class ColorTextureObjectEditor : CustomInspectorBase
    {
        protected const string EDIT_IN_WINDOW = "Edit in the window";
        protected const string TEXTURE_INFO = "Texture in this scriptable will be assigned in the edit window, so it's easier to change the color ramp and override exported texture.";

        protected SerializedProperty widthProperty = null;
        protected SerializedProperty heightProperty = null;
        protected SerializedProperty pixelsOrientationProperty = null;
        protected SerializedProperty textureToOverrideProperty = null;

        protected override void OnEnable()
        {
            base.OnEnable();

            widthProperty = serializedObject.FindProperty("width");
            heightProperty = serializedObject.FindProperty("height");
            pixelsOrientationProperty = serializedObject.FindProperty("pixelsOrientation");
            textureToOverrideProperty = serializedObject.FindProperty("textureToOverride");
        }

        protected void drawVariables()
        {
            NormalSpace();
            EditorGUILayout.PropertyField(widthProperty);
            EditorGUILayout.PropertyField(heightProperty);
            EditorGUILayout.PropertyField(pixelsOrientationProperty);
            EditorGUILayout.PropertyField(textureToOverrideProperty);

            NormalSpace();
            EditorGUILayout.HelpBox(TEXTURE_INFO, MessageType.Info);
            NormalSpace();


        }
    }
}
