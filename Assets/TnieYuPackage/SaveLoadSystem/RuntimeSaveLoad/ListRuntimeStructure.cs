using System;
using System.Collections.Generic;
using System.Linq;
using TnieYuPackage.Utils;
using UnityEngine;

namespace TnieYuPackage.SaveLoadSystem.RuntimeSaveLoad
{
    public interface IListRuntimeStructure
    {
        /// <summary>
        /// Readonly list.
        /// Return List each time calling.
        /// If write it not occur any change.
        /// </summary>
        List<IRuntimeData> RuntimeList { get; }

        /// <summary>
        /// Return Dictionary each time calling.
        /// </summary>
        Dictionary<SerializableGuid, IRuntimeData> RuntimeDict { get; }
    }

    [Serializable]
    public class ListRuntimeStructure : IListRuntimeStructure
    {
        [SerializeReference] public List<BaseRuntimeData> runtimes = new();

        public List<IRuntimeData> RuntimeList =>
            runtimes.Select(r => (IRuntimeData)r).ToList();

        public Dictionary<SerializableGuid, IRuntimeData> RuntimeDict =>
            runtimes.ToDictionary(r => r.RuntimeId, r => (IRuntimeData)r);
    }
}