using System;
using EditorAttributes;
using UnityEngine;
using UnityEngine.Events;

namespace TnieYuPackage.SOAP.Event
{

    [CreateAssetMenu(fileName = "SoapVoidEvent", menuName = "TnieYuPackage/Soap/Event/Void")]
    public class SoapEventVoidSo : ScriptableObject
    {
        public Action Event;
    }

    public abstract class SoapEventSo<T> : ScriptableObject
    {
        public Action<T> Event;
    }

    public abstract class SoapEventSo<T1, T2> : ScriptableObject
    {
        public Action<T1, T2> Event;
    }
}