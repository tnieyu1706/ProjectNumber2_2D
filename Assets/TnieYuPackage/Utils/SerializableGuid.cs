using System;
using UnityEngine;

namespace TnieYuPackage.Utils
{
    [Serializable]
    public struct SerializableGuid : IEquatable<SerializableGuid>
    {
        [SerializeField] private string value;

        public Guid Guid
        {
            get => string.IsNullOrEmpty(value) ? Guid.Empty : Guid.Parse(value);
            set => this.value = value.ToString();
        }

        public static SerializableGuid NewGuid()
        {
            return new SerializableGuid { Guid = Guid.NewGuid() };
        }

        public static SerializableGuid Empty => new SerializableGuid() { Guid = Guid.Empty };

        public override string ToString() => value;

        public override bool Equals(object obj)
        {
            if (obj is SerializableGuid guid)
            {
                return guid.Guid.Equals(Guid);
            }

            return false;
        }

        public bool Equals(SerializableGuid other)
        {
            return value.Equals(other.value);
        }

        public override int GetHashCode()
        {
            return (value != null ? value.GetHashCode() : 0);
        }

        public static bool operator ==(SerializableGuid left, SerializableGuid right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(SerializableGuid left, SerializableGuid right)
        {
            return !(left.Equals(right));
        }

        public static implicit operator Guid(SerializableGuid serializableGuid) => serializableGuid.Guid;
        public static implicit operator SerializableGuid(Guid guid) => new SerializableGuid { Guid = guid };
    }
}