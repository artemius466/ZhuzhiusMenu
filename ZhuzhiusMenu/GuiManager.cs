using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Zhuzhius
{
    public class GuiManager : MonoBehaviour
    {
        public static Color32 currentColor;
        public static Color32 textColor;
        public static Color32 enabledColor;

        private static Color32 startColor = new Color32(164, 0, 209, 255);
        private static Color32 endColor = new Color32(108, 0, 138, 255);

        private static bool toStart;

        private static float transitionDuration = 2.0f;
        private static float transitionProgress = 0.0f;

        private void Start()
        {
            currentColor = startColor;
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
