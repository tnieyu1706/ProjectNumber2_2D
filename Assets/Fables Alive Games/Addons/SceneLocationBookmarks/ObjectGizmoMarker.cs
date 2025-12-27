using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using System.Collections.Generic;
#endif

namespace FablesAliveGames
{
    [ExecuteInEditMode] // Bu sayede Update() Editor'da da çalışır
    public class ObjectGizmoMarker : MonoBehaviour
    {
        [Header("Visibility Settings")]
        [SerializeField] private bool showInEditor = true;
        [SerializeField] private bool showInGameView = false;
        [SerializeField] private bool stayOnTop = true; // YENİ: Stay On Top özelliği
        
        [Header("Gizmo Settings")]
        [SerializeField] private Color gizmoColor = Color.yellow;
        [SerializeField] private float gizmoRadius = 1.0f;
        [SerializeField] private bool showGizmo = true;
        [SerializeField] private bool showObjectName = true;
        
        [Header("Label Settings")]
        [SerializeField] private Vector3 labelOffset = Vector3.up;
        
        // Runtime mesh and material for Game View rendering
        private Mesh discMesh;
        private Material runtimeMaterial;
        private Material runtimeMaterialStayOnTop; // YENİ: Stay on top için ayrı material
        
        // GUI variables for runtime label rendering
        private Vector3 screenPosition;
        private bool shouldDrawRuntimeLabel = false;

#if UNITY_EDITOR
        // Static system to handle all gizmo markers (like the original bookmark system)
        private static List<ObjectGizmoMarker> allMarkers = new List<ObjectGizmoMarker>();
        private static bool isSceneGUIRegistered = false;

        // Register this marker when enabled
        private void OnEnable()
        {
            RegisterMarker();
            
            // Create runtime resources if needed (for Game View)
            if (showInGameView)
            {
                CreateRuntimeResources();
            }
        }

        private void OnDisable()
        {
            UnregisterMarker();
        }

        private void OnDestroy()
        {
            UnregisterMarker();
            CleanupRuntimeResources();
        }

        private void RegisterMarker()
        {
            if (!allMarkers.Contains(this))
            {
                allMarkers.Add(this);
            }

            // Register scene GUI callback when first marker is added
            if (!isSceneGUIRegistered && allMarkers.Count > 0)
            {
                SceneView.duringSceneGui += OnSceneGUIGlobal;
                isSceneGUIRegistered = true;
            }
        }

        private void UnregisterMarker()
        {
            allMarkers.Remove(this);

            // Unregister scene GUI callback when no markers left
            if (isSceneGUIRegistered && allMarkers.Count == 0)
            {
                SceneView.duringSceneGui -= OnSceneGUIGlobal;
                isSceneGUIRegistered = false;
            }
        }

        // Static method that draws all markers - just like the original bookmark system
        private static void OnSceneGUIGlobal(SceneView sceneView)
        {
            // Clean up null references first
            for (int i = allMarkers.Count - 1; i >= 0; i--)
            {
                if (allMarkers[i] == null)
                {
                    allMarkers.RemoveAt(i);
                }
            }

            // Draw all valid markers
            foreach (var marker in allMarkers)
            {
                if (marker != null && marker.ShouldDrawGizmo())
                {
                    marker.DrawSceneGizmo();
                }
            }
        }

        // Check if this marker should draw its gizmo
        private bool ShouldDrawGizmo()
        {
            if (!showInEditor) return false;
            
            // Always show when selected OR when showGizmo is true
            bool isSelected = Selection.gameObjects != null && System.Array.IndexOf(Selection.gameObjects, gameObject) >= 0;
            return showGizmo || isSelected;
        }

