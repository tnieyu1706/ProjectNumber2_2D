using System;
using EditorAttributes;
using TnieYuPackage.CustomAttributes;
using UnityEngine;

namespace TnieYuPackage.SOAP.Data
{
    public interface ISoapData<T>
    {
        public T Value { get; set; }
        public Action<T> OnValueChange { get; set; }
    }


    [Serializable]
    public struct SoapData<T> : ISoapData<T>
    {
        [SerializeField] private T value;

        public T Value
        {
            get => value;
            set
            {
                this.value = value;
                OnValueChange?.Invoke(value);
            }
        }

        public Action<T> OnValueChange { get; set; }
    }

    public abstract class SoapDataSo<TData, T> : ScriptableObject
        where TData : struct, ISoapData<T>
    {
        public TData data;

        #region ResetOnPlay

        [SerializeField] private bool resetOnPlay = false;

        [SerializeField] [ShowField(nameof(resetOnPlay))]
        private T defaultValue;

        protected virtual void OnDisable()
        {
            if (resetOnPlay)
                data.Value = defaultValue;
        }

        #endregion
    }
}