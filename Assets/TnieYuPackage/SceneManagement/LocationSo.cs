using UnityEngine;
using UnityEngine.Audio;

namespace TnieYuPackage.SceneManagement
{
    [CreateAssetMenu(menuName = "TnieYuPackage/SceneManagement/Location")]
    public class LocationSo : ScriptableObject
    {
        public SceneGroup sceneGroup = new SceneGroup();
        public string shortDescription;
        public Sprite loadingImage;
        public AudioResource loadingAudio;
    }
}