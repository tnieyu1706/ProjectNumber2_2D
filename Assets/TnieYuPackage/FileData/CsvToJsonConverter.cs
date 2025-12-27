using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using CsvHelper;
using UnityEditor;
using UnityEngine;

namespace TnieYuPackage.FileData
{
    public class CsvToJsonEditorWindow : EditorWindow
    {
        private string csvPath = "";
        private string jsonPath = "";
        private Mode mode = Mode.Generic;
        private string dtoTypeName = "Full.Namespace.DialogueNodeDto, AssemblyName"; // guide user
        private Vector2 scroll;
        private string previewJson = "";
        private string lastLog = "";

        private enum Mode
        {
            Generic,
            DTO
        }

        [MenuItem("Tools/TnieYu/Csv → Json Converter")]
        public static void OpenWindow()
        {
            var w = GetWindow<CsvToJsonEditorWindow>("CSV → JSON");
            w.minSize = new Vector2(600, 400);
        }

        private void OnGUI()
        {
            GUILayout.Label("CSV → JSON Converter", EditorStyles.boldLabel);

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("CSV file", GUILayout.Width(70));
            csvPath = EditorGUILayout.TextField(csvPath);
            if (GUILayout.Button("Browse", GUILayout.Width(80)))
            {
                string p = EditorUtility.OpenFilePanel("Select CSV file", Application.dataPath, "csv");
                if (!string.IsNullOrEmpty(p)) csvPath = p;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Output JSON", GUILayout.Width(70));
            jsonPath = EditorGUILayout.TextField(jsonPath);
            if (GUILayout.Button("Save As", GUILayout.Width(80)))
            {
                string suggested = Path.Combine(Application.dataPath, Path.GetFileNameWithoutExtension(csvPath) + ".json");
                string p = EditorUtility.SaveFilePanel("Save JSON file", Application.dataPath, Path.GetFileNameWithoutExtension(csvPath), "json");
                if (!string.IsNullOrEmpty(p)) jsonPath = p;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            mode = (Mode)EditorGUILayout.EnumPopup("Mode", mode);

            if (mode == Mode.DTO)
            {
                EditorGUILayout.HelpBox("Enter the full type name (including namespace). If the type is in a separate assembly, append ', AssemblyName'. Example:\n_Project.Features.DialogueSheet.Models.DialogueNodeDto, Assembly-CSharp", MessageType.Info);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("DTO Type", GUILayout.Width(70));
                dtoTypeName = EditorGUILayout.TextField(dtoTypeName);
                EditorGUILayout.EndHorizontal();

                if (GUILayout.Button("Try Resolve Type"))
                {
                    Type t = ResolveType(dtoTypeName);
                    lastLog = t != null ? $"Type resolved: {t.FullName}" : "Type not found. Ensure correct namespace and assembly.";
                }
            }

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Convert", GUILayout.Height(32)))
            {
                ConvertAction();
            }
            if (GUILayout.Button("Preview Only", GUILayout.Height(32)))
            {
                PreviewAction();
            }
            if (GUILayout.Button("Clear Preview", GUILayout.Height(32)))
            {
                previewJson = "";
                lastLog = "Preview cleared.";
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Preview JSON", EditorStyles.boldLabel);
            scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.Height(240));
            EditorGUILayout.TextArea(previewJson, GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Log", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(lastLog, MessageType.None);
        }

        private void ConvertAction()
        {
            try
            {
                if (!File.Exists(csvPath))
                {
                    lastLog = "CSV file not found.";
                    return;
                }
                if (string.IsNullOrEmpty(jsonPath))
                {
                    lastLog = "Please choose an output path.";
                    return;
                }

                string json = mode == Mode.Generic ? ConvertGenericCsvToJson(csvPath) : ConvertCsvToJsonByDto(csvPath, dtoTypeName);

                // write file
                File.WriteAllText(jsonPath, json, Encoding.UTF8);
                AssetDatabase.Refresh();
                previewJson = json;
                lastLog = $"Converted and saved to {jsonPath}";
            }
            catch (Exception ex)
            {
                lastLog = "Error: " + ex.Message + "\n" + ex.StackTrace;
                Debug.LogException(ex);
            }
        }

        private void PreviewAction()
        {
            try
            {
                if (!File.Exists(csvPath))
                {
                    lastLog = "CSV file not found.";
                    return;
                }
                previewJson = mode == Mode.Generic ? ConvertGenericCsvToJson(csvPath) : ConvertCsvToJsonByDto(csvPath, dtoTypeName);
                lastLog = "Preview generated (not saved).";
            }
            catch (Exception ex)
            {
                lastLog = "Error: " + ex.Message;
                Debug.LogException(ex);
            }
        }

        // Generic builder: read headers + rows, build JSON string manually.
        private string ConvertGenericCsvToJson(string csvFile)
        {
            using var reader = new StreamReader(csvFile, Encoding.UTF8);
            using var csv = new CsvReader(reader, CsvSerializer.GetCsvConfiguration());

            csv.Read();
            csv.ReadHeader();
            var headers = csv.HeaderRecord;

            var rows = new List<Dictionary<string, string>>();
            while (csv.Read())
            {
                var row = new Dictionary<string, string>();
                foreach (var h in headers)
                {
                    string key = ToCamelCase(h);
                    string raw = csv.GetField(h);
                    row[key] = raw;
                }
                rows.Add(row);
            }

            // Build JSON
            var sb = new StringBuilder();
            sb.Append("{\"items\":[");
            for (int i = 0; i < rows.Count; i++)
            {
                if (i > 0) sb.Append(",");
                sb.Append("{");
                var keys = rows[i].Keys.ToList();
                for (int k = 0; k < keys.Count; k++)
                {
                    string key = keys[k];
                    string value = rows[i][key];
                    sb.Append($"\"{EscapeString(key)}\":");
                    sb.Append(SerializeValueGuess(value));
                    if (k < keys.Count - 1) sb.Append(",");
                }
                sb.Append("}");
            }
            sb.Append("]}");

            // Pretty print for preview / file: try parse to JSON and pretty print if possible (use simple indent)
            return PrettyJson(sb.ToString());
        }

        // DTO mode: use CsvService.ReadListData<T> via reflection, then JsonUtility.ToJson for each item.
        private string ConvertCsvToJsonByDto(string csvFile, string typeName)
        {
            Type dtoType = ResolveType(typeName);
            if (dtoType == null) throw new Exception("DTO type not found. Make sure you provide 'Namespace.TypeName, AssemblyName' if needed.");

            // call CsvService.ReadListData<dtoType>(csvFile)
            var csvService = new CsvSerializer();
            MethodInfo readMethod = typeof(CsvSerializer).GetMethod("ReadListData").MakeGenericMethod(dtoType);
            var readResult = readMethod.Invoke(csvService, new object[] { csvFile }); // IEnumerable<T>

            // iterate result (IEnumerable)
            var enumerable = readResult as IEnumerable;
            if (enumerable == null) throw new Exception("ReadListData returned null or not enumerable.");

            var itemsJson = new List<string>();
            foreach (var item in enumerable)
            {
                // use JsonUtility to serialize each item (JsonUtility respects fields & [SerializeField])
                string j = JsonUtility.ToJson(item, false);
                itemsJson.Add(j);
            }

            // combine to wrapper
            var sb = new StringBuilder();
            sb.Append("{\"items\":[");
            for (int i = 0; i < itemsJson.Count; i++)
            {
                if (i > 0) sb.Append(",");
                sb.Append(itemsJson[i]);
            }
            sb.Append("]}");

            return PrettyJson(sb.ToString());
        }

        // Helper: try to guess type of value and serialize appropriately:
        // - null or empty -> null or ""
        // - explicit "null" (case-insensitive) -> null
        // - integer -> raw number
        // - float -> raw number
        // - boolean -> true/false
        // - otherwise -> quoted escaped string
        private string SerializeValueGuess(string raw)
        {
            if (raw == null) return "null";
            string t = raw.Trim();

            if (string.Equals(t, "null", StringComparison.OrdinalIgnoreCase))
                return "null";

            if (int.TryParse(t, out int vi))
                return vi.ToString();

            if (long.TryParse(t, out long vl))
                return vl.ToString();

            if (double.TryParse(t, NumberStyles.Float, CultureInfo.InvariantCulture, out double vd))
                return vd.ToString(CultureInfo.InvariantCulture);

            if (bool.TryParse(t, out bool vb))
                return vb ? "true" : "false";

            // else string
            return $"\"{EscapeString(t)}\"";
        }

        private string EscapeString(string s)
        {
            if (s == null) return "";
            return s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r").Replace("\t", "\\t");
        }

        private string ToCamelCase(string header)
        {
            if (string.IsNullOrEmpty(header)) return header;
            header = header.Trim();
            if (header.Length == 1) return header.ToLowerInvariant();
            return char.ToLowerInvariant(header[0]) + header.Substring(1);
        }

        // Try pretty print (simple): indent braces/brackets
        private string PrettyJson(string compactJson)
        {
            try
            {
                var sb = new StringBuilder();
                int indent = 0;
                bool inQuotes = false;
                for (int i = 0; i < compactJson.Length; i++)
                {
                    char ch = compactJson[i];
                    if (ch == '"' && (i == 0 || compactJson[i - 1] != '\\'))
                    {
                        inQuotes = !inQuotes;
                        sb.Append(ch);
                    }
                    else if (!inQuotes)
                    {
                        if (ch == '{' || ch == '[')
                        {
                            sb.Append(ch);
                            sb.AppendLine();
                            indent++;
                            sb.Append(new string(' ', indent * 2));
                        }
                        else if (ch == '}' || ch == ']')
                        {
                            sb.AppendLine();
                            indent--;
                            sb.Append(new string(' ', indent * 2));
                            sb.Append(ch);
                        }
                        else if (ch == ',')
                        {
                            sb.Append(ch);
                            sb.AppendLine();
                            sb.Append(new string(' ', indent * 2));
                        }
                        else if (ch == ':')
                        {
                            sb.Append(ch);
                            sb.Append(" ");
                        }
                        else if (!char.IsWhiteSpace(ch))
                        {
                            sb.Append(ch);
                        }
                    }
                    else
                    {
                        sb.Append(ch);
                    }
                }
                return sb.ToString();
            }
            catch
            {
                return compactJson;
            }
        }

        private Type ResolveType(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName)) return null;
            // try Type.GetType first (supports assembly-qualified)
            var t = Type.GetType(fullName);
            if (t != null) return t;

            // search loaded assemblies
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    t = asm.GetType(fullName);
                    if (t != null) return t;
                }
                catch { }
            }

            // also try without assembly (just namespace + type)
            if (!fullName.Contains(","))
            {
                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    try
                    {
                        var candidates = asm.GetTypes().Where(x => x.Name == fullName || x.FullName == fullName).ToArray();
                        if (candidates.Length > 0) return candidates[0];
                    }
                    catch { }
                }
            }

            return null;
        }
    }
}
