using UnityEngine;

namespace TnieYuPackage.CustomAttributes
{
    /// <summary>
    /// Apply to dummy field to Reserialize property.
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Field)]
    public class TnieShowPropertyAttribute : PropertyAttribute
    {
        public string PropertyName { get; }
        public TnieShowPropertyAttribute(string propertyName)
        {
            PropertyName = propertyName;
        }
    }
}