using System;
using UnityEngine;

namespace TnieYuPackage.CustomAttributes
{
    /// <summary>
    /// Apply for class not inherit Unity.Object to show actual type
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class ShowRefNameAttribute : PropertyAttribute
    {
        
    }
}