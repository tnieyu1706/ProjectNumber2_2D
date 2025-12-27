using System.IO;
using EditorAttributes;
using TnieCustomPackage.BackboneLogger;
using TnieCustomPackage.SerializeInterface;
using TnieYuPackage.CustomAttributes;
using UnityEngine;
using Logger = TnieCustomPackage.BackboneLogger.Logger;

namespace TnieYuPackage.SaveLoadSystem.RuntimeSaveLoad
{
    public class RuntimeSaveLoadBehaviour : MonoBehaviour
    {
        [TniePath(typeof(TextAsset), ".json")] public string jsonFilePath;

        public InterfaceReference<IRuntimeData> runtimeDataSaveLoad;

        private void OnEnable()
        {
            AutoLoad();
        }

        private void OnDisable()
        {
            AutoSave();
        }

        [Button]
        public void AutoSave()
        {
            if (runtimeDataSaveLoad == null || string.IsNullOrEmpty(jsonFilePath))
            {
                Logger.Log("filePath or runtimeData not setup.", LogLevel.Error, "Debug");
                return;
            }

            File.WriteAllText(jsonFilePath, runtimeDataSaveLoad.Value.Save());
        }

        [Button]
        public void AutoLoad()
        {
            if (runtimeDataSaveLoad == null || string.IsNullOrEmpty(jsonFilePath))
            {
                Logger.Log("filePath or runtimeData not setup.", LogLevel.Error, "Debug");
                return;
            }

            string jsonData = File.ReadAllText(jsonFilePath);

            runtimeDataSaveLoad.Value.Load(jsonData);
        }
    }
}