#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace TnieYuPackage.SOAP.Data.Generator
{
    public class SoapDataTypeGenerator : EditorWindow
    {
        private string folderPath = "Assets/";
        private DefaultAsset folderAsset;

        private string customTypeName = "";  // user-defined type name (class name)
        private string customType = "";      // input: "Namespace.Class, Assembly"
        private string checkResult = "";     // check reflection result
        private string resolvedFullName = ""; // resolved Namespace.Class

        private int selectedPresetIndex = 0;

        private readonly (string name, string type)[] presets =
        {
            ("Int", "int"),
            ("Float", "float"),
            ("Bool", "bool"),
            ("String", "string"),
            ("Vector3", "UnityEngine.Vector3"),
            ("GameObject", "UnityEngine.GameObject")
        };

        [MenuItem("Tools/TnieYu/SOAP/Data - Base Type Generator")]
        private static void OpenWindow()
        {
            var window = GetWindow<SoapDataTypeGenerator>("SOAP Data Type Generator");
            window.minSize = new Vector2(480, 300);
            window.Show();
        }

        private void OnGUI()
        {
            GUILayout.Space(10);
            EditorGUILayout.LabelField("SOAP Type Generator", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Generate new SoapDataSo<T> types automatically.", MessageType.Info);

            GUILayout.Space(10);

            // ===========================
            //       FOLDER SECTION
            // ===========================
            EditorGUILayout.LabelField("Output Folder:", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            folderAsset = (DefaultAsset)EditorGUILayout.ObjectField("Folder", folderAsset, typeof(DefaultAsset), false);
            if (EditorGUI.EndChangeCheck() && folderAsset != null)
            {
                string assetPath = AssetDatabase.GetAssetPath(folderAsset);

                if (AssetDatabase.IsValidFolder(assetPath))
                {
                    folderPath = assetPath;
                }
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
                if (!string.IsNullOrEmpty(selected))
                {
                    if (selected.StartsWith(Application.dataPath))
                    {
                        folderPath = "Assets" + selected.Substring(Application.dataPath.Length);
                        folderAsset = AssetDatabase.LoadAssetAtPath<DefaultAsset>(folderPath);
                    }
                    else
                        Debug.LogWarning("Selected folder must be inside Assets/");
                }
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);

            // ===========================
            //      PRESET TYPE
            // ===========================
            EditorGUILayout.LabelField("Preset Type:", EditorStyles.boldLabel);
            selectedPresetIndex = EditorGUILayout.Popup(selectedPresetIndex, GetPresetNames());
            var (presetName, presetType) = presets[selectedPresetIndex];

            GUILayout.Space(10);

            // ===========================
            //      CUSTOM TYPE NAME
            // ===========================
            EditorGUILayout.LabelField("Custom Type Name (generated class name):", EditorStyles.boldLabel);
            customTypeName = EditorGUILayout.TextField("Type Name", customTypeName);

            GUILayout.Space(8);

            // ===========================
            //      CUSTOM C# TYPE
            // ===========================
            EditorGUILayout.LabelField("Custom C# Type (Namespace.Class, Assembly):", EditorStyles.boldLabel);
            customType = EditorGUILayout.TextField("Type, Assembly", customType);

            if (GUILayout.Button("Check Type"))
            {
                CheckCustomType();
            }

            // Validation output
            if (!string.IsNullOrEmpty(checkResult))
            {
                EditorGUILayout.HelpBox(
                    checkResult,
                    checkResult.StartsWith("✔") ? MessageType.Info : MessageType.Error
                );
            }

            GUILayout.Space(15);

            // ===========================
            //          GENERATE
            // ===========================
            if (GUILayout.Button("Generate Type", GUILayout.Height(30)))
            {
                if (string.IsNullOrEmpty(folderPath))
                {
                    EditorUtility.DisplayDialog("Error", "Please select output folder.", "OK");
                    return;
                }

                string typeName = string.IsNullOrEmpty(customTypeName) ? presetName : customTypeName;

                string typeCs = !string.IsNullOrEmpty(resolvedFullName)
                    ? resolvedFullName
                    : (string.IsNullOrEmpty(customType) ? presetType : customType);

                GenerateTypeFile(typeName, typeCs);
            }
        }

        // ===============================================================
        //                      CHECK CUSTOM TYPE
        // ===============================================================
        private void CheckCustomType()
        {
            checkResult = "";
            resolvedFullName = "";

            if (string.IsNullOrWhiteSpace(customType))
            {
                checkResult = "❌ Please enter: Namespace.ClassName, AssemblyName";
                return;
            }

            string[] parts = customType.Split(',');
            if (parts.Length != 2)
            {
                checkResult = "❌ Invalid format. Correct: Namespace.Class, Assembly";
                return;
            }

            string typeName = parts[0].Trim();
            string assemblyName = parts[1].Trim();

            var asm = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == assemblyName);

            if (asm == null)
            {
                checkResult = $"❌ Assembly '{assemblyName}' not found.";
                return;
            }

            var type = asm.GetType(typeName);

            if (type == null)
            {
                checkResult = $"❌ Type '{typeName}' not found in assembly '{assemblyName}'.";
                return;
            }

            resolvedFullName = type.FullName;
            checkResult = $"✔ Found Type: {resolvedFullName}";
        }

        // ===============================================================
        //                      GENERATE FILE
        // ===============================================================
        private void GenerateTypeFile(string name, string type)
        {
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            string fileName = $"{name}SoapDataSo";
            string filePath = Path.Combine(folderPath, $"{fileName}.cs");

            if (File.Exists(filePath))
            {
                if (!EditorUtility.DisplayDialog("File Exists",
                        $"File '{filePath}' already exists.\nOverwrite?", "Yes", "No"))
                    return;
            }

            string code = $@"
using UnityEngine;

namespace TnieYuPackage.SOAP.Data
{{
    [CreateAssetMenu(fileName = ""{name}"", menuName = ""TnieYuPackage/Soap/BaseData/{name}"")]
    public class {fileName} : SoapDataSo<SoapData<{type}>, {type}>
    {{
        
    }}
}}";

            File.WriteAllText(filePath, code);
            AssetDatabase.Refresh();

            Debug.Log($"✅ Generated: {filePath}");
        }

        private string[] GetPresetNames()
        {
            return presets.Select(p => p.name).ToArray();
        }
    }
}
#endif
