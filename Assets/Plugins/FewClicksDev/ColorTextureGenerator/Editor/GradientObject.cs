namespace FewClicksDev.ColorTextureGenerator
{
    using UnityEngine;

    [CreateAssetMenu(menuName = "FewClicks Dev/Color Texture Generator/Gradient", fileName = "gradient_NewGradient")]
    public class GradientObject : ColorTextureObject
    {
        [SerializeField] private Gradient gradient = new Gradient();

        public override void OpenInWindow()
        {
            ColorTextureGeneratorWindow.OpenWithGradient(this);
        }

        public void SetGradient(Gradient _gradient, int _width, int _height, PixelsOrientation _pixelsOrientation, Texture2D _textureToOverride = null)
        {
            gradient = new Gradient();
            gradient.SetKeys(_gradient.colorKeys, _gradient.alphaKeys);

            assignVariables(_width, _height, _pixelsOrientation, _textureToOverride);
        }

        public Gradient GetGradientCopy()
        {
            var _gradient = new Gradient();
            _gradient.SetKeys(gradient.colorKeys, gradient.alphaKeys);

            return _gradient;
        }
    }
}
