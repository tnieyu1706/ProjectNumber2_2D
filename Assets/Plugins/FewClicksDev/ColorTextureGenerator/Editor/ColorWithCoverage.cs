namespace FewClicksDev.ColorTextureGenerator
{
    using UnityEngine;

    [System.Serializable]
    public class ColorWithCoverage
    {
        private const float MIN_COVERAGE = 0.1f;
        private const float MAX_COVERAGE = 10f;
        private const float DEFAULT_COVERAGE = 1f;
        private const float DEFAULT_LERP_COVERAGE = 1f;

        [SerializeField] private Color rampColor = Color.white;
        [SerializeField] private float coverage = DEFAULT_COVERAGE;
        [SerializeField] private bool useLerpToNextColor = false;
        [SerializeField] private float lerpCoverage = DEFAULT_LERP_COVERAGE;

        public Color RampColor
        {
            get => rampColor;
            set => rampColor = value;
        }

        public float Coverage
        {
            get => coverage;
            set => coverage = Mathf.Clamp(value, MIN_COVERAGE, MAX_COVERAGE);
        }

        public bool UseLerpToNextColor
        {
            get => useLerpToNextColor;
            set => useLerpToNextColor = value;
        }

        public float LerpCoverage
        {
            get => lerpCoverage;
            set => lerpCoverage = Mathf.Clamp(value, MIN_COVERAGE, MAX_COVERAGE);
        }

        public ColorWithCoverage(Color _color, float _coverage)
        {
            RampColor = _color;
            Coverage = _coverage;
        }

        public ColorWithCoverage(ColorWithCoverage _base)
        {
            RampColor = _base.RampColor;
            Coverage = _base.Coverage;
            UseLerpToNextColor = _base.UseLerpToNextColor;
            LerpCoverage = _base.LerpCoverage;
        }

        public void ResetValues()
        {
            Coverage = DEFAULT_COVERAGE;
            UseLerpToNextColor = false;
            LerpCoverage = DEFAULT_LERP_COVERAGE;
        }
    }
}
