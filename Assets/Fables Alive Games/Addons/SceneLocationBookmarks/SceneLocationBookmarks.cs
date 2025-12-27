#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.IO;
using UnityEditorInternal;

namespace FablesAliveGames
{
    public class SceneLocationBookmarks : EditorWindow
    {
        private BookmarkCollection bookmarkCollection = new BookmarkCollection();
        private string bookmarkName = "New Bookmark";
        private bool isBookmarkNameManuallySet = false;
        private Vector2 scrollPosition;
        private BookmarkCollectionData bookmarkDataComponent;
        private GameObject bookmarkParent;
        private string currentSceneName;

        // ReorderableList'ler
        private ReorderableList objectBookmarksList;
        private ReorderableList viewBookmarksList;
        private List<BookmarkData> objectBookmarks = new List<BookmarkData>();
        private List<BookmarkData> viewBookmarks = new List<BookmarkData>();

        //[MenuItem("Tools/Fables Alive Games/Objects & Scene Bookmarks %&#b")]
        [MenuItem("Tools/Fables Alive Games/Objects and Scene Bookmarks %&#b")]
        public static void ShowWindow()
        {
            GetWindow<SceneLocationBookmarks>("Objects & Scene Bookmarks");
        }

        // Handle scene closing to ensure proper cleanup
        [InitializeOnLoadMethod]
        private static void RegisterSceneClosingCallback()
        {
            EditorSceneManager.sceneClosing += (scene, removingScene) =>
            {
                // Find all bookmark objects and remove special flags before scene closes
                string parentName = "Scene Location Bookmarks - " + scene.name;
                GameObject parent = GameObject.Find(parentName);

                if (parent != null)
                {
                    // Remove the HideFlags so Unity can properly clean up the object
                    parent.hideFlags = HideFlags.None;

                    // Also reset hideFlags for any children
                    foreach (Transform child in parent.transform)
                    {
                        child.gameObject.hideFlags = HideFlags.None;
                    }
                }
            };
        }

        private GameObject GetBookmarkParent()
        {
            string parentName = "Scene Location Bookmarks - " + currentSceneName;
            
            Debug.Log($"Looking for bookmark parent: {parentName}");
            
            // Önce mevcut parent'ı kontrol et
            if (bookmarkParent != null && bookmarkParent.name == parentName)
            {
                Debug.Log("Using existing bookmark parent");
                return bookmarkParent;
            }

            // Scene'de ara
            GameObject existingParent = GameObject.Find(parentName);
            if (existingParent != null)
            {
                Debug.Log($"Found existing parent: {existingParent.name}");
                bookmarkParent = existingParent;
                return bookmarkParent;
            }

            // Yoksa yeni oluştur
            Debug.Log($"Creating new parent: {parentName}");
            bookmarkParent = new GameObject(parentName);
            
            // SADECE gizle, silme
            bookmarkParent.hideFlags = HideFlags.HideInHierarchy;
            
            // Scene'i dirty yap
            EditorUtility.SetDirty(bookmarkParent);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            
            return bookmarkParent;
        }

        private void OnEnable()
        {
            Debug.Log("SceneLocationBookmarks window OnEnable called");
            
            // Register for scene change events
            EditorSceneManager.sceneOpened += OnSceneOpened;

            // Initial load for the current scene
            LoadBookmarksForCurrentScene();

            // Register for scene view drawing
            SceneView.duringSceneGui += OnSceneGUI;

            // Register for editor updates to track bookmark object movements
            Undo.postprocessModifications += OnTransformChanged;

            // Update the name field based on current selection
            UpdateBookmarkNameFromSelection();
            
            Debug.Log($"OnEnable completed - Bookmark count: {bookmarkCollection?.bookmarks?.Count ?? 0}");
        }

        private void OnDisable()
        {
            // Unregister when window is closed
            SceneView.duringSceneGui -= OnSceneGUI;
            Undo.postprocessModifications -= OnTransformChanged;
            EditorSceneManager.sceneOpened -= OnSceneOpened;
        }

        // Make sure to clean up objects when the window is closed
        private void OnDestroy()
        {
            // Unregister events
            SceneView.duringSceneGui -= OnSceneGUI;
            Undo.postprocessModifications -= OnTransformChanged;
            EditorSceneManager.sceneOpened -= OnSceneOpened;

            // Don't destroy parent objects as they're saved with the scene now
            // Simply let Unity handle the cleanup when the scene is closed
        }

        private void OnSceneOpened(Scene scene, OpenSceneMode mode)
        {
            Debug.Log($"Scene opened: {scene.name}");
            // Load bookmarks for the newly opened scene
            LoadBookmarksForCurrentScene();

            // Refresh the window
            Repaint();
        }

