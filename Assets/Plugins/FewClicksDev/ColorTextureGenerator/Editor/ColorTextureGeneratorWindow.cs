namespace FewClicksDev.ColorTextureGenerator
{
    using FewClicksDev.Core;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using UnityEditor;
    using UnityEngine;

    using static FewClicksDev.Core.EditorDrawer;

    public class ColorTextureGeneratorWindow : CustomEditorWindow
    {
        public enum WindowMode
        {
            Gradient = 0,
            ColorRamp = 1,
            Objects = 2
        }

        public enum SettingsExportMode
        {
            ExportToFile = 0,
            OverrideExisting = 1
        }

        public enum TextureExportMode
        {
            CreateNew = 0,
            OverrideExisting = 1
        }

        public enum ColorObjectsSortMode
        {
            Default = 0,
            Name = 1,
            CreationDate = 2
        }

        private const float LABEL_WIDTH = 160f;
        private const float INDEX_WIDTH = 30f;
        private const float DELETE_COLOR_RAMP_WIDTH = 25f;
        private const float COVERAGE_WIDTH = 50f;
        private const float TOGGLE_WIDTH = 15f;
        private const float TOOLBAR_WIDTH = 0.75f;
        private const float CREATE_BUTTON_WIDTH = 0.45f;
        private const float EXPORT_BUTTON_HEIGHT = 26f;
        private const int MIN_TEXTURE_SIZE = 16;
        private const int MAX_VISIBLE_ROWS = 12;
        private const float OBJECT_FIELD_WIDTH = 120f;
        private const float COPY_BUTTON_WIDTH = 84f;

        private const string WIDTH_LABEL = "Width";
        private const string HEIGHT_LABEL = "Height";
        private const string INDEX_LABEL = "Index";
        private const string COLOR_LABEL = "Color";
        private const string COVERAGE_LABEL = "Coverage";
        private const string COLOR_LERP_LABEL = "Color lerp";
        private const string ASSET = "asset";
        private const string DOT = ".";
        private const string GRADIENT_LABEL = "Gradient";
        private const string RESET_LIST_LABEL = "Reset list";
        private const string RESET_COVERAGE_LABEL = "Reset coverage";
        private const string EXPORT_FORMAT = "Export format";
        private const string EXPORT_MODE = "Export mode";
        private const string DEFAULT_GRADIENT_NAME = "New Gradient";
        private const string DEFAULT_COLOR_RAMP_NAME = "New Color Ramp";
        private const string SETTINGS_EXPORT_MODE = "Settings export mode";
        private const string PIXELS_ORIENTATION_LABEL = "Pixels orientation";
        private const string SAVE_GRADIENT_AS_TEXTURE = "Save gradient as texture";
        private const string SAVE_COLOR_RAMP_AS_TEXTURE = "Save color ramp as texture";
        private const string GRADIENT_TO_OVERRIDE = "Gradient to override";
        private const string COLOR_RAMP_TO_OVERRIDE = "Color ramp to override";
        private const string TEXTURE_TO_OVERRIDE = "Texture to override";
        private const string DELETE = " X ";
        private const string PLUS = "+";
        private const string TEXTURE_TO_OVERRIDE_CANNOT_BE_EMPTY = "Texture to override cannot be empty.";
        private const string FILE_PATH_ERROR = "The generated texture must be saved within the current project assets folder.";
        private const string SCRIPTABLE_PATH_ERROR = "The scriptable object must be saved within the current project assets folder!";
        private const string EXPORT_SETTINGS_INTO_SCRIPTABLE = "Export settings into the scriptable object";
        private const string SAVE_GRADIENT_INTO_SCRIPTABLE = "Save gradient into the scriptable object";
        private const string SAVE_COLOR_RAMP_INTO_SCRIPTABLE = "Save color ramp into the scriptable object";
        private const string OVERRIDE_GRADIENT_IN_ATTACHED_FILE = "Override gradient in the attached file";
        private const string OVERRIDE_COLOR_RAMP_IN_ATTACHED_FILE = "Override color ramp in the attached file";
        private const string NO_OBJECTS_INFO = "There are no color texture objects that match all your filters!";
        private const string REFRESH_OBJECTS = "Refresh objects";
        private const string FILTER_BY_LABEL = "Filter by label";
        private const string SORT_BY = "Sort by";
        private const string NAME = "Name";
        private const string REFERENCE = "Reference";
        private const string CONFIRM_DELETION_QUESTION = "Are you sure you want to delete this object?";
        private const string COPY_HEIGHT = "Copy height";
        private const string COPY_WIDTH = "Copy width";

        private const string SAVE_AS_TEXTURE = "Save as texture";
        private const string OVERRIDE_TEXTURE = "Override texture";
        private const string PNG_EXTENSION = ".png";
        private const string TGA_EXTENSION = ".tga";
        private const string JPG_EXTENSION = ".jpg";

        protected override string windowName => ColorTextureGenerator.NAME;
        protected override string version => ColorTextureGenerator.VERSION;
        protected override Vector2 minWindowSize => new Vector2(480f, 780f);
        protected override Color mainColor => ColorTextureGenerator.MAIN_COLOR;

        protected override bool askForReview => true;
        protected override string reviewURL => ColorTextureGenerator.REVIEW_URL;
        protected override bool hasDocumentation => true;
        protected override string documentationURL => ColorTextureGenerator.DOCUMENTATION_URL;

        private WindowMode windowMode = WindowMode.Gradient;
        private SettingsExportMode settingsExportMode = SettingsExportMode.ExportToFile;
        private ExportFormat exportFormat = ExportFormat.PNG;
        private TextureExportMode exportMode = TextureExportMode.CreateNew;

        private Gradient gradient = new Gradient();
        private ColorRamp colorRamp = new ColorRamp();

        private bool canGenerateTexture = false;
        private Texture2D generatedGradientTexture = null;
        private int textureWidth = 128;
        private int textureHeight = 16;
        private PixelsOrientation pixelsOrientation = PixelsOrientation.Horizontal;

        private Texture2D gradientTextureToOverride = null;
        private Texture2D colorRampTextureToOverride = null;
        private GradientObject gradientObject = null;
        private ColorRampObject colorRampObject = null;
        private Vector2 colorRampScrollPosition = Vector2.zero;

        private string foundObjectLabelFilter = string.Empty;
        private ColorObjectsSortMode colorObjectsSortMode = ColorObjectsSortMode.Default;
        private bool objectsSortOrder = true;
        private Vector2 foundObjectsScrollPosition = Vector2.zero;
        private ColorTextureObject[] allColorTextureObjects = null;
        private List<ColorTextureObject> sortedAndFilteredColorObjects = new List<ColorTextureObject>();

        protected override void OnEnable()
        {
            base.OnEnable();

            regenerateTextures();
            findAndSortColorTextureObjects();
        }

        protected override void drawWindowGUI()
        {
            NormalSpace();
            var _windowMode = this.DrawEnumToolbar(windowMode, TOOLBAR_WIDTH, mainColor);

            if (_windowMode != windowMode)
            {
                windowMode = _windowMode;
                regenerateTextures();
            }

            SmallSpace();
            DrawLine();
            SmallSpace();

            switch (windowMode)
            {
                case WindowMode.Gradient:
                    drawGradientTab();
                    break;

                case WindowMode.ColorRamp:
                    drawColorRampTab();
                    break;

                case WindowMode.Objects:
                    drawObjectsTab();
                    break;
            }
        }

        private void drawGradientTab()
        {
            SmallSpace();

            using (var _changeScope = new ChangeCheckScope())
            {
                using (new LabelWidthScope(LABEL_WIDTH))
                {
                    gradient = EditorGUILayout.GradientField(GRADIENT_LABEL, gradient);
                    drawTextureSettings();
                }

                if (_changeScope.changed)
                {
                    var _previewTextureSize = ColorTextureGenerator.GetPreviewTextureSize(textureWidth, textureHeight);
                    generatedGradientTexture = ColorTextureGenerator.CreateTextureFromGradient(gradient, pixelsOrientation, _previewTextureSize.Item1, _previewTextureSize.Item2);
                }
            }

            NormalSpace();
            DrawLine();
            NormalSpace();
            drawTexturePreview(generatedGradientTexture);
            NormalSpace();

            if (canGenerateTexture == false)
            {
                return;
            }

            using (new HorizontalScope())
            {
                FlexibleSpace();
                float _buttonWidth = windowWidthWithPaddings * CREATE_BUTTON_WIDTH;

                using (ColorScope.Background(DEFAULT_GRAY))
                {
                    switch (exportMode)
                    {
                        case TextureExportMode.CreateNew:

                            if (DrawClearBoxButton(SAVE_AS_TEXTURE, FixedWidthAndHeight(_buttonWidth, EXPORT_BUTTON_HEIGHT)))
                            {
                                createOrOverrideGradientTexture();
                            }

                            break;

                        case TextureExportMode.OverrideExisting:

                            if (DrawClearBoxButton(OVERRIDE_TEXTURE, FixedWidthAndHeight(_buttonWidth, EXPORT_BUTTON_HEIGHT)))
                            {
                                createOrOverrideGradientTexture();
                            }

                            break;
                    }
                }

                FlexibleSpace();
            }
        }

        private void createOrOverrideGradientTexture()
        {
            string _path = string.Empty;
            ExportFormat _exportFormat = exportFormat;

            switch (exportMode)
            {
                case TextureExportMode.CreateNew:
                    _path = EditorUtility.SaveFilePanel(SAVE_GRADIENT_AS_TEXTURE, Application.dataPath, DEFAULT_GRADIENT_NAME, getFileExtension(exportFormat).TrimStart(DOT));
                    break;

                case TextureExportMode.OverrideExisting:
                    _path = Path.GetFullPath(AssetDatabase.GetAssetPath(gradientTextureToOverride)).Replace("\\", "/");
                    _exportFormat = getFormatFromFile(gradientTextureToOverride);
                    break;
            }

            if (_path.IsNullEmptyOrWhitespace())
            {
                return;
            }

            Texture2D _texture = ColorTextureGenerator.CreateTextureFromGradient(gradient, pixelsOrientation, textureWidth, textureHeight);
            _texture.name = Path.GetFileName(_path).TrimEnd(getFileExtension(_exportFormat));

            if (_path.StartsWith(Application.dataPath))
            {
                saveAndPingTheTexture(_path, _texture, _exportFormat);
            }
            else
            {
                ColorTextureGenerator.Error(FILE_PATH_ERROR);
            }
        }

        private void drawColorRampTab()
        {
            SmallSpace();

            using (new HorizontalScope())
            {
                GUILayout.Label(INDEX_LABEL, Styles.GrayMiniLabel, FixedWidth(INDEX_WIDTH));
                GUILayout.Label(COLOR_LABEL, Styles.GrayMiniLabel);
                GUILayout.Label(COVERAGE_LABEL, Styles.GrayMiniLabel, FixedWidth(COVERAGE_WIDTH));
                GUILayout.Label(COLOR_LERP_LABEL, Styles.GrayMiniLabel, FixedWidth(TOGGLE_WIDTH + COVERAGE_WIDTH));
                GUILayout.Label(string.Empty, FixedWidth(DELETE_COLOR_RAMP_WIDTH));
            }

            using (new LabelWidthScope(LABEL_WIDTH))
            {
                using (var _changeScope = new ChangeCheckScope())
                {
                    if (colorRamp.NumberOfColors > MAX_VISIBLE_ROWS)
                    {
                        float _height = MAX_VISIBLE_ROWS * DEFAULT_LINE_HEIGHT;

                        using (var _scrollScope = new ScrollViewScope(colorRampScrollPosition, FixedHeight(_height)))
                        {
                            drawColorsInRamp();
                            colorRampScrollPosition = _scrollScope.scrollPosition;
                        }
                    }
                    else
                    {
                        drawColorsInRamp();
                    }

                    SmallSpace();

                    using (new HorizontalScope())
                    {
                        if (DrawBoxButton(RESET_LIST_LABEL, FixedWidthAndHeight(75f, DEFAULT_LINE_HEIGHT)))
                        {
                            colorRamp.ResetList();
                        }

                        SmallSpace();

                        if (DrawBoxButton(RESET_COVERAGE_LABEL, FixedWidthAndHeight(105f, DEFAULT_LINE_HEIGHT)))
                        {
                            colorRamp.ResetCoverage();
                        }

                        FlexibleSpace();

                        if (colorRamp.CanAddColors && DrawBoxButton(PLUS, FixedWidthAndHeight(25f, DEFAULT_LINE_HEIGHT)))
                        {
                            colorRamp.AddColor(Color.white, 1f);
                        }
                    }

                    drawTextureSettings();

                    if (_changeScope.changed)
                    {
                        colorRamp.RegenerateTexture(pixelsOrientation, textureWidth, textureHeight);
                    }
                }
            }

            NormalSpace();
            DrawLine();
            NormalSpace();
            drawTexturePreview(colorRamp.GeneratedTexture);
            NormalSpace();

            if (canGenerateTexture == false)
            {
                return;
            }

            using (new HorizontalScope())
            {
                FlexibleSpace();
                float _buttonWidth = windowWidthWithPaddings * CREATE_BUTTON_WIDTH;

                using (ColorScope.Background(DEFAULT_GRAY))
                {
                    switch (exportMode)
                    {
                        case TextureExportMode.CreateNew:

                            if (DrawClearBoxButton(SAVE_AS_TEXTURE, FixedWidthAndHeight(_buttonWidth, EXPORT_BUTTON_HEIGHT)))
                            {
                                createOrOverrideColorRampTexture();
                            }

                            break;

                        case TextureExportMode.OverrideExisting:

                            if (DrawClearBoxButton(OVERRIDE_TEXTURE, FixedWidthAndHeight(_buttonWidth, EXPORT_BUTTON_HEIGHT)))
                            {
                                createOrOverrideColorRampTexture();
                            }

                            break;
                    }
                }

                FlexibleSpace();
            }
        }

        private int drawColorsInRamp()
        {
            int _index = 0;

            foreach (var _color in colorRamp.Colors)
            {
                using (new HorizontalScope())
                {
                    GUIStyle _label = new GUIStyle(EditorStyles.label);
                    _label.alignment = TextAnchor.MiddleLeft;

                    GUILayout.Label($"{(_index + 1).NumberToString(2)}", Styles.BoxButton, FixedWidthAndHeight(INDEX_WIDTH, DEFAULT_LINE_HEIGHT));

                    using (new HorizontalScope(Styles.BoxButton, FixedHeight(DEFAULT_LINE_HEIGHT)))
                    {
                        _color.RampColor = EditorGUILayout.ColorField(_color.RampColor);
                    }

                    using (new HorizontalScope(Styles.BoxButton, FixedWidthAndHeight(COVERAGE_WIDTH, DEFAULT_LINE_HEIGHT)))
                    {
                        _color.Coverage = EditorGUILayout.FloatField(_color.Coverage, FixedWidth(COVERAGE_WIDTH));
                    }

                    float _width = TOGGLE_WIDTH + COVERAGE_WIDTH + 1f;

                    using (new HorizontalScope(Styles.BoxButton, FixedWidthAndHeight(_width, DEFAULT_LINE_HEIGHT)))
                    {
                        if (_index == colorRamp.NumberOfColors - 1)
                        {
                            _color.UseLerpToNextColor = false;
                            GUILayout.Label(string.Empty, FixedWidth(_width)); // Empty space when it's the last color
                        }
                        else
                        {

                            _color.UseLerpToNextColor = EditorGUILayout.Toggle(_color.UseLerpToNextColor, FixedWidth(TOGGLE_WIDTH));

                            using (new DisabledScope(_color.UseLerpToNextColor == false))
                            {
                                _color.LerpCoverage = EditorGUILayout.FloatField(_color.LerpCoverage, FixedWidth(COVERAGE_WIDTH));
                            }
                        }
                    }

                    if (DrawBoxButton(DELETE, FixedWidthAndHeight(DELETE_COLOR_RAMP_WIDTH, DEFAULT_LINE_HEIGHT)))
                    {
                        colorRamp.Colors.RemoveAt(_index);
                        break;
                    }
                }

                _index++;
            }

            return _index;
        }

        private void drawObjectsTab()
        {
            SmallSpace();

            using (var _changeScope = new ChangeCheckScope())
            {
                foundObjectLabelFilter = EditorGUILayout.TextField(FILTER_BY_LABEL, foundObjectLabelFilter);
                colorObjectsSortMode = DrawEnumWithOrder(colorObjectsSortMode, SORT_BY, ref objectsSortOrder, sumOfPaddings);

                if (_changeScope.changed)
                {
                    findAndSortColorTextureObjects();
                }
            }

            SmallSpace();

            using (new HorizontalScope())
            {
                FlexibleSpace();

                if (DrawBoxButton(REFRESH_OBJECTS, FixedWidthAndHeight(wholeSizeButtonWidth, DEFAULT_LINE_HEIGHT)))
                {
                    findAndSortColorTextureObjects();
                }

                FlexibleSpace();
            }

            SmallSpace();
            DrawLine();
            SmallSpace();

            if (sortedAndFilteredColorObjects.IsNullOrEmpty())
            {
                EditorGUILayout.HelpBox(NO_OBJECTS_INFO, MessageType.Info);
                return;
            }

            using (new HorizontalScope())
            {
                GUILayout.Label(string.Empty, Styles.GrayMiniLabel, FixedWidth(INDEX_WIDTH));
                GUILayout.Button(NAME, Styles.GrayMiniLabel);
                GUILayout.Button(REFERENCE, Styles.GrayMiniLabel, FixedWidth(OBJECT_FIELD_WIDTH));
                GUILayout.Label(string.Empty, Styles.GrayMiniLabel, FixedWidth(DEFAULT_LINE_HEIGHT));
                GUILayout.Label(string.Empty, Styles.GrayMiniLabel, FixedWidth(DEFAULT_LINE_HEIGHT));
            }

            VerySmallSpace();

            using (var _scrollView = new ScrollViewScope(foundObjectsScrollPosition))
            {
                foundObjectsScrollPosition = _scrollView.scrollPosition;
                int _index = 1;

                foreach (var _object in sortedAndFilteredColorObjects)
                {
                    if (_object == null)
                    {
                        findAndSortColorTextureObjects();
                        return;
                    }

                    using (new HorizontalScope())
                    {
                        GUILayout.Label(_index.ToString(), Styles.DefaultLabelCenter, FixedWidthAndHeight(INDEX_WIDTH, DEFAULT_LINE_HEIGHT));

                        if (GUILayout.Button(_object.name, Styles.DefaultLabelLeft, FixedHeight(DEFAULT_LINE_HEIGHT)))
                        {
                            _object.OpenInWindow();
                        }

                        using (new ScopeGroup(new DisabledScope(), new HorizontalScope(Styles.BoxButton, FixedWidth(OBJECT_FIELD_WIDTH))))
                        {
                            VerySmallSpace();
                            EditorGUILayout.ObjectField(_object, typeof(UnityEngine.Object), false, FixedWidth(OBJECT_FIELD_WIDTH - 8));
                            VerySmallSpace();
                        }

                        if (GUILayout.Button(string.Empty, Styles.FixedSelect(DEFAULT_LINE_HEIGHT), FixedWidthAndHeight(DEFAULT_LINE_HEIGHT)))
                        {
                            AssetsUtilities.Ping(_object);
                        }

                        if (GUILayout.Button(string.Empty, Styles.FixedClose(DEFAULT_LINE_HEIGHT), FixedWidthAndHeight(DEFAULT_LINE_HEIGHT)))
                        {
                            if (EditorUtility.DisplayDialog($"Delete {_object.name}", CONFIRM_DELETION_QUESTION, YES, CANCEL))
                            {
                                string _path = AssetDatabase.GetAssetPath(_object);

                                if (_path.IsNullEmptyOrWhitespace() == false)
                                {
                                    AssetDatabase.DeleteAsset(_path);
                                    AssetDatabase.Refresh();
                                    findAndSortColorTextureObjects();
                                    return;
                                }
                            }
                        }
                    }

                    _index++;
                }
            }
        }

        private void findAndSortColorTextureObjects()
        {
            allColorTextureObjects = AssetsUtilities.GetAssetsOfType<ColorTextureObject>();
            sortedAndFilteredColorObjects.Clear();

            if (allColorTextureObjects == null || allColorTextureObjects.Length == 0)
            {
                return;
            }

            string _filterToLower = foundObjectLabelFilter.ToLower().Trim();

            foreach (var _object in allColorTextureObjects)
            {
                if (_object == null)
                {
                    continue;
                }

                string _nameToLower = _object.name.ToLower().Trim();

                if (_filterToLower.IsNullEmptyOrWhitespace() || _nameToLower.Contains(_filterToLower))
                {
                    sortedAndFilteredColorObjects.Add(_object);
                }
            }

            switch (colorObjectsSortMode)
            {
                case ColorObjectsSortMode.Name:
                    sortedAndFilteredColorObjects.Sort((x, y) => x.name.CompareTo(y.name));
                    break;
                case ColorObjectsSortMode.CreationDate:
                    sortedAndFilteredColorObjects.Sort((x, y) => File.GetCreationTime(AssetDatabase.GetAssetPath(x)).CompareTo(File.GetCreationTime(AssetDatabase.GetAssetPath(y))));
                    break;
            }

            if (objectsSortOrder == false)
            {
                sortedAndFilteredColorObjects.Reverse();
            }
        }

        private void createOrOverrideColorRampTexture()
        {
            string _path = string.Empty;
            ExportFormat _exportFormat = exportFormat;

            switch (exportMode)
            {
                case TextureExportMode.CreateNew:
                    _path = EditorUtility.SaveFilePanel(SAVE_COLOR_RAMP_AS_TEXTURE, Application.dataPath, DEFAULT_COLOR_RAMP_NAME, getFileExtension(exportFormat).TrimStart(DOT));
                    break;

                case TextureExportMode.OverrideExisting:
                    _path = Path.GetFullPath(AssetDatabase.GetAssetPath(colorRampTextureToOverride)).Replace("\\", "/");
                    _exportFormat = getFormatFromFile(colorRampTextureToOverride);
                    break;
            }

            if (_path.IsNullEmptyOrWhitespace())
            {
                return;
            }

            Texture2D _texture = ColorTextureGenerator.CreateTextureFromColorRamp(colorRamp, pixelsOrientation, textureWidth, textureHeight);
            _texture.name = Path.GetFileName(_path).TrimEnd(getFileExtension(_exportFormat));

            if (_path.StartsWith(Application.dataPath))
            {
                saveAndPingTheTexture(_path, _texture, _exportFormat);
            }
            else
            {
                ColorTextureGenerator.Error(FILE_PATH_ERROR);
            }
        }

        private void drawTexturePreview(Texture2D _texture)
        {
            GUIStyle _textureStyle = new GUIStyle();
            _textureStyle.normal.background = _texture;

            using (new HorizontalScope())
            {
                FlexibleSpace();
                EditorGUILayout.LabelField(string.Empty, _textureStyle, FixedWidthAndHeight(_texture.width, _texture.height));
                FlexibleSpace();
            }
        }

        private void drawTextureSettings()
        {
            NormalSpace();
            DrawLine();
            NormalSpace();

            using (new HorizontalScope())
            {
                textureWidth = Mathf.Max(MIN_TEXTURE_SIZE, EditorGUILayout.IntField(WIDTH_LABEL, textureWidth));
                VerySmallSpace();

                if (GUILayout.Button(COPY_HEIGHT, FixedWidth(COPY_BUTTON_WIDTH)))
                {
                    textureWidth = textureHeight;
                }
            }

            using (new HorizontalScope())
            {
                textureHeight = Mathf.Max(MIN_TEXTURE_SIZE, EditorGUILayout.IntField(HEIGHT_LABEL, textureHeight));
                VerySmallSpace();

                if (GUILayout.Button(COPY_WIDTH, FixedWidth(COPY_BUTTON_WIDTH)))
                {
                    textureHeight = textureWidth;
                }
            }

            pixelsOrientation = (PixelsOrientation) EditorGUILayout.EnumPopup(PIXELS_ORIENTATION_LABEL, pixelsOrientation);

            NormalSpace();
            DrawLine();
            NormalSpace();

            exportMode = (TextureExportMode) EditorGUILayout.EnumPopup(EXPORT_MODE, exportMode);

            switch (exportMode)
            {
                case TextureExportMode.CreateNew:

                    exportFormat = (ExportFormat) EditorGUILayout.EnumPopup(EXPORT_FORMAT, exportFormat);
                    canGenerateTexture = true;
                    break;

                case TextureExportMode.OverrideExisting:
                    drawTextureToOverride();
                    break;
            }

            NormalSpace();
            DrawLine();
            NormalSpace();

            settingsExportMode = (SettingsExportMode) EditorGUILayout.EnumPopup(SETTINGS_EXPORT_MODE, settingsExportMode);

            switch (settingsExportMode)
            {
                case SettingsExportMode.ExportToFile:
                    drawExportSettingsButton();
                    break;

                case SettingsExportMode.OverrideExisting:

                    switch (windowMode)
                    {
                        case WindowMode.Gradient:
                            gradientObject = EditorGUILayout.ObjectField(GRADIENT_TO_OVERRIDE, gradientObject, typeof(GradientObject), false, FixedHeight(SingleLineHeight)) as GradientObject;
                            break;

                        case WindowMode.ColorRamp:
                            colorRampObject = EditorGUILayout.ObjectField(COLOR_RAMP_TO_OVERRIDE, colorRampObject, typeof(ColorRampObject), false, FixedHeight(SingleLineHeight)) as ColorRampObject;
                            break;
                    }

                    drawOverrideExistingSettingsButton();
                    break;
            }
        }

        private void drawTextureToOverride()
        {
            switch (windowMode)
            {
                case WindowMode.Gradient:
                    gradientTextureToOverride = EditorGUILayout.ObjectField(TEXTURE_TO_OVERRIDE, gradientTextureToOverride, typeof(Texture2D), false, FixedHeight(SingleLineHeight)) as Texture2D;

                    if (gradientTextureToOverride == null)
                    {
                        canGenerateTexture = false;
                        SmallSpace();
                        EditorGUILayout.HelpBox(TEXTURE_TO_OVERRIDE_CANNOT_BE_EMPTY, MessageType.Warning);
                    }
                    else
                    {
                        canGenerateTexture = true;
                    }

                    break;

                case WindowMode.ColorRamp:
                    colorRampTextureToOverride = EditorGUILayout.ObjectField(TEXTURE_TO_OVERRIDE, colorRampTextureToOverride, typeof(Texture2D), false, FixedHeight(SingleLineHeight)) as Texture2D;

                    if (colorRampTextureToOverride == null)
                    {
                        canGenerateTexture = false;
                        SmallSpace();
                        EditorGUILayout.HelpBox(TEXTURE_TO_OVERRIDE_CANNOT_BE_EMPTY, MessageType.Warning);
                    }
                    else
                    {
                        canGenerateTexture = true;
                    }

                    break;
            }
        }

        private void drawExportSettingsButton()
        {
            SmallSpace();
            float _buttonWidth = windowWidthScaled(0.7f);

            using (new ScopeGroup(new HorizontalScope(), ColorScope.Background(mainColor)))
            {
                FlexibleSpace();

                if (DrawClearBoxButton(EXPORT_SETTINGS_INTO_SCRIPTABLE, FixedWidthAndHeight(_buttonWidth, DEFAULT_LINE_HEIGHT)))
                {
                    switch (windowMode)
                    {
                        case WindowMode.Gradient:
                            exportGradientSettings();
                            break;

                        case WindowMode.ColorRamp:
                            exportColorRampSettings();
                            break;
                    }
                }

                FlexibleSpace();
            }
        }

        private void exportGradientSettings()
        {
            var _newGradient = ScriptableObject.CreateInstance<GradientObject>();
            _newGradient.name = DEFAULT_GRADIENT_NAME;
            _newGradient.SetGradient(gradient, textureWidth, textureHeight, pixelsOrientation, gradientTextureToOverride);

            var _path = EditorUtility.SaveFilePanel(SAVE_GRADIENT_INTO_SCRIPTABLE, Application.dataPath, _newGradient.name, ASSET);

            if (_path.IsNullEmptyOrWhitespace())
            {
                return;
            }

            if (_path.StartsWith(Application.dataPath))
            {
                AssetDatabase.CreateAsset(_newGradient, AssetsUtilities.ConvertAbsolutePathToDataPath(_path));
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                AssetsUtilities.Ping(_newGradient);
            }
            else
            {
                ColorTextureGenerator.Error(SCRIPTABLE_PATH_ERROR);
            }
        }

        private void exportColorRampSettings()
        {
            var _newColorRamp = ScriptableObject.CreateInstance<ColorRampObject>();
            _newColorRamp.name = DEFAULT_COLOR_RAMP_NAME;
            _newColorRamp.SetColorRamp(colorRamp, textureWidth, textureHeight, pixelsOrientation, colorRampTextureToOverride);

            var _path = EditorUtility.SaveFilePanel(SAVE_COLOR_RAMP_INTO_SCRIPTABLE, Application.dataPath, _newColorRamp.name, ASSET);

            if (_path.IsNullEmptyOrWhitespace())
            {
                return;
            }

            if (_path.StartsWith(Application.dataPath))
            {
                AssetDatabase.CreateAsset(_newColorRamp, AssetsUtilities.ConvertAbsolutePathToDataPath(_path));
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                AssetsUtilities.Ping(_newColorRamp);
            }
            else
            {
                ColorTextureGenerator.Error(SCRIPTABLE_PATH_ERROR);
            }
        }

        private void drawOverrideExistingSettingsButton()
        {
            float _buttonWidth = windowWidthScaled(0.7f);

            switch (windowMode)
            {
                case WindowMode.Gradient:

                    if (gradientObject == null)
                    {
                        return;
                    }

                    SmallSpace();

                    using (new ScopeGroup(new HorizontalScope(), ColorScope.Background(mainColor)))
                    {
                        FlexibleSpace();

                        if (DrawClearBoxButton(OVERRIDE_GRADIENT_IN_ATTACHED_FILE, FixedWidthAndHeight(_buttonWidth, DEFAULT_LINE_HEIGHT)))
                        {
                            gradientObject.SetGradient(gradient, textureWidth, textureHeight, pixelsOrientation, gradientTextureToOverride);
                        }

                        FlexibleSpace();
                    }

                    break;

                case WindowMode.ColorRamp:

                    if (colorRampObject == null)
                    {
                        return;
                    }

                    SmallSpace();

                    using (new ScopeGroup(new HorizontalScope(), ColorScope.Background(mainColor)))
                    {
                        FlexibleSpace();

                        if (DrawClearBoxButton(OVERRIDE_COLOR_RAMP_IN_ATTACHED_FILE, FixedWidthAndHeight(_buttonWidth, DEFAULT_LINE_HEIGHT)))
                        {
                            colorRampObject.SetColorRamp(colorRamp, textureWidth, textureHeight, pixelsOrientation, colorRampTextureToOverride);
                        }

                        FlexibleSpace();
                    }

                    break;
            }
        }

        private void regenerateTextures()
        {
            switch (windowMode)
            {
                case WindowMode.Gradient:
                    var _previewTextureSize = ColorTextureGenerator.GetPreviewTextureSize(textureWidth, textureHeight);
                    generatedGradientTexture = ColorTextureGenerator.CreateTextureFromGradient(gradient, pixelsOrientation, _previewTextureSize.Item1, _previewTextureSize.Item2);
                    break;

                case WindowMode.ColorRamp:
                    colorRamp.RegenerateTexture(pixelsOrientation, textureWidth, textureHeight);
                    break;
            }
        }

        public static void OpenWithGradient(GradientObject _gradient)
        {
            if (_gradient == null)
            {
                return;
            }

            var _window = GetWindow<ColorTextureGeneratorWindow>();
            _window.windowMode = WindowMode.Gradient;
            _window.gradient = _gradient.GetGradientCopy();
            _window.gradientObject = _gradient;

            assignVariablesAndOpenWindow(_window, _gradient);
        }

        public static void OpenWithColorRamp(ColorRampObject _colorRamp)
        {
            if (_colorRamp == null)
            {
                return;
            }

            var _window = GetWindow<ColorTextureGeneratorWindow>();
            _window.windowMode = WindowMode.ColorRamp;
            _window.colorRamp = _colorRamp.GetColorRampCopy();
            _window.colorRampObject = _colorRamp;

            assignVariablesAndOpenWindow(_window, _colorRamp);
        }

        private static void assignVariablesAndOpenWindow(ColorTextureGeneratorWindow _window, ColorTextureObject _object)
        {
            _window.settingsExportMode = SettingsExportMode.OverrideExisting;
            _window.textureWidth = _object.Width;
            _window.textureHeight = _object.Height;
            _window.pixelsOrientation = _object.OrientationOfPixels;

            if (_object.TextureToOverride != null)
            {
                _window.exportMode = TextureExportMode.OverrideExisting;

                switch (_window.windowMode)
                {
                    case WindowMode.Gradient:
                        _window.gradientTextureToOverride = _object.TextureToOverride;
                        break;

                    case WindowMode.ColorRamp:
                        _window.colorRampTextureToOverride = _object.TextureToOverride;
                        break;
                }
            }

            _window.regenerateTextures();
            _window.Show();
        }

        private static void saveAndPingTheTexture(string _filePath, Texture2D _texture, ExportFormat _exportFormat)
        {
            byte[] _pixelsData = null;

            switch (_exportFormat)
            {
                case ExportFormat.PNG:
                    _pixelsData = _texture.EncodeToPNG();
                    break;

                case ExportFormat.TGA:
                    _pixelsData = _texture.EncodeToTGA();
                    break;

                case ExportFormat.JPG:
                    _pixelsData = _texture.EncodeToJPG();
                    break;
            }

            File.WriteAllBytes(_filePath, _pixelsData);
            AssetDatabase.Refresh();

            Texture2D _loaded = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetsUtilities.ConvertAbsolutePathToDataPath(_filePath));
            AssetsUtilities.Ping(_loaded);
        }

        private static ExportFormat getFormatFromFile(Texture2D _texture)
        {
            string _path = AssetDatabase.GetAssetPath(_texture);
            string _extension = Path.GetExtension(_path);

            return _extension switch
            {
                PNG_EXTENSION => ExportFormat.PNG,
                TGA_EXTENSION => ExportFormat.TGA,
                JPG_EXTENSION => ExportFormat.JPG,
                _ => ExportFormat.PNG,
            };
        }

        private static string getFileExtension(ExportFormat _exportFormat)
        {
            return _exportFormat switch
            {
                ExportFormat.PNG => PNG_EXTENSION,
                ExportFormat.TGA => TGA_EXTENSION,
                ExportFormat.JPG => JPG_EXTENSION,
                _ => PNG_EXTENSION,
            };
        }

        [MenuItem("Window/FewClicks Dev/Color Texture Generator", priority = 103)]
        private static void ShowWindow()
        {
            GetWindow<ColorTextureGeneratorWindow>().Show();
        }
    }
}