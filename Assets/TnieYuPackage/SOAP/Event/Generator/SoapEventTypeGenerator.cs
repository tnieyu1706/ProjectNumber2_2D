#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace TnieYuPackage.SOAP.Event.Generator
{
    public class SoapEventTypeGenerator : EditorWindow
    {
        private string folderPath = "Assets/";
        private DefaultAsset folderAsset;

        private string customTypeName = "";  // Generated class name (Int, Float...)
        private string customType = "";      // Namespace.Class, Assembly
        private string checkResult = "";
        private string resolvedFullName = "";

        private int selectedPresetIndex = 0;

        private readonly (string name, string type)[] presets =
        {
            ("Int", "int"),
            ("Float", "float"),
            ("Bool", "bool"),
            ("String", "string"),
            ("Vector2", "UnityEngine.Vector2"),
            ("Vector3", "UnityEngine.Vector3"),
            ("GameObject", "UnityEngine.GameObject"),
            ("Transform", "UnityEngine.Transform")
        };

        [MenuItem("Tools/TnieYu/SOAP/Event - Generic Event Generator")]
        private static void OpenWindow()
        {
            var window = GetWindow<SoapEventTypeGenerator>("SOAP Event Type Generator");
            window.minSize = new Vector2(480, 300);
            window.Show();
        }

        private void OnGUI()
        {
            GUILayout.Space(10);
            EditorGUILayout.LabelField("SOAP Event Type Generator", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Generate SoapEvent<T> + SoapEventSo<TData, T> types automatically.", MessageType.Info);

            GUILayout.Space(10);

            DrawFolderSelection();
            GUILayout.Space(10);

            DrawPresetType();
            GUILayout.Space(10);

            DrawCustomTypeInput();
            GUILayout.Space(15);

            DrawGenerateButton();
        }

        // ------------------------------------------------------------------
        // FOLDER SELECTION
        // ------------------------------------------------------------------
        private void DrawFolderSelection()
        {
            EditorGUILayout.LabelField("Output Folder:", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            folderAsset = (DefaultAsset)EditorGUILayout.ObjectField("Folder", folderAsset, typeof(DefaultAsset), false);
            if (EditorGUI.EndChangeCheck() && folderAsset != null)
            {
                string path = AssetDatabase.GetAssetPath(folderAsset);
                if (AssetDatabase.IsValidFolder(path))
                    folderPath = path;
                else
                {
                    Debug.LogWarning("Selected object is not a folder!");
                    folderAsset = null;
                }
            }

            EditorGUILayout.BeginHorizontal();
            folderPath = EditorGUILayout.TextField(folderPath);

            if (GUILayout.Button("...", GUILayout.Width(30)))
            {
                string selected = EditorUtility.OpenFolderPanel("Select Output Folder", "Assets", "");
                if (!string.IsNullOrEmpty(selected) && selected.StartsWith(Application.dataPath))
                {
                    folderPath = "Assets" + selected.Substring(Application.dataPath.Length);
                    folderAsset = AssetDatabase.LoadAssetAtPath<DefaultAsset>(folderPath);
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        // ------------------------------------------------------------------
        // PRESET SELECTION
        // ------------------------------------------------------------------
        private void DrawPresetType()
        {
            EditorGUILayout.LabelField("Preset Type:", EditorStyles.boldLabel);
            selectedPresetIndex = EditorGUILayout.Popup(selectedPresetIndex, presets.Select(p => p.name).ToArray());
        }

        // ------------------------------------------------------------------
        // CUSTOM TYPE INPUT
        // ------------------------------------------------------------------
        private void DrawCustomTypeInput()
        {
            EditorGUILayout.LabelField("Custom Type Name (Generated Class Name):", EditorStyles.boldLabel);
            customTypeName = EditorGUILayout.TextField("Type Name", customTypeName);

            GUILayout.Space(5);

            EditorGUILayout.LabelField("Custom C# Type (Namespace.Class, Assembly):", EditorStyles.boldLabel);
            customType = EditorGUILayout.TextField("Type, Assembly", customType);

            if (GUILayout.Button("Check Type"))
                CheckCustomType();

            if (!string.IsNullOrEmpty(checkResult))
            {
                EditorGUILayout.HelpBox(
                    checkResult,
                    checkResult.StartsWith("✔") ? MessageType.Info : MessageType.Error
                );
            }
        }

        // ------------------------------------------------------------------
        // CHECK TYPE VIA REFLECTION
        // ------------------------------------------------------------------
        private void CheckCustomType()
        {
            checkResult = "";
            resolvedFullName = "";

            if (string.IsNullOrWhiteSpace(customType))
            {
                checkResult = "❌ Please enter: Namespace.Class, Assembly";
                return;
            }

            string[] parts = customType.Split(',');
            if (parts.Length != 2)
            {
                checkResult = "❌ Format must be: Namespace.Class, AssemblyName";
                return;
            }

            string typeName = parts[0].Trim();
            string asmName = parts[1].Trim();

            var asm = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == asmName);

            if (asm == null)
            {
                checkResult = $"❌ Assembly '{asmName}' not found.";
                return;
            }

            var type = asm.GetType(typeName);

            if (type == null)
            {
                checkResult = $"❌ Type '{typeName}' not found in '{asmName}'.";
                return;
            }

            resolvedFullName = type.FullName;
            checkResult = $"✔ Found Type: {resolvedFullName}";
        }

        // ------------------------------------------------------------------
        // GENERATE BUTTON
        // ------------------------------------------------------------------
        private void DrawGenerateButton()
        {
            if (GUILayout.Button("Generate Event Type", GUILayout.Height(30)))
            {
                string presetName = presets[selectedPresetIndex].name;
                string presetType = presets[selectedPresetIndex].type;

                string typeName = string.IsNullOrEmpty(customTypeName) ? presetName : customTypeName;

                string typeCs = !string.IsNullOrEmpty(resolvedFullName)
                    ? resolvedFullName
                    : (string.IsNullOrEmpty(customType) ? presetType : customType);

                GenerateEventFile(typeName, typeCs);
            }
        }

        // ------------------------------------------------------------------
        // GENERATE FILE
        // ------------------------------------------------------------------
        private void GenerateEventFile(string name, string type)
        {
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            string fileName = $"{name}SoapEventSo";
            string filePath = Path.Combine(folderPath, $"{fileName}.cs");

            if (File.Exists(filePath))
            {
                if (!EditorUtility.DisplayDialog("File Exists", $"{fileName} already exists.\nOverwrite?", "Yes", "No"))
                    return;
            }

            // Generate the event script
            string code = $@"
using UnityEngine;

namespace TnieYuPackage.SOAP.Event
{{
    [CreateAssetMenu(fileName = ""{name}"", menuName = ""TnieYuPackage/Soap/Event/{name}"")]
    public class {fileName} : SoapEventSo<{type}>
    {{
    }}
}}";

            File.WriteAllText(filePath, code);
            AssetDatabase.Refresh();

            Debug.Log($"✅ Generated SOAP Event Type: {filePath}");
        }
    }
}
#endif
