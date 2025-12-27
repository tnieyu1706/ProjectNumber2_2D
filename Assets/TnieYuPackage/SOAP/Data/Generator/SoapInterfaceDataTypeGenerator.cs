#if UNITY_EDITOR
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace TnieYuPackage.SOAP.Data.Generator
{
    public class SoapAbstractDataTypeGenerator : EditorWindow
    {
        private string folderPath = "Assets/";
        private DefaultAsset folderAsset;

        private string abstractTypeName = ""; // class name muốn đặt (MyItem, MyEnemyData…)
        private string csharpType = ""; // dạng: Namespace.Abstract, Assembly
        private string checkResult = "";
        private string resolvedFullName = "";

        [MenuItem("Tools/TnieYu/SOAP/Data - Abstract Type Generator")]
        private static void OpenWindow()
        {
            var window = GetWindow<SoapAbstractDataTypeGenerator>("SOAP Abstract Type Generator");
            window.minSize = new Vector2(480, 250);
            window.Show();
        }

        private void OnGUI()
        {
            GUILayout.Space(10);
            EditorGUILayout.LabelField("SOAP Abstract Type Generator", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Generate new SoapDataAbstractSo<T> types automatically.",
                MessageType.Info);

            GUILayout.Space(10);

            DrawFolderSection();
            GUILayout.Space(10);

            DrawAbstractInput();
            GUILayout.Space(15);

            DrawGenerateButton();
        }

        // ================================================
        // FOLDER PICKER
        // ================================================
        private void DrawFolderSection()
        {
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
                        Debug.LogWarning("Must be inside Assets/");
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        // ================================================
        // ABSTRACT SETTINGS UI
        // ================================================
        private void DrawAbstractInput()
        {
            EditorGUILayout.LabelField("Abstract Type Settings:", EditorStyles.boldLabel);

            abstractTypeName = EditorGUILayout.TextField("Generated Type Name", abstractTypeName);

            GUILayout.Space(5);
            csharpType = EditorGUILayout.TextField("C# Type (Namespace.Type, Assembly)", csharpType);

            if (GUILayout.Button("Check Type"))
                CheckCSharpType();

            if (!string.IsNullOrEmpty(checkResult))
            {
                EditorGUILayout.HelpBox(
                    checkResult,
                    checkResult.StartsWith("✔") ? MessageType.Info : MessageType.Error
                );
            }
        }

        // ================================================
        // CHECK TYPE
        // ================================================
        private void CheckCSharpType()
        {
            checkResult = "";
            resolvedFullName = "";

            if (string.IsNullOrWhiteSpace(csharpType))
            {
                checkResult = "❌ Please enter: Namespace.Class, Assembly";
                return;
            }

            string[] parts = csharpType.Split(',');
            if (parts.Length != 2)
            {
                checkResult = "❌ Invalid format. Correct: Namespace.Class, AssemblyName";
                return;
            }

            string typeName = parts[0].Trim();
            string asmName = parts[1].Trim();

            var asm = System.AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == asmName);

            if (asm == null)
            {
                checkResult = $"❌ Assembly '{asmName}' not found.";
                return;
            }

            var type = asm.GetType(typeName);
            if (type == null)
            {
                checkResult = $"❌ Type '{typeName}' not found in assembly '{asmName}'.";
                return;
            }

            resolvedFullName = type.FullName;
            checkResult = $"✔ Found Type: {resolvedFullName}";
        }

        // ================================================
        // GENERATE BUTTON
        // ================================================
        private void DrawGenerateButton()
        {
            GUI.enabled = !string.IsNullOrEmpty(abstractTypeName)
                          && (!string.IsNullOrEmpty(resolvedFullName) || !string.IsNullOrEmpty(csharpType));

            if (GUILayout.Button("Generate Abstract Data", GUILayout.Height(30)))
            {
                string typeCs = string.IsNullOrEmpty(resolvedFullName) ? csharpType : resolvedFullName;
                GenerateTypeFile(abstractTypeName, typeCs);
            }

            GUI.enabled = true;
        }

        // ================================================
        // GENERATE FILE
        // ================================================
        private void GenerateTypeFile(string name, string type)
        {
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            string fileName = $"{name}SoapDataSo";
            string filePath = Path.Combine(folderPath, $"{fileName}.cs");

            if (File.Exists(filePath))
            {
                if (!EditorUtility.DisplayDialog(
                        "File Exists",
                        $"File '{fileName}' already exists.\nOverwrite?",
                        "Yes", "No"))
                    return;
            }

            string code = $@"
using UnityEngine;

namespace TnieYuPackage.SOAP.Data
{{
    [CreateAssetMenu(fileName = ""{name}"", menuName = ""TnieYuPackage/Soap/AbstractData/{name}"")]
    public class {fileName} : SoapAbstractDataSo<SoapAbstractData<{type}>, {type}>
    {{

    }}
}}";

            File.WriteAllText(filePath, code);
            AssetDatabase.Refresh();

            Debug.Log($"✅ Generated Abstract Data: {filePath}");
        }
    }
}
#endif