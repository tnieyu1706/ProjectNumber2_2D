using System;
using EditorAttributes;
using Newtonsoft.Json.Linq;
using TnieYuPackage.Utils;
using UnityEngine;

namespace TnieYuPackage.SaveLoadSystem.RuntimeSaveLoad
{
    [Serializable]
    public class ListRuntimeData : IRuntimeData
    {
        [SerializeField, ReadOnly] private SerializableGuid runtimeId = SerializableGuid.NewGuid();

        public SerializableGuid RuntimeId
        {
            get => runtimeId;
            set => runtimeId = value;
        }

        [SerializeField] private bool isOverrideExisting = true;

        public ListRuntimeStructure listRuntime = new();

        public string Save()
        {
            JObject result = new JObject();
            foreach (var runtimeKvp in listRuntime.RuntimeDict)
            {
                result[runtimeKvp.Key.ToString()] = JObject.Parse(runtimeKvp.Value.Save());
            }

            return result.ToString();
        }

        public void Load(string jsonData)
        {
            JObject data = JObject.Parse(jsonData);

            if (!isOverrideExisting)
            {
                foreach (var runtimeKvp in listRuntime.RuntimeDict)
                {
                    if (data.TryGetValue(runtimeKvp.Key.Guid.ToString(), out var runtimeData))
                    {
                        runtimeKvp.Value.Load(runtimeData.ToString());
                    }
                }
            }
            else
            {
                listRuntime.runtimes.Clear();
                BaseRuntimeData runtimeDataTemp;
                foreach (var detailDataProp in data.Properties())
                {
                    runtimeDataTemp = BaseRuntimeData.Deserialize(detailDataProp.Value.ToString());

                    if (runtimeDataTemp != null)
                    {
                        listRuntime.runtimes.Add(runtimeDataTemp);
                    }
                }
            }
        }
        
    }
    
}