        private void LoadBookmarksForCurrentScene()
        {
            // Get current scene name
            Scene currentScene = EditorSceneManager.GetActiveScene();
            string newSceneName = string.IsNullOrEmpty(currentScene.name) ? "UnsavedScene" : currentScene.name;

            Debug.Log($"LoadBookmarks - Current: '{currentSceneName}' -> New: '{newSceneName}'");

            // SADECE scene gerçekten değiştiyse reset et
            bool sceneChanged = (currentSceneName != newSceneName);
            
            if (sceneChanged || bookmarkParent == null)
            {
                currentSceneName = newSceneName;
                bookmarkParent = null;
                bookmarkDataComponent = null;
                Debug.Log("Reset bookmark references due to scene change");
            }

            // Find or create the parent object for this scene
            GetBookmarkParent();

            // Get or create the bookmark data component
            if (bookmarkDataComponent == null)
            {
                bookmarkDataComponent = bookmarkParent.GetComponent<BookmarkCollectionData>();
                if (bookmarkDataComponent == null)
                {
                    Debug.Log("Creating new BookmarkCollectionData component");
                    bookmarkDataComponent = bookmarkParent.AddComponent<BookmarkCollectionData>();
                    // Yeni component için boş collection oluştur
                    bookmarkDataComponent.bookmarks = new BookmarkCollection();
                }
                else
                {
                    Debug.Log($"Found existing component with {bookmarkDataComponent.bookmarks?.bookmarks?.Count ?? 0} bookmarks");
                }
            }

            // MEVCUT bookmark collection'ı kullan, yeniden yaratma
            if (bookmarkDataComponent.bookmarks != null)
            {
                bookmarkCollection = bookmarkDataComponent.bookmarks;
            }
            else
            {
                Debug.Log("Component bookmarks was null, creating new");
                bookmarkDataComponent.bookmarks = new BookmarkCollection();
                bookmarkCollection = bookmarkDataComponent.bookmarks;
            }
            
            Debug.Log($"Final bookmark count: {bookmarkCollection.bookmarks.Count}");
            
            // ReorderableList'leri güncelle
            UpdateSeparatedLists();
            SetupReorderableLists();
        }

        private void SetupReorderableLists()
        {
            // Object Bookmarks List
            objectBookmarksList = new ReorderableList(objectBookmarks, typeof(BookmarkData), true, true, false, false);
            
            objectBookmarksList.drawHeaderCallback = (Rect rect) => {
                EditorGUI.LabelField(rect, "GameObject Bookmarks", EditorStyles.boldLabel);
            };
            
            objectBookmarksList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
                if (index >= objectBookmarks.Count) return;
                
                // Zebra stripe background - handle'ı da kapsayacak şekilde
                if (Event.current.type == EventType.Repaint)
                {
                    Color backgroundColor;
                    if (index % 2 == 0)
                    {
                        backgroundColor = EditorGUIUtility.isProSkin ?
                            new Color(0.24f, 0.24f, 0.24f, 1f) :
                            new Color(0.9f, 0.9f, 0.9f, 1f);
                    }
                    else
                    {
                        backgroundColor = EditorGUIUtility.isProSkin ?
                            new Color(0.3f, 0.3f, 0.3f, 1f) :
                            new Color(0.95f, 0.95f, 0.95f, 1f);
                    }
                    
                    // Handle'ın solundan başla ve sağ margini artır
                    Rect backgroundRect = new Rect(rect.x - 24, rect.y - 3, rect.width + 32, 30);
                    EditorGUI.DrawRect(backgroundRect, backgroundColor);
                    
                    // Manuel handle çizimi (3 çizgi)
                    Color handleColor = EditorGUIUtility.isProSkin ? 
                        new Color(0.6f, 0.6f, 0.6f, 1f) : 
                        new Color(0.4f, 0.4f, 0.4f, 1f);
                    
                    float handleX = rect.x - 16;
                    float handleY = rect.y + rect.height / 2f;
                    float lineWidth = 10f;
                    float lineSpacing = 3f;
                    
                    for (int i = -1; i <= 1; i++)
                    {
                        Rect lineRect = new Rect(handleX, handleY + (i * lineSpacing) - 0.5f, lineWidth, 1f);
                        EditorGUI.DrawRect(lineRect, handleColor);
                    }
                }
                
                DrawBookmarkElement(rect, objectBookmarks[index], false);
            };
            
            objectBookmarksList.onReorderCallback = (ReorderableList list) => {
                RebuildBookmarkCollection();
                SaveBookmarks();
            };

            objectBookmarksList.elementHeight = 28;

            // View Bookmarks List
            viewBookmarksList = new ReorderableList(viewBookmarks, typeof(BookmarkData), true, true, false, false);
            
            viewBookmarksList.drawHeaderCallback = (Rect rect) => {
                EditorGUI.LabelField(rect, "Scene View Bookmarks", EditorStyles.boldLabel);
            };
            
            viewBookmarksList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
                if (index >= viewBookmarks.Count) return;
                
