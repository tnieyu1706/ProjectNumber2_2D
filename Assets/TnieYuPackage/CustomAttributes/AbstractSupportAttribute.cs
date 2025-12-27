using System;
using UnityEngine;

namespace TnieYuPackage.CustomAttributes
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class AbstractSupportAttribute : PropertyAttribute
    {
        public Type AbstractType { get; }

        public AbstractSupportAttribute(Type abstractType = null)
        {
            AbstractType = abstractType;
        }
    }
}