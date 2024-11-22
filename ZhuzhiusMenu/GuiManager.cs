using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UIElements;

namespace Zhuzhius
{
    public class GuiManager : MonoBehaviour
    {
        public static Color32 currentColor;
        public static Color32 textColor;
        public static Color32 enabledColor;
        public static GUIStyle textStyle;
        public static GUIStyle veryBigTextStyle;
        public static GUIStyle bigTextStyle;
        public static GUIStyle smallTextStyle;

        private static Color32 startColor = new Color32(164, 0, 209, 255);
        private static Color32 endColor = new Color32(88, 0, 118, 255);

        private static bool toStart;

        private static float transitionDuration = 2.0f;
        private static float transitionProgress = 0.0f;

        private void Start()
        {
            currentColor = startColor;

            Font gameFont = GetFontFromGame("SuperMarioDsRegular-Ea4R8");
            if (gameFont != null)
            {
                bigTextStyle = new GUIStyle
                {
                    font = gameFont,
                    fontSize = 19,
                    normal = { textColor = Color.white }
                };

                textStyle = new GUIStyle
                {
                    font = gameFont,
                    fontSize = 15,
                    normal = { textColor = Color.white }
                };

                smallTextStyle = new GUIStyle
                {
                    font = gameFont,
                    fontSize = 17,
                    normal = { textColor = Color.white }
                };

                veryBigTextStyle = new GUIStyle
                {
                    font = gameFont,
                    fontSize = 30,
                    normal = { textColor = Color.white }
                };
            }
            

            Debug.Log($"Используемый шрифт: {textStyle.font.name}");
        }

        private Font GetFontFromGame(string fontName)
        {
            // Ищем все шрифты, загруженные в игру
            Font[] allFonts = Resources.FindObjectsOfTypeAll<Font>();

            foreach (var font in allFonts)
            {
                Debug.Log($"Найден шрифт: {font.name}");
                if (font.name == fontName)
                {
                    Debug.Log($"Используем шрифт: {font.name}");
                    return font;
                }
            }

            Debug.LogError($"Шрифт с именем {fontName} не найден!");
            return null;
        }

        private void Update()
        {
            if (transitionProgress == 1f)
            {
                transitionProgress = 0f;
                toStart = !toStart;
            }

            transitionProgress += Time.deltaTime / transitionDuration;

            transitionProgress = Mathf.Clamp01(transitionProgress);

            if (toStart)
            {
                currentColor = Color32.Lerp(startColor, endColor, transitionProgress);
            } else
            {
                currentColor = Color32.Lerp(endColor, startColor, transitionProgress);
            }

            textColor = currentColor;
            textColor.r += 30;
            textColor.g += 30;
            textColor.b += 30;

            enabledColor = currentColor;
            enabledColor.r -= 80;
            //enabledColor.g -= 30;
            enabledColor.b -= 80;
        }
    }
}
