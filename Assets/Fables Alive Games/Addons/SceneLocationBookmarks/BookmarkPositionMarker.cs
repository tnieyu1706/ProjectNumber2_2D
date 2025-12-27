#if UNITY_EDITOR
using UnityEngine;

namespace FablesAliveGames
{
    // Simple marker component to identify bookmark objects in the scene
    public class BookmarkPositionMarker : MonoBehaviour
    {
        public int bookmarkIndex = -1;
        
        // This is just a marker component, no functionality needed
        // It simply identifies which bookmark this GameObject is associated with
    }
}
#endif