        // Draw this marker's gizmo
        private void DrawSceneGizmo()
        {
            // Get current camera direction for proper disc orientation
            SceneView sceneView = SceneView.lastActiveSceneView;
            if (sceneView == null) return;

            // Get camera forward direction (towards the camera, not away from it)
            Vector3 cameraForward = sceneView.camera.transform.forward;
            Vector3 discNormal = -cameraForward; // Point towards camera

            // Apply color with transparency
            Color finalColor = GetFinalGizmoColor();
            
            // Save original gizmo color and matrix
            Color originalHandlesColor = Handles.color;
            Matrix4x4 originalMatrix = Handles.matrix;
            
            // YENİ: Stay on top için z-test ayarları
            if (stayOnTop)
            {
                // Z-test'i devre dışı bırak (her zaman önde görün)
                Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;
            }
            else
            {
                // Normal depth testing (arkadaki objeler tarafından gizlenebilir)
                Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;
            }
            
            // Set gizmo color and draw solid disc
            Handles.color = finalColor;
            
            // Draw solid disc facing the camera
            Handles.DrawSolidDisc(transform.position, discNormal, gizmoRadius);
            
            // Draw object name if enabled
            if (showObjectName)
            {
                Vector3 labelPosition = transform.position + (labelOffset * gizmoRadius);
                
                // Use same color as gizmo but fully opaque for readability
                Color labelColor = gizmoColor;
                labelColor.a = 1f;
                
                // Store original GUI color
                Color originalGUIColor = GUI.color;
                GUI.color = labelColor;
                
                Handles.Label(labelPosition, gameObject.name);
                
                // Restore original GUI color
                GUI.color = originalGUIColor;
            }
            
            // Restore original colors and settings
            Handles.color = originalHandlesColor;
            Handles.matrix = originalMatrix;
            Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual; // Varsayılan değere geri döndür
        }

        // Static method to initialize the system on editor load
        [InitializeOnLoadMethod]
        private static void InitializeOnLoad()
        {
            // Make sure we clean up on editor reload
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode || state == PlayModeStateChange.ExitingPlayMode)
            {
                // Clean up the system
                if (isSceneGUIRegistered)
                {
                    SceneView.duringSceneGui -= OnSceneGUIGlobal;
                    isSceneGUIRegistered = false;
                }
                allMarkers.Clear();
            }
        }

        // Property accessors for easy runtime access
        public bool ShowInEditor
        {
            get { return showInEditor; }
            set 
            { 
                showInEditor = value;
                SceneView.RepaintAll();
            }
        }

        public bool ShowInGameView
        {
            get { return showInGameView; }
            set 
            { 
                showInGameView = value;
                if (value)
                    CreateRuntimeResources();
                else
                    CleanupRuntimeResources();
            }
        }

        // YENİ: Stay On Top property
        public bool StayOnTop
        {
            get { return stayOnTop; }
            set 
            { 
                stayOnTop = value;
                SceneView.RepaintAll();
            }
        }
#endif

        private void Start()
        {
            // Runtime resources are already created in OnEnable if needed
            // This ensures they're available both in Editor and Play mode
            if (showInGameView)
            {
                CreateRuntimeResources();
            }
        }

