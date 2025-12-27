using System;
using EditorAttributes;
using Newtonsoft.Json.Linq;
using TnieCustomPackage.BackboneLogger;
using TnieYuPackage.Utils;
using UnityEngine;
using Logger = TnieCustomPackage.BackboneLogger.Logger;

namespace TnieYuPackage.SaveLoadSystem.RuntimeSaveLoad
{
    public class ListComponentRuntimeData : MonoBehaviour, IRuntimeData
    {
        private const string DATA_JSON_NAME = "data";
        private const string DETAILS_DATA_JSON_NAME = "details";

        [SerializeField, ReadOnly] SerializableGuid runtimeId = SerializableGuid.NewGuid();

        public SerializableGuid RuntimeId
        {
            get => runtimeId;
            set => runtimeId = value;
        }

        public ListComponentRuntimeStruct listComponentRuntime = new();

        public string Save()
        {
            //save: component data
            JObject jsonData = JObject.Parse(JsonUtility.ToJson(listComponentRuntime, true));

            //save: detail component data
            JObject jsonDetails = new JObject();
            foreach (var runtimeKvp in listComponentRuntime.RuntimeDict)
            {
                jsonDetails[runtimeKvp.Key.ToString()] = JObject.Parse(runtimeKvp.Value.Save());
            }

            JObject result = new JObject()
            {
                [DATA_JSON_NAME] = jsonData,
                [DETAILS_DATA_JSON_NAME] = jsonDetails
            };

            return result.ToString();
        }

        public void Load(string jsonData)
        {
            JObject data = JObject.Parse(jsonData);

            //load: load component arrange
            if (!data.TryGetValue(DATA_JSON_NAME, out var dataBlock))
            {
                Logger.Log("Json data format is invalid - missing [data] block", LogLevel.Error, "Runtime");
                return;
            }

            JsonUtility.FromJsonOverwrite(dataBlock.ToString(), listComponentRuntime);

            //load: load detail data.
            if (!data.TryGetValue(DETAILS_DATA_JSON_NAME, out var dataDetailsBlock))
            {
                Logger.Log("Json data format is invalid - missing [details] block", LogLevel.Error, "Runtime");
                return;
            }

            JObject dataDetailsBlockJObject = JObject.Parse(dataDetailsBlock.ToString());
            var runtimeDict = listComponentRuntime.RuntimeDict;
            foreach (var detailProp in dataDetailsBlockJObject.Properties())
            {
                if (runtimeDict.TryGetValue(Guid.Parse(detailProp.Name), out var runtimeKvp))
                {
                    runtimeKvp.Load(detailProp.Value.ToString());
                }
            }
        }
    }
}