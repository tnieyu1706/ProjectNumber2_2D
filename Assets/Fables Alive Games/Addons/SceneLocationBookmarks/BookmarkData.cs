#if UNITY_EDITOR

using UnityEngine;

namespace FablesAliveGames
{
    [System.Serializable]
    public class BookmarkData
    {
        public string name;
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;
        public bool showGizmo = true; // Default to showing gizmo
        public string objectId = ""; // Empty string or GameObject instanceID
        public Color color = Color.yellow; // Individual color for this bookmark

        // View bookmark specific properties
        public bool isViewBookmark = false; // True for camera view bookmarks, false for object bookmarks
        public float viewSize = 10f; // Scene view size for orthographic mode
        public bool isPerspective = true; // True for perspective view, false for orthographic
    }
}
#endif