        private void CreateRuntimeResources()
        {
            // Create disc mesh
            CreateDiscMesh();
            
            // YENİ: İki farklı material oluştur - normal ve stay on top için
            // Normal material - depth testing ile
            runtimeMaterial = new Material(Shader.Find("Sprites/Default"));
            runtimeMaterial.color = GetFinalGizmoColor();
            
            // Stay on top material - depth testing olmadan
            runtimeMaterialStayOnTop = new Material(Shader.Find("Sprites/Default"));
            runtimeMaterialStayOnTop.color = GetFinalGizmoColor();
            runtimeMaterialStayOnTop.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Always);
            runtimeMaterialStayOnTop.SetInt("_ZWrite", 0); // Z-buffer'a yazma
        }

        private void CleanupRuntimeResources()
        {
            if (discMesh != null)
            {
                DestroyImmediate(discMesh);
                discMesh = null;
            }
            
            if (runtimeMaterial != null)
            {
                DestroyImmediate(runtimeMaterial);
                runtimeMaterial = null;
            }
            
            // YENİ: Stay on top material'ı da temizle
            if (runtimeMaterialStayOnTop != null)
            {
                DestroyImmediate(runtimeMaterialStayOnTop);
                runtimeMaterialStayOnTop = null;
            }
        }

        private void CreateDiscMesh()
        {
            discMesh = new Mesh();
            discMesh.name = "GizmoDisc";
            
            int segments = 32;
            Vector3[] vertices = new Vector3[segments + 1];
            int[] triangles = new int[segments * 3];
            Vector2[] uvs = new Vector2[segments + 1];
            
            // Center vertex
            vertices[0] = Vector3.zero;
            uvs[0] = new Vector2(0.5f, 0.5f);
            
            // Edge vertices - create disc in XY plane (Z=0)
            // This will be rotated to face camera in RenderInGameView
            for (int i = 0; i < segments; i++)
            {
                float angle = (float)i / segments * Mathf.PI * 2;
                vertices[i + 1] = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0); // XY plane
                uvs[i + 1] = new Vector2(Mathf.Cos(angle) * 0.5f + 0.5f, Mathf.Sin(angle) * 0.5f + 0.5f);
            }
            
            // Triangles - make sure they face the right direction
            for (int i = 0; i < segments; i++)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = (i + 1) % segments + 1;
            }
            
            discMesh.vertices = vertices;
            discMesh.triangles = triangles;
            discMesh.uv = uvs;
            discMesh.RecalculateNormals();
            discMesh.RecalculateBounds();
        }

        private void Update()
        {
            // Render in Game View both in Editor and Play mode
            if (showInGameView && showGizmo)
            {
                RenderInGameView();
            }
        }

        private void RenderInGameView()
        {
            if (discMesh == null || (runtimeMaterial == null && runtimeMaterialStayOnTop == null)) return;
            
            // YENİ: Hangi material kullanılacağını belirle
            Material materialToUse = stayOnTop ? runtimeMaterialStayOnTop : runtimeMaterial;
            if (materialToUse == null) return;
            
            // Update material color
            Color finalColor = GetFinalGizmoColor();
            materialToUse.color = finalColor;
            
            // Get current camera for billboard effect
            Camera currentCamera = Camera.current ?? Camera.main;
            if (currentCamera == null) return;
            
            // Calculate billboard rotation exactly like Scene View
            // Make the disc face towards the camera (perpendicular to camera forward)
            Vector3 cameraForward = currentCamera.transform.forward;
            Vector3 cameraUp = currentCamera.transform.up;
            
            // Create rotation that makes the disc face the camera
            // The disc normal should point towards the camera
            Vector3 discNormal = -cameraForward; // Point towards camera
            
            // Create rotation matrix that aligns the disc with camera plane
            Quaternion billboardRotation = Quaternion.LookRotation(discNormal, cameraUp);
            
            // Create transformation matrix
            Matrix4x4 matrix = Matrix4x4.TRS(
                transform.position,
                billboardRotation,
                Vector3.one * gizmoRadius
            );
            
            // Render the disc with appropriate material
            Graphics.DrawMesh(discMesh, matrix, materialToUse, gameObject.layer);
            
            // Calculate screen position for runtime label
            if (showObjectName)
            {
                Vector3 labelWorldPosition = transform.position + (labelOffset * gizmoRadius);
                screenPosition = currentCamera.WorldToScreenPoint(labelWorldPosition);
                // Check if the label is in front of the camera and within screen bounds
                shouldDrawRuntimeLabel = screenPosition.z > 0 && 
                                       screenPosition.x >= 0 && screenPosition.x <= Screen.width &&
                                       screenPosition.y >= 0 && screenPosition.y <= Screen.height;
            }
            else
            {
                shouldDrawRuntimeLabel = false;
            }
        }

        // OnGUI is called for rendering and handling GUI events
        private void OnGUI()
        {
            // Only draw runtime label if we're in game view and should show it
            if (showInGameView && showGizmo && shouldDrawRuntimeLabel && showObjectName)
            {
                // Use same color as gizmo but fully opaque for readability
                Color labelColor = gizmoColor;
                labelColor.a = 1f;
                
                // Create GUI style with the label color
                GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
                labelStyle.normal.textColor = labelColor;
                labelStyle.fontSize = 12;
                labelStyle.fontStyle = FontStyle.Bold;
                
                // Convert screen position (Y is flipped in GUI coordinates)
                Vector2 guiPosition = new Vector2(screenPosition.x, Screen.height - screenPosition.y);
                
                // Calculate label size and center it
                GUIContent labelContent = new GUIContent(gameObject.name);
                Vector2 labelSize = labelStyle.CalcSize(labelContent);
                
                // Draw the label centered on the position
                GUI.Label(new Rect(guiPosition.x - labelSize.x * 0.5f, 
                                 guiPosition.y - labelSize.y * 0.5f, 
                                 labelSize.x, labelSize.y), 
                         labelContent, labelStyle);
            }
        }

        private Color GetFinalGizmoColor()
        {
            // Use the alpha channel from gizmoColor directly
            return gizmoColor;
        }

        public Color GizmoColor
        {
            get { return gizmoColor; }
            set { gizmoColor = value; }
        }

        public float GizmoRadius
        {
            get { return gizmoRadius; }
            set { gizmoRadius = Mathf.Max(0.1f, value); }
        }

        public float GizmoTransparency
        {
            get { return 1f - gizmoColor.a; } // Convert alpha to transparency
            set { 
                Color newColor = gizmoColor;
                newColor.a = 1f - Mathf.Clamp01(value);
                gizmoColor = newColor;
            }
        }

        public bool ShowGizmo
        {
            get { return showGizmo; }
            set { showGizmo = value; }
        }

        public bool ShowObjectName
        {
            get { return showObjectName; }
            set { showObjectName = value; }
        }

        public Vector3 LabelOffset
        {
            get { return labelOffset; }
            set { labelOffset = value; }
        }

        // Utility methods
        public void SetRandomPastelColor()
        {
            float r = 0.5f + (Random.value * 0.5f);
            float g = 0.5f + (Random.value * 0.5f);
            float b = 0.5f + (Random.value * 0.5f);
            gizmoColor = new Color(r, g, b, gizmoColor.a); // Preserve existing alpha/transparency
        }

        public void ToggleGizmo()
        {
            showGizmo = !showGizmo;
        }

        public void SetGizmoSettings(Color color, float radius, float transparency)
        {
            gizmoColor = color;
            gizmoColor.a = 1f - Mathf.Clamp01(transparency); // Set alpha from transparency
            gizmoRadius = Mathf.Max(0.1f, radius);
            
            // Update runtime materials if they exist
            if (runtimeMaterial != null)
            {
                runtimeMaterial.color = GetFinalGizmoColor();
            }
            if (runtimeMaterialStayOnTop != null)
            {
                runtimeMaterialStayOnTop.color = GetFinalGizmoColor();
            }
        }

        public void SetVisibility(bool inEditor, bool inGameView)
        {
#if UNITY_EDITOR
            showInEditor = inEditor;
#endif
            showInGameView = inGameView;
        }

        // YENİ: Stay on top ayarlama metodu
        public void SetStayOnTop(bool stayOnTop)
        {
            this.stayOnTop = stayOnTop;
#if UNITY_EDITOR
            SceneView.RepaintAll();
#endif
        }
    }

