using System;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using TnieCustomPackage.BackboneLogger;
using TnieYuPackage.GlobalExtensions;
using TnieYuPackage.Utils;
using UnityEngine;
using Logger = TnieCustomPackage.BackboneLogger.Logger;

namespace TnieYuPackage.SaveLoadSystem.RuntimeSaveLoad
{
    /// <summary>
    /// BaseRuntimeData is object RuntimeData save load.
    /// It's not handle circle saving.
    /// </summary>
    [Serializable]
    public abstract class BaseRuntimeData : IRuntimeData
    {
        private SerializableGuid runtimeId = SerializableGuid.NewGuid();

        public SerializableGuid RuntimeId
        {
            get => runtimeId;
            set => runtimeId = value;
        }

        public string Save()
        {
            string saveJson = BaseSave();

            JObject saveData = JsonUtility.FromJson<JObject>(saveJson);

            saveData["type"] = GetType().GetShortAssemblyName();

            return saveData.ToString();
        }

        /// <summary>
        /// return json string data of BaseRuntimeData.
        /// </summary>
        /// <returns></returns>
        protected abstract string BaseSave();

        public abstract void Load(string jsonData);


        [CanBeNull]
        public static BaseRuntimeData Deserialize(string jsonData)
        {
            JObject loadData = JObject.Parse(jsonData);

            if (!loadData.TryGetValue("type", out JToken typeToken))
            {
                Logger.Log("can't deserialize runtime data - missing [type] property!", LogLevel.Error, "Runtime");
                return null;
            }

            try
            {
                string typeName = typeToken.ToString();
                Type actualType = Type.GetType(typeName);

                if (actualType == null)
                {
                    Logger.Log($"can't convert type C# Type from - {typeName}", LogLevel.Error, "Runtime");
                    return null;
                }

                if (!typeof(BaseRuntimeData).IsAssignableFrom(actualType))
                {
                    Logger.Log($"{typeName} is not derive of {nameof(BaseRuntimeData)}", LogLevel.Error, "Runtime");
                    return null;
                }

                BaseRuntimeData runtimeData = (BaseRuntimeData)Activator.CreateInstance(actualType!);

                runtimeData.Load(jsonData);
                return runtimeData;
            }
            catch (Exception e)
            {
                Logger.Log(e.Message, LogLevel.Error, "Runtime");

                return null;
            }
        }
    }
}