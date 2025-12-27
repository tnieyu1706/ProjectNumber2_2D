using TnieYuPackage.CustomAttributes;
using UnityEngine;

namespace TnieYuPackage.Utils.DictionaryUtil
{
    [System.Serializable]
    public class SerializableKeyPairAbstract<TKey, TValue> : BaseSerializableKeyPair<TKey, TValue>
    {
        [SerializeReference]
        [AbstractSupport]
        public TValue value;

        public override TValue Value
        {
            get => value;
            set => this.value = value;
        }

        public SerializableKeyPairAbstract() { }

        public SerializableKeyPairAbstract(TKey key, TValue value) : base(key)
        {
            this.value = value;
        }
    }
}