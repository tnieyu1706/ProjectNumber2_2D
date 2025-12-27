using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace TnieYuPackage.Utils
{
    public class SceneManagerUtil
    {
        public static List<string> GetNameOfCurrentLoadingScenes()
        {
            List<string> scenes = new List<string>();
            Scene scene;
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                scene = SceneManager.GetSceneAt(i);
                if (scene.isLoaded)
                    scenes.Add(scene.name);
            }
            
            return scenes;
        }

        public static List<Scene> GetSceneOfCurrentLoadingScenes()
        {
            List<Scene> scenes = new();
            Scene scene;
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                scene = SceneManager.GetSceneAt(i);
                if (scene.isLoaded)
                    scenes.Add(scene);
            }
            
            return scenes;
        }
    }
}