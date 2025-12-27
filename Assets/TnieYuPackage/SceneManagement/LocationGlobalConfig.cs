using TnieYuPackage.DesignPatterns.Patterns.Singleton;
using UnityEngine;

namespace TnieYuPackage.SceneManagement
{
    [CreateAssetMenu(fileName = "LocationGlobalConfig",
        menuName = "TnieYuPackage/SceneManagement/LocationGlobalConfig")]
    public class LocationGlobalConfig : SingletonScriptable<LocationGlobalConfig>
    {
        public LocationSo currentLocation;
        public Vector3 currentPosition;
        public bool isLoadingBootstrapperInitialized = true;

        [HideInInspector] public LocationManager loader;
        [HideInInspector] public GameObject playerGameObject;
    }
}