using UnityEngine;

namespace TnieYuPackage.SceneManagement
{
    [CreateAssetMenu(menuName = "TnieYuPackage/SceneManagement/Position")]
    public class PositionSo : ScriptableObject
    {
        public Vector3 position;
        public LocationSo location;
    }
}