#if UNITY_EDITOR
using UnityEngine;
using System.Collections.Generic;

namespace FablesAliveGames
{
    [System.Serializable]
    public class BookmarkCollection
    {
        public List<BookmarkData> bookmarks = new List<BookmarkData>();

        public float textAlpha = 1.0f;

        public bool stayOnTop = true; // YENİ - Stay on top özelliği

        public bool showAllGizmos = true; // Master toggle for all gizmos
        public float gizmoRadius = 0.5f; // Global gizmo radius
        public float gizmoAlpha = 0.7f; // Global transparency level for gizmos
        public int gizmoDrawType = 0; // 0 = solid disc, 1 = wire circle
    }
}

#endif