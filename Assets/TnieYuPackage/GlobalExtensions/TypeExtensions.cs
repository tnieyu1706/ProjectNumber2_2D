using System;

namespace TnieYuPackage.GlobalExtensions
{
    public static class TypeExtensions
    {
        public static string GetShortAssemblyName(this Type type)
        {
            string fullName = type.FullName;
            string assemblyName = type.Assembly.GetName().Name;
            
            return $"{fullName}, {assemblyName}";
        }
    }
}