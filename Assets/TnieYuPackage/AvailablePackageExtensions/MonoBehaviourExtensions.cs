using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using TnieYuPackage.DesignPatterns.Patterns.Singleton;
using UnityEngine;

namespace TnieYuPackage.AvailablePackageExtensions
{
    public static class MonoBehaviourExtensions
    {
        public static void Invoke(this MonoBehaviour monoBehaviour, Action action, float delay)
        {
            monoBehaviour.StartCoroutine(InvokeCoroutine(action, delay));
        }
    
        public static IEnumerator InvokeCoroutine(Action action, float delay)
        {
            yield return UniTask.WaitForSeconds(delay);
            
            action?.Invoke();
        }
    }
}