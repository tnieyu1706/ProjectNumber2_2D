using System;
using System.Collections.Generic;
using System.Reflection;

namespace TnieYuPackage.DesignPatterns.Patterns.Singleton
{
    [AttributeUsage(AttributeTargets.Class)]
    public class SingletonFactoryAttribute : Attribute
    {
        
    }    
    
    public static class SingletonFactory
    {
        private static Dictionary<Type, object> instances = new Dictionary<Type, object>();

        public static T GetInstance<T>()
            where T : class, new()
        {
            object obj;
            if (!instances.TryGetValue(typeof(T), out obj))
            {
                var attr = typeof(T).GetCustomAttribute<SingletonFactoryAttribute>();

                if (attr != null)
                {
                    obj = new T();
                    instances.Add(typeof(T), obj);
                }
                else
                {
                    throw new InvalidOperationException($"{typeof(T).Name} is not marked as [SingletonFactory]");
                }
            }

            return (T)obj;
        }
    }
}

