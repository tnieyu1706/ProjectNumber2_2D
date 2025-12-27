using System;
using System.Reflection;

namespace TnieYuPackage.Utils.DictionaryUtil
{
    [Serializable]
    public abstract class BaseSerializableKeyPair<TKey, TValue>
    {
        public TKey key;

        public const string VALUE_FIELD_NAME = "value";

        public abstract TValue Value { get; set; }

        private FieldInfo ReflectValueField()
        {
            return this.GetType().GetField(
                VALUE_FIELD_NAME,
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic);
        }

        protected BaseSerializableKeyPair()
        {
            
        }

        protected BaseSerializableKeyPair(TKey key)
        {
            this.key = key;
        }
    }
    
    [Serializable]
    public class SerializableKeyPair<TKey, TValue> : BaseSerializableKeyPair<TKey, TValue>
    {
        public TValue value;

        public override TValue Value
        {
            get => value;
            set => this.value = value;
        }

        public SerializableKeyPair() { }

        public SerializableKeyPair(TKey key, TValue value) : base(key)
        {
            this.value = value;
        }
    }
}