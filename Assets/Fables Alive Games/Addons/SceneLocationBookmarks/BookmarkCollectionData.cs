#if UNITY_EDITOR

using UnityEngine;

namespace FablesAliveGames
{
    // Component to store bookmarks data in the scene
    public class BookmarkCollectionData : MonoBehaviour
    {
        [SerializeField]
        public BookmarkCollection bookmarks = new BookmarkCollection();
    }
}
#endif