                // Zebra stripe background - handle'ı da kapsayacak şekilde
                if (Event.current.type == EventType.Repaint)
                {
                    Color backgroundColor;
                    if (index % 2 == 0)
                    {
                        backgroundColor = EditorGUIUtility.isProSkin ?
                            new Color(0.24f, 0.24f, 0.24f, 1f) :
                            new Color(0.9f, 0.9f, 0.9f, 1f);
                    }
                    else
                    {
                        backgroundColor = EditorGUIUtility.isProSkin ?
                            new Color(0.3f, 0.3f, 0.3f, 1f) :
                            new Color(0.95f, 0.95f, 0.95f, 1f);
                    }
                    
                    // Handle'ın solundan başla ve sağ margini artır
                    Rect backgroundRect = new Rect(rect.x - 24, rect.y - 3, rect.width + 32, 30);
                    EditorGUI.DrawRect(backgroundRect, backgroundColor);
                    
                    // Manuel handle çizimi (3 çizgi)
                    Color handleColor = EditorGUIUtility.isProSkin ? 
                        new Color(0.6f, 0.6f, 0.6f, 1f) : 
                        new Color(0.4f, 0.4f, 0.4f, 1f);
                    
                    float handleX = rect.x - 16;
                    float handleY = rect.y + rect.height / 2f;
                    float lineWidth = 10f;
                    float lineSpacing = 3f;
                    
                    for (int i = -1; i <= 1; i++)
                    {
                        Rect lineRect = new Rect(handleX, handleY + (i * lineSpacing) - 0.5f, lineWidth, 1f);
                        EditorGUI.DrawRect(lineRect, handleColor);
                    }
                }
                
                DrawBookmarkElement(rect, viewBookmarks[index], true);
            };
            
            viewBookmarksList.onReorderCallback = (ReorderableList list) => {
                RebuildBookmarkCollection();
                SaveBookmarks();
            };

