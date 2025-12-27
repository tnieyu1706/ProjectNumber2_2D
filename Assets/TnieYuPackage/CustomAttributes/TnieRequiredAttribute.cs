using System;
using UnityEngine;

namespace TnieYuPackage.CustomAttributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class TnieRequiredAttribute : PropertyAttribute
    {
    }
}