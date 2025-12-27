namespace FewClicksDev.ColorTextureGenerator
{
    using FewClicksDev.Core;
    using System.Collections.Generic;
    using UnityEngine;

    public enum ExportFormat
    {
        PNG = 0,
        TGA = 1,
        JPG = 2
    }

    public enum PixelsOrientation
    {
        Horizontal = 0,
        Vertical = 1,
        ReversedHorizontal = 2,
        ReversedVertical = 3
    }

    public static class ColorTextureGenerator
    {
        public const string NAME = "Color Texture Generator";
        public const string CAPS_NAME = "COLOR TEXTURE GENERATOR";
        public const string VERSION = "1.2.1";
        public const string REVIEW_URL = "https://assetstore.unity.com/packages/tools/painting/color-texture-generator-281099#reviews";
        public const string DOCUMENTATION_URL = "https://docs.google.com/document/d/1oQCWgvJgchAZtqUMHL0Hs6qGRhO96mrBfsd3dp2XO1c/edit?usp=sharing";

        public static readonly Color MAIN_COLOR = new Color(0.3945085f, 0.2938768f, 0.490566f, 1f);
        public static readonly Color LOGS_COLOR = new Color(0.6291776f, 0.4522962f, 0.7924528f, 1f);

        private const int MAX_PREVIEW_TEXTURE_HEIGHT = 96;
        private const int MAX_PREVIEW_TEXTURE_WIDTH = 256;
        private const int COLORS_PRECISION = 50;

        public static void Error(string _message)
        {
            BaseLogger.Error(CAPS_NAME, _message, LOGS_COLOR);
        }

        public static (int, int) GetPreviewTextureSize(int _width, int _height)
        {
            int _biggerValue = Mathf.Max(_width, _height);
            float _multiplier = _width > _height ? (float) MAX_PREVIEW_TEXTURE_WIDTH / _biggerValue : (float) MAX_PREVIEW_TEXTURE_HEIGHT / _biggerValue;

            int _newWidth = Mathf.RoundToInt(_width * _multiplier);
            int _newHeight = Mathf.RoundToInt(_height * _multiplier);

            return (_newWidth, _newHeight);
        }

        /// <summary>
        /// Returns a Texture2D with the gradient applied
        /// </summary>
        /// <param name="_gradient">Gradient</param>
        /// <param name="_width">Width of the texture in pixels</param>
        /// <param name="_height">Height of the texture in pixels</param>
        public static Texture2D CreateTextureFromGradient(Gradient _gradient, PixelsOrientation _orientation, int _width, int _height)
        {
            Texture2D _gradientTex = new Texture2D(_width, _height, TextureFormat.ARGB32, false);
            _gradientTex.filterMode = FilterMode.Bilinear;

            switch (_orientation)
            {
                case PixelsOrientation.Horizontal:
                case PixelsOrientation.ReversedHorizontal:

                    float _oneByWidth = 1f / _width;

                    for (int x = 0; x < _width; x++)
                    {
                        float _t = _orientation is PixelsOrientation.Horizontal ? x * _oneByWidth : 1f - (x * _oneByWidth);
                        Color _col = _gradient.Evaluate(_t);

                        for (int y = 0; y < _height; y++)
                        {
                            _gradientTex.SetPixel(x, y, _col);
                        }
                    }

                    break;

                case PixelsOrientation.Vertical:
                case PixelsOrientation.ReversedVertical:

                    float _oneByHeight = 1f / _height;

                    for (int y = 0; y < _height; y++)
                    {
                        float _t = _orientation is PixelsOrientation.Vertical ? 1f - (y * _oneByHeight) : y * _oneByHeight;
                        Color _col = _gradient.Evaluate(_t);

                        for (int x = 0; x < _width; x++)
                        {
                            _gradientTex.SetPixel(x, y, _col);
                        }
                    }

                    break;
            }

            _gradientTex.Apply();

            return _gradientTex;
        }

        /// <summary>
        /// Returns a Texture2D with the color ramp applied
        /// </summary>
        /// <param name="_colorRamp">Color ramp</param>
        /// <param name="_width">Width of the texture in pixels</param>
        /// <param name="_height">Height of the texture in pixels</param>
        public static Texture2D CreateTextureFromColorRamp(ColorRamp _colorRamp, PixelsOrientation _orientation, int _width, int _height)
        {
            Texture2D _colorRampTex = new Texture2D(_width, _height, TextureFormat.ARGB32, false);
            _colorRampTex.filterMode = FilterMode.Point;

            List<Color> _colors = new List<Color>();

            for (int i = 0; i < _colorRamp.Colors.Count; i++)
            {
                int _count = (int) (_colorRamp.Colors[i].Coverage * COLORS_PRECISION);
                Color _currentColor = _colorRamp.Colors[i].RampColor;

                for (int j = 0; j < _count; j++)
                {
                    _colors.Add(_currentColor);
                }

                if (_colorRamp.Colors[i].UseLerpToNextColor)
                {
                    int _lerpCount = (int) (_colorRamp.Colors[i].LerpCoverage * COLORS_PRECISION);
                    Color _nextColor = _colorRamp.Colors[(i + 1)].RampColor;

                    for (int j = 0; j < _lerpCount; j++)
                    {
                        float _t = (float) j / _lerpCount;
                        Color _lerpedColor = Color.Lerp(_currentColor, _nextColor, _t);

                        _colors.Add(_lerpedColor);
                    }
                }
            }

            switch (_orientation)
            {
                case PixelsOrientation.Horizontal:
                case PixelsOrientation.ReversedHorizontal:

                    float _oneByWidth = 1f / _width;

                    try
                    {
                        for (int x = 0; x < _width; x++)
                        {
                            float _t = _orientation is PixelsOrientation.Horizontal ? x * _oneByWidth : 1f - (x * _oneByWidth);
                            int _index = (int) (_t * (_colors.Count - 1));
                            Color _col = _colors[_index];

                            for (int y = 0; y < _height; y++)
                            {
                                _colorRampTex.SetPixel(x, y, _col);
                            }
                        }
                    }
                    catch (System.Exception e)
                    {
                        Debug.Log(e);
                    }

                    break;

                case PixelsOrientation.Vertical:
                case PixelsOrientation.ReversedVertical:

                    float _oneByHeight = 1f / _height;

                    for (int y = 0; y < _height; y++)
                    {
                        float _t = _orientation is PixelsOrientation.Vertical ? 1f - (y * _oneByHeight) : y * _oneByHeight;
                        int _index = (int) (_t * (_colors.Count - 1));
                        Color _col = _colors[_index];

                        for (int x = 0; x < _width; x++)
                        {
                            _colorRampTex.SetPixel(x, y, _col);
                        }
                    }

                    break;
            }

            _colorRampTex.Apply();

            return _colorRampTex;
        }
    }
}
