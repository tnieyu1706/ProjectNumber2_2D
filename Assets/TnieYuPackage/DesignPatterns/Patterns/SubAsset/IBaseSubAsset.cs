using UnityEngine;
using UnityEngine.Serialization;

namespace TnieYuPackage.DesignPatterns.Patterns.SubAsset
{
    public abstract class BaseSubAsset : ScriptableObject
    {
        public abstract BaseParentAsset Parent { get; set; }
    }
}