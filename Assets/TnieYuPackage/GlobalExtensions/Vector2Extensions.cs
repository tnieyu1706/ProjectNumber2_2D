using UnityEngine;

namespace TnieYuPackage.GlobalExtensions
{
    public static class Vector2Extensions
    {
        public static Vector2 With(this Vector2 vector, float? x = null, float? y = null)
        {
            return new Vector2(x ?? vector.x, y ?? vector.y);
        }

        public static Vector2 Add(this Vector2 vector, float? x = null, float? y = null)
        {
            return new Vector2(vector.x + (x ?? 0), vector.y + (y ?? 0));
        }
    
        public static Vector2 ConvertPixelToWorld(this Vector2 uvPixel, int width, int height)
        {
            float u = uvPixel.x / width - 0.5f;
            float v = uvPixel.y / height - 0.5f;
            return new Vector2(u, v);
        }

        public static Vector2 ApplyScale(this Vector2 vector, Vector2 scale)
        {
            float x = vector.x * scale.x;
            float y = vector.y * scale.y;
            return new Vector2(x, y);
        }

        public static Vector2 ApplyPosition(this Vector2 vector, Vector2 position)
        {
            float x = vector.x + position.x;
            float y = vector.y + position.y;
            return new Vector2(x, y);
        }

        public static float GetRandomNumber(this Vector2 vector)
        {
            return UnityEngine.Random.Range(vector.x, vector.y);
        }
    }
}