using UnityEngine;

namespace TnieYuPackage.GlobalExtensions
{
    public static class Vector3Extensions
    {
        public static Vector3 With(this Vector3 vector, float? x = null, float? y = null, float? z = null)
        {
            return new Vector3(x ?? vector.x, y ?? vector.y, z ?? vector.z);
        }

        public static Vector3 Add(this Vector3 vector, float? x = null, float? y = null, float? z = null)
        {
            return new Vector3(vector.x + (x ?? 0), vector.y + (y ?? 0), vector.z + (z ?? 0));
        }

        public static Vector2 ConvertToVector2(this Vector3 vector)
        {
            return new Vector2(vector.x, vector.z);
        }

        public static (float x, float y, float z) ConvertToUnit(this Vector3 vector)
        {
            return (vector.x, vector.y, vector.z);
        }
    }
}