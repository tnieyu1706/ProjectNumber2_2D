namespace FewClicksDev.ColorTextureGenerator
{
    using FewClicksDev.Core;
    using UnityEngine;

    public abstract class ColorTextureObject : ScriptableObject
    {
        [SerializeField] protected int width = 128;
        [SerializeField] protected int height = 32;
        [SerializeField] protected PixelsOrientation pixelsOrientation = PixelsOrientation.Horizontal;
        [SerializeField] protected Texture2D textureToOverride = null;

        public int Width => width;
        public int Height => height;
        public PixelsOrientation OrientationOfPixels => pixelsOrientation;
        public Texture2D TextureToOverride => textureToOverride;

        public abstract void OpenInWindow();

        protected void assignVariables(int _width, int _height, PixelsOrientation _pixelsOrientation, Texture2D _texture)
        {
            width = _width;
            height = _height;
            pixelsOrientation = _pixelsOrientation;

            if (_texture != null)
            {
                textureToOverride = _texture;
            }

            this.SetAsDirty();
        }
    }
}
