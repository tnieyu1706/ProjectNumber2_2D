using TnieYuPackage.DesignPatterns.Patterns.Singleton;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TnieYuPackage.SceneManagement
{
    public class Bootstrapper : SingletonBehavior<Bootstrapper>
    {
        public const string BOOTSTRAPPER_NAME = "Bootstrapper";
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static async void Init()
        {
            if (LocationGlobalConfig.Instance != null && !LocationGlobalConfig.Instance.isLoadingBootstrapperInitialized)
            {
                return;
            }
            Debug.Log("Bootstrapper Initialized...");
            await SceneManager.LoadSceneAsync(BOOTSTRAPPER_NAME, LoadSceneMode.Single);
        }   
    }
}