#if UNITY_EDITOR
    // Custom Inspector for better usability with multi-object editing support
    [CustomEditor(typeof(ObjectGizmoMarker)), CanEditMultipleObjects]
    public class ObjectGizmoMarkerEditor : Editor
    {
        private SerializedProperty showInEditor;
        private SerializedProperty showInGameView;
        private SerializedProperty stayOnTop; // YENİ
        private SerializedProperty gizmoColor;
        private SerializedProperty gizmoRadius;
        private SerializedProperty showGizmo;
        private SerializedProperty showObjectName;
        private SerializedProperty labelOffset;

        private void OnEnable()
        {
            // Cache all serialized properties
            showInEditor = serializedObject.FindProperty("showInEditor");
            showInGameView = serializedObject.FindProperty("showInGameView");
            stayOnTop = serializedObject.FindProperty("stayOnTop"); // YENİ
            gizmoColor = serializedObject.FindProperty("gizmoColor");
            gizmoRadius = serializedObject.FindProperty("gizmoRadius");
            showGizmo = serializedObject.FindProperty("showGizmo");
            showObjectName = serializedObject.FindProperty("showObjectName");
            labelOffset = serializedObject.FindProperty("labelOffset");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("Object Gizmo Marker", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // Visibility settings
            EditorGUILayout.LabelField("Visibility Settings", EditorStyles.boldLabel);
            
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(showInEditor, new GUIContent("Show in Editor", "Show gizmo in Scene View (Editor only)"));
            EditorGUILayout.PropertyField(showInGameView, new GUIContent("Show in Game View", "Show gizmo in Game View (Runtime)"));
            
            // YENİ: Stay On Top seçeneği
            EditorGUILayout.PropertyField(stayOnTop, new GUIContent("Stay On Top", "Gizmo her zaman önde görünsün (mesh'lerin arkasında gizlenmesin)"));
            
            if (EditorGUI.EndChangeCheck())
            {
                SceneView.RepaintAll();
            }

            EditorGUILayout.Space();

            // Gizmo visibility toggle
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(showGizmo, new GUIContent("Show Gizmo", "Master toggle for gizmo visibility"));
            if (EditorGUI.EndChangeCheck())
            {
                SceneView.RepaintAll();
            }

            EditorGUI.BeginDisabledGroup(!showGizmo.boolValue);

            // Color picker
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(gizmoColor, new GUIContent("Gizmo Color", "Color of the gizmo disc and label"));
            if (EditorGUI.EndChangeCheck())
            {
                SceneView.RepaintAll();
            }

            // Radius slider
            EditorGUI.BeginChangeCheck();
            gizmoRadius.floatValue = EditorGUILayout.Slider(
                new GUIContent("Gizmo Radius", "Size of the gizmo disc"), 
                gizmoRadius.floatValue, 0.1f, 10f
            );
            if (EditorGUI.EndChangeCheck())
            {
                SceneView.RepaintAll();
            }

            EditorGUILayout.Space();

            // Object name display toggle
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(showObjectName, new GUIContent("Show Object Name", "Display the GameObject's name above the gizmo"));
            if (EditorGUI.EndChangeCheck())
            {
                SceneView.RepaintAll();
            }

            // Label offset - only show if showObjectName is true
            if (showObjectName.boolValue)
            {
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(labelOffset, new GUIContent("Label Offset", "Offset of the name label relative to gizmo radius"));
                if (EditorGUI.EndChangeCheck())
                {
                    SceneView.RepaintAll();
                }
            }

            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space();

            // Utility buttons - only work when single object is selected
            if (!serializedObject.isEditingMultipleObjects)
            {
                EditorGUILayout.LabelField("Utilities", EditorStyles.boldLabel);
                
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Random Pastel Color"))
                {
                    ObjectGizmoMarker marker = target as ObjectGizmoMarker;
                    Undo.RecordObject(marker, "Set Random Color");
                    marker.SetRandomPastelColor();
                    SceneView.RepaintAll();
                    serializedObject.Update();
                }
                
                if (GUILayout.Button("Reset to Default"))
                {
                    Undo.RecordObjects(targets, "Reset to Default");
                    foreach (ObjectGizmoMarker marker in targets)
                    {
                        marker.GizmoColor = Color.yellow;
                        marker.GizmoRadius = 1.0f;
                        marker.ShowGizmo = true;
                        marker.ShowObjectName = true;
                        marker.LabelOffset = Vector3.up;
                        marker.SetStayOnTop(true); // YENİ: Varsayılan olarak stay on top açık
                    }
                    SceneView.RepaintAll();
                    serializedObject.Update();
                }
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.HelpBox($"Multi-object editing: {targets.Length} objects selected. Utility buttons are disabled for multi-selection.", MessageType.Info);
            }

            // Apply changes
            serializedObject.ApplyModifiedProperties();

            // YENİ: Stay on top hakkında bilgi
            if (stayOnTop.boolValue)
            {
                EditorGUILayout.HelpBox("Stay On Top aktif: Gizmo her zaman mesh'lerin önünde görünecek.", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox("Stay On Top kapalı: Gizmo mesh'lerin arkasında gizlenebilir.", MessageType.Info);
            }

            // Show info about the system
            EditorGUILayout.HelpBox("This component uses a global SceneView.duringSceneGui system that works regardless of Unity's gizmo settings. Labels use the same color as gizmo but are fully opaque for readability.", MessageType.Info);
        }
    }
#endif
}