            viewBookmarksList.elementHeight = 28;
        }

        private void RebuildBookmarkCollection()
        {
            // Ana bookmark koleksiyonunu yeniden oluştur
            bookmarkCollection.bookmarks.Clear();
            bookmarkCollection.bookmarks.AddRange(objectBookmarks);
            bookmarkCollection.bookmarks.AddRange(viewBookmarks);
        }

        private void UpdateSeparatedLists()
        {
            objectBookmarks.Clear();
            viewBookmarks.Clear();

            foreach (var bookmark in bookmarkCollection.bookmarks)
            {
                if (bookmark.isViewBookmark)
                    viewBookmarks.Add(bookmark);
                else
                    objectBookmarks.Add(bookmark);
            }
        }

        // We need to make OnGUI method called frequently to update color changes
        private void OnInspectorUpdate()
        {
            this.Repaint();
        }

        // Update the bookmark name field based on current selection
        private void UpdateBookmarkNameFromSelection()
        {
            // Only update if the user hasn't manually set a name
            if (!isBookmarkNameManuallySet)
            {
                GameObject selectedObject = Selection.activeGameObject;
                if (selectedObject != null)
                {
                    // Null check for bookmarkParent before accessing it
                    if (bookmarkParent == null)
                    {
                        bookmarkName = selectedObject.name;
                        Repaint();
                        return;
                    }
                    
                    // Only update if it's a "real" game object (not our bookmark objects)
                    if (selectedObject != bookmarkParent &&
                        (selectedObject.transform.parent == null || selectedObject.transform.parent != bookmarkParent.transform))
                    {
                        bookmarkName = selectedObject.name;
                        Repaint(); // Refresh the editor window to show the new name
                    }
                }
            }
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            if (!bookmarkCollection.showAllGizmos)
                return;

            // Save current color and matrix
            Color originalColor = Handles.color;
            Matrix4x4 originalMatrix = Handles.matrix;

            foreach (var bookmark in bookmarkCollection.bookmarks)
            {
                if (bookmark.showGizmo)
                {
                    // Use the bookmark's individual color with global alpha
                    Color gizmoColor = bookmark.color;
                    gizmoColor.a = bookmarkCollection.gizmoAlpha;
                    Handles.color = gizmoColor;

                    // Stay on top ayarını uygula
                    if (bookmarkCollection.stayOnTop)
                    {
                        Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;
                    }
                    else
                    {
                        Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;
                    }

                    // Get the current camera view direction in the scene view
                    Vector3 cameraDirection = SceneView.lastActiveSceneView.camera.transform.forward;

                    // Draw different gizmos for camera bookmarks vs object bookmarks
                    if (bookmark.isViewBookmark)
                    {
                        // Draw a camera-like gizmo for view bookmarks
                        DrawCameraGizmo(bookmark.position, bookmark.rotation, bookmarkCollection.gizmoRadius);
                    }
                    else
                    {
                        // Draw a gizmo based on the selected draw type for object bookmarks
                        if (bookmarkCollection.gizmoDrawType == 0) // Solid disc
                        {
                            Handles.DrawSolidDisc(bookmark.position, cameraDirection, bookmarkCollection.gizmoRadius);
                        }
                        else // Wire circle
                        {
                            Handles.DrawWireDisc(bookmark.position, cameraDirection, bookmarkCollection.gizmoRadius);

                            // Draw cross lines for better visibility of wire circle
                            Vector3 upVector = Vector3.Cross(cameraDirection, Vector3.up).normalized;
                            if (upVector.magnitude < 0.001f)
                                upVector = Vector3.Cross(cameraDirection, Vector3.right).normalized;

                            Vector3 rightVector = Vector3.Cross(cameraDirection, upVector).normalized;

                            Handles.DrawLine(
                                bookmark.position - upVector * bookmarkCollection.gizmoRadius,
                                bookmark.position + upVector * bookmarkCollection.gizmoRadius
                            );

                            Handles.DrawLine(
                                bookmark.position - rightVector * bookmarkCollection.gizmoRadius,
                                bookmark.position + rightVector * bookmarkCollection.gizmoRadius
                            );
                        }
                    }

                    // YENİ: Merkeze nokta çiz (hem view hem object bookmarks için)
                    float centerDotRadius = bookmarkCollection.gizmoRadius * 0.05f; // Gizmo'nun %5'i kadar
                    centerDotRadius = Mathf.Max(centerDotRadius, 0.02f); // Minimum 0.02 unit
                    centerDotRadius = Mathf.Min(centerDotRadius, 0.1f);  // Maximum 0.1 unit

                    // Merkez nokta için daha koyu renk kullan (daha iyi görünürlük için)
                    Color centerColor = gizmoColor;
                    centerColor.r *= 0.7f;
                    centerColor.g *= 0.7f;
                    centerColor.b *= 0.7f;
                    centerColor.a = gizmoColor.a; // Aynı şeffaflık

                    Handles.color = centerColor;
                    Handles.DrawSolidDisc(bookmark.position, cameraDirection, centerDotRadius);

                    // Draw the bookmark name
                    GUIStyle labelStyle = new GUIStyle();
                    labelStyle.normal.textColor = new Color(1f, 1f, 1f, bookmarkCollection.textAlpha);
                    labelStyle.fontSize = 12;
                    labelStyle.fontStyle = FontStyle.Bold;

                    Handles.Label(bookmark.position + Vector3.up * (bookmarkCollection.gizmoRadius * 0.8f), bookmark.name, labelStyle);
                }
            }

            // Restore original color, matrix and z-test
            Handles.color = originalColor;
            Handles.matrix = originalMatrix;
            Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;
        }

        // Draw a camera-like gizmo for view bookmarks
        private void DrawCameraGizmo(Vector3 position, Quaternion rotation, float size)
        {
            // Draw a wireframe camera representation
            Vector3 forward = rotation * Vector3.forward;
            Vector3 up = rotation * Vector3.up;
            Vector3 right = rotation * Vector3.right;

            float cameraSize = size * 0.8f;
            float lensFactor = 1.5f;

            // Camera body (rectangle)
            Vector3[] bodyCorners = new Vector3[4]
            {
                position + (right * cameraSize) + (up * cameraSize * 0.6f),
                position - (right * cameraSize) + (up * cameraSize * 0.6f),
                position - (right * cameraSize) - (up * cameraSize * 0.6f),
                position + (right * cameraSize) - (up * cameraSize * 0.6f)
            };

            // Draw camera body
            for (int i = 0; i < 4; i++)
            {
                Handles.DrawLine(bodyCorners[i], bodyCorners[(i + 1) % 4]);
            }

            // Draw lens (forward projection)
            Vector3 lensCenter = position + forward * cameraSize * lensFactor;
            Handles.DrawLine(position, lensCenter);

            // Draw lens circle
            Handles.DrawWireDisc(lensCenter, forward, cameraSize * 0.4f);

            // Draw view direction line
            Handles.DrawLine(lensCenter, lensCenter + forward * cameraSize);
        }

        private void OnGUI()
        {
            // Check for selection changes to update the bookmark name
            if (Event.current.type == EventType.Layout)
            {
                UpdateBookmarkNameFromSelection();
            }

            //GUILayout.Label("GameObjects and Scene Bookmarks", EditorStyles.boldLabel);

            // Name field
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("New Bookmark Name", GUILayout.Width(150));

            // Detect if the user manually changed the name
            string previousName = bookmarkName;
            bookmarkName = EditorGUILayout.TextField(bookmarkName);

            // If the name changed and it wasn't from automatic selection update, mark as manually set
            if (bookmarkName != previousName && Event.current.type == EventType.Used)
            {
                isBookmarkNameManuallySet = true;
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            // Listeleri güncelle
            UpdateSeparatedLists();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            // Object Bookmarks Section
            GUILayout.Label("GameObject Bookmarks", EditorStyles.boldLabel);

            // Object bookmark creation button
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent("Bookmark Selected Object", "Save the position, rotation and scale of the currently selected object")))
            {
                CreateObjectBookmark();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            // Draw object bookmarks with ReorderableList
            if (objectBookmarksList != null && objectBookmarks.Count > 0)
            {
                objectBookmarksList.DoLayoutList();
            }
            else
            {
                EditorGUILayout.LabelField("No object bookmarks yet", EditorStyles.centeredGreyMiniLabel);
            }

            EditorGUILayout.Space(10);

            // View Bookmarks Section
            GUILayout.Label("Scene View Bookmarks", EditorStyles.boldLabel);

            // View bookmark creation button
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent("Bookmark Current View", "Save the current Scene View camera position and angle")))
            {
                CreateViewBookmark();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            // Draw view bookmarks with ReorderableList
            if (viewBookmarksList != null && viewBookmarks.Count > 0)
            {
                viewBookmarksList.DoLayoutList();
            }
            else
            {
                EditorGUILayout.LabelField("No view bookmarks yet", EditorStyles.centeredGreyMiniLabel);
            }

            EditorGUILayout.Space(15);

            // Settings Section - at the bottom
            EditorGUILayout.BeginVertical("GroupBox");
            GUILayout.Label("Settings", EditorStyles.boldLabel);

            // Gizmo draw type selection
            EditorGUILayout.BeginHorizontal();
            GUIContent gizmoTypeLabel = new GUIContent("Gizmo Type", "Choose how gizmos are displayed in Scene View");
            int newGizmoType = EditorGUILayout.Popup(gizmoTypeLabel, bookmarkCollection.gizmoDrawType, new string[] { "Solid Disc", "Wire Circle" });
            if (newGizmoType != bookmarkCollection.gizmoDrawType)
            {
                bookmarkCollection.gizmoDrawType = newGizmoType;
                SaveBookmarks();
                SceneView.RepaintAll();
            }
            EditorGUILayout.EndHorizontal();

            // Gizmo radius slider - shared by all bookmarks
            EditorGUILayout.BeginHorizontal();
            GUIContent radiusLabel = new GUIContent("Gizmo Radius", "Size of the gizmos displayed in Scene View");
            float newRadius = EditorGUILayout.Slider(radiusLabel, bookmarkCollection.gizmoRadius, 0.1f, 50.0f);
            if (newRadius != bookmarkCollection.gizmoRadius)
            {
                bookmarkCollection.gizmoRadius = newRadius;
                SaveBookmarks();
                SceneView.RepaintAll();
            }
            EditorGUILayout.EndHorizontal();

            // Global transparency slider
            EditorGUILayout.BeginHorizontal();
            GUIContent transparencyLabel = new GUIContent("Gizmo Transparency", "Transparency level of all gizmos (0 = opaque, 1 = transparent)");
            float newAlpha = EditorGUILayout.Slider(transparencyLabel, 1f - bookmarkCollection.gizmoAlpha, 0f, 1f);
            if (!Mathf.Approximately(newAlpha, 1f - bookmarkCollection.gizmoAlpha))
            {
                bookmarkCollection.gizmoAlpha = 1f - newAlpha;
                SaveBookmarks();
                SceneView.RepaintAll();
            }
            EditorGUILayout.EndHorizontal();

            // Text transparency slider
            EditorGUILayout.BeginHorizontal();
            GUIContent textTransparencyLabel = new GUIContent("Text Transparency", "Transparency level of bookmark text labels (0 = opaque, 1 = transparent)");
            float newTextAlpha = EditorGUILayout.Slider(textTransparencyLabel, 1f - bookmarkCollection.textAlpha, 0f, 1f);
            if (!Mathf.Approximately(newTextAlpha, 1f - bookmarkCollection.textAlpha))
            {
                bookmarkCollection.textAlpha = 1f - newTextAlpha;
                SaveBookmarks();
                SceneView.RepaintAll();
            }
            EditorGUILayout.EndHorizontal();

            // Stay On Top toggle
            EditorGUILayout.BeginHorizontal();
            GUIContent stayOnTopLabel = new GUIContent("Stay On Top", "Gizmolar her zaman mesh'lerin önünde görünsün");
            bool newStayOnTop = EditorGUILayout.Toggle(stayOnTopLabel, bookmarkCollection.stayOnTop);
            if (newStayOnTop != bookmarkCollection.stayOnTop)
            {
                bookmarkCollection.stayOnTop = newStayOnTop;
                SaveBookmarks();
                SceneView.RepaintAll();
            }
            EditorGUILayout.EndHorizontal();

            // Master toggle for showing all gizmos
            EditorGUILayout.BeginHorizontal();
            GUIContent showAllLabel = new GUIContent("Show All Gizmos", "Master toggle to show/hide all bookmark gizmos in Scene View");
            bool newShowAllGizmos = EditorGUILayout.Toggle(showAllLabel, bookmarkCollection.showAllGizmos);
            if (newShowAllGizmos != bookmarkCollection.showAllGizmos)
            {
                bookmarkCollection.showAllGizmos = newShowAllGizmos;
                SaveBookmarks();
                SceneView.RepaintAll(); // Repaint all scene views to update gizmos
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical(); // End GroupBox

            EditorGUILayout.EndScrollView();
        }
        
        private void DrawBookmarkElement(Rect rect, BookmarkData bookmark, bool isViewBookmark)
        {
            float xPos = rect.x + 2;
            float yPos = rect.y + 2;
            int originalIndex = bookmarkCollection.bookmarks.IndexOf(bookmark);

            // Get icons for buttons with tooltips
            GUIContent deleteButtonContent = new GUIContent(EditorGUIUtility.IconContent("Toolbar Minus").image, "Delete this bookmark");
            GUIContent focusButtonContent = new GUIContent(EditorGUIUtility.IconContent("d_scenevis_visible_hover").image, "Focus on this location/view");
            GUIContent moveButtonContent = new GUIContent(EditorGUIUtility.IconContent("d_NavMeshAgent Icon").image, "Move selected object to this location/view");

            // Get icons for bookmark types
            GUIContent objectBookmarkIcon = EditorGUIUtility.IconContent("Prefab Icon");
            GUIContent viewBookmarkIcon = EditorGUIUtility.IconContent("Camera Icon");

            // Fallback to these if above don't work well
            if (objectBookmarkIcon.image == null)
                objectBookmarkIcon = EditorGUIUtility.IconContent("GameObject Icon");
            if (viewBookmarkIcon.image == null)
                viewBookmarkIcon = EditorGUIUtility.IconContent("SceneViewCamera");

            // Focus View button with eye icon
            if (GUI.Button(new Rect(xPos, yPos, 28, 24), focusButtonContent))
            {
                if (bookmark.isViewBookmark)
                {
                    FocusOnViewBookmark(bookmark);
                }
                else
                {
                    FocusOnBookmarkLocation(bookmark);
                }
            }
            xPos += 30;

            // Color picker for this specific bookmark
            GUIContent colorLabel = new GUIContent("", "Change the color of this bookmark's gizmo");
            Color newColor = EditorGUI.ColorField(new Rect(xPos, yPos, 40, 24), colorLabel, bookmark.color);
            if (newColor != bookmark.color)
            {
                bookmark.color = newColor;
                SaveBookmarks();
                SceneView.RepaintAll();
            }
            xPos += 42;

            // Toggle for showing gizmo for this specific bookmark
            GUIContent gizmoToggleLabel = new GUIContent("", bookmark.showGizmo ? "Hide this bookmark's gizmo" : "Show this bookmark's gizmo");
            bool newShowGizmo = EditorGUI.Toggle(new Rect(xPos, yPos + 5, 20, 18), gizmoToggleLabel, bookmark.showGizmo);
            if (newShowGizmo != bookmark.showGizmo)
            {
                bookmark.showGizmo = newShowGizmo;
                SaveBookmarks();
                SceneView.RepaintAll();
            }
            xPos += 22;

            // Show bookmark type icon
            string typeTooltip = bookmark.isViewBookmark ? "View Bookmark - Camera position and angle" : "Object Bookmark - Object position, rotation and scale";
            GUIContent typeIcon = bookmark.isViewBookmark ?
                new GUIContent(viewBookmarkIcon.image, typeTooltip) :
                new GUIContent(objectBookmarkIcon.image, typeTooltip);
            GUI.Label(new Rect(xPos, yPos, 20, 24), typeIcon);
            xPos += 22;

            // Bookmark name text field - takes remaining space
            float nameWidth = rect.width - (xPos - rect.x) - 60;
            string newName = EditorGUI.TextField(new Rect(xPos, yPos, nameWidth, 24), bookmark.name);
            if (newName != bookmark.name)
            {
                bookmark.name = newName;
                SaveBookmarks();

                // Update name of focus object if it exists
                if (!string.IsNullOrEmpty(bookmark.objectId))
                {
                    GameObject focusObj = EditorUtility.InstanceIDToObject(int.Parse(bookmark.objectId)) as GameObject;
                    if (focusObj != null)
                    {
                        focusObj.name = "Bookmark_" + newName;
                    }
                }
            }
            xPos += nameWidth + 2;

            // Move Selected To Here button with move icon
            if (GUI.Button(new Rect(xPos, yPos, 28, 24), moveButtonContent))
            {
                if (bookmark.isViewBookmark)
                {
                    MoveSelectedObjectToViewBookmark(bookmark);
                }
                else
                {
                    MoveSelectedObjectToBookmark(bookmark);
                }
            }
            xPos += 30;

            // Delete button with trash icon
            if (GUI.Button(new Rect(xPos, yPos, 28, 24), deleteButtonContent))
            {
                // Delete any focus object associated with this bookmark
                if (!string.IsNullOrEmpty(bookmark.objectId))
                {
                    GameObject focusObj = EditorUtility.InstanceIDToObject(int.Parse(bookmark.objectId)) as GameObject;
                    if (focusObj != null)
                    {
                        Object.DestroyImmediate(focusObj);
                    }
                }

                bookmarkCollection.bookmarks.Remove(bookmark);
                UpdateSeparatedLists();
                SaveBookmarks();
                SceneView.RepaintAll();
            }
        }

        private void CreateObjectBookmark()
        {
            if (Selection.activeGameObject != null)
            {
                Transform transform = Selection.activeGameObject.transform;

                // Önce parent'ın hazır olduğundan emin ol
                GetBookmarkParent();
                
                if (bookmarkDataComponent == null)
                {
                    LoadBookmarksForCurrentScene();
                }

                // Generate a random pastel color
                Color randomColor = GenerateRandomPastelColor();

                BookmarkData newBookmark = new BookmarkData
                {
                    name = bookmarkName,
                    position = transform.position,
                    rotation = transform.rotation,
                    scale = transform.localScale,
                    color = randomColor,
                    isViewBookmark = false
                };

                bookmarkCollection.bookmarks.Add(newBookmark);
                
                Debug.Log($"Added bookmark. Total count: {bookmarkCollection.bookmarks.Count}");
                
                UpdateSeparatedLists();
                SaveBookmarks();

                bookmarkName = "New Bookmark";
                isBookmarkNameManuallySet = false;
            }
            else
            {
                EditorUtility.DisplayDialog("No Selection", "Please select an object in the scene to bookmark its location.", "OK");
            }
        }

        private void CreateViewBookmark()
        {
            SceneView sceneView = SceneView.lastActiveSceneView;
            if (sceneView != null)
            {
                // Önce parent'ın hazır olduğundan emin ol
                GetBookmarkParent();
                
                if (bookmarkDataComponent == null)
                {
                    LoadBookmarksForCurrentScene();
                }

                // Generate a random pastel color with a blue tint for view bookmarks
                Color randomColor = GenerateRandomPastelColor();
                randomColor.b = Mathf.Min(1f, randomColor.b + 0.3f); // Add blue tint

                BookmarkData newBookmark = new BookmarkData
                {
                    name = bookmarkName,
                    position = sceneView.pivot, // Store pivot for LookAt to work
                    rotation = sceneView.rotation, // Store scene rotation for LookAt to work
                    scale = Vector3.one, // Not used for view bookmarks
                    color = randomColor,
                    isViewBookmark = true,
                    viewSize = sceneView.size,
                    isPerspective = sceneView.camera.orthographic == false
                };

                bookmarkCollection.bookmarks.Add(newBookmark);
                
                Debug.Log($"Added view bookmark. Total count: {bookmarkCollection.bookmarks.Count}");
                
                UpdateSeparatedLists();
                SaveBookmarks();

                bookmarkName = "New View";
                isBookmarkNameManuallySet = false;
            }
            else
            {
                EditorUtility.DisplayDialog("No Scene View", "Please ensure a Scene View is open to bookmark the current view.", "OK");
            }
        }

        // Helper method to generate nice pastel colors for new bookmarks
        private Color GenerateRandomPastelColor()
        {
            // Generate random RGB values in the pastel range (0.5-1.0)
            float r = 0.5f + (Random.value * 0.5f);
            float g = 0.5f + (Random.value * 0.5f);
            float b = 0.5f + (Random.value * 0.5f);

            return new Color(r, g, b, 1f);
        }

        private void FocusOnBookmarkLocation(BookmarkData bookmark)
        {
            SceneView sceneView = SceneView.lastActiveSceneView;
            if (sceneView != null)
            {
                // Ensure we have a parent object
                GameObject parent = GetBookmarkParent();

                // Create a persistent empty object for focusing if it doesn't exist
                GameObject focusObject = null;

                // Check if this bookmark already has a focus object
                if (!string.IsNullOrEmpty(bookmark.objectId))
                {
                    focusObject = EditorUtility.InstanceIDToObject(int.Parse(bookmark.objectId)) as GameObject;
                }

                // If the focus object doesn't exist, create a new one
                if (focusObject == null)
                {
                    focusObject = new GameObject("Bookmark_" + bookmark.name);
                    focusObject.transform.position = bookmark.position;
                    focusObject.transform.rotation = bookmark.rotation;
                    focusObject.transform.localScale = bookmark.scale;

                    // Add a custom component to make it obvious this is a bookmark object in Scene view
                    BookmarkPositionMarker marker = focusObject.AddComponent<BookmarkPositionMarker>();
                    marker.bookmarkIndex = bookmarkCollection.bookmarks.IndexOf(bookmark);

                    // Don't hide these objects anymore, so they get saved with the scene
                    // But still hide them in hierarchy to reduce clutter
                    focusObject.hideFlags = HideFlags.HideInHierarchy;

                    // Set as child of the parent object
                    focusObject.transform.SetParent(parent.transform, true);

                    // Store the instance ID of the focus object in the bookmark
                    bookmark.objectId = focusObject.GetInstanceID().ToString();
                    SaveBookmarks();
                }
                else
                {
                    // Make sure position and rotation are up to date
                    focusObject.transform.position = bookmark.position;
                    focusObject.transform.rotation = bookmark.rotation;
                    focusObject.transform.localScale = bookmark.scale;

                    // Make sure the marker component exists and has the correct index
                    BookmarkPositionMarker marker = focusObject.GetComponent<BookmarkPositionMarker>();
                    if (marker == null)
                    {
                        marker = focusObject.AddComponent<BookmarkPositionMarker>();
                    }
                    marker.bookmarkIndex = bookmarkCollection.bookmarks.IndexOf(bookmark);
                }

                // Select the focus object
                Selection.activeGameObject = focusObject;

                // Frame the selected object in the scene view
                sceneView.FrameSelected();
                sceneView.Repaint();
            }
        }

        private void FocusOnViewBookmark(BookmarkData bookmark)
        {
            SceneView sceneView = SceneView.lastActiveSceneView;
            if (sceneView != null)
            {
                // Set perspective/orthographic mode and size first
                sceneView.orthographic = !bookmark.isPerspective;
                sceneView.size = bookmark.viewSize;

                // Simply use LookAt with the stored pivot and rotation
                sceneView.LookAt(bookmark.position, bookmark.rotation, bookmark.viewSize);

                Debug.Log($"Restoring View Bookmark:");
                Debug.Log($"Stored Pivot: {bookmark.position}");
                Debug.Log($"Stored Rotation: {bookmark.rotation}");
                Debug.Log($"Stored Size: {bookmark.viewSize}");

                sceneView.Repaint();

                // Check what actually happened
                EditorApplication.delayCall += () =>
                {
                    if (sceneView?.camera != null)
                    {
                        Debug.Log($"Restored Camera Position: {sceneView.camera.transform.position}");
                        Debug.Log($"Restored Scene Pivot: {sceneView.pivot}");
                    }
                };
            }
        }

        private void MoveSelectedObjectToBookmark(BookmarkData bookmark)
        {
            if (Selection.activeGameObject != null)
            {
                // Skip if the selected object is a focus object
                if (!string.IsNullOrEmpty(bookmark.objectId) &&
                    Selection.activeGameObject.GetInstanceID() == int.Parse(bookmark.objectId))
                {
                    EditorUtility.DisplayDialog("Invalid Selection", "Cannot move a bookmark focus object to a bookmark location.", "OK");
                    return;
                }

                Undo.RecordObject(Selection.activeGameObject.transform, "Move to Bookmarked Location");

                Transform transform = Selection.activeGameObject.transform;
                transform.position = bookmark.position;
                transform.rotation = bookmark.rotation;
                // Keep the original scale of the object
                // transform.localScale = bookmark.scale;
            }
            else
            {
                EditorUtility.DisplayDialog("No Selection", "Please select an object to move to the bookmarked location.", "OK");
            }
        }

        private void MoveSelectedObjectToViewBookmark(BookmarkData bookmark)
        {
            if (Selection.activeGameObject != null)
            {
                // First, focus on the view to get the actual camera position
                SceneView sceneView = SceneView.lastActiveSceneView;
                if (sceneView != null)
                {
                    // Temporarily set the view to the bookmark
                    sceneView.orthographic = !bookmark.isPerspective;
                    sceneView.size = bookmark.viewSize;
                    sceneView.LookAt(bookmark.position, bookmark.rotation, bookmark.viewSize);

                    // Now get the actual camera position
                    Vector3 actualCameraPosition = sceneView.camera.transform.position;
                    Quaternion actualCameraRotation = sceneView.camera.transform.rotation;

                    // Move the selected object to the camera position
                    Undo.RecordObject(Selection.activeGameObject.transform, "Move to View Bookmark Location");
                    Transform transform = Selection.activeGameObject.transform;
                    transform.position = actualCameraPosition;
                    transform.rotation = actualCameraRotation;

                    Debug.Log($"Moved object to actual camera position: {actualCameraPosition}");
                }
            }
            else
            {
                // Just log to console instead of showing dialog
                Debug.Log("No object selected to move to view bookmark location.");
            }
        }

        private void SaveBookmarks()
        {
            if (bookmarkDataComponent != null)
            {
                // Component ve scene'i dirty olarak işaretle
                EditorUtility.SetDirty(bookmarkDataComponent);
                EditorUtility.SetDirty(bookmarkParent);
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                
                Debug.Log($"Bookmarks saved - Count: {bookmarkCollection.bookmarks.Count}");
                Debug.Log($"Scene marked as dirty");
            }
        }

        // We don't need to load from JSON anymore, but keep this as a stub
        // in case we want to implement import/export functionality later
        private void LoadBookmarks()
        {
            // Nothing to do here - data is loaded from scene component
        }

        // This gets called when transforms are modified in the editor
        private UndoPropertyModification[] OnTransformChanged(UndoPropertyModification[] modifications)
        {
            bool dataChanged = false;

            // Check if any of our bookmark objects have been modified
            foreach (UndoPropertyModification modification in modifications)
            {
                Object target = modification.currentValue.target;

                // Only care about transform modifications
                if (target is Transform)
                {
                    Transform transform = target as Transform;
                    GameObject gameObject = transform.gameObject;

                    // Check if this GameObject has our marker component
                    BookmarkPositionMarker marker = gameObject.GetComponent<BookmarkPositionMarker>();
                    if (marker != null && marker.bookmarkIndex >= 0 && marker.bookmarkIndex < bookmarkCollection.bookmarks.Count)
                    {
                        // Update bookmark data
                        BookmarkData bookmark = bookmarkCollection.bookmarks[marker.bookmarkIndex];
                        bookmark.position = transform.position;
                        bookmark.rotation = transform.rotation;
                        bookmark.scale = transform.localScale;
                        dataChanged = true;
                    }
                }
            }

            // If any bookmarks changed, save and repaint
            if (dataChanged)
            {
                SaveBookmarks();
                SceneView.RepaintAll();
            }

            return modifications;
        }
    }
}
#endif