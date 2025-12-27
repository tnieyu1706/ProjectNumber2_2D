using System;

namespace TnieYuPackage.Utils.DictionaryUtil
{
    [Serializable]
    public class SerializableDictionaryAbstract<TKey, TValue> :
        BaseSerializableDictionary<SerializableKeyPairAbstract<TKey, TValue>, TKey, TValue>
    {
    }
}