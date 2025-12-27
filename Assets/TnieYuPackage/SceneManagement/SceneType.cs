using System;
using System.Collections.Generic;
using System.Linq;
using Eflatun.SceneReference;

namespace TnieYuPackage.SceneManagement
{
    [Serializable]
    public class SceneGroup
    {
        public string name;
        public List<SceneData> Scenes;

        public string FindSceneNameByType(SceneType sceneType)
        {
            return Scenes.FirstOrDefault(scene => scene.SceneType == sceneType)?.Name;
        }
    }
    
    [Serializable]
    public class SceneData
    {
        public SceneReference Reference;
        public SceneType SceneType;
        public string Name => Reference.Name;
    }
    
    public enum SceneType
    {
        SceneActive,
        MainMenu,
        UserInterface,
        HUD,
        Environment,
        Tool
    }
}