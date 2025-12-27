using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TnieYuPackage.Utils.DictionaryUtil
{
    [Serializable]
    public abstract class BaseSerializableDictionary<TKeyPair, TKey, TValue> : ISerializationCallbackReceiver
        where TKeyPair : BaseSerializableKeyPair<TKey, TValue>, new()
    {
        /// <summary>
        /// data is the real data
        /// </summary>
        public List<TKeyPair> data = new();
        private Dictionary<TKey, TValue> dictionary;

        /// <summary>
        /// Get Dictionary first time in runtime to setup Dictionary
        /// </summary>
        public Dictionary<TKey, TValue> Dictionary
        {
            get
            {
                if (dictionary == null)
                {
                    RebuildDictionary();
                }

                return dictionary;
            }
        }
        
        public TValue this[TKey key]
        {
            get => Dictionary[key];
            set => Dictionary[key] = value;
        }

        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
            dictionary = null; // rebuild khi cần
        }

        /// <summary>
        /// Rebuild Dictionary to reload data form data list.
        /// </summary>
        public void RebuildDictionary()
        {
            data ??= new();

            dictionary = new Dictionary<TKey, TValue>();

            foreach (var kvp in data)
            {
                if (kvp == null || kvp.key == null)
                    continue;

                dictionary[kvp.key] = kvp.Value;
            }
        }

        public void AddOrUpdate(TKey key, TValue value)
        {
            if (key == null)
            {
                return;
            } 
            
            foreach (var kvp in data)
            {
                if (kvp.key.Equals(key))
                {
                    kvp.Value = value;
                    return;
                }
            }

            var keyPair = new TKeyPair { key = key, Value = value };
            data.Add(keyPair);
        }

        public void AddRange(List<TValue> values, Func<TValue, TKey> keySelector)
        {
            foreach (var value in values)
            {
                AddOrUpdate(keySelector(value), value);
            }
        }

        public void AddRange(Dictionary<TKey, TValue> dictValues)
        {
            foreach (var kvp in dictValues)
            {
                AddOrUpdate(kvp.Key, kvp.Value);
            }
        }

        public void AddRange(BaseSerializableDictionary<TKeyPair, TKey, TValue> serializableDictionary)
        {
            foreach (var kvp in serializableDictionary.data)
            {
                AddOrUpdate(kvp.key, kvp.Value);
            }
        }
    }
}