namespace FewClicksDev.ColorTextureGenerator
{
    using System.Collections.Generic;
    using UnityEngine;

    [System.Serializable]
    public class ColorRamp
    {
        private const int MAX_COLORS = 32;

        [SerializeField] private List<ColorWithCoverage> colors = new List<ColorWithCoverage>();

        public List<ColorWithCoverage> Colors => colors;

        public int NumberOfColors => colors.Count;
        public bool CanAddColors => colors.Count < MAX_COLORS;

        public Texture2D GeneratedTexture { get; private set; } = null;

        public ColorRamp()
        {
            ResetList();
        }

        public void AddColor(Color _color, float _coverage)
        {
            colors.Add(new ColorWithCoverage(_color, _coverage));
        }

        public void AddColor(ColorWithCoverage _color)
        {
            colors.Add(_color);
        }

        public void ResetList()
        {
            colors.Clear();
            colors.Add(new ColorWithCoverage(Color.white, 1f));
            colors.Add(new ColorWithCoverage(ColorTextureGenerator.MAIN_COLOR, 1f));
        }

        public void ResetCoverage()
        {
            foreach (var _color in colors)
            {
                _color.ResetValues();
            }
        }

        public void RegenerateTexture(PixelsOrientation _orientation, int _width, int _height)
        {
            var _previewTextureSize = ColorTextureGenerator.GetPreviewTextureSize(_width, _height);
            GeneratedTexture = ColorTextureGenerator.CreateTextureFromColorRamp(this, _orientation, _previewTextureSize.Item1, _previewTextureSize.Item2);
        }
    }
}
