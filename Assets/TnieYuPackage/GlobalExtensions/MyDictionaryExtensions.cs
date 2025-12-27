using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TnieYuPackage.GlobalExtensions
{
    public static class MyDictionaryExtensions
    {
        public static void Resize<T, TValue>(this Dictionary<T, TValue> dict, T defaultKey, int newSize)
        {
            if (newSize < 0)
            {
                Debug.LogError($"New size cannot be less than 0");
                return;
            }

            int resizedCount = dict.Count - newSize;
            while (resizedCount != 0)
            {
                if (resizedCount > 0)
                {
                    dict.Remove(dict.Keys.Last());
                    resizedCount--;
                }
                else
                {
                    dict.Add(defaultKey, default(TValue));
                    resizedCount++;
                }
            }
        }
    }
}

