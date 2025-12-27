using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TnieYuPackage.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TnieYuPackage.SceneManagement
{
    [Serializable]
    public class SceneGroupManager
    {
        public SceneGroup currentActiveSceneGroup;
        private const int PROGRESS_DELAY = 100;

        public event Action OnPreSceneGroupLoaded = delegate { };
        public event Action<string> OnSceneLoaded = delegate { };
        public event Action<string> OnSceneUnloaded = delegate { };
        public event Action OnSceneGroupUnloaded = delegate { };
        public event Action OnSceneGroupLoaded = delegate { };
        public event Action OnActiveSceneGroupLoaded = delegate { };

        public async Task LoadSceneAsync(SceneGroup sceneGroup, IProgress<float> progress)
        {
            OnPreSceneGroupLoaded?.Invoke();

            await UnLoadScenesAsync(sceneGroup);

            currentActiveSceneGroup = sceneGroup;
            //ensure
            List<string> loadedScenes = SceneManagerUtil.GetNameOfCurrentLoadingScenes();

            var totalScenesToLoad = currentActiveSceneGroup.Scenes.Count;
            var operationGroup = new AsyncOperationGroup(totalScenesToLoad);

            //LoadingScene
            AsyncOperation loaderOperation = null;
            foreach (var scene in currentActiveSceneGroup.Scenes)
            {
                if (loadedScenes.Contains(scene.Name)) continue;

                var operation = SceneManager.LoadSceneAsync(scene.Name, LoadSceneMode.Additive);
                // await Task.Yield();

                operationGroup.Operations.Add(operation);
                operation.completed += op => OnSceneLoaded?.Invoke(scene.Name);

                if (scene.SceneType == SceneType.SceneActive)
                {
                    operation.completed += async op =>
                    {
                        OnActiveSceneGroupLoaded?.Invoke();
                        await Task.Yield();

                        Scene newActiveScene =
                            SceneManager.GetSceneByName(sceneGroup.FindSceneNameByType(SceneType.SceneActive));
                        Scene oldActiveScene = SceneManager.GetActiveScene();

                        if (newActiveScene.IsValid())
                        {
                            SceneManager.SetActiveScene(newActiveScene);
                        }

                        if (oldActiveScene.name != Bootstrapper.BOOTSTRAPPER_NAME)
                        {
                            loaderOperation ??= SceneManager.UnloadSceneAsync(oldActiveScene);
                        }
                    };
                }
            }

            //Waiting
            while (!operationGroup.IsDone)
            {
                progress?.Report(operationGroup.Progress);
                await Task.Delay(PROGRESS_DELAY);
            }

            while (loaderOperation is { isDone: false })
            {
                await Task.Delay(PROGRESS_DELAY);
            }

            OnSceneGroupLoaded?.Invoke();
        }

        public async Task UnLoadScenesAsync(SceneGroup sceneGroup)
        {
            var unloadingScenes = new List<string>();
            var activeSceneName = SceneManager.GetActiveScene().name;

            //Prepare UnloadScenes
            List<string> newScenes = sceneGroup.Scenes.Select(sd => sd.Name).ToList();
            Scene scene;
            string sceneName;
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                scene = SceneManager.GetSceneAt(i);
                if (!scene.isLoaded) continue;

                sceneName = scene.name;
                if (sceneName.Equals(activeSceneName) ||
                    sceneName == Bootstrapper.BOOTSTRAPPER_NAME ||
                    newScenes.Contains(sceneName))
                    continue;

                unloadingScenes.Add(sceneName);
            }

            //Handle UnloadScenes
            var operationGroup = new AsyncOperationGroup(unloadingScenes.Count);
            foreach (var s in unloadingScenes)
            {
                var operation = SceneManager.UnloadSceneAsync(s);
                operationGroup.Operations.Add(operation);

                OnSceneUnloaded?.Invoke(s);
            }

            //Waiting
            while (!operationGroup.IsDone)
            {
                await Task.Delay(100); // tight loop
            }

            // Optional: UnloadUnusedAssets - unload all unused asset from memory 
            await Resources.UnloadUnusedAssets();

            OnSceneGroupUnloaded?.Invoke();
        }
    }

    public readonly struct AsyncOperationGroup
    {
        public readonly List<AsyncOperation> Operations;

        public float Progress => Operations.Count == 0 ? 0 : Operations.Average(o => o.progress);

        public bool IsDone => Operations.Count == 0 || Operations.All(o => o.isDone);

        public AsyncOperationGroup(int capacity)
        {
            Operations = new List<AsyncOperation>(capacity);
        }
    }
}