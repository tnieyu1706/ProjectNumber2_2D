using UnityEngine;

namespace TnieYuPackage.DesignPatterns.Patterns.UnityTechnique
{
    /// <summary>
    /// ScriptableObject - CacheData.
    /// Contain 1 class Serializable to config.
    /// Data - Can Runtime Generate.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class CachedEntityScriptable<T> : ScriptableObject
    {
        public T entity;
    }
}