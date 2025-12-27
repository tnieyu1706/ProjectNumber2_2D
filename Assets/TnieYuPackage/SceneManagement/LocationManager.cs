using System;
using System.Threading.Tasks;
using EditorAttributes;
using UnityEngine;
using UnityEngine.UI;

namespace TnieYuPackage.SceneManagement
{
    public class LocationManager : MonoBehaviour
    {
        #region Properties
        
        public readonly SceneGroupManager SceneGroupManager = new SceneGroupManager();

        [SerializeField] private float delay = 1f;

        [SerializeField] Canvas loadingCanvas;
        [SerializeField] Camera loadingCamera;
        [SerializeField] Image loadingImage;
        [SerializeField] Image loadingBar;
        
        [SerializeField] AudioSource loadingAudioSource;
        [SerializeField] float fillSpeed = 0.5f;

        private SceneGroup currentSceneGroup;

        float targetProgress;
        bool isLoading;
        
        #endregion

        async void Start()
        {
            try
            {
                if (LocationGlobalConfig.Instance != null)
                {
                    LocationGlobalConfig.Instance.loader ??= this;
                    Vector3 playerPosition = LocationGlobalConfig.Instance.currentPosition;
                    
                    await LoadLocation(LocationGlobalConfig.Instance.currentLocation, playerPosition, true);
                }
                else
                {
                    Debug.Log("Current LocationGlobalConfig is null");
                }
            }
            catch (Exception e)
            {   
                throw new Exception($"Failed to load current SceneGroup", e);
            }
        }

        private void OnDestroy()
        {
            if (LocationGlobalConfig.Instance != null && LocationGlobalConfig.Instance.playerGameObject != null)
            {
                LocationGlobalConfig.Instance.currentPosition = LocationGlobalConfig.Instance.playerGameObject.transform.position;
            }
        }

        #region DebugTest
        
        private void OnEnable()
        {
            SceneGroupManager.OnPreSceneGroupLoaded += OnPreSceneGroupLoaded;
            SceneGroupManager.OnSceneGroupLoaded += OnSceneGroupLoaded;
            SceneGroupManager.OnSceneLoaded += OnSceneLoaded;
            SceneGroupManager.OnSceneUnloaded += OnSceneUnloaded;
            SceneGroupManager.OnSceneGroupUnloaded += OnSceneGroupUnLoaded;
        }

        private void OnDisable()
        {
            SceneGroupManager.OnPreSceneGroupLoaded -= OnPreSceneGroupLoaded;
            SceneGroupManager.OnSceneGroupLoaded -= OnSceneGroupLoaded;
            SceneGroupManager.OnSceneLoaded -= OnSceneLoaded;
            SceneGroupManager.OnSceneUnloaded -= OnSceneUnloaded;
            SceneGroupManager.OnSceneGroupUnloaded -= OnSceneGroupUnLoaded;
        }

        private void OnPreSceneGroupLoaded() => Debug.Log("Pre Scene Group Loaded");
        private void OnSceneGroupLoaded() => Debug.Log("Scene Group Loaded");
        private void OnSceneLoaded(string sceneName) => Debug.Log("Scene Loaded: " + sceneName);
        private void OnSceneUnloaded(string sceneName) => Debug.Log("Scene Unloaded: " + sceneName);
        private void OnSceneGroupUnLoaded() => Debug.Log("Scene Group Unloaded");

        #endregion

        private float currentFillAmount;
        private float progressDifference;

        void Update()
        {
            if (!isLoading) return;

            currentFillAmount = loadingBar.fillAmount;
            progressDifference = Mathf.Abs(currentFillAmount - targetProgress);

            loadingBar.fillAmount =
                Mathf.Lerp(
                    currentFillAmount,
                    targetProgress,
                    Time.deltaTime * progressDifference * fillSpeed
                );
        }
        
        #region Loading Methods

        public async Task LoadLocation(LocationSo location, Vector3 position, bool isReload = false)
        {
            var currentLocation = LocationGlobalConfig.Instance.currentLocation;
            if (isReload || currentLocation == null || currentLocation != location)
            {
                //setup ui
                loadingImage.sprite = location.loadingImage;
                loadingAudioSource.resource = location.loadingAudio;
                
                await LoadSceneGroup(location.sceneGroup);
                
                //option: save location
                LocationGlobalConfig.Instance.currentLocation = location;
            }

            if (LocationGlobalConfig.Instance.playerGameObject == null)
            {
                Debug.Log("playerGameObject is null");
                return;
            }

            LocationGlobalConfig.Instance.playerGameObject.transform.position = position;
            //option: save position
            LocationGlobalConfig.Instance.currentPosition = position;
        }

        private async Task LoadSceneGroup(SceneGroup sceneGroup)
        {
            loadingBar.fillAmount = 0;
            targetProgress = 1f;

            LoadingProgress progress = new LoadingProgress();
            progress.ProgressAction += target => targetProgress = Mathf.Max(target, targetProgress);

            EnableLoadCanvas(true);
            Task loadTask = SceneGroupManager.LoadSceneAsync(sceneGroup, progress);

            await Task.Delay(TimeSpan.FromSeconds(delay));
            await loadTask;

            await Task.Yield();
            EnableLoadCanvas(false);
        }

        void EnableLoadCanvas(bool enable = true)
        {
            isLoading = enable;

            loadingCanvas.gameObject.SetActive(enable);
            
            if (enable)
            {
                loadingCamera.gameObject.SetActive(true);
                loadingCamera.tag = "MainCamera";
                loadingCamera.enabled = true;
            }
            else
            {
                loadingCamera.tag = "Camera";
                loadingCamera.enabled = false;
                loadingCamera.gameObject.SetActive(false);
            }
            
            loadingAudioSource.gameObject.SetActive(enable);
            
            if (enable)
                loadingAudioSource.Play();
            else
                loadingAudioSource.Stop();
        }
        
        #endregion
        
        #region Loading Manual

        [Space(20)]
        [SerializeField] private PositionSo position;
        
        [Button]
        private async Task LoadPositionManual()
        {
            await LoadLocation(position.location, position.position);
        }
        
        #endregion
    }
}