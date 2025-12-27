using System;

namespace TnieYuPackage.Utils.DictionaryUtil
{
    [Serializable]
    public class SerializableDictionary<TKey, TValue> :
        BaseSerializableDictionary<SerializableKeyPair<TKey, TValue>, TKey, TValue>
    {
    }
}