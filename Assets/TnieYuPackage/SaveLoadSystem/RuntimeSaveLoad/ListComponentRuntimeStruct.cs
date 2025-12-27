using System;
using System.Collections.Generic;
using System.Linq;
using TnieCustomPackage.SerializeInterface;
using TnieYuPackage.Utils;
using UnityEngine;

namespace TnieYuPackage.SaveLoadSystem.RuntimeSaveLoad
{
    [Serializable]
    public class ListComponentRuntimeStruct : IListRuntimeStructure
    {
        [SerializeField] private List<InterfaceReference<IRuntimeData, MonoBehaviour>> runtimes = new();

        public List<IRuntimeData> RuntimeList => runtimes.Select(r => r.Value).ToList();

        public Dictionary<SerializableGuid, IRuntimeData> RuntimeDict =>
            runtimes.ToDictionary(r => r.Value.RuntimeId, r => r.Value);
    }
}