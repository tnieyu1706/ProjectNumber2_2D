namespace FewClicksDev.ColorTextureGenerator
{
    using UnityEngine;

    [CreateAssetMenu(menuName = "FewClicks Dev/Color Texture Generator/Color Ramp", fileName = "colorRamp_NewColorRamp")]
    public class ColorRampObject : ColorTextureObject
    {
        [SerializeField] private ColorRamp colorRamp = new ColorRamp();

        public ColorRamp Color => colorRamp;

        public override void OpenInWindow()
        {
            ColorTextureGeneratorWindow.OpenWithColorRamp(this);
        }

        public void SetColorRamp(ColorRamp _colorRamp, int _width, int _height, PixelsOrientation _pixelsOrientation, Texture2D _textureToOverride = null)
        {
            var _ramp = new ColorRamp();
            _ramp.Colors.Clear();

            foreach (var _color in _colorRamp.Colors)
            {
                _ramp.AddColor(new ColorWithCoverage(_color));
            }

            colorRamp = _ramp;
            assignVariables(_width, _height, _pixelsOrientation, _textureToOverride);
        }

        public ColorRamp GetColorRampCopy()
        {
            var _colorRamp = new ColorRamp();
            _colorRamp.Colors.Clear();

            foreach (var _color in colorRamp.Colors)
            {
                _colorRamp.AddColor(new ColorWithCoverage(_color));
            }

            return _colorRamp;
        }
    }
}
