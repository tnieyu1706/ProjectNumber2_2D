namespace TnieYuPackage.GlobalExtensions
{
    public static class LayerMaskExtensions
    {
        public static bool ContainLayer(this int layerMask, int layer)
        {
            return (layerMask & (1 << layer)) != 0;
        }
    }
}