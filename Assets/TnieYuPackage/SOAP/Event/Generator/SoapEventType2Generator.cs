#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace TnieYuPackage.SOAP.Event.Generator
{
    public class SoapEventType2Generator : EditorWindow
    {
        private string folderPath = "Assets/";
        private DefaultAsset folderAsset;

        private string customName = "";

        private string customType1 = "";
        private string customType2 = "";

        private string checkResult1 = "";
        private string checkResult2 = "";

        private string resolved1 = "";
        private string resolved2 = "";

        private int preset1 = 0;
        private int preset2 = 0;

        private readonly (string name, string type)[] presets =
        {
            ("Int", "int"),
            ("Float", "float"),
            ("Bool", "bool"),
            ("String", "string"),
            ("Vector2", "UnityEngine.Vector2"),
            ("Vector3", "UnityEngine.Vector3"),
            ("GameObject", "UnityEngine.GameObject"),
            ("Transform", "UnityEngine.Transform"),
        };

        [MenuItem("Tools/TnieYu/SOAP/Event - Dual Event Generator")]
        private static void OpenWindow()
        {
            var window = GetWindow<SoapEventType2Generator>("SOAP Event T1,T2");
            window.minSize = new Vector2(500, 350);
            window.Show();
        }

        private void OnGUI()
        {
            GUILayout.Space(10);
            EditorGUILayout.LabelField("SOAP Event (T1, T2) Generator", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Generate SoapEventSo<T1, T2> ScriptableObject.", MessageType.Info);

            GUILayout.Space(10);

            DrawFolderSelection();
            GUILayout.Space(10);

            DrawPresetTypes();
            GUILayout.Space(15);

            DrawCustomTypes();
            GUILayout.Space(20);

            DrawGenerate();
        }

        // -------------------------
        // Folder
        // -------------------------
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
                    Debug.LogWarning("Not a folder!");
                    folderAsset = null;
                }
            }

            EditorGUILayout.BeginHorizontal();
            folderPath = EditorGUILayout.TextField(folderPath);

            if (GUILayout.Button("...", GUILayout.Width(30)))
            {
                string select = EditorUtility.OpenFolderPanel("Select Output", "Assets", "");
                if (!string.IsNullOrEmpty(select) && select.StartsWith(Application.dataPath))
                {
                    folderPath = "Assets" + select.Substring(Application.dataPath.Length);
                    folderAsset = AssetDatabase.LoadAssetAtPath<DefaultAsset>(folderPath);
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        // -------------------------
        // Preset selectors
        // -------------------------
        private void DrawPresetTypes()
        {
            EditorGUILayout.LabelField("Preset Types:", EditorStyles.boldLabel);

            preset1 = EditorGUILayout.Popup("Type 1", preset1, presets.Select(x => x.name).ToArray());
            preset2 = EditorGUILayout.Popup("Type 2", preset2, presets.Select(x => x.name).ToArray());
        }

        // -------------------------
        // Custom input
        // -------------------------
        private void DrawCustomTypes()
        {
            EditorGUILayout.LabelField("Custom Class Name:", EditorStyles.boldLabel);
            customName = EditorGUILayout.TextField("Name", customName);

            GUILayout.Space(5);

            EditorGUILayout.LabelField("Custom C# Types (Namespace.Type, Assembly)", EditorStyles.boldLabel);

            customType1 = EditorGUILayout.TextField("Type 1", customType1);
            if (GUILayout.Button("Check Type 1")) CheckType(customType1, ref resolved1, ref checkResult1);
            if (!string.IsNullOrEmpty(checkResult1))
                EditorGUILayout.HelpBox(checkResult1,
                    checkResult1.StartsWith("✔") ? MessageType.Info : MessageType.Error);

            GUILayout.Space(5);

            customType2 = EditorGUILayout.TextField("Type 2", customType2);
            if (GUILayout.Button("Check Type 2")) CheckType(customType2, ref resolved2, ref checkResult2);
            if (!string.IsNullOrEmpty(checkResult2))
                EditorGUILayout.HelpBox(checkResult2,
                    checkResult2.StartsWith("✔") ? MessageType.Info : MessageType.Error);
        }

        private void CheckType(string input, ref string resolved, ref string result)
        {
            resolved = "";
            result = "";

            if (string.IsNullOrWhiteSpace(input))
            {
                result = "❌ Enter: Namespace.Type, Assembly";
                return;
            }

            var parts = input.Split(',');
            if (parts.Length != 2)
            {
                result = "❌ Format: Namespace.Class, Assembly";
                return;
            }

            string full = parts[0].Trim();
            string asm = parts[1].Trim();

            var assembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == asm);

            if (assembly == null)
            {
                result = $"❌ Assembly '{asm}' not found.";
                return;
            }

            var type = assembly.GetType(full);
            if (type == null)
            {
                result = $"❌ Type '{full}' not found.";
                return;
            }

            resolved = type.FullName;
            result = $"✔ Found {resolved}";
        }

        // -------------------------
        // Generate Button
        // -------------------------
        private void DrawGenerate()
        {
            if (GUILayout.Button("Generate Event (T1, T2)", GUILayout.Height(32)))
            {
                string name = string.IsNullOrEmpty(customName)
                    ? $"{presets[preset1].name}_{presets[preset2].name}"
                    : customName;

                string type1 = !string.IsNullOrEmpty(resolved1)
                    ? resolved1
                    : (string.IsNullOrEmpty(customType1) ? presets[preset1].type : customType1);

                string type2 = !string.IsNullOrEmpty(resolved2)
                    ? resolved2
                    : (string.IsNullOrEmpty(customType2) ? presets[preset2].type : customType2);

                GenerateScript(name, type1, type2);
            }
        }

        private void GenerateScript(string name, string t1, string t2)
        {
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            string className = $"{name}SoapEvent2So";
            string path = Path.Combine(folderPath, $"{className}.cs");

            if (File.Exists(path))
            {
                if (!EditorUtility.DisplayDialog("Exists", $"{className} already exists.\nOverwrite?", "Yes", "No"))
                    return;
            }

            string code = $@"
using UnityEngine;

namespace TnieYuPackage.SOAP.Event
{{
    [CreateAssetMenu(fileName = ""{name}"", menuName = ""TnieYuPackage/Soap/Event/{name}"")]
    public class {className} : SoapEventSo<{t1}, {t2}>
    {{
    }}
}}";

            File.WriteAllText(path, code);
            AssetDatabase.Refresh();

            Debug.Log($"✅ Generated dual event: {path}");
        }
    }
}
#endif