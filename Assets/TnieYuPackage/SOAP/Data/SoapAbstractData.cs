using System;
using EditorAttributes;
using TnieYuPackage.CustomAttributes;
using UnityEngine;

namespace TnieYuPackage.SOAP.Data
{
    [Serializable]
    public struct SoapAbstractData<T> : ISoapData<T>
    {
        [SerializeReference, AbstractSupport()]
        private T value;

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

    public abstract class SoapAbstractDataSo<TData, T> : ScriptableObject
        where TData : struct, ISoapData<T>
    {
        public TData data;

        #region ResetOnPlay

        [SerializeField] private bool resetOnPlay = false;

        [ShowField(nameof(resetOnPlay))] [SerializeReference, AbstractSupport()] 
        private T defaultValue;

        protected virtual void OnDisable()
        {
            if (resetOnPlay)
            {
                data.Value = defaultValue;
            }
        }

        #endregion
    }
}