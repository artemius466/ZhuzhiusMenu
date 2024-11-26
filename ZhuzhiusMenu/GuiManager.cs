using UnityEngine;
using System;

namespace Zhuzhius
{
    public class GuiManager : MonoBehaviour
    {
        private static GuiManager _instance;
        public static GuiManager Instance
        {
            get
            {
                if (_instance == null)
                    Debug.LogWarning("GuiManager instance is null!");
                return _instance;
            }
            private set => _instance = value;
        }

        // Style configuration
        private const string GAME_FONT_NAME = "SuperMarioDsRegular-Ea4R8";
        private const float TRANSITION_DURATION = 2.0f;
        
        private static readonly Color32 START_COLOR = new Color32(164, 0, 209, 255);
        private static readonly Color32 END_COLOR = new Color32(88, 0, 118, 255);

        // Public properties for styles
        public static Color32 CurrentColor { get; private set; }
        public static Color32 TextColor { get; private set; }
        public static Color32 EnabledColor { get; private set; }
        
        public static GUIStyle TextStyle { get; private set; }
        public static GUIStyle VeryBigTextStyle { get; private set; }
        public static GUIStyle BigTextStyle { get; private set; }
        public static GUIStyle SmallTextStyle { get; private set; }

        // Private fields
        private bool _transitionToStart;
        private float _transitionProgress;
        private Font _gameFont;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeStyles();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializeStyles()
        {
            CurrentColor = START_COLOR;
            _gameFont = GetFontFromGame(GAME_FONT_NAME);

            if (_gameFont != null)
            {
                InitializeTextStyles();
            }
            else
            {
                Debug.LogError($"Failed to initialize styles: Font {GAME_FONT_NAME} not found");
            }
        }

        private void InitializeTextStyles()
        {
            BigTextStyle = CreateTextStyle(19);
            TextStyle = CreateTextStyle(15);
            SmallTextStyle = CreateTextStyle(17);
            VeryBigTextStyle = CreateTextStyle(30);
        }

        private GUIStyle CreateTextStyle(int fontSize)
        {
            return new GUIStyle
            {
                font = _gameFont,
                fontSize = fontSize,
                normal = { textColor = Color.white },
                fontStyle = FontStyle.Normal,
                // alignment = TextAnchor.MiddleLeft,
                wordWrap = true
            };
        }

        private Font GetFontFromGame(string fontName)
        {
            try
            {
                var allFonts = Resources.FindObjectsOfTypeAll<Font>();
                foreach (var font in allFonts)
                {
                    if (string.Equals(font.name, fontName, StringComparison.OrdinalIgnoreCase))
                    {
                        Debug.Log($"Found and using font: {font.name}");
                        return font;
                    }
                }
                
                Debug.LogWarning($"Font '{fontName}' not found in loaded resources");
                return null;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error loading font: {ex.Message}");
                return null;
            }
        }

        private void Update()
        {
            UpdateTransition();
            UpdateColors();
        }

        private void UpdateTransition()
        {
            if (_transitionProgress >= 1f)
            {
                _transitionProgress = 0f;
                _transitionToStart = !_transitionToStart;
            }

            _transitionProgress += Time.deltaTime / TRANSITION_DURATION;
            _transitionProgress = Mathf.Clamp01(_transitionProgress);
        }

        private void UpdateColors()
        {
            CurrentColor = _transitionToStart
                ? Color32.Lerp(START_COLOR, END_COLOR, _transitionProgress)
                : Color32.Lerp(END_COLOR, START_COLOR, _transitionProgress);

            TextColor = new Color32(
                (byte)Mathf.Min(255, CurrentColor.r + 30),
                (byte)Mathf.Min(255, CurrentColor.g + 30),
                (byte)Mathf.Min(255, CurrentColor.b + 30),
                CurrentColor.a
            );

            EnabledColor = new Color32(
                (byte)Mathf.Max(0, CurrentColor.r - 80),
                CurrentColor.g,
                (byte)Mathf.Max(0, CurrentColor.b - 80),
                CurrentColor.a
            );
        }

        private void OnDestroy()
        {
            if (_gameFont != null)
            {
                Resources.UnloadAsset(_gameFont);
                _gameFont = null;
            }
        }
    }
}
