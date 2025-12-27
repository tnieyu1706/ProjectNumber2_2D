using UnityEngine;

namespace TnieYuPackage.DesignPatterns.Patterns.Singleton
{
    public class SingletonBehavior<T> : MonoBehaviour
        where T : Component
    {
        private static T instance;
        
        private static bool isQuitting = false;
        
        /// <summary>
        /// Noted when SingletonBehavior in Disable.
        /// When game Stop/Close, it can stop Singleton before disable call
        /// so Instance will be null.
        /// </summary>
        public static T Instance
        {
            get
            {
                if (isQuitting) return null;
                
                if (instance == null)
                {
                    instance = Object.FindFirstObjectByType<T>();

                    if (instance == null)
                    {
                        GameObject go = new GameObject(typeof(T).Name + " (Singleton)");
                        instance = go.AddComponent<T>();
                    }
                }

                return instance;
            }
        }

        protected virtual void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this as T;
            
            gameObject.SetActive(true);
            DontDestroyOnLoad(gameObject);
        }

        protected virtual void OnApplicationQuit()
        {
            isQuitting = true;
        }